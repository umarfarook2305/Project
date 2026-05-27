-- ============================================================
-- HART Job Profile Stored Procedure
-- Purpose: Fetch job profiles with optional job level filter
-- Mimics Power Platform workflow logic
-- ============================================================

IF OBJECT_ID('dbo.usp_GetJobProfiles', 'P') IS NOT NULL
	DROP PROCEDURE dbo.usp_GetJobProfiles;
GO

CREATE PROCEDURE dbo.usp_GetJobProfiles
	@JobLevel NVARCHAR(100) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @JobLevelGUID UNIQUEIDENTIFIER = NULL;

	-- If JobLevel is provided, try to find the matching JobLevel GUID
	-- Supports lookup by: Name, ID (GUID string), or Code
	IF @JobLevel IS NOT NULL AND LTRIM(RTRIM(@JobLevel)) <> ''
	BEGIN
		-- Try to find JobLevel by name, ID, or code
		SELECT TOP 1 @JobLevelGUID = JobLevelId
		FROM dbo.JobLevel
		WHERE 
			JobLevelName = @JobLevel
			OR CAST(JobLevelId AS NVARCHAR(50)) = @JobLevel
			OR JobLevelCode = @JobLevel
			AND IsActive = 1;

		-- If no matching JobLevel found, return empty result
		IF @JobLevelGUID IS NULL
		BEGIN
			SELECT 
				CAST(NULL AS NVARCHAR(255)) AS Profile,
				CAST(NULL AS NVARCHAR(255)) AS Family,
				CAST(NULL AS NVARCHAR(50)) AS ProfileID,
				CAST(NULL AS NVARCHAR(50)) AS FamilyID
			WHERE 1 = 0; -- Returns empty result set with correct schema
			RETURN;
		END
	END

	-- Main query: Get job profiles
	SELECT 
		jp.JobProfileName AS Profile,
		jf.JobFamilyName AS Family,
		CAST(jp.JobProfileId AS NVARCHAR(50)) AS ProfileID,
		CAST(jf.JobFamilyId AS NVARCHAR(50)) AS FamilyID
	FROM 
		dbo.JobProfile jp
		LEFT JOIN dbo.JobFamily jf ON jp.JobFamilyId = jf.JobFamilyId
		LEFT JOIN dbo.JobLevel jl ON jp.JobLevelId = jl.JobLevelId
	WHERE 
		jp.IsActive = 1
		AND jp.JobProfileName IS NOT NULL
		AND LTRIM(RTRIM(jp.JobProfileName)) <> ''
		AND (@JobLevelGUID IS NULL OR jp.JobLevelId = @JobLevelGUID)
	ORDER BY 
		jp.JobProfileName;
END;
GO

-- ============================================================
-- Grant Execute Permissions (adjust schema/user as needed)
-- ============================================================
-- GRANT EXECUTE ON dbo.usp_GetJobProfiles TO [YourAppUser];
GO

PRINT 'Stored procedure usp_GetJobProfiles created successfully!';
PRINT 'Usage: EXEC dbo.usp_GetJobProfiles @JobLevel = NULL (for all) or specific level';
GO

-- ============================================================
-- Test Examples
-- ============================================================
-- Get all job profiles:
-- EXEC dbo.usp_GetJobProfiles;

-- Get profiles by job level name:
-- EXEC dbo.usp_GetJobProfiles @JobLevel = '4';

-- Get profiles by job level ID:
-- EXEC dbo.usp_GetJobProfiles @JobLevel = '12be35c3-bc43-f111-bec6-7ced8d9ee5ad';
GO
