CREATE PROCEDURE [dbo].[usp_GetPositionDistributionStatus]
	@Duration NVARCHAR(10) = NULL,
	@FilterType NVARCHAR(20) = 'DURATION',
	@StartDate DATE = NULL,
	@EndDate DATE = NULL,
	@InsightsType NVARCHAR(10) = 'All',
	@LoggedInEmail NVARCHAR(255),
	@ViewType NVARCHAR(20) = 'MyView'
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		-- Variables for date range calculation
		DECLARE @CalculatedStartDate DATE;
		DECLARE @CalculatedEndDate DATE;
		DECLARE @CurrentDate DATE = CAST(GETDATE() AS DATE);
		DECLARE @CurrentMonth INT = MONTH(GETDATE());
		DECLARE @CurrentYear INT = YEAR(GETDATE());
		DECLARE @Grouping NVARCHAR(10);

		-- Determine grouping level (day for 1M, month for others)
		IF @Duration = '1M'
			SET @Grouping = 'day';
		ELSE
			SET @Grouping = 'month';

		-- Calculate date range based on FilterType
		IF @FilterType = 'CUSTOM'
		BEGIN
			-- Use provided dates for custom range
			SET @CalculatedStartDate = @StartDate;
			SET @CalculatedEndDate = @EndDate;
		END
		ELSE IF @FilterType = 'FINANCIAL_YEAR'
		BEGIN
			-- Financial Year: April 1 to March 31
			IF @CurrentMonth >= 4
			BEGIN
				-- Current FY: This year April 1 to Next year March 31
				SET @CalculatedStartDate = DATEFROMPARTS(@CurrentYear, 4, 1);
				SET @CalculatedEndDate = DATEFROMPARTS(@CurrentYear + 1, 3, 31);
			END
			ELSE
			BEGIN
				-- Current FY: Last year April 1 to This year March 31
				SET @CalculatedStartDate = DATEFROMPARTS(@CurrentYear - 1, 4, 1);
				SET @CalculatedEndDate = DATEFROMPARTS(@CurrentYear, 3, 31);
			END
		END
		ELSE -- DURATION
		BEGIN
			SET @CalculatedEndDate = @CurrentDate;

			-- Calculate start date based on duration
			IF @Duration = '1M'
				SET @CalculatedStartDate = DATEADD(MONTH, -1, @CurrentDate);
			ELSE IF @Duration = '3M'
				SET @CalculatedStartDate = DATEADD(MONTH, -3, @CurrentDate);
			ELSE IF @Duration = '6M'
				SET @CalculatedStartDate = DATEADD(MONTH, -6, @CurrentDate);
			ELSE IF @Duration = '1Y'
				SET @CalculatedStartDate = DATEADD(YEAR, -1, @CurrentDate);
			ELSE IF @Duration = 'FY'
			BEGIN
				-- Same as FINANCIAL_YEAR
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
			ELSE
			BEGIN
				-- Default to 1 month if duration not recognized
				SET @CalculatedStartDate = DATEADD(MONTH, -1, @CurrentDate);
			END
		END

		-- Create temp table for email filtering
		CREATE TABLE #FilterEmails (
			Email NVARCHAR(255)
		);

		-- Populate filter emails based on ViewType
		IF @ViewType = 'MyView'
		BEGIN
			INSERT INTO #FilterEmails (Email)
			VALUES (@LoggedInEmail);
		END
		ELSE IF @ViewType = 'MyTeam'
		BEGIN
			-- Get direct reports from Employee table
			INSERT INTO #FilterEmails (Email)
			SELECT DISTINCT e.Email
			FROM dbo.Employee e
			INNER JOIN dbo.Employee m ON e.ManagerId = m.EmployeeId
			WHERE m.Email = @LoggedInEmail
				AND e.IsActive = 1;
		END
		ELSE IF @ViewType = 'MyOrg'
		BEGIN
			-- Get all reports in organizational hierarchy (recursive)
			WITH OrgTree AS (
				-- Start with direct reports
				SELECT 
					e.EmployeeId,
					e.Email,
					e.ManagerId,
					1 AS Level
				FROM dbo.Employee e
				INNER JOIN dbo.Employee m ON e.ManagerId = m.EmployeeId
				WHERE m.Email = @LoggedInEmail
					AND e.IsActive = 1

				UNION ALL

				-- Recursively get reports of reports
				SELECT 
					e.EmployeeId,
					e.Email,
					e.ManagerId,
					ot.Level + 1
				FROM dbo.Employee e
				INNER JOIN OrgTree ot ON e.ManagerId = ot.EmployeeId
				WHERE e.IsActive = 1
					AND ot.Level < 10 -- Prevent infinite recursion
			)
			INSERT INTO #FilterEmails (Email)
			SELECT DISTINCT Email
			FROM OrgTree;
		END
		-- If ViewType = 'All', don't add any filter emails (no filtering)

		-- Create index on temp table for better performance
		CREATE INDEX IX_FilterEmails ON #FilterEmails(Email);

		-- Get approved requests distribution over time
		WITH PositionDistribution AS (
			SELECT 
				YEAR(r.RequestedOn) AS Year,
				MONTH(r.RequestedOn) AS Month,
				DAY(r.RequestedOn) AS Day,
				vt.VacancyTypeName AS Category,
				r.RequestType,
				COUNT(r.RequestId) AS Count
			FROM dbo.Request r
			INNER JOIN dbo.PositionStatus ps ON r.CurrentStatusId = ps.PositionStatusId
			INNER JOIN dbo.VacancyType vt ON r.VacancyTypeId = vt.VacancyTypeId
			WHERE ps.StatusName = 'Approved'
				AND r.RequestedOn >= @CalculatedStartDate
				AND r.RequestedOn <= @CalculatedEndDate
				AND (
					@InsightsType = 'All'
					OR (@InsightsType = 'FTE' AND r.RequestType = 'FTE')
					OR (@InsightsType = 'CW' AND r.RequestType = 'CW')
				)
				AND (
					@ViewType = 'All'
					OR EXISTS (
						SELECT 1 
						FROM #FilterEmails fe 
						WHERE fe.Email = r.RequestBy
					)
				)
			GROUP BY 
				YEAR(r.RequestedOn),
				MONTH(r.RequestedOn),
				DAY(r.RequestedOn),
				vt.VacancyTypeName,
				r.RequestType
		)
		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			-- Format period based on grouping level
			CASE 
				WHEN @Grouping = 'day' THEN
					SUBSTRING('JanFebMarAprMayJunJulAugSepOctNovDec', (pd.Month - 1) * 3 + 1, 3) + ' ' + CAST(pd.Day AS NVARCHAR(2))
				ELSE
					SUBSTRING('JanFebMarAprMayJunJulAugSepOctNovDec', (pd.Month - 1) * 3 + 1, 3) + ' ' + CAST(pd.Year AS NVARCHAR(4))
			END AS Period,
			pd.Category AS Type,
			pd.Count,
			pd.RequestType AS RequestTpe, -- Note: Matching the typo in the sample response
			@CalculatedStartDate AS StartDate,
			@CalculatedEndDate AS EndDate
		FROM PositionDistribution pd
		ORDER BY pd.Year, pd.Month, pd.Day, pd.Category;

		-- Cleanup
		DROP TABLE #FilterEmails;

	END TRY
	BEGIN CATCH
		-- Return error response
		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			'' AS Period,
			'' AS Type,
			0 AS Count,
			'' AS RequestTpe,
			NULL AS StartDate,
			NULL AS EndDate;

		-- Cleanup temp table if it exists
		IF OBJECT_ID('tempdb..#FilterEmails') IS NOT NULL
			DROP TABLE #FilterEmails;
	END CATCH
END
GO
