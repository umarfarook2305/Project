-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-23
-- Description: Get delegated approval list for a user
-- Based on Power Platform API: getDataList_Approval_Delegation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetApprovalDelegationList]
	@CurrentUserEmail NVARCHAR(255),
	@IsPending BIT = 1,
	@PageNumber INT = 1,
	@PageSize INT = 10
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @PendingStatusId UNIQUEIDENTIFIER;
	DECLARE @ReturnedStatusId UNIQUEIDENTIFIER;
	DECLARE @AwaitingStatusId UNIQUEIDENTIFIER;
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

	-- Get delegated approvers for the current user (active delegations)
	DECLARE @DelegatedApprovers TABLE (DelegateEmail NVARCHAR(255));

	INSERT INTO @DelegatedApprovers (DelegateEmail)
	SELECT DelegateUserEmail
	FROM Delegate
	WHERE BehalfUserEmail = @CurrentUserEmail
		AND DelegateFrom <= GETUTCDATE()
		AND DelegateTo >= GETUTCDATE();

	-- Check if there are any delegated approvers
	IF NOT EXISTS (SELECT 1 FROM @DelegatedApprovers)
	BEGIN
		-- Return empty result with pagination metadata
		SELECT 
			CAST(NULL AS UNIQUEIDENTIFIER) AS RequestUniqueId,
			CAST(NULL AS NVARCHAR(50)) AS RequestId,
			CAST(NULL AS NVARCHAR(255)) AS Status,
			CAST(NULL AS NVARCHAR(255)) AS Requestor,
			CAST(NULL AS NVARCHAR(255)) AS Funding,
			CAST(NULL AS NVARCHAR(50)) AS Type,
			CAST(NULL AS NVARCHAR(50)) AS Level,
			CAST(NULL AS DATETIME2) AS SubmittedOn,
			CAST(NULL AS DATETIME2) AS CompletedOn,
			CAST(NULL AS NVARCHAR(255)) AS Delegator,
			0 AS TotalRecords
		WHERE 1 = 0;
		RETURN;
	END

	-- Build main query based on isPending flag
	IF @IsPending = 1
	BEGIN
		-- Get total count first
		SELECT @TotalRecords = COUNT(*)
		FROM Approval a
		INNER JOIN Request r ON a.RequestId = r.RequestId
		INNER JOIN PositionStatus ps ON a.PositionStatusId = ps.PositionStatusId
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND (a.PositionStatusId = @PendingStatusId OR a.PositionStatusId = @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT DelegateEmail FROM @DelegatedApprovers);

		-- Get paginated pending approvals
		SELECT 
			r.RequestId AS RequestUniqueId,
			r.RequestCode AS RequestId,
			ps.StatusName AS Status,
			r.RequestByName AS Requestor,
			ft.FundingTypeName AS Funding,
			r.RequestType AS Type,
			jl.JobLevelName AS Level,
			a.RequestedOn AS SubmittedOn,
			r.CompletedOn,
			a.RequestedTo AS Delegator,
			@TotalRecords AS TotalRecords
		FROM Approval a
		INNER JOIN Request r ON a.RequestId = r.RequestId
		INNER JOIN PositionStatus ps ON a.PositionStatusId = ps.PositionStatusId
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND (a.PositionStatusId = @PendingStatusId OR a.PositionStatusId = @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT DelegateEmail FROM @DelegatedApprovers)
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
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND a.PositionStatusId NOT IN (@PendingStatusId, @AwaitingStatusId, @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT DelegateEmail FROM @DelegatedApprovers);

		-- Get paginated non-pending approvals
		SELECT 
			r.RequestId AS RequestUniqueId,
			r.RequestCode AS RequestId,
			ps.StatusName AS Status,
			r.RequestByName AS Requestor,
			ft.FundingTypeName AS Funding,
			r.RequestType AS Type,
			jl.JobLevelName AS Level,
			a.RequestedOn AS SubmittedOn,
			r.CompletedOn,
			a.RequestedTo AS Delegator,
			@TotalRecords AS TotalRecords
		FROM Approval a
		INNER JOIN Request r ON a.RequestId = r.RequestId
		INNER JOIN PositionStatus ps ON a.PositionStatusId = ps.PositionStatusId
		LEFT JOIN JobLevel jl ON r.JobLevelId = jl.JobLevelId
		LEFT JOIN FundingType ft ON r.FundingTypeId = ft.FundingTypeId
		WHERE 
			a.IsApprover = 1
			AND a.PositionStatusId NOT IN (@PendingStatusId, @AwaitingStatusId, @ReturnedStatusId)
			AND a.RequestedToEmail IN (SELECT DelegateEmail FROM @DelegatedApprovers)
		ORDER BY a.RequestedOn DESC
		OFFSET @Offset ROWS
		FETCH NEXT @PageSize ROWS ONLY;
	END
END
GO
