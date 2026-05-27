using HART.Application.Interfaces;
using HART.Application.Services;
using HART.Domain.Interfaces;
using HART.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HART.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories with SQL Server
        services.AddScoped<ILookupDataRepository, SqlLookupDataRepository>();
        services.AddScoped<IJobProfileRepository, SqlJobProfileRepository>();
        services.AddScoped<IHierarchyRepository, SqlHierarchyRepository>();
        services.AddScoped<IFormSubmissionRepository, SqlFormSubmissionRepository>();
        services.AddScoped<IRequestFilterRepository, SqlRequestFilterRepository>();
        services.AddScoped<IPositionRepository, SqlPositionRepository>();
        services.AddScoped<ICancelRequestRepository, SqlCancelRequestRepository>();
        services.AddScoped<IApprovalListRepository, SqlApprovalListRepository>();
        services.AddScoped<IApprovalDelegationListRepository, SqlApprovalDelegationListRepository>();
        services.AddScoped<IApproverActionRepository, SqlApproverActionRepository>();
        services.AddScoped<IStatusCardDetailsRepository, SqlStatusCardDetailsRepository>();
        services.AddScoped<IFundingDistributionRepository, SqlFundingDistributionRepository>();
        services.AddScoped<IJobLevelApprovedRepository, SqlJobLevelApprovedRepository>();
        services.AddScoped<IApprovedRoleDistributionRepository, SqlApprovedRoleDistributionRepository>();
        services.AddScoped<IJobFamilyDistributionRepository, SqlJobFamilyDistributionRepository>();
        services.AddScoped<IPendingStatusDistributionRepository, SqlPendingStatusDistributionRepository>();
        services.AddScoped<IPositionDistributionStatusRepository, SqlPositionDistributionStatusRepository>();
        services.AddScoped<IHiringTrendDataRepository, SqlHiringTrendDataRepository>();
        services.AddScoped<ICancelDelegateRepository, SqlCancelDelegateRepository>();
        services.AddScoped<IGetDelegateListRepository, SqlGetDelegateListRepository>();
        services.AddScoped<IUpdateDelegateRepository, SqlUpdateDelegateRepository>();
        services.AddScoped<IInsertDelegateRepository, SqlInsertDelegateRepository>();
        services.AddScoped<IResubmitRequestRepository, SqlResubmitRequestRepository>();

        // Register application services
        services.AddScoped<ILookupDataService, LookupDataService>();
        services.AddScoped<IJobProfileService, JobProfileService>();
        services.AddScoped<IHierarchyService, HierarchyService>();
        services.AddScoped<IFormSubmissionService, FormSubmissionService>();
        services.AddScoped<IRequestFilterService, RequestFilterService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<ICancelRequestService, CancelRequestService>();
        services.AddScoped<IApprovalListService, ApprovalListService>();
        services.AddScoped<IApprovalDelegationListService, ApprovalDelegationListService>();
        services.AddScoped<IApproverActionService, ApproverActionService>();
        services.AddScoped<IStatusCardDetailsService, StatusCardDetailsService>();
        services.AddScoped<IFundingDistributionService, FundingDistributionService>();
        services.AddScoped<IJobLevelApprovedService, JobLevelApprovedService>();
        services.AddScoped<IApprovedRoleDistributionService, ApprovedRoleDistributionService>();
        services.AddScoped<IJobFamilyDistributionService, JobFamilyDistributionService>();
        services.AddScoped<IPendingStatusDistributionService, PendingStatusDistributionService>();
        services.AddScoped<IPositionDistributionStatusService, PositionDistributionStatusService>();
        services.AddScoped<IHiringTrendDataService, HiringTrendDataService>();
        services.AddScoped<ICancelDelegateService, CancelDelegateService>();
        services.AddScoped<IGetDelegateListService, GetDelegateListService>();
        services.AddScoped<IUpdateDelegateService, UpdateDelegateService>();
        services.AddScoped<IInsertDelegateService, InsertDelegateService>();
        services.AddScoped<IResubmitRequestService, ResubmitRequestService>();

        return services;
    }
}
