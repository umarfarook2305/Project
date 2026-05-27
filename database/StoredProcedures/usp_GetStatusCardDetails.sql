-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-23
-- Description: Get status card details (dashboard statistics)
-- Based on Power Platform API: getStatusCardDetails
-- Returns: total, fte, cw, approved, pending, rejected counts
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetStatusCardDetails]
	@Duration NVARCHAR(10) = NULL,        -- '1M', '3M', '6M', '1Y', 'FY' (financial year)
	@FilterType NVARCHAR(50) = 'DURATION', -- 'DURATION' or 'FINANCIAL_YEAR'
	@StartDate DATE = NULL,                -- Custom start date (if FilterType = 'CUSTOM')
	@EndDate DATE = NULL,                  -- Custom end date (if FilterType = 'CUSTOM')
	@LoggedInEmail NVARCHAR(255),          -- Current user's email
	@ViewType NVARCHAR(50) = 'MyView'      -- 'MyView', 'MyTeam', 'MyOrg', 'All'
AS
BEGIN
	SET NOCOUNT ON;

	-- Date range variables
	DECLARE @CalculatedStartDate DATE;
	DECLARE @CalculatedEndDate DATE;

	-- Result counters
	DECLARE @Total INT = 0;
	DECLARE @FTE INT = 0;
	DECLARE @CW INT = 0;
	DECLARE @Approved INT = 0;
	DECLARE @Pending INT = 0;
	DECLARE @Rejected INT = 0;

	BEGIN TRY
		-- ========================
		-- CALCULATE DATE RANGE
		-- ========================

		IF @FilterType = 'FINANCIAL_YEAR'
		BEGIN
			-- Financial year: April 1 to March 31
			-- If current month >= April, FY is current year to next year
			-- If current month < April, FY is previous year to current year
			DECLARE @CurrentMonth INT = MONTH(GETUTCDATE());
			DECLARE @CurrentYear INT = YEAR(GETUTCDATE());

			IF @CurrentMonth >= 4
			BEGIN
				SET @CalculatedStartDate = DATEFROMPARTS(@CurrentYear, 4, 1);
				SET @CalculatedEndDate = DATEFROMPARTS(@CurrentYear + 1, 3, 31);
			END
			ELSE
			BEGIN
				SET @CalculatedStartDate = DATEFROMPARTS(@CurrentYear - 1, 4, 1);
				SET @CalculatedEndDate = DATEFROMPARTS(@CurrentYear, 3, 31);
			END
		END
		ELSE IF @FilterType = 'CUSTOM' AND @StartDate IS NOT NULL AND @EndDate IS NOT NULL
		BEGIN
			SET @CalculatedStartDate = @StartDate;
			SET @CalculatedEndDate = @EndDate;
		END
		ELSE
		BEGIN
			-- Duration-based calculation
			IF @Duration = '1M'
			BEGIN
				SET @CalculatedStartDate = DATEADD(MONTH, -1, CAST(GETUTCDATE() AS DATE));
				SET @CalculatedEndDate = CAST(GETUTCDATE() AS DATE);
			END
			ELSE IF @Duration = '3M'
			BEGIN
				SET @CalculatedStartDate = DATEADD(MONTH, -3, CAST(GETUTCDATE() AS DATE));
				SET @CalculatedEndDate = CAST(GETUTCDATE() AS DATE);
			END
			ELSE IF @Duration = '6M'
			BEGIN
				SET @CalculatedStartDate = DATEADD(MONTH, -6, CAST(GETUTCDATE() AS DATE));
				SET @CalculatedEndDate = CAST(GETUTCDATE() AS DATE);
			END
			ELSE IF @Duration = '1Y'
			BEGIN
				SET @CalculatedStartDate = DATEADD(YEAR, -1, CAST(GETUTCDATE() AS DATE));
				SET @CalculatedEndDate = CAST(GETUTCDATE() AS DATE);
			END
			ELSE
			BEGIN
				-- Default to 1 month if duration not recognized
				SET @CalculatedStartDate = DATEADD(MONTH, -1, CAST(GETUTCDATE() AS DATE));
				SET @CalculatedEndDate = CAST(GETUTCDATE() AS DATE);
			END
		END

		-- ========================
		-- BUILD EMAIL FILTER LIST
		-- ========================

		-- Create temp table for email filters
		CREATE TABLE #FilterEmails (
			EmailId NVARCHAR(255)
		);

		IF @ViewType = 'MyView'
		BEGIN
			-- Just the logged-in user
			INSERT INTO #FilterEmails (EmailId)
			VALUES (@LoggedInEmail);
		END
		ELSE IF @ViewType = 'MyTeam'
		BEGIN
			-- Get direct reports from Employee table
			INSERT INTO #FilterEmails (EmailId)
			SELECT e.Email
			FROM dbo.Employee e
			INNER JOIN dbo.Employee mgr ON e.ManagerId = mgr.EmployeeId
			WHERE mgr.Email = @LoggedInEmail
				AND e.IsActive = 1;

			-- Also include the logged-in user
			INSERT INTO #FilterEmails (EmailId)
			VALUES (@LoggedInEmail);
		END
		ELSE IF @ViewType = 'MyOrg'
		BEGIN
			-- Get all reportees (direct and indirect) from Employee table
			-- This uses a recursive CTE to get the entire hierarchy
			WITH OrgHierarchyCTE AS (
				-- Anchor: Start with the logged-in user
				SELECT 
					e.Email AS EmailId,
					e.EmployeeId,
					e.ManagerId,
					1 AS [Level]
				FROM dbo.Employee e
				WHERE e.Email = @LoggedInEmail
					AND e.IsActive = 1

				UNION ALL

				-- Recursive: Get all reportees
				SELECT 
					e.Email AS EmailId,
					e.EmployeeId,
					e.ManagerId,
					cte.[Level] + 1
				FROM dbo.Employee e
				INNER JOIN OrgHierarchyCTE cte ON e.ManagerId = cte.EmployeeId
				WHERE e.IsActive = 1
					AND cte.[Level] < 10
			)
			INSERT INTO #FilterEmails (EmailId)
			SELECT DISTINCT EmailId
			FROM OrgHierarchyCTE;
		END
		ELSE -- 'All' or no filter
		BEGIN
			-- No email filter - include all requests
			-- We'll handle this in the WHERE clause
			INSERT INTO #FilterEmails (EmailId)
			VALUES (NULL); -- Placeholder to indicate "all"
		END

		-- ========================
		-- GET AGGREGATED COUNTS
		-- ========================

		-- Get counts grouped by RequestType and Status
		;WITH RequestCounts AS (
			SELECT 
				r.RequestType,
				ps.StatusName,
				COUNT(*) AS RequestCount
			FROM dbo.Request r
			INNER JOIN dbo.PositionStatus ps ON r.CurrentStatusId = ps.PositionStatusId
			WHERE r.RequestedOn >= @CalculatedStartDate
				AND r.RequestedOn <= @CalculatedEndDate
				AND r.IsActive = 1
				AND (
					@ViewType = 'All' 
					OR r.RequestBy IN (SELECT EmailId FROM #FilterEmails WHERE EmailId IS NOT NULL)
				)
			GROUP BY r.RequestType, ps.StatusName
		)
		SELECT 
			@Total = @Total + RequestCount,
			@FTE = @FTE + CASE WHEN RequestType = 'FTE' THEN RequestCount ELSE 0 END,
			@CW = @CW + CASE WHEN RequestType = 'CW' THEN RequestCount ELSE 0 END,
			@Approved = @Approved + CASE WHEN StatusName = 'Approved' THEN RequestCount ELSE 0 END,
			@Pending = @Pending + CASE WHEN StatusName = 'Pending' THEN RequestCount ELSE 0 END,
			@Rejected = @Rejected + CASE WHEN StatusName IN ('Rejected', 'Cancelled') THEN RequestCount ELSE 0 END
		FROM RequestCounts;

		-- Clean up temp table
		DROP TABLE #FilterEmails;

		-- ========================
		-- RETURN RESULTS
		-- ========================

		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			@Total AS Total,
			@FTE AS FTE,
			@CW AS CW,
			@Approved AS Approved,
			@Pending AS Pending,
			@Rejected AS Rejected,
			@CalculatedStartDate AS StartDate,
			@CalculatedEndDate AS EndDate;

	END TRY
	BEGIN CATCH
		-- Return error
		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			ERROR_MESSAGE() AS ErrorMessage,
			ERROR_NUMBER() AS ErrorNumber,
			ERROR_LINE() AS ErrorLine,
			0 AS Total,
			0 AS FTE,
			0 AS CW,
			0 AS Approved,
			0 AS Pending,
			0 AS Rejected,
			NULL AS StartDate,
			NULL AS EndDate;
	END CATCH
END
GO
