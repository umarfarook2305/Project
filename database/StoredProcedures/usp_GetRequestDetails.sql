-- =============================================
-- Author:      HART System
-- Create date: 2026
-- Description: Get request details with optional filtering and pagination
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetRequestDetails]
	@RequestId UNIQUEIDENTIFIER = NULL,
	@LoggedInEmail NVARCHAR(255) = NULL,
	@ViewType NVARCHAR(50) = NULL, -- 'MyView' or 'MyTeam'
	@PendingWith NVARCHAR(255) = NULL,
	@Type NVARCHAR(50) = NULL, -- 'FTE' or 'CW'
	@Funding NVARCHAR(255) = NULL,
	@Level NVARCHAR(50) = NULL,
	@RequestorEmails NVARCHAR(MAX) = NULL, -- Comma-separated emails
	@IsPending BIT = NULL,
	@TimelineRangeType NVARCHAR(50) = NULL, -- 'month' or 'custom'
	@TimelineValue INT = NULL, -- Number of months
	@TimelineStartDate DATETIME2 = NULL,
	@TimelineEndDate DATETIME2 = NULL,
	@IncludeDetails BIT = 0, -- If 1, return full details; if 0, return list only
	@PageNumber INT = 1, -- Page number (1-based)
	@PageSize INT = 10 -- Number of records per page
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX);
	DECLARE @WhereClause NVARCHAR(MAX) = '';
	DECLARE @DirectReports TABLE (Email NVARCHAR(255));
	DECLARE @Offset INT;
	DECLARE @TotalRecords INT = 0;

	-- Validate and set pagination parameters
	IF @PageNumber < 1 SET @PageNumber = 1;
	IF @PageSize < 1 SET @PageSize = 10;
	IF @PageSize > 100 SET @PageSize = 100; -- Maximum 100 records per page

	SET @Offset = (@PageNumber - 1) * @PageSize;

	BEGIN TRY
		-- Build WHERE clause dynamically
		SET @WhereClause = '1=1';

		-- Filter by RequestId (for detail view)
		IF @RequestId IS NOT NULL
		BEGIN
			SET @WhereClause = @WhereClause + ' AND R.RequestId = ''' + CAST(@RequestId AS NVARCHAR(50)) + '''';
		END

		-- Filter by Type (FTE/CW)
		IF @Type IS NOT NULL AND @Type != ''
		BEGIN
			SET @WhereClause = @WhereClause + ' AND R.RequestType = ''' + @Type + '''';
		END

		-- Filter by PendingWith
		IF @PendingWith IS NOT NULL AND @PendingWith != ''
		BEGIN
			SET @WhereClause = @WhereClause + ' AND R.PendingWithEmail = ''' + @PendingWith + '''';
		END

		-- Filter by IsPending (Pending status)
		IF @IsPending IS NOT NULL
		BEGIN
			IF @IsPending = 1
			BEGIN
				SET @WhereClause = @WhereClause + ' AND PS.StatusName = ''Pending''';
			END
			ELSE
			BEGIN
				SET @WhereClause = @WhereClause + ' AND PS.StatusName != ''Pending''';
			END
		END

		-- Filter by Funding
		IF @Funding IS NOT NULL AND @Funding != ''
		BEGIN
			SET @WhereClause = @WhereClause + ' AND FT.FundingTypeName = ''' + @Funding + '''';
		END

		-- Filter by Level
		IF @Level IS NOT NULL AND @Level != ''
		BEGIN
			SET @WhereClause = @WhereClause + ' AND JL.JobLevelName = ''' + @Level + '''';
		END

		-- Filter by Requestor emails
		IF @RequestorEmails IS NOT NULL AND @RequestorEmails != ''
		BEGIN
			SET @WhereClause = @WhereClause + ' AND R.RequestBy IN (SELECT value FROM STRING_SPLIT(''' + @RequestorEmails + ''', '',''))';
		END

		-- Filter by ViewType (MyView or MyTeam)
		IF @ViewType = 'MyView' AND @LoggedInEmail IS NOT NULL
		BEGIN
			SET @WhereClause = @WhereClause + ' AND R.RequestBy = ''' + @LoggedInEmail + '''';
		END
		ELSE IF @ViewType = 'MyTeam' AND @LoggedInEmail IS NOT NULL
		BEGIN
			-- MyTeam filter: For now, just filter by logged in email
			-- TODO: Implement hierarchical query when Employee table is available
			SET @WhereClause = @WhereClause + ' AND R.RequestBy = ''' + @LoggedInEmail + '''';
		END

		-- Filter by Timeline
		IF @TimelineRangeType = 'month' AND @TimelineValue IS NOT NULL
		BEGIN
			SET @WhereClause = @WhereClause + ' AND R.CreatedOn >= DATEADD(MONTH, -' + CAST(@TimelineValue AS NVARCHAR(10)) + ', GETUTCDATE())';
		END
		ELSE IF @TimelineRangeType = 'custom' AND @TimelineStartDate IS NOT NULL AND @TimelineEndDate IS NOT NULL
		BEGIN
			SET @WhereClause = @WhereClause + ' AND R.CreatedOn >= ''' + CONVERT(NVARCHAR(50), @TimelineStartDate, 127) + ''' AND R.CreatedOn <= ''' + CONVERT(NVARCHAR(50), @TimelineEndDate, 127) + '''';
		END

		-- Return list or detail based on IncludeDetails flag
		IF @IncludeDetails = 0
		BEGIN
			-- Get total count first
			SET @SQL = '
			SELECT @TotalRecords = COUNT(*)
			FROM [Request] R
			LEFT JOIN PositionStatus PS ON R.CurrentStatusId = PS.PositionStatusId
			LEFT JOIN JobProfile JP ON R.JobProfileId = JP.JobProfileId
			LEFT JOIN FundingType FT ON R.FundingTypeId = FT.FundingTypeId
			LEFT JOIN JobLevel JL ON R.JobLevelId = JL.JobLevelId
			WHERE ' + @WhereClause;

			EXEC sp_executesql @SQL, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;

			-- Return paginated list view
			SET @SQL = '
			SELECT 
				R.RequestCode AS RequestId,
				PS.StatusName AS Status,
				R.RequestByName AS Requestor,
				R.PendingWithName AS PendingWith,
				JP.JobProfileName AS JobTitle,
				FT.FundingTypeName AS Funding,
				R.RequestType AS Type,
				JL.JobLevelName AS Level,
				R.CreatedOn AS SubmittedOn,
				R.CompletedOn,
				R.RequestId AS UniqueID
			FROM [Request] R
			LEFT JOIN PositionStatus PS ON R.CurrentStatusId = PS.PositionStatusId
			LEFT JOIN JobProfile JP ON R.JobProfileId = JP.JobProfileId
			LEFT JOIN FundingType FT ON R.FundingTypeId = FT.FundingTypeId
			LEFT JOIN JobLevel JL ON R.JobLevelId = JL.JobLevelId
			WHERE ' + @WhereClause + '
			ORDER BY R.CreatedOn DESC
			OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + ' ROWS
			FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY';

			-- Execute dynamic SQL
			EXEC sp_executesql @SQL;

			-- Return pagination metadata
			SELECT 
				@TotalRecords AS TotalRecords,
				@PageNumber AS CurrentPage,
				@PageSize AS PageSize,
				CAST(CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS INT) AS TotalPages,
				CASE WHEN @PageNumber > 1 THEN 1 ELSE 0 END AS HasPreviousPage,
				CASE WHEN @PageNumber < CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) THEN 1 ELSE 0 END AS HasNextPage;
		END
		ELSE
		BEGIN
			-- Return detail view (full request details) - no pagination for detail
			SET @SQL = '
			SELECT 
				R.RequestId,
				R.RequestType,
				R.ProjectName,
				R.Rationale,
				R.City,
				R.IsAIDataRole AS AI_or_DataRole,
				R.CompletedOn,
				R.CWAnnualRate,
				R.CWHourlyRate,
				R.HasNoIntraLevelReporting,
				R.MeetsSpanOfControlRequirements,
				R.IsApplicationEngineeringInvest AS IsApplicationEngineeringInvestment,
				R.IsAIGovernanceInvestment AS IsAiGovernanceInvestment,
				R.IsCWConversionFuture AS IsCwConversionFuture,
				JP.JobProfileName AS JobProfileLabel,
				JP.JobProfileId AS JobProfileValue,
				FT.FundingTypeName AS FundingLabel,
				FT.FundingTypeId AS FundingValue,
				JL.JobLevelName AS JobLevelLabel,
				JL.JobLevelId AS JobLevelValue,
				CL.CountryCode AS CountryLabel,
				CL.CountryListId AS CountryValue,
				PS.StatusName AS CurrentStatus
			FROM [Request] R
			LEFT JOIN JobProfile JP ON R.JobProfileId = JP.JobProfileId
			LEFT JOIN FundingType FT ON R.FundingTypeId = FT.FundingTypeId
			LEFT JOIN JobLevel JL ON R.JobLevelId = JL.JobLevelId
			LEFT JOIN CountryList CL ON R.CountryId = CL.CountryListId
			LEFT JOIN PositionStatus PS ON R.CurrentStatusId = PS.PositionStatusId
			WHERE ' + @WhereClause;

			-- Execute dynamic SQL
			EXEC sp_executesql @SQL;

			-- Return approvals (always, even if empty)
			IF @RequestId IS NOT NULL
			BEGIN
				SELECT 
					A.ApprovalId AS UniqueID,
					A.RequestedTo AS RequestToName,
					A.RequestedToEmail AS RequestToEmail,
					A.RequestToRole,
					A.SeqOrder,
					A.ApprovedBy,
					A.ApprovedOn,
					A.RejectedBy,
					A.RejectedOn,
					A.CancelledBy,
					A.CancelledOn,
					A.ReturnedBy,
					A.ReturnedOn,
					A.RequestedOn,
					A.IsApprover,
					PS.StatusName AS ApproverStatus
				FROM [Approval] A
				LEFT JOIN PositionStatus PS ON A.PositionStatusId = PS.PositionStatusId
				WHERE A.RequestId = @RequestId
				ORDER BY A.SeqOrder;
			END
			ELSE
			BEGIN
				-- Empty result set with structure
				SELECT 
					CAST(NULL AS UNIQUEIDENTIFIER) AS UniqueID,
					CAST(NULL AS NVARCHAR(255)) AS RequestToName,
					CAST(NULL AS NVARCHAR(255)) AS RequestToEmail,
					CAST(NULL AS NVARCHAR(255)) AS RequestToRole,
					CAST(NULL AS INT) AS SeqOrder,
					CAST(NULL AS NVARCHAR(255)) AS ApprovedBy,
					CAST(NULL AS DATETIME2) AS ApprovedOn,
					CAST(NULL AS NVARCHAR(255)) AS RejectedBy,
					CAST(NULL AS DATETIME2) AS RejectedOn,
					CAST(NULL AS NVARCHAR(255)) AS CancelledBy,
					CAST(NULL AS DATETIME2) AS CancelledOn,
					CAST(NULL AS NVARCHAR(255)) AS ReturnedBy,
					CAST(NULL AS DATETIME2) AS ReturnedOn,
					CAST(NULL AS DATETIME2) AS RequestedOn,
					CAST(NULL AS BIT) AS IsApprover,
					CAST(NULL AS NVARCHAR(255)) AS ApproverStatus
				WHERE 1 = 0;
			END

			-- Return comments (always, even if empty)
			IF @RequestId IS NOT NULL AND OBJECT_ID('CommentsHistory', 'U') IS NOT NULL
			BEGIN
				SELECT 
					CAST(CH.CommentedById AS NVARCHAR(255)) AS ApproverName,
					CH.CreatedOn AS CommentedOn,
					PS.StatusName AS ApproverStatus,
					CH.Comments
				FROM CommentsHistory CH
				LEFT JOIN PositionStatus PS ON CH.RequestStatusId = PS.PositionStatusId
				WHERE CH.RequestId = @RequestId
				ORDER BY CH.CreatedOn ASC;
			END
			ELSE
			BEGIN
				-- Empty result set with structure
				SELECT 
					CAST(NULL AS NVARCHAR(255)) AS ApproverName,
					CAST(NULL AS DATETIME2) AS CommentedOn,
					CAST(NULL AS NVARCHAR(255)) AS ApproverStatus,
					CAST(NULL AS NVARCHAR(MAX)) AS Comments
				WHERE 1 = 0;
			END
		END

	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();

		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO
