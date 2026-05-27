-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-23
-- Description: Cancel a position request and update status
-- Based on Power Platform API: postCancelRequest
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_CancelRequest]
	@RequestID UNIQUEIDENTIFIER,
	@RequestorName NVARCHAR(255),
	@RequestorEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CancelledStatusId UNIQUEIDENTIFIER;
	DECLARE @CancelledOn DATETIME2 = GETUTCDATE();
	DECLARE @PendingStatusId UNIQUEIDENTIFIER ; -- Pending status GUID from Power Platform

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT TOP 1 @PendingStatusId = PositionStatusId
		FROM PositionStatus
		WHERE StatusName = 'Pending';

		-- Get Cancelled status ID
		SELECT TOP 1 @CancelledStatusId = PositionStatusId
		FROM PositionStatus
		WHERE StatusName = 'Cancelled';

		IF @CancelledStatusId IS NULL
		BEGIN
			RAISERROR('Cancelled status not found in PositionStatus table', 16, 1);
			RETURN;
		END

		-- Update all pending approvals to cancelled
		UPDATE Approval
		SET 
			CancelledBy = @RequestorName,
			CancelledOn = @CancelledOn,
			PositionStatusId = @CancelledStatusId
		WHERE 
			RequestId = @RequestID 
			AND PositionStatusId = @PendingStatusId;

		-- Update request status to cancelled
		UPDATE Request
		SET 
			CompletedOn = @CancelledOn,
			CurrentStatusId = @CancelledStatusId
		WHERE 
			RequestId = @RequestID;

		

		COMMIT TRANSACTION;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();

		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO
