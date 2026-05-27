CREATE PROCEDURE [dbo].[usp_InsertDelegate]
	@DelegateUserEmail NVARCHAR(255),
	@BehalfUserEmail NVARCHAR(255),
	@DelegateFrom DATE,
	@DelegateTo DATE
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Validate required fields
		IF @DelegateUserEmail IS NULL OR @DelegateUserEmail = ''
		BEGIN
			SELECT 
				'400' AS StatusCode,
				'error' AS Status,
				'DelegateUserEmail is required' AS Message;
			ROLLBACK TRANSACTION;
			RETURN;
		END

		IF @BehalfUserEmail IS NULL OR @BehalfUserEmail = ''
		BEGIN
			SELECT 
				'400' AS StatusCode,
				'error' AS Status,
				'BehalfUserEmail is required' AS Message;
			ROLLBACK TRANSACTION;
			RETURN;
		END

		IF @DelegateFrom IS NULL
		BEGIN
			SELECT 
				'400' AS StatusCode,
				'error' AS Status,
				'DelegateFrom is required' AS Message;
			ROLLBACK TRANSACTION;
			RETURN;
		END

		IF @DelegateTo IS NULL
		BEGIN
			SELECT 
				'400' AS StatusCode,
				'error' AS Status,
				'DelegateTo is required' AS Message;
			ROLLBACK TRANSACTION;
			RETURN;
		END

		-- Validate date range
		IF @DelegateTo < @DelegateFrom
		BEGIN
			SELECT 
				'400' AS StatusCode,
				'error' AS Status,
				'DelegateTo must be greater than or equal to DelegateFrom' AS Message;
			ROLLBACK TRANSACTION;
			RETURN;
		END

		DECLARE @NewDelegateId UNIQUEIDENTIFIER = NEWID();
		DECLARE @DelegateCode NVARCHAR(50) = 'DEL-' + FORMAT(GETDATE(), 'yyyyMMddHHmmss');

		-- Insert new delegation record
		INSERT INTO dbo.Delegate (
			DelegateId,
			DelegateCode,
			DelegateUserEmail,
			BehalfUserEmail,
			DelegateFrom,
			DelegateTo,
			CreatedOn,
			ModifiedOn
		)
		VALUES (
			@NewDelegateId,
			@DelegateCode,
			@DelegateUserEmail,
			@BehalfUserEmail,
			@DelegateFrom,
			@DelegateTo,
			GETDATE(),
			GETDATE()
		);

		COMMIT TRANSACTION;

		-- Return success response
		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			'Delegate created successfully' AS Message,
			CAST(@NewDelegateId AS NVARCHAR(50)) AS DelegateId,
			@DelegateCode AS DelegateCode;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			'Failed to create delegation: ' + ERROR_MESSAGE() AS Message;
	END CATCH
END
GO
