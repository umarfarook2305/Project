-- ============================================================
-- HART Employee Hierarchy Stored Procedure
-- Purpose: Get management hierarchy for an employee by email
-- Recursively traverses up the management chain
-- ============================================================

IF OBJECT_ID('dbo.usp_GetEmployeeHierarchy', 'P') IS NOT NULL
	DROP PROCEDURE dbo.usp_GetEmployeeHierarchy;
GO

CREATE PROCEDURE dbo.usp_GetEmployeeHierarchy
	@Email NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	-- Validate input
	IF @Email IS NULL OR LTRIM(RTRIM(@Email)) = ''
	BEGIN
		RAISERROR('Email parameter is required', 16, 1);
		RETURN;
	END

	-- Use recursive CTE to build management hierarchy
	;WITH ManagerHierarchy AS
	(
		-- Anchor: Find the employee and their direct manager
		SELECT 
			1 AS Level,
			m.EmployeeId,
			m.FullName AS Name,
			m.Email,
			m.JobTitle AS Title,
			m.ManagerId
		FROM dbo.Employee e
		INNER JOIN dbo.Employee m ON e.ManagerId = m.EmployeeId
		WHERE 
			e.Email = @Email
			AND e.IsActive = 1
			AND m.IsActive = 1

		UNION ALL

		-- Recursive: Get each manager's manager
		SELECT 
			h.Level + 1,
			m.EmployeeId,
			m.FullName AS Name,
			m.Email,
			m.JobTitle AS Title,
			m.ManagerId
		FROM ManagerHierarchy h
		INNER JOIN dbo.Employee m ON h.ManagerId = m.EmployeeId
		WHERE 
			m.IsActive = 1
			AND h.Level < 10  -- Prevent infinite loops (max 10 levels)
	)
	SELECT 
		Level,
		Name,
		Email,
		Title
	FROM ManagerHierarchy
	ORDER BY Level;

	-- If no results, return empty set with correct schema
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT 
			CAST(NULL AS INT) AS Level,
			CAST(NULL AS NVARCHAR(255)) AS Name,
			CAST(NULL AS NVARCHAR(255)) AS Email,
			CAST(NULL AS NVARCHAR(255)) AS Title
		WHERE 1 = 0;
	END
END;
GO

-- ============================================================
-- Grant Execute Permissions (adjust schema/user as needed)
-- ============================================================
-- GRANT EXECUTE ON dbo.usp_GetEmployeeHierarchy TO [YourAppUser];
GO

PRINT 'Stored procedure usp_GetEmployeeHierarchy created successfully!';
PRINT 'Usage: EXEC dbo.usp_GetEmployeeHierarchy @Email = ''employee@company.com''';
GO

-- ============================================================
-- Test Examples
-- ============================================================
-- Get hierarchy for an employee:
-- EXEC dbo.usp_GetEmployeeHierarchy @Email = 'prabakar.krishnamoorthy@kumaran.com';

-- Test with non-existent email (returns empty result):
-- EXEC dbo.usp_GetEmployeeHierarchy @Email = 'invalid@company.com';
GO

-- ============================================================
-- NOTES:
-- ============================================================
-- This stored procedure assumes the following table structure:
-- 
-- Table: dbo.Employee
-- Columns:
--   - EmployeeId (INT or UNIQUEIDENTIFIER) - Primary key
--   - FullName (NVARCHAR) - Employee's full name
--   - Email (NVARCHAR) - Employee's email address (unique)
--   - JobTitle (NVARCHAR) - Employee's job title
--   - ManagerId (INT or UNIQUEIDENTIFIER) - FK to Employee.EmployeeId
--   - IsActive (BIT) - Active flag
--
-- The CTE recursively walks up the management chain:
-- Level 1 = Direct manager
-- Level 2 = Manager's manager
-- Level 3 = Manager's manager's manager, etc.
--
-- Max recursion depth is limited to 10 levels for safety.
-- ============================================================
