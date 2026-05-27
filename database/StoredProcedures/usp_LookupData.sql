-- ============================================================
-- HART Lookup Data Stored Procedures
-- Purpose: Fetch lookup data for CW and FTE request types
-- ============================================================

-- ============================================================
-- SP 1: Get CW Lookup Data
-- Returns 6 lookup categories for Consultant/Worker requests
-- ============================================================
IF OBJECT_ID('dbo.usp_GetCWLookupData', 'P') IS NOT NULL
	DROP PROCEDURE dbo.usp_GetCWLookupData;
GO

CREATE PROCEDURE dbo.usp_GetCWLookupData
AS
BEGIN
	SET NOCOUNT ON;

	-- LineOfBusiness
	SELECT 
		LineOfBusinessId AS [Value],
		BusinessName AS Label
	FROM dbo.LineOfBusiness
	WHERE IsActive = 1
	ORDER BY BusinessName;

	-- ProjectTheme
	SELECT 
		ProjectThemeId AS [Value],
		ThemeName AS Label
	FROM dbo.ProjectTheme
	WHERE IsActive = 1
	ORDER BY ThemeName;

	-- SWPRole
	SELECT 
		SWPRoleId AS [Value],
		SWPRoleName AS Label
	FROM dbo.SWPRole
	WHERE IsActive = 1
	ORDER BY SWPRoleName;

	-- FundingType
	SELECT 
		FundingTypeId AS [Value],
		FundingTypeName AS Label
	FROM dbo.FundingType
	WHERE IsActive = 1
	ORDER BY FundingTypeName;

	-- CountryList
	SELECT 
		CountryListId AS [Value],
		CountryCode AS Label
	FROM dbo.CountryList
	WHERE IsActive = 1
	ORDER BY CountryCode;

	-- Status (PositionStatus)
	SELECT 
		PositionStatusId AS [Value],
		StatusName AS Label
	FROM dbo.PositionStatus
	WHERE IsActive = 1
	ORDER BY StatusName;
END;
GO

-- ============================================================
-- SP 2: Get FTE Lookup Data
-- Returns 8 lookup categories for Full-Time Employee requests
-- ============================================================
IF OBJECT_ID('dbo.usp_GetFTELookupData', 'P') IS NOT NULL
	DROP PROCEDURE dbo.usp_GetFTELookupData;
GO

CREATE PROCEDURE dbo.usp_GetFTELookupData
AS
BEGIN
	SET NOCOUNT ON;

	-- Joblevel
	SELECT 
		JobLevelId AS [Value],
		JobLevelName AS Label
	FROM dbo.JobLevel
	WHERE IsActive = 1
	ORDER BY JobLevelName;

	-- SWPRole
	SELECT 
		SWPRoleId AS [Value],
		SWPRoleName AS Label
	FROM dbo.SWPRole
	WHERE IsActive = 1
	ORDER BY SWPRoleName;

	-- PositionType
	SELECT 
		PositionTypeId AS [Value],
		PositionTypeName AS Label
	FROM dbo.PositionType
	WHERE IsActive = 1
	ORDER BY PositionTypeName;

	-- FundingType
	SELECT 
		FundingTypeId AS [Value],
		FundingTypeName AS Label
	FROM dbo.FundingType
	WHERE IsActive = 1
	ORDER BY FundingTypeName;

	-- RoleType
	SELECT 
		RoleTypeId AS [Value],
		RoleTypeName AS Label
	FROM dbo.RoleType
	WHERE IsActive = 1
	ORDER BY RoleTypeName;

	-- CountryList
	SELECT 
		CountryListId AS [Value],
		CountryCode AS Label
	FROM dbo.CountryList
	WHERE IsActive = 1
	ORDER BY CountryCode;

	-- Status (PositionStatus)
	SELECT 
		PositionStatusId AS [Value],
		StatusName AS Label
	FROM dbo.PositionStatus
	WHERE IsActive = 1
	ORDER BY StatusName;

	-- StatusOfPosition (VacancyType)
	SELECT 
		VacancyTypeId AS [Value],
		VacancyTypeName AS Label
	FROM dbo.VacancyType
	WHERE IsActive = 1
	ORDER BY VacancyTypeName;
END;
GO

-- ============================================================
-- Grant Execute Permissions (adjust schema/user as needed)
-- ============================================================
-- GRANT EXECUTE ON dbo.usp_GetCWLookupData TO [YourAppUser];
-- GRANT EXECUTE ON dbo.usp_GetFTELookupData TO [YourAppUser];
GO

PRINT 'Stored procedures created successfully!';
PRINT 'CW returns 6 result sets: LineOfBusiness, ProjectTheme, SWPRole, FundingType, CountryList, Status';
PRINT 'FTE returns 8 result sets: Joblevel, SWPRole, PositionType, FundingType, RoleType, CountryList, Status, StatusOfPosition';
GO
