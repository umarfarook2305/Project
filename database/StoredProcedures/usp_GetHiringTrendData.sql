CREATE PROCEDURE [dbo].[usp_GetHiringTrendData]
	@Duration NVARCHAR(10) = NULL,
	@FilterType NVARCHAR(20) = 'DURATION',
	@StartDate DATE = NULL,
	@EndDate DATE = NULL,
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
		DECLARE @BucketSizeDays INT;
		DECLARE @TotalDays INT;

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
				SET @CalculatedEndDate = @CurrentDate; -- Up to today, not full FY
			END
			ELSE
			BEGIN
				-- Current FY: Last year April 1 to This year March 31
				SET @CalculatedStartDate = DATEFROMPARTS(@CurrentYear - 1, 4, 1);
				SET @CalculatedEndDate = @CurrentDate;
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
					SET @CalculatedEndDate = @CurrentDate;
				END
				ELSE
				BEGIN
					SET @CalculatedStartDate = DATEFROMPARTS(@CurrentYear - 1, 4, 1);
					SET @CalculatedEndDate = @CurrentDate;
				END
			END
			ELSE
			BEGIN
				-- Default to 1 month if duration not recognized
				SET @CalculatedStartDate = DATEADD(MONTH, -1, @CurrentDate);
			END
		END

		-- Calculate total days and bucket size (divide into 6 buckets)
		SET @TotalDays = DATEDIFF(DAY, @CalculatedStartDate, @CalculatedEndDate);
		SET @BucketSizeDays = @TotalDays / 6;
		IF @BucketSizeDays = 0
			SET @BucketSizeDays = 1; -- Minimum 1 day per bucket

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

		-- Create 6 time buckets and count FTE/CW requests in each
		WITH BucketNumbers AS (
			SELECT 0 AS BucketNum UNION ALL
			SELECT 1 UNION ALL
			SELECT 2 UNION ALL
			SELECT 3 UNION ALL
			SELECT 4 UNION ALL
			SELECT 5
		),
		BucketRanges AS (
			SELECT 
				BucketNum,
				DATEADD(DAY, BucketNum * @BucketSizeDays, @CalculatedStartDate) AS BucketStart,
				DATEADD(DAY, (BucketNum + 1) * @BucketSizeDays, @CalculatedStartDate) AS BucketEnd
			FROM BucketNumbers
		),
		BucketCounts AS (
			SELECT 
				br.BucketNum,
				br.BucketEnd,
				COUNT(CASE WHEN r.RequestType = 'FTE' THEN 1 END) AS FTE_Count,
				COUNT(CASE WHEN r.RequestType = 'CW' THEN 1 END) AS CW_Count
			FROM BucketRanges br
			LEFT JOIN dbo.Request r ON 
				r.RequestedOn >= br.BucketStart 
				AND r.RequestedOn < br.BucketEnd
				AND r.RequestType IS NOT NULL
				AND (
					@ViewType = 'All'
					OR EXISTS (
						SELECT 1 
						FROM #FilterEmails fe 
						WHERE fe.Email = r.RequestBy
					)
				)
			GROUP BY br.BucketNum, br.BucketEnd
		)
		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			-- Format date based on duration (day for 1M, month for others)
			CASE 
				WHEN @Duration = '1M' THEN
					SUBSTRING('JanFebMarAprMayJunJulAugSepOctNovDec', (MONTH(bc.BucketEnd) - 1) * 3 + 1, 3) + ' ' + 
					RIGHT('0' + CAST(DAY(bc.BucketEnd) AS NVARCHAR(2)), 2)
				ELSE
					SUBSTRING('JanFebMarAprMayJunJulAugSepOctNovDec', (MONTH(bc.BucketEnd) - 1) * 3 + 1, 3) + ' ' + 
					CAST(YEAR(bc.BucketEnd) AS NVARCHAR(4))
			END AS Date,
			CAST(bc.FTE_Count AS NVARCHAR(10)) AS FTE,
			CAST(bc.CW_Count AS NVARCHAR(10)) AS CW,
			@CalculatedStartDate AS StartDate,
			@CalculatedEndDate AS EndDate
		FROM BucketCounts bc
		ORDER BY bc.BucketNum;

		-- Cleanup
		DROP TABLE #FilterEmails;

	END TRY
	BEGIN CATCH
		-- Return error response
		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			'' AS Date,
			'0' AS FTE,
			'0' AS CW,
			NULL AS StartDate,
			NULL AS EndDate;

		-- Cleanup temp table if it exists
		IF OBJECT_ID('tempdb..#FilterEmails') IS NOT NULL
			DROP TABLE #FilterEmails;
	END CATCH
END
GO
