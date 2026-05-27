CREATE PROCEDURE [dbo].[usp_CancelDelegate]
	@DelegateId UNIQUEIDENTIFIER,
	@CancelledOn DATE
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Check if delegate exists
		IF NOT EXISTS (SELECT 1 FROM dbo.Delegate WHERE DelegateId = @DelegateId)
		BEGIN
			SELECT 
				'404' AS StatusCode,
				'error' AS Status,
				'Delegate not found' AS Message,
				NULL AS DelegatedBy,
				NULL AS CancelledOn;
			RETURN;
		END;

		-- Update delegate record to cancel it by setting end date to cancellation date
		-- This way existing queries checking DelegateTo >= GETUTCDATE() will exclude it
		UPDATE dbo.Delegate
		SET 
			DelegateTo = @CancelledOn,  -- Set end date to cancellation date (makes it inactive)
			ModifiedOn = GETDATE()
		WHERE DelegateId = @DelegateId;

		-- Return updated delegate information
		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			'Delegate Cancelled Successfully' AS Message,
			d.DelegateUserEmail AS DelegatedBy,
			@CancelledOn AS CancelledOn
		FROM dbo.Delegate d
		WHERE d.DelegateId = @DelegateId;

		COMMIT TRANSACTION;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		-- Return error response
		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			'Error while Cancelling Delegate' AS Message,
			NULL AS DelegatedBy,
			NULL AS CancelledOn;
	END CATCH
END
GO
