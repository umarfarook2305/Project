-- =============================================
-- Author:      HART System
-- Create date: 2026
-- Description: Submit Request with Approvals (Insert/Update)
--              Handles both FTE and CW request types
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_SubmitFormRequest]
	-- Request parameters
	@RequestId UNIQUEIDENTIFIER = NULL,  -- If NULL, creates new request; if provided, updates existing
	@RequestType NVARCHAR(50),
	@RequestedOn DATETIME2 = NULL,
	@CurrentStatusId UNIQUEIDENTIFIER,
	@PendingWithEmail NVARCHAR(255),
	@PendingWithName NVARCHAR(255),
	@RequestBy NVARCHAR(255),
	@RequestByName NVARCHAR(255),

	-- Position/Job parameters
	@PositionTypeId UNIQUEIDENTIFIER = NULL,
	@JobLevelId UNIQUEIDENTIFIER = NULL,
	@FundingTypeId UNIQUEIDENTIFIER,
	@VacancyTypeId UNIQUEIDENTIFIER = NULL,  -- StatusOfPositionId
	@RoleTypeId UNIQUEIDENTIFIER = NULL,
	@SWPRoleId UNIQUEIDENTIFIER = NULL,
	@JobFamilyId UNIQUEIDENTIFIER = NULL,
	@JobProfileId UNIQUEIDENTIFIER = NULL,
	@CountryId UNIQUEIDENTIFIER,
	@City NVARCHAR(255),
	@PositionId UNIQUEIDENTIFIER = NULL,

	-- Project/Business parameters
	@LineOfBusinessId UNIQUEIDENTIFIER = NULL,
	@ProjectThemeId UNIQUEIDENTIFIER = NULL,
	@ProjectName NVARCHAR(500) = NULL,
	@Rationale NVARCHAR(MAX),

	-- Employee references
	@ReplacementEmployeeId UNIQUEIDENTIFIER = NULL,
	@PromotedEmployeeId UNIQUEIDENTIFIER = NULL,
	@CWEmployeeEmailId UNIQUEIDENTIFIER = NULL,

	-- Rates
	@CWAnnualRate DECIMAL(18,2) = NULL,
	@CWHourlyRate DECIMAL(18,2) = NULL,

	-- Flags
	@IsAIDataRole BIT = NULL,
	@IsAIGovernanceInvestment BIT = NULL,
	@IsApplicationEngineeringInvest BIT = NULL,
	@IsCWConversionFuture BIT = NULL,
	@IsNewPosition BIT = NULL,
	@HasNoIntraLevelReporting BIT = NULL,
	@MeetsSpanOfControlRequirements BIT = NULL,

	-- Approvals (JSON array)
	@ApprovalsJSON NVARCHAR(MAX),

	-- Output parameter
	@OutRequestId UNIQUEIDENTIFIER OUTPUT,
	@OutRequestCode NVARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrorMessage NVARCHAR(4000);
	DECLARE @IsUpdate BIT = 0;
	DECLARE @NewRequestCode NVARCHAR(50);
	DECLARE @MaxCode INT;

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Determine if this is an insert or update
		IF @RequestId IS NOT NULL AND EXISTS (SELECT 1 FROM [Request] WHERE RequestId = @RequestId)
		BEGIN
			SET @IsUpdate = 1;
		END
		ELSE
		BEGIN
			-- Generate new RequestId for insert
			SET @RequestId = NEWID();

			-- Generate RequestCode (next sequential number)
			SELECT @MaxCode = ISNULL(MAX(CAST(RequestCode AS INT)), 0) FROM [Request] WHERE ISNUMERIC(RequestCode) = 1;
			SET @NewRequestCode = CAST(@MaxCode + 1 AS NVARCHAR(50));
		END

		-- Set default RequestedOn if not provided
		IF @RequestedOn IS NULL
			SET @RequestedOn = GETUTCDATE();

		IF @IsUpdate = 1
		BEGIN
			-- UPDATE existing request
			UPDATE [Request]
			SET 
				RequestType = @RequestType,
				CurrentStatusId = @CurrentStatusId,
				PendingWithEmail = @PendingWithEmail,
				PendingWithName = @PendingWithName,
				RequestBy = @RequestBy,
				RequestByName = @RequestByName,
				PositionTypeId = @PositionTypeId,
				JobLevelId = @JobLevelId,
				FundingTypeId = @FundingTypeId,
				VacancyTypeId = @VacancyTypeId,
				RoleTypeId = @RoleTypeId,
				SWPRoleId = @SWPRoleId,
				JobFamilyId = @JobFamilyId,
				JobProfileId = @JobProfileId,
				CountryId = @CountryId,
				City = @City,
				PositionId = @PositionId,
				LineOfBusinessId = @LineOfBusinessId,
				ProjectThemeId = @ProjectThemeId,
				ProjectName = @ProjectName,
				Rationale = @Rationale,
				ReplacementEmployeeId = @ReplacementEmployeeId,
				PromotedEmployeeId = @PromotedEmployeeId,
				CWEmployeeEmailId = @CWEmployeeEmailId,
				CWAnnualRate = @CWAnnualRate,
				CWHourlyRate = @CWHourlyRate,
				IsAIDataRole = @IsAIDataRole,
				IsAIGovernanceInvestment = @IsAIGovernanceInvestment,
				IsApplicationEngineeringInvest = @IsApplicationEngineeringInvest,
				IsCWConversionFuture = @IsCWConversionFuture,
				IsNewPosition = @IsNewPosition,
				HasNoIntraLevelReporting = @HasNoIntraLevelReporting,
				MeetsSpanOfControlRequirements = @MeetsSpanOfControlRequirements,
				ModifiedOn = GETUTCDATE()
			WHERE RequestId = @RequestId;

			SELECT @OutRequestCode = RequestCode FROM [Request] WHERE RequestId = @RequestId;
		END
		ELSE
		BEGIN
			-- INSERT new request
			INSERT INTO [Request]
			(
				RequestId,
				RequestCode,
				RequestType,
				RequestedOn,
				CurrentStatusId,
				PendingWithEmail,
				PendingWithName,
				RequestBy,
				RequestByName,
				PositionTypeId,
				JobLevelId,
				FundingTypeId,
				VacancyTypeId,
				RoleTypeId,
				SWPRoleId,
				JobFamilyId,
				JobProfileId,
				CountryId,
				City,
				PositionId,
				LineOfBusinessId,
				ProjectThemeId,
				ProjectName,
				Rationale,
				ReplacementEmployeeId,
				PromotedEmployeeId,
				CWEmployeeEmailId,
				CWAnnualRate,
				CWHourlyRate,
				IsAIDataRole,
				IsAIGovernanceInvestment,
				IsApplicationEngineeringInvest,
				IsCWConversionFuture,
				IsNewPosition,
				HasNoIntraLevelReporting,
				MeetsSpanOfControlRequirements,
				IsActive,
				CreatedOn
			)
			VALUES
			(
				@RequestId,
				@NewRequestCode,
				@RequestType,
				@RequestedOn,
				@CurrentStatusId,
				@PendingWithEmail,
				@PendingWithName,
				@RequestBy,
				@RequestByName,
				@PositionTypeId,
				@JobLevelId,
				@FundingTypeId,
				@VacancyTypeId,
				@RoleTypeId,
				@SWPRoleId,
				@JobFamilyId,
				@JobProfileId,
				@CountryId,
				@City,
				@PositionId,
				@LineOfBusinessId,
				@ProjectThemeId,
				@ProjectName,
				@Rationale,
				@ReplacementEmployeeId,
				@PromotedEmployeeId,
				@CWEmployeeEmailId,
				@CWAnnualRate,
				@CWHourlyRate,
				@IsAIDataRole,
				@IsAIGovernanceInvestment,
				@IsApplicationEngineeringInvest,
				@IsCWConversionFuture,
				@IsNewPosition,
				@HasNoIntraLevelReporting,
				@MeetsSpanOfControlRequirements,
				1,  -- IsActive
				GETUTCDATE()
			);

			SET @OutRequestCode = @NewRequestCode;
		END

		-- Handle Approvals (if JSON provided)
		IF @ApprovalsJSON IS NOT NULL AND LEN(@ApprovalsJSON) > 0
		BEGIN
			-- Delete existing approvals for this request (if update)
			IF @IsUpdate = 1
			BEGIN
				DELETE FROM [Approval] WHERE RequestId = @RequestId;
			END

			-- Insert approvals from JSON
			DECLARE @MaxApprovalCode INT;
			SELECT @MaxApprovalCode = ISNULL(MAX(CAST(ApprovalCode AS INT)), 0) FROM [Approval] WHERE ISNUMERIC(ApprovalCode) = 1;

			INSERT INTO [Approval]
			(
				ApprovalId,
				ApprovalCode,
				RequestId,
				SeqOrder,
				RequestedTo,
				RequestedToEmail,
				RequestToRole,
				IsApprover,
				RequestedOn,
				ApprovedBy,
				ApprovedOn,
				PositionStatusId,
				IsActive,
				CreatedOn
			)
			SELECT 
				NEWID(),
				CAST(@MaxApprovalCode + ROW_NUMBER() OVER (ORDER BY SeqOrder) AS NVARCHAR(50)),
				@RequestId,
				SeqOrder,
				RequestedTo,
				RequestedToEmail,
				RequestedToRole,
				IsApprover,
				CASE WHEN RequestedOn IS NOT NULL THEN TRY_CAST(RequestedOn AS DATETIME2) ELSE NULL END,
				ApprovedBy,
				CASE WHEN ApprovedOn IS NOT NULL THEN TRY_CAST(ApprovedOn AS DATETIME2) ELSE NULL END,
				PositionStatusId,
				1,  -- IsActive
				GETUTCDATE()
			FROM OPENJSON(@ApprovalsJSON)
			WITH (
				PositionStatusId UNIQUEIDENTIFIER '$.positionStatusId',
				RequestedTo NVARCHAR(255) '$.requestedTo',
				RequestedToEmail NVARCHAR(255) '$.requestedToEmail',
				SeqOrder INT '$.sequenceOrder',
				IsApprover BIT '$.isApprover',
				RequestedToRole NVARCHAR(255) '$.requestedToRole',
				ApprovedOn NVARCHAR(50) '$.approvedOn',
				ApprovedBy NVARCHAR(255) '$.approvedBy',
				RequestedOn NVARCHAR(50) '$.requestedOn'
			);
		END

		-- Set output parameter
		SET @OutRequestId = @RequestId;

		COMMIT TRANSACTION;

		-- Return success
		SELECT 
			@OutRequestId AS RequestId,
			@OutRequestCode AS RequestCode,
			'success' AS Status,
			'' AS Error,
			'' AS FailedStep;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @ErrorMessage = ERROR_MESSAGE();

		-- Return error
		SELECT 
			NULL AS RequestId,
			NULL AS RequestCode,
			'error' AS Status,
			@ErrorMessage AS Error,
			'Database Operation' AS FailedStep;

		-- Re-throw error for logging
		THROW;
	END CATCH
END
GO
