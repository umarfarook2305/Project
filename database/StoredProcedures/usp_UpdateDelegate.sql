CREATE PROCEDURE [dbo].[usp_UpdateDelegate]
	@DelegateId UNIQUEIDENTIFIER,
	@DelegateUserEmail NVARCHAR(255) = NULL,
	@BehalfUserEmail NVARCHAR(255) = NULL,
	@DelegateFrom DATE = NULL,
	@DelegateTo DATE = NULL
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
				NULL AS DelegateId;
			RETURN;
		END;

		-- Update delegate record (only update fields that are provided)
		UPDATE dbo.Delegate
		SET 
			DelegateUserEmail = ISNULL(@DelegateUserEmail, DelegateUserEmail),
			BehalfUserEmail = ISNULL(@BehalfUserEmail, BehalfUserEmail),
			DelegateFrom = ISNULL(@DelegateFrom, DelegateFrom),
			DelegateTo = ISNULL(@DelegateTo, DelegateTo),
			ModifiedOn = GETDATE()
		WHERE DelegateId = @DelegateId;

		-- Return success response
		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			'Delegation updated successfully' AS Message,
			@DelegateId AS DelegateId;

		COMMIT TRANSACTION;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		-- Return error response
		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			'Failed to update delegation' AS Message,
			NULL AS DelegateId;
	END CATCH
END
GO
