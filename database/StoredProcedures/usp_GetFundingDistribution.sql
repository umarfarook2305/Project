-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-23
-- Description: Get funding distribution statistics
-- Based on Power Platform API: Dash_FundingDistribution
-- Returns: Project Funded vs Base Funded breakdown with FTE/CW counts
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetFundingDistribution]
	@Duration NVARCHAR(10) = NULL,        -- '1M', '3M', '6M', '1Y', 'FY' (financial year)
	@FilterType NVARCHAR(50) = 'DURATION', -- 'DURATION', 'FINANCIAL_YEAR', 'CUSTOM'
	@StartDate DATE = NULL,                -- Custom start date (if FilterType = 'CUSTOM')
	@EndDate DATE = NULL,                  -- Custom end date (if FilterType = 'CUSTOM')
	@InsightsType NVARCHAR(50) = 'All',    -- 'All', 'FTE', 'CW'
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
	DECLARE @ProjectFundedTotal INT = 0;
	DECLARE @ProjectFundedFTE INT = 0;
	DECLARE @ProjectFundedCW INT = 0;
	DECLARE @BaseFundedTotal INT = 0;
	DECLARE @BaseFundedFTE INT = 0;
	DECLARE @BaseFundedCW INT = 0;

	BEGIN TRY
		-- ========================
		-- CALCULATE DATE RANGE
		-- ========================

		IF @FilterType = 'FINANCIAL_YEAR'
		BEGIN
			-- Financial year: April 1 to March 31
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
				-- Default to 1 month
				SET @CalculatedStartDate = DATEADD(MONTH, -1, CAST(GETUTCDATE() AS DATE));
				SET @CalculatedEndDate = CAST(GETUTCDATE() AS DATE);
			END
		END

		-- ========================
		-- BUILD EMAIL FILTER LIST
		-- ========================

		CREATE TABLE #FilterEmails (
			EmailId NVARCHAR(255)
		);

		IF @ViewType = 'MyView'
		BEGIN
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

			INSERT INTO #FilterEmails (EmailId)
			VALUES (@LoggedInEmail);
		END
		ELSE IF @ViewType = 'MyOrg'
		BEGIN
			WITH OrgHierarchyCTE AS (
				-- Start with logged-in user
				SELECT 
					e.Email AS EmailId,
					e.EmployeeId,
					e.ManagerId,
					1 AS [Level]
				FROM dbo.Employee e
				WHERE e.Email = @LoggedInEmail
					AND e.IsActive = 1

				UNION ALL

				-- Recursively get all reports
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
		ELSE -- 'All'
		BEGIN
			INSERT INTO #FilterEmails (EmailId)
			VALUES (NULL);
		END

		-- ========================
		-- GET FUNDING DISTRIBUTION
		-- ========================

		;WITH FundingCounts AS (
			SELECT 
				r.RequestType,
				ft.FundingTypeName,
				COUNT(*) AS RequestCount
			FROM dbo.Request r
			INNER JOIN dbo.FundingType ft ON r.FundingTypeId = ft.FundingTypeId
			WHERE r.RequestedOn >= @CalculatedStartDate
				AND r.RequestedOn <= @CalculatedEndDate
				AND r.IsActive = 1
				AND (
					@ViewType = 'All' 
					OR r.RequestBy IN (SELECT EmailId FROM #FilterEmails WHERE EmailId IS NOT NULL)
				)
				AND (
					@InsightsType = 'All'
					OR r.RequestType = @InsightsType
				)
			GROUP BY r.RequestType, ft.FundingTypeName
		)
		SELECT 
			@Total = @Total + RequestCount,
			@ProjectFundedTotal = @ProjectFundedTotal + CASE WHEN FundingTypeName = 'Project Funded' THEN RequestCount ELSE 0 END,
			@ProjectFundedFTE = @ProjectFundedFTE + CASE WHEN FundingTypeName = 'Project Funded' AND RequestType = 'FTE' THEN RequestCount ELSE 0 END,
			@ProjectFundedCW = @ProjectFundedCW + CASE WHEN FundingTypeName = 'Project Funded' AND RequestType = 'CW' THEN RequestCount ELSE 0 END,
			@BaseFundedTotal = @BaseFundedTotal + CASE WHEN FundingTypeName = 'Base Funded' THEN RequestCount ELSE 0 END,
			@BaseFundedFTE = @BaseFundedFTE + CASE WHEN FundingTypeName = 'Base Funded' AND RequestType = 'FTE' THEN RequestCount ELSE 0 END,
			@BaseFundedCW = @BaseFundedCW + CASE WHEN FundingTypeName = 'Base Funded' AND RequestType = 'CW' THEN RequestCount ELSE 0 END
		FROM FundingCounts;

		-- Clean up
		DROP TABLE #FilterEmails;

		-- ========================
		-- RETURN RESULTS
		-- ========================

		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			@Total AS Total,
			@ProjectFundedTotal AS ProjectFundedTotal,
			@ProjectFundedFTE AS ProjectFundedFTE,
			@ProjectFundedCW AS ProjectFundedCW,
			@BaseFundedTotal AS BaseFundedTotal,
			@BaseFundedFTE AS BaseFundedFTE,
			@BaseFundedCW AS BaseFundedCW,
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
			0 AS ProjectFundedTotal,
			0 AS ProjectFundedFTE,
			0 AS ProjectFundedCW,
			0 AS BaseFundedTotal,
			0 AS BaseFundedFTE,
			0 AS BaseFundedCW,
			NULL AS StartDate,
			NULL AS EndDate;
	END CATCH
END
GO
