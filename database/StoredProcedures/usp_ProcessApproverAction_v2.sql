-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-23
-- Description: Process approver action (Approve/Reject/Return)
-- Based on Power Platform API: postApproverAction
-- NOTE: Notification inserts are commented out until NotificationQueue table is created
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_ProcessApproverAction]
	@RequestID UNIQUEIDENTIFIER,
	@RequestorEmail NVARCHAR(255),
	@ApproverName NVARCHAR(255),
	@ApproverEmail NVARCHAR(255),
	@ApproverAction NVARCHAR(50), -- 'Approved', 'Rejected', 'Returned to Requestor'
	@Comments NVARCHAR(MAX) = NULL,
	@IsTesting BIT = 0 -- Set to 1 to skip notifications
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	-- Status IDs
	DECLARE @PendingStatusId UNIQUEIDENTIFIER;
	DECLARE @AwaitingStatusId UNIQUEIDENTIFIER;
	DECLARE @ApprovedStatusId UNIQUEIDENTIFIER;
	DECLARE @RejectedStatusId UNIQUEIDENTIFIER;
	DECLARE @ReturnedStatusId UNIQUEIDENTIFIER;

	-- Request variables
	DECLARE @CurrentApprovalId UNIQUEIDENTIFIER;
	DECLARE @CurrentSeqOrder NVARCHAR(100);
	DECLARE @MaxSeqOrder NVARCHAR(100);
	DECLARE @PendingWithEmail NVARCHAR(255);
	DECLARE @NextApprovalId UNIQUEIDENTIFIER;
	DECLARE @UserDetailId UNIQUEIDENTIFIER;

	-- Request details for notifications
	DECLARE @RequestCode NVARCHAR(50);
	DECLARE @RequestType NVARCHAR(50);
	DECLARE @FundingType NVARCHAR(100);
	DECLARE @JobLevel NVARCHAR(50);
	DECLARE @Position NVARCHAR(255);

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Get status IDs
		SELECT @PendingStatusId = PositionStatusId FROM dbo.PositionStatus WHERE StatusName = 'Pending';
		SELECT @AwaitingStatusId = PositionStatusId FROM dbo.PositionStatus WHERE StatusName = 'Awaiting';
		SELECT @ApprovedStatusId = PositionStatusId FROM dbo.PositionStatus WHERE StatusName = 'Approved';
		SELECT @RejectedStatusId = PositionStatusId FROM dbo.PositionStatus WHERE StatusName = 'Rejected';
		SELECT @ReturnedStatusId = PositionStatusId FROM dbo.PositionStatus WHERE StatusName = 'Returned to Requestor';

		-- Get request details
		SELECT 
			@RequestCode = r.RequestCode,
			@RequestType = r.RequestType,
			@FundingType = ft.FundingTypeName,
			@JobLevel = jl.JobLevelName,
			@Position = jp.JobProfileName,
			@PendingWithEmail = r.PendingWithEmail
		FROM dbo.Request r
		LEFT JOIN dbo.FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		LEFT JOIN dbo.JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN dbo.JobProfile jp ON r.JobProfileId = jp.JobProfileId
		WHERE r.RequestId = @RequestID;

		IF @RequestCode IS NULL
		BEGIN
			RAISERROR('Request not found', 16, 1);
			RETURN;
		END

		-- Find current approval record
		SELECT TOP 1
			@CurrentApprovalId = ApprovalId,
			@CurrentSeqOrder = SeqOrder
		FROM dbo.Approval
		WHERE RequestId = @RequestID
			AND IsApprover = 1
			AND (RequestedToEmail = @ApproverEmail OR RequestedToEmail = @PendingWithEmail)
		ORDER BY CAST(SeqOrder AS INT) DESC;

		IF @CurrentApprovalId IS NULL
		BEGIN
			RAISERROR('Approval record not found for this approver', 16, 1);
			RETURN;
		END

		-- Get max sequence order
		SELECT @MaxSeqOrder = MAX(SeqOrder)
		FROM dbo.Approval
		WHERE RequestId = @RequestID
			AND IsApprover = 1;

		-- Handle approver action
		IF @ApproverAction = 'Approved'
		BEGIN
			UPDATE dbo.Approval
			SET 
				PositionStatusId = @ApprovedStatusId,
				ApprovedBy = @ApproverName,
				ApprovedOn = GETUTCDATE()
			WHERE ApprovalId = @CurrentApprovalId;

			IF CAST(@CurrentSeqOrder AS INT) = CAST(@MaxSeqOrder AS INT)
			BEGIN
				UPDATE dbo.Request
				SET 
					CurrentStatusId = @ApprovedStatusId,
					CompletedOn = GETUTCDATE()
				WHERE RequestId = @RequestID;
			END
			ELSE
			BEGIN
				DECLARE @NextSeqOrder NVARCHAR(100);
				SET @NextSeqOrder = CAST(CAST(@CurrentSeqOrder AS INT) + 1 AS NVARCHAR(100));

				SELECT TOP 1 @NextApprovalId = ApprovalId
				FROM dbo.Approval
				WHERE RequestId = @RequestID AND IsApprover = 1 AND SeqOrder = @NextSeqOrder;

				UPDATE dbo.Approval
				SET PositionStatusId = @PendingStatusId, RequestedOn = GETUTCDATE()
				WHERE ApprovalId = @NextApprovalId;
			END
		END
		ELSE IF @ApproverAction = 'Rejected'
		BEGIN
			UPDATE dbo.Approval
			SET 
				PositionStatusId = @RejectedStatusId,
				RejectedBy = @ApproverName,
				RejectedOn = GETUTCDATE()
			WHERE ApprovalId = @CurrentApprovalId;

			UPDATE dbo.Request
			SET 
				CurrentStatusId = @RejectedStatusId,
				CompletedOn = GETUTCDATE()
			WHERE RequestId = @RequestID;
		END
		ELSE IF @ApproverAction = 'Returned to Requestor'
		BEGIN
			UPDATE dbo.Approval
			SET 
				PositionStatusId = @ReturnedStatusId,
				ReturnedBy = @ApproverName,
				ReturnedOn = GETUTCDATE()
			WHERE ApprovalId = @CurrentApprovalId;

			UPDATE dbo.Request
			SET CurrentStatusId = @ReturnedStatusId
			WHERE RequestId = @RequestID;
		END
		ELSE
		BEGIN
			RAISERROR('Invalid ApproverAction. Must be: Approved, Rejected, or Returned to Requestor', 16, 1);
			RETURN;
		END

		-- Add comment to history
		IF @Comments IS NOT NULL AND LEN(@Comments) > 0
		BEGIN
			SELECT TOP 1 @UserDetailId = UserDetailId
			FROM dbo.UserDetail
			WHERE EmailId = @ApproverEmail;

			IF @UserDetailId IS NOT NULL
			BEGIN
				DECLARE @CommentStatusId UNIQUEIDENTIFIER;

				IF @ApproverAction = 'Approved'
					SET @CommentStatusId = @ApprovedStatusId;
				ELSE IF @ApproverAction = 'Rejected'
					SET @CommentStatusId = @RejectedStatusId;
				ELSE IF @ApproverAction = 'Returned to Requestor'
					SET @CommentStatusId = @ReturnedStatusId;

				INSERT INTO dbo.CommentsHistory (
					CommentedById, ApprovalId, Comments, RequestId, RequestStatusId, CreatedOn
				)
				VALUES (
					@UserDetailId, @CurrentApprovalId, @Comments, @RequestID, @CommentStatusId, GETUTCDATE()
				);
			END
		END

		COMMIT TRANSACTION;

		-- Return success
		SELECT 
			'200' AS StatusCode,
			'Records Has updated and Notification sends to User' AS Message,
			@RequestCode AS RequestCode,
			@ApproverAction AS Action,
			CAST(@CurrentSeqOrder AS INT) AS CurrentSeqOrder,
			CAST(@MaxSeqOrder AS INT) AS MaxSeqOrder,
			CASE 
				WHEN @ApproverAction = 'Approved' AND CAST(@CurrentSeqOrder AS INT) = CAST(@MaxSeqOrder AS INT) THEN 'Completed'
				WHEN @ApproverAction = 'Approved' AND CAST(@CurrentSeqOrder AS INT) < CAST(@MaxSeqOrder AS INT) THEN 'MovedToNextApprover'
				ELSE @ApproverAction
			END AS ResultStatus;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SELECT 
			'500' AS StatusCode,
			ERROR_MESSAGE() AS Message,
			ERROR_NUMBER() AS ErrorNumber,
			ERROR_LINE() AS ErrorLine;
	END CATCH
END
GO
