-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-23
-- Description: Get approval list with comprehensive filters and pagination
-- Based on Power Platform APIs: getDataList_Approval + getDataByFilter_Request (merged)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetApprovalList]
	@ApproverEmails NVARCHAR(MAX), -- Comma-separated list of emails
	@IsPending BIT = 1,
	@Count INT = 5000,
	@LoggedInEmail NVARCHAR(255) = NULL,
	@ViewType NVARCHAR(50) = NULL, -- 'MyView' or 'MyTeam'
	@PendingWith NVARCHAR(255) = NULL,
	@PositionType NVARCHAR(255) = NULL, -- Job profile name or GUID
	@RequestType NVARCHAR(50) = NULL, -- 'FTE' or 'CW'
	@Funding NVARCHAR(255) = NULL, -- Funding type name or GUID
	@Level NVARCHAR(50) = NULL, -- Job level name
	@Requestors NVARCHAR(MAX) = NULL, -- Comma-separated list of requestor emails
	@TimelineRangeType NVARCHAR(20) = NULL, -- 'month' or 'custom'
	@TimelineMonths INT = NULL, -- Number of months to go back
	@TimelineStartDate DATETIME2 = NULL,
	@TimelineEndDate DATETIME2 = NULL,
	@PageNumber INT = 1, -- Page number (1-based)
	@PageSize INT = 10 -- Records per page
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @PendingStatusId UNIQUEIDENTIFIER;
	DECLARE @ReturnedStatusId UNIQUEIDENTIFIER;
	DECLARE @AwaitingStatusId UNIQUEIDENTIFIER;
	DECLARE @PositionTypeId UNIQUEIDENTIFIER;
	DECLARE @FundingTypeId UNIQUEIDENTIFIER;
	DECLARE @JobLevelId UNIQUEIDENTIFIER;
	DECLARE @Offset INT;
	DECLARE @TotalRecords INT;

	-- Calculate offset for pagination
	SET @Offset = (@PageNumber - 1) * @PageSize;

	-- Get status IDs
	SELECT TOP 1 @PendingStatusId = PositionStatusId
	FROM PositionStatus
	WHERE StatusName = 'Pending';

	SELECT TOP 1 @ReturnedStatusId = PositionStatusId
	FROM PositionStatus
	WHERE StatusName = 'Returned to Requestor';

	SELECT TOP 1 @AwaitingStatusId = PositionStatusId
	FROM PositionStatus
	WHERE StatusName = 'Awaiting';

	-- Resolve Position Type (Job Profile) ID if name provided
	IF @PositionType IS NOT NULL
	BEGIN
		IF TRY_CAST(@PositionType AS UNIQUEIDENTIFIER) IS NOT NULL
			SET @PositionTypeId = CAST(@PositionType AS UNIQUEIDENTIFIER);
		ELSE
			SELECT TOP 1 @PositionTypeId = JobProfileId
			FROM JobProfile
			WHERE JobProfileName = @PositionType;
	END

	-- Resolve Funding Type ID if name provided
	IF @Funding IS NOT NULL
	BEGIN
		IF TRY_CAST(@Funding AS UNIQUEIDENTIFIER) IS NOT NULL
			SET @FundingTypeId = CAST(@Funding AS UNIQUEIDENTIFIER);
		ELSE
			SELECT TOP 1 @FundingTypeId = FundingTypeId
			FROM FundingType
			WHERE FundingTypeName = @Funding;
	END

	-- Resolve Job Level ID if name provided
	IF @Level IS NOT NULL
	BEGIN
		IF TRY_CAST(@Level AS UNIQUEIDENTIFIER) IS NOT NULL
			SET @JobLevelId = CAST(@Level AS UNIQUEIDENTIFIER);
		ELSE
			SELECT TOP 1 @JobLevelId = JobLevelId
			FROM JobLevel
			WHERE JobLevelName = @Level;
	END

	-- Build main query with all filters
	IF @IsPending = 1
	BEGIN
		-- Get total count first
		SELECT @TotalRecords = COUNT(*)
		FROM Approval a
		INNER JOIN Request r ON a.RequestId = r.RequestId
		INNER JOIN PositionStatus ps ON a.PositionStatusId = ps.PositionStatusId
		LEFT JOIN JobProfile jp ON r.JobProfileId = jp.JobProfileId
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND (a.PositionStatusId = @PendingStatusId OR a.PositionStatusId = @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT value FROM STRING_SPLIT(@ApproverEmails, ','))
			-- Additional filters
			AND (@PendingWith IS NULL OR a.RequestedToEmail = @PendingWith)
			AND (@PositionTypeId IS NULL OR r.JobProfileId = @PositionTypeId)
			AND (@RequestType IS NULL OR r.RequestType = @RequestType)
			AND (@FundingTypeId IS NULL OR r.FundingTypeId = @FundingTypeId)
			AND (@JobLevelId IS NULL OR r.JobLevelId = @JobLevelId)
			AND (@Requestors IS NULL OR r.RequestBy IN (SELECT value FROM STRING_SPLIT(@Requestors, ',')))
			AND (@LoggedInEmail IS NULL OR 
				(@ViewType = 'MyView' AND r.RequestBy = @LoggedInEmail) OR
				(@ViewType = 'MyTeam' AND r.RequestBy = @LoggedInEmail) OR
				@ViewType IS NULL
			)
			-- Timeline filters
			AND (
				@TimelineRangeType IS NULL OR
				(@TimelineRangeType = 'month' AND a.RequestedOn >= DATEADD(MONTH, -@TimelineMonths, GETUTCDATE())) OR
				(@TimelineRangeType = 'custom' AND a.RequestedOn >= @TimelineStartDate AND a.RequestedOn <= @TimelineEndDate)
			);

		-- Get paginated Pending or Returned to Requestor approvals
		SELECT 
			r.RequestId AS RequestUniqueId,
			r.RequestCode AS RequestId,
			ps.StatusName AS Status,
			r.RequestByName AS Requestor,
			a.RequestedTo AS PendingWith,
			jp.JobProfileName AS Position,
			ft.FundingTypeName AS Funding,
			r.RequestType AS Type,
			jl.JobLevelName AS Level,
			a.RequestedOn AS SubmittedOn,
			r.CompletedOn,
			a.RequestedTo AS ApproverName,
			@TotalRecords AS TotalRecords
		FROM Approval a
		INNER JOIN Request r ON a.RequestId = r.RequestId
		INNER JOIN PositionStatus ps ON a.PositionStatusId = ps.PositionStatusId
		LEFT JOIN JobProfile jp ON r.JobProfileId = jp.JobProfileId
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND (a.PositionStatusId = @PendingStatusId OR a.PositionStatusId = @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT value FROM STRING_SPLIT(@ApproverEmails, ','))
			-- Additional filters
			AND (@PendingWith IS NULL OR a.RequestedToEmail = @PendingWith)
			AND (@PositionTypeId IS NULL OR r.JobProfileId = @PositionTypeId)
			AND (@RequestType IS NULL OR r.RequestType = @RequestType)
			AND (@FundingTypeId IS NULL OR r.FundingTypeId = @FundingTypeId)
			AND (@JobLevelId IS NULL OR r.JobLevelId = @JobLevelId)
			AND (@Requestors IS NULL OR r.RequestBy IN (SELECT value FROM STRING_SPLIT(@Requestors, ',')))
			AND (@LoggedInEmail IS NULL OR 
				(@ViewType = 'MyView' AND r.RequestBy = @LoggedInEmail) OR
				(@ViewType = 'MyTeam' AND r.RequestBy = @LoggedInEmail) OR
				@ViewType IS NULL
			)
			-- Timeline filters
			AND (
				@TimelineRangeType IS NULL OR
				(@TimelineRangeType = 'month' AND a.RequestedOn >= DATEADD(MONTH, -@TimelineMonths, GETUTCDATE())) OR
				(@TimelineRangeType = 'custom' AND a.RequestedOn >= @TimelineStartDate AND a.RequestedOn <= @TimelineEndDate)
			)
		ORDER BY a.RequestedOn DESC
		OFFSET @Offset ROWS
		FETCH NEXT @PageSize ROWS ONLY;
	END
	ELSE
	BEGIN
		-- Get total count first
		SELECT @TotalRecords = COUNT(*)
		FROM Approval a
		INNER JOIN Request r ON a.RequestId = r.RequestId
		INNER JOIN PositionStatus ps ON a.PositionStatusId = ps.PositionStatusId
		LEFT JOIN JobProfile jp ON r.JobProfileId = jp.JobProfileId
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND a.PositionStatusId NOT IN (@PendingStatusId, @AwaitingStatusId, @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT value FROM STRING_SPLIT(@ApproverEmails, ','))
			-- Additional filters
			AND (@PendingWith IS NULL OR a.RequestedToEmail = @PendingWith)
			AND (@PositionTypeId IS NULL OR r.JobProfileId = @PositionTypeId)
			AND (@RequestType IS NULL OR r.RequestType = @RequestType)
			AND (@FundingTypeId IS NULL OR r.FundingTypeId = @FundingTypeId)
			AND (@JobLevelId IS NULL OR r.JobLevelId = @JobLevelId)
			AND (@Requestors IS NULL OR r.RequestBy IN (SELECT value FROM STRING_SPLIT(@Requestors, ',')))
			AND (@LoggedInEmail IS NULL OR 
				(@ViewType = 'MyView' AND r.RequestBy = @LoggedInEmail) OR
				(@ViewType = 'MyTeam' AND r.RequestBy = @LoggedInEmail) OR
				@ViewType IS NULL
			)
			-- Timeline filters
			AND (
				@TimelineRangeType IS NULL OR
				(@TimelineRangeType = 'month' AND a.RequestedOn >= DATEADD(MONTH, -@TimelineMonths, GETUTCDATE())) OR
				(@TimelineRangeType = 'custom' AND a.RequestedOn >= @TimelineStartDate AND a.RequestedOn <= @TimelineEndDate)
			);

		-- Get paginated non-pending approvals
		SELECT 
			r.RequestId AS RequestUniqueId,
			r.RequestCode AS RequestId,
			ps.StatusName AS Status,
			r.RequestByName AS Requestor,
			a.RequestedTo AS PendingWith,
			jp.JobProfileName AS Position,
			ft.FundingTypeName AS Funding,
			r.RequestType AS Type,
			jl.JobLevelName AS Level,
			a.RequestedOn AS SubmittedOn,
			r.CompletedOn,
			a.RequestedTo AS ApproverName,
			@TotalRecords AS TotalRecords
		FROM Approval a
		INNER JOIN Request r ON a.RequestId = r.RequestId
		INNER JOIN PositionStatus ps ON a.PositionStatusId = ps.PositionStatusId
		LEFT JOIN JobProfile jp ON r.JobProfileId = jp.JobProfileId
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND a.PositionStatusId NOT IN (@PendingStatusId, @AwaitingStatusId, @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT value FROM STRING_SPLIT(@ApproverEmails, ','))
			-- Additional filters
			AND (@PendingWith IS NULL OR a.RequestedToEmail = @PendingWith)
			AND (@PositionTypeId IS NULL OR r.JobProfileId = @PositionTypeId)
			AND (@RequestType IS NULL OR r.RequestType = @RequestType)
			AND (@FundingTypeId IS NULL OR r.FundingTypeId = @FundingTypeId)
			AND (@JobLevelId IS NULL OR r.JobLevelId = @JobLevelId)
			AND (@Requestors IS NULL OR r.RequestBy IN (SELECT value FROM STRING_SPLIT(@Requestors, ',')))
			AND (@LoggedInEmail IS NULL OR 
				(@ViewType = 'MyView' AND r.RequestBy = @LoggedInEmail) OR
				(@ViewType = 'MyTeam' AND r.RequestBy = @LoggedInEmail) OR
				@ViewType IS NULL
			)
			-- Timeline filters
			AND (
				@TimelineRangeType IS NULL OR
				(@TimelineRangeType = 'month' AND a.RequestedOn >= DATEADD(MONTH, -@TimelineMonths, GETUTCDATE())) OR
				(@TimelineRangeType = 'custom' AND a.RequestedOn >= @TimelineStartDate AND a.RequestedOn <= @TimelineEndDate)
			)
		ORDER BY a.RequestedOn DESC
		OFFSET @Offset ROWS
		FETCH NEXT @PageSize ROWS ONLY;
	END
END
GO
