using HART.Application.DTOs;
using HART.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HART.Infrastructure.Repositories;

/// <summary>
/// Repository for Get Delegate List operations using ADO.NET
/// </summary>
public class SqlGetDelegateListRepository : IGetDelegateListRepository
{
    private readonly string _connectionString;

    public SqlGetDelegateListRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<GetDelegateListResponse> GetDelegateListAsync(GetDelegateListRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_GetDelegateList", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            // Add parameters
            command.Parameters.AddWithValue("@EmailId", request.EmailId);
            command.Parameters.AddWithValue("@DelegationStatus", request.DelegationStatus ? 1 : 0);

            using var reader = await command.ExecuteReaderAsync();

            var delegateList = new List<DelegateItem>();

            while (await reader.ReadAsync())
            {
                var statusCode = reader["StatusCode"] != DBNull.Value
                    ? reader["StatusCode"].ToString()
                    : "500";

                var status = reader["Status"] != DBNull.Value
                    ? reader["Status"].ToString()
                    : "error";

                // Check if this is an error response
                if (statusCode != "200" || status != "success")
                {
                    return new GetDelegateListResponse
                    {
                        StatusCode = statusCode ?? "500",
                        Body = new GetDelegateListBody
                        {
                            Status = "error",
                            Data = new List<DelegateItem>()
                        }
                    };
                }

                // Success response - read delegate data
                var delegateId = reader["DelegateId"] != DBNull.Value
                    ? reader["DelegateId"].ToString()
                    : string.Empty;

                var delegateUser = reader["DelegateUser"] != DBNull.Value
                    ? reader["DelegateUser"].ToString()
                    : string.Empty;

                var delegateUserEmail = reader["DelegateUserEmail"] != DBNull.Value
                    ? reader["DelegateUserEmail"].ToString()
                    : string.Empty;

                var delegationStatus = reader["DelegationStatus"] != DBNull.Value
                    ? reader["DelegationStatus"].ToString()
                    : "False";

                var delegateFrom = reader["DelegateFrom"] != DBNull.Value
                    ? Convert.ToDateTime(reader["DelegateFrom"]).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : string.Empty;

                var delegateTo = reader["DelegateTo"] != DBNull.Value
                    ? Convert.ToDateTime(reader["DelegateTo"]).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : string.Empty;

                var behalfUser = reader["BehalfUser"] != DBNull.Value
                    ? reader["BehalfUser"].ToString()
                    : string.Empty;

                var behalfUserEmail = reader["BehalfUserEmail"] != DBNull.Value
                    ? reader["BehalfUserEmail"].ToString()
                    : string.Empty;

                var delegatedBy = reader["DelegatedBy"] != DBNull.Value
                    ? reader["DelegatedBy"].ToString()
                    : string.Empty;

                var delegatedByEmail = reader["DelegatedByEmail"] != DBNull.Value
                    ? reader["DelegatedByEmail"].ToString()
                    : string.Empty;

                var delegatedOn = reader["DelegatedOn"] != DBNull.Value
                    ? Convert.ToDateTime(reader["DelegatedOn"]).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : string.Empty;

                var uniqueID = reader["UniqueID"] != DBNull.Value
                    ? reader["UniqueID"].ToString()
                    : string.Empty;

                delegateList.Add(new DelegateItem
                {
                    DelegateId = delegateId ?? string.Empty,
                    DelegateUser = delegateUser ?? string.Empty,
                    DelegateUserEmail = delegateUserEmail ?? string.Empty,
                    DelegationStatus = delegationStatus ?? "False",
                    DelegateFrom = delegateFrom,
                    DelegateTo = delegateTo,
                    BehalfUser = behalfUser ?? string.Empty,
                    BehalfUserEmail = behalfUserEmail ?? string.Empty,
                    DelegatedBy = delegatedBy ?? string.Empty,
                    DelegatedByEmail = delegatedByEmail ?? string.Empty,
                    DelegatedOn = delegatedOn,
                    UniqueID = uniqueID ?? string.Empty
                });
            }

            return new GetDelegateListResponse
            {
                StatusCode = "200",
                Body = new GetDelegateListBody
                {
                    Status = "success",
                    Data = delegateList
                }
            };
        }
        catch (SqlException ex)
        {
            return new GetDelegateListResponse
            {
                StatusCode = "500",
                Body = new GetDelegateListBody
                {
                    Status = "error",
                    Data = new List<DelegateItem>()
                }
            };
        }
        catch (Exception ex)
        {
            return new GetDelegateListResponse
            {
                StatusCode = "500",
                Body = new GetDelegateListBody
                {
                    Status = "error",
                    Data = new List<DelegateItem>()
                }
            };
        }
    }
}
