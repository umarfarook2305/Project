-- =============================================
-- Author:      AI Assistant
-- Create date: 2026-05-23
-- Description: Get Position IDs with User Details and Job Requisitions
-- Based on Power Platform API: PositionId
-- Simple query with no parameters
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetPositionDetails]
AS
BEGIN
	SET NOCOUNT ON;

	-- Simple query - no parameters, no temp tables
	SELECT 
		a.EmployeeName,
		a.UserDetailId AS employeeGuid,
		b.PositionCode AS positionId,
		b.PositionId AS positionIdGuid,
		b.StatusLabel AS positionStatus,
		c.JobRequisitionId AS jobRequisitionGuid,
		c.RequisitionCode AS jobRequisitionId,
		d.JobProfileName
	FROM Position b
	INNER JOIN UserDetail a   ON a.PositionId = b.PositionId
	LEFT JOIN JobRequisition c ON b.PositionId = c.PositionId
	LEFT JOIN JobProfile d ON c.JobProfileId = d.JobProfileId;
END
GO



CREATE NONCLUSTERED INDEX IX_JobRequisition_PositionId
ON dbo.JobRequisition(PositionId)
INCLUDE (JobRequisitionId, RequisitionCode, JobProfileId);


CREATE NONCLUSTERED INDEX IX_UserDetail_PositionId
ON dbo.UserDetail(PositionId)
INCLUDE (UserDetailId, EmployeeName);