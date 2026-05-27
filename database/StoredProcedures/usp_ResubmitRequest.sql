-- =============================================
-- Author:      HART System
-- Create date: 2026
-- Description: Resubmit Request with Updated Approvals
--              Updates request and creates new approval workflow
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_ResubmitRequest]
	-- Request update parameters (all optional except RequestId)
	@RequestId UNIQUEIDENTIFIER,
	@JobProfileId UNIQUEIDENTIFIER = NULL,
	@LineOfBusinessId UNIQUEIDENTIFIER = NULL,
	@ProjectName NVARCHAR(500) = NULL,
	@ProjectThemeId UNIQUEIDENTIFIER = NULL,
	@FundingTypeId UNIQUEIDENTIFIER = NULL,
	@SWPRoleId UNIQUEIDENTIFIER = NULL,
	@JobFamilyId UNIQUEIDENTIFIER = NULL,
	@CountryId UNIQUEIDENTIFIER = NULL,
	@City NVARCHAR(255) = NULL,
	@Rationale NVARCHAR(MAX) = NULL,
	@CurrentStatusId UNIQUEIDENTIFIER = NULL,
	@RequestType NVARCHAR(50) = NULL,
	@IsAIDataRole BIT = NULL,
	@IsApplicationEngineeringInvest BIT = NULL,
	@IsAIGovernanceInvestment BIT = NULL,
	@IsCWConversionFuture BIT = NULL,
	@RequestedOn DATETIME2 = NULL,
	@PendingWithName NVARCHAR(255) = NULL,
	@PendingWithEmail NVARCHAR(255) = NULL,
	@RequestedByEmail NVARCHAR(255) = NULL,
	@RequestedByName NVARCHAR(255) = NULL,
	@JobLevelId UNIQUEIDENTIFIER = NULL,
	@PositionTypeId UNIQUEIDENTIFIER = NULL,
	@PositionId UNIQUEIDENTIFIER = NULL,
	@RoleTypeId UNIQUEIDENTIFIER = NULL,
	@VacancyTypeId UNIQUEIDENTIFIER = NULL,
	@PromotedEmployeeId UNIQUEIDENTIFIER = NULL,
	@ReplacementEmployeeId UNIQUEIDENTIFIER = NULL,
	@CWEmployeeEmailId UNIQUEIDENTIFIER = NULL,
	@CWAnnualRate DECIMAL(18,2) = NULL,
	@CWHourlyRate DECIMAL(18,2) = NULL,
	@HasNoIntraLevelReporting BIT = NULL,
	@IsNewPosition BIT = NULL,
	@MeetsSpanOfControlRequirements BIT = NULL,

	-- Approvals (JSON array)
	@ApprovalsJSON NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	DECLARE @ErrorMessage NVARCHAR(4000);
	DECLARE @ReturnedStatusId UNIQUEIDENTIFIER;

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Validate RequestId exists
		IF NOT EXISTS (SELECT 1 FROM dbo.[Request] WHERE RequestId = @RequestId)
		BEGIN
			SELECT 
				'404' AS StatusCode,
				'error' AS Status,
				'Request not found' AS Message;
			ROLLBACK TRANSACTION;
			RETURN;
		END

		-- Get "Returned to Requestor" status ID
		SELECT @ReturnedStatusId = PositionStatusId 
		FROM dbo.PositionStatus 
		WHERE StatusName = 'Returned to Requestor';

		-- Update Request with provided values (preserve existing if NULL)
		UPDATE dbo.[Request]
		SET 
			JobProfileId = ISNULL(@JobProfileId, JobProfileId),
			LineOfBusinessId = ISNULL(@LineOfBusinessId, LineOfBusinessId),
			ProjectName = ISNULL(@ProjectName, ProjectName),
			ProjectThemeId = ISNULL(@ProjectThemeId, ProjectThemeId),
			FundingTypeId = ISNULL(@FundingTypeId, FundingTypeId),
			SWPRoleId = ISNULL(@SWPRoleId, SWPRoleId),
			JobFamilyId = ISNULL(@JobFamilyId, JobFamilyId),
			CountryId = ISNULL(@CountryId, CountryId),
			City = ISNULL(@City, City),
			Rationale = ISNULL(@Rationale, Rationale),
			CurrentStatusId = ISNULL(@CurrentStatusId, CurrentStatusId),
			RequestType = ISNULL(@RequestType, RequestType),
			IsAIDataRole = ISNULL(@IsAIDataRole, IsAIDataRole),
			IsApplicationEngineeringInvest = ISNULL(@IsApplicationEngineeringInvest, IsApplicationEngineeringInvest),
			IsAIGovernanceInvestment = ISNULL(@IsAIGovernanceInvestment, IsAIGovernanceInvestment),
			IsCWConversionFuture = ISNULL(@IsCWConversionFuture, IsCWConversionFuture),
			RequestedOn = ISNULL(@RequestedOn, RequestedOn),
			PendingWithName = ISNULL(@PendingWithName, PendingWithName),
			PendingWithEmail = ISNULL(@PendingWithEmail, PendingWithEmail),
			RequestBy = ISNULL(@RequestedByEmail, RequestBy),
			RequestByName = ISNULL(@RequestedByName, RequestByName),
			JobLevelId = ISNULL(@JobLevelId, JobLevelId),
			PositionTypeId = ISNULL(@PositionTypeId, PositionTypeId),
			PositionId = ISNULL(@PositionId, PositionId),
			RoleTypeId = ISNULL(@RoleTypeId, RoleTypeId),
			VacancyTypeId = ISNULL(@VacancyTypeId, VacancyTypeId),
			PromotedEmployeeId = ISNULL(@PromotedEmployeeId, PromotedEmployeeId),
			ReplacementEmployeeId = ISNULL(@ReplacementEmployeeId, ReplacementEmployeeId),
			CWEmployeeEmailId = ISNULL(@CWEmployeeEmailId, CWEmployeeEmailId),
			CWAnnualRate = ISNULL(@CWAnnualRate, CWAnnualRate),
			CWHourlyRate = ISNULL(@CWHourlyRate, CWHourlyRate),
			HasNoIntraLevelReporting = ISNULL(@HasNoIntraLevelReporting, HasNoIntraLevelReporting),
			IsNewPosition = ISNULL(@IsNewPosition, IsNewPosition),
			MeetsSpanOfControlRequirements = ISNULL(@MeetsSpanOfControlRequirements, MeetsSpanOfControlRequirements),
			ModifiedOn = GETUTCDATE()
		WHERE RequestId = @RequestId;

		-- Before inserting new approvals, update any "Returned to Requestor" approvals to isApprover=false
		UPDATE dbo.Approval
		SET IsApprover = 0
		WHERE RequestId = @RequestId
			AND PositionStatusId = @ReturnedStatusId
			AND RequestedToEmail = @PendingWithEmail;

		-- Parse and insert new approval records
		IF @ApprovalsJSON IS NOT NULL AND @ApprovalsJSON != '[]'
		BEGIN
			INSERT INTO dbo.Approval (
				ApprovalId,
				RequestId,
				PositionStatusId,
				RequestedTo,
				RequestedToEmail,
				SeqOrder,
				IsApprover,
				RequestToRole,
				ApprovedBy,
				ApprovedOn,
				RequestedOn,
				CreatedOn
			)
			SELECT 
				NEWID() AS ApprovalId,
				@RequestId AS RequestId,
				TRY_CAST(JSON_VALUE(value, '$.positionStatusId') AS UNIQUEIDENTIFIER) AS PositionStatusId,
				JSON_VALUE(value, '$.requestedTo') AS RequestedTo,
				JSON_VALUE(value, '$.requestedToEmail') AS RequestedToEmail,
				JSON_VALUE(value, '$.sequenceOrder') AS SeqOrder,
				TRY_CAST(JSON_VALUE(value, '$.isApprover') AS BIT) AS IsApprover,
				JSON_VALUE(value, '$.requestedToRole') AS RequestToRole,
				JSON_VALUE(value, '$.approvedBy') AS ApprovedBy,
				TRY_CAST(JSON_VALUE(value, '$.approvedOn') AS DATETIME2) AS ApprovedOn,
				TRY_CAST(JSON_VALUE(value, '$.requestedOn') AS DATETIME2) AS RequestedOn,
				GETUTCDATE() AS CreatedOn
			FROM OPENJSON(@ApprovalsJSON);
		END

		COMMIT TRANSACTION;

		-- Return success response
		SELECT 
			'200' AS StatusCode,
			'success' AS Status,
			'Record updated successfully' AS Message,
			CAST(@RequestId AS NVARCHAR(50)) AS RequestId;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @ErrorMessage = ERROR_MESSAGE();

		SELECT 
			'500' AS StatusCode,
			'error' AS Status,
			'Update failed: ' + @ErrorMessage AS Message;
	END CATCH
END
GO
