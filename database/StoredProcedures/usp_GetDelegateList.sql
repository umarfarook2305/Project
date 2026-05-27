CREATE PROCEDURE [dbo].[usp_GetDelegateList]
	@EmailId NVARCHAR(255),
	@DelegationStatus BIT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- Get delegate list based on email and delegation status
		-- Active delegations: delegationStatus = true AND delegateTo >= today
		-- Inactive delegations: delegationStatus = false OR delegateTo < today

		-- Note: The Delegate table only has email columns, not name columns
		-- Name columns will be returned as empty strings or extracted from email

		IF @DelegationStatus = 1
		BEGIN
			-- Get active delegations
			SELECT 
				'200' AS StatusCode,
				'success' AS Status,
				ISNULL(d.DelegateCode, '') AS DelegateId,
				'' AS DelegateUser,  -- Name column doesn't exist
				ISNULL(d.DelegateUserEmail, '') AS DelegateUserEmail,
				CAST(CASE 
					WHEN d.DelegateTo >= CAST(GETUTCDATE() AS DATE) THEN 1 
					ELSE 0 
				END AS NVARCHAR(5)) AS DelegationStatus,
				d.DelegateFrom,
				d.DelegateTo,
				'' AS BehalfUser,  -- Name column doesn't exist
				ISNULL(d.BehalfUserEmail, '') AS BehalfUserEmail,
				'' AS DelegatedBy,  -- Name column doesn't exist
				ISNULL(d.DelegateUserEmail, '') AS DelegatedByEmail,  -- Use DelegateUserEmail as DelegatedByEmail
				d.CreatedOn AS DelegatedOn,
				d.DelegateId AS UniqueID
			FROM dbo.Delegate d
			WHERE d.BehalfUserEmail = @EmailId
				AND d.DelegateTo >= CAST(GETUTCDATE() AS DATE)
			ORDER BY d.CreatedOn DESC;
		END
		ELSE
		BEGIN
			-- Get inactive delegations (expired or cancelled)
			SELECT 
				'200' AS StatusCode,
				'success' AS Status,
				ISNULL(d.DelegateCode, '') AS DelegateId,
				'' AS DelegateUser,  -- Name column doesn't exist
				ISNULL(d.DelegateUserEmail, '') AS DelegateUserEmail,
				CAST(CASE 
					WHEN d.DelegateTo >= CAST(GETUTCDATE() AS DATE) THEN 1 
					ELSE 0 
				END AS NVARCHAR(5)) AS DelegationStatus,
				d.DelegateFrom,
				d.DelegateTo,
				'' AS BehalfUser,  -- Name column doesn't exist
				ISNULL(d.BehalfUserEmail, '') AS BehalfUserEmail,
				'' AS DelegatedBy,  -- Name column doesn't exist
				ISNULL(d.DelegateUserEmail, '') AS DelegatedByEmail,  -- Use DelegateUserEmail as DelegatedByEmail
				d.CreatedOn AS DelegatedOn,
				d.DelegateId AS UniqueID
			FROM dbo.Delegate d
			WHERE d.BehalfUserEmail = @EmailId
				AND d.DelegateTo < CAST(GETUTCDATE() AS DATE)
			ORDER BY d.CreatedOn DESC;
		END

	END TRY
	BEGIN CATCH
		-- Return error response
		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			'' AS DelegateId,
			'' AS DelegateUser,
			'' AS DelegateUserEmail,
			'False' AS DelegationStatus,
			NULL AS DelegateFrom,
			NULL AS DelegateTo,
			'' AS BehalfUser,
			'' AS BehalfUserEmail,
			'' AS DelegatedBy,
			'' AS DelegatedByEmail,
			NULL AS DelegatedOn,
			NULL AS UniqueID;
	END CATCH
END
GO
