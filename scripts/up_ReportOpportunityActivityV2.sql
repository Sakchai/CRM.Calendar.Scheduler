USE [FAAD2]
GO
/****** Object:  StoredProcedure [dbo].[up_ReportOpportunityActivityV2]    Script Date: 2/13/2021 11:20:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROC [dbo].[up_ReportOpportunityActivityV2]
@DocNoStart nvarchar(max) = null,
@DocNoEnd nvarchar(max) = null,
@CustomerNoStart nvarchar(max) = null,
@CustomerNoEnd nvarchar(max) = null,
@LeadNoStart nvarchar(max) = null,
@LeadNoEnd nvarchar(max) = null,
@CreatedDateStart nvarchar(max) = null,
@CreatedDateEnd nvarchar(max) = null,
@StageNoStart nvarchar(max) = null,
@StageNoEnd nvarchar(max) = null,
@EmpNoStart nvarchar(max) = null,
@EmpNoEnd nvarchar(max) = null,
@BranchID nvarchar(max) = null,
@Era nvarchar(10) = null

as 
BEGIN

WITH dataTemp AS (
	SELECT 
		crmOpp.OppID,
		crmOpp.DocDate,
		crmOpp.DocNo,
		crmOpp.CreatedDate,
		CASE crmOpp.IsLead WHEN 1 THEN crmLead.LeadNo ELSE smCustomer.CustomerNo END AS CustomerNo,
		CASE crmOpp.IsLead WHEN 1 THEN crmLead.LeadName ELSE crmOpp.CustomerName END AS CustomerName,
		crmOpp.OppTopic,smEnum.EnumNo AS InterestNo,smEnum.EnumName AS InterestName,
		crmOpp.OppExptCloseDate,
		crmOpp.TotalAmnt,smOppStage.StageNo,smOppStage.StageName,crmOpp.OppProb,smEmployee.EmpNo,
		smEmployee.EmpFirstName + ' ' + smEmployee.EmpLastName AS EmpFullname,
		crmActivity.StartDate,
		crmActivity.EndDate,		
		crmLead.LeadID,
		crmLead.LeadNo,
		crmLead.LeadName,
		CAST(crmActivity.StartTime AS nvarchar(max)) AS StartTime,CAST(crmActivity.EndTime AS nvarchar(max)) AS EndTime,
		CASE 
			WHEN crmActivity.ActivityType = 1 AND crmActivity.Type = 1 THEN 'ทำงานนอกบริษัท'
			WHEN crmActivity.ActivityType = 1 AND crmActivity.Type = 2 THEN 'ทำงานในบริษัท'
			WHEN crmActivity.ActivityType = 2 AND crmActivity.Type = 1 THEN 'โทรเข้า'
			WHEN crmActivity.ActivityType = 2 AND crmActivity.Type = 2 THEN 'โทรออก'
			WHEN crmActivity.ActivityType = 3 AND crmActivity.Type = 1 THEN 'รับอีเมล์'
			WHEN crmActivity.ActivityType = 3 AND crmActivity.Type = 2 THEN 'ส่งอีเมล์'
		END AS ActivityTypeName,

		crmActivity.ActivityType,crmActivity.Type,crmActivity.Topic AS ActivityTopic,aEnum.EnumNo AS ActivityStatusNo,aEnum.EnumName AS ActivityStatusName,crmActivity.Detail,crmOpp.IsDelete,crmOpp.BranchID
	FROM crmOpp 
		LEFT JOIN crmActivity ON crmOpp.OppID = crmActivity.RefDocID 
		LEFT JOIN smCustomer ON crmOpp.CustomerID = smCustomer.CustomerID 
		LEFT JOIN smEnum ON crmOpp.OppRateEnumID = smEnum.EnumID 
		LEFT JOIN smOppStage ON crmOpp.OppStageID = smOppStage.OppStageID 
		LEFT JOIN smEmployee ON crmOpp.OwnerID = smEmployee.EmpID 
		LEFT JOIN smEnum AS aEnum ON crmActivity.StatusEnumID = aEnum.EnumID
		LEFT JOIN dbo.crmLead ON crmOpp.CustomerID = dbo.crmLead.LeadID 
	WHERE 
		crmOpp.IsDelete = '0' AND crmActivity.RefMenuID = '8da4d70f-a3e6-4c0a-9a02-40d7debab3f5' AND crmOpp.BranchID = @BranchID
)
SELECT 
	dataTemp.DocDate,
	dbo.fn_DateDisplay(dataTemp.DocDate,@Era) AS StrDocDate,
	dataTemp.DocNo,
	dataTemp.CreatedDate,
	dbo.fn_DateDisplay(dataTemp.CreatedDate,@Era) AS StrCreatedDatee,
	dataTemp.CustomerNo,
	dataTemp.CustomerName,
	dataTemp.OppTopic,
	dataTemp.InterestNo,
	dataTemp.InterestName,
	dataTemp.OppExptCloseDate, 
	dbo.fn_DateDisplay(dataTemp.OppExptCloseDate,@Era) AS StrOppExptCloseDate,
	dataTemp.TotalAmnt,
	dataTemp.StageNo,
	dataTemp.LeadID,
	dataTemp.LeadNo,
	dataTemp.LeadName,
	dataTemp.StageName,
	dataTemp.OppProb,
	dataTemp.EmpNo,
	dataTemp.EmpFullname as EmpName,
	dataTemp.StartDate,
	dbo.fn_DateDisplay(dataTemp.StartDate,@Era) AS StrStartDate,
	dataTemp.EndDate,
	dbo.fn_DateDisplay(dataTemp.EndDate,@Era) AS StrEndDate,
	dataTemp.StartTime,
	dataTemp.EndTime,
	dataTemp.ActivityTypeName,
	dataTemp.ActivityTopic,
	dataTemp.ActivityStatusName,
	dataTemp.Detail
FROM 
	dataTemp
WHERE
		(
			(dataTemp.DocNo >= ISNULL(@DocNoStart, dataTemp.DocNo) and dataTemp.DocNo <= ISNULL(@DocNoEnd, dataTemp.DocNo)) OR 
			(ISNULL(dataTemp.DocNo, '') = '' and ISNULL(@DocNoStart, '') = '' and ISNULL(@DocNoEnd, '') = '')
		)
		AND
		(
			(dataTemp.CustomerNo >= ISNULL(@CustomerNoStart, dataTemp.CustomerNo) and dataTemp.CustomerNo <= ISNULL(@CustomerNoEnd, dataTemp.CustomerNo)) OR 
			(ISNULL(dataTemp.CustomerNo, '') = '' and ISNULL(@CustomerNoStart, '') = '' and ISNULL(@CustomerNoEnd, '') = '')
		)
		AND
		(
			(
				DATEADD(dd, DATEDIFF(dd, 0, dataTemp.CreatedDate), 0) >= ISNULL(dbo.fn_ConvertStrDateToSystem(@CreatedDateStart, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.CreatedDate), 0)) and 
				DATEADD(dd, DATEDIFF(dd, 0, dataTemp.CreatedDate), 0) <= ISNULL(dbo.fn_ConvertStrDateToSystem(@CreatedDateEnd, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.CreatedDate), 0)) 
			) OR 
			(
				dataTemp.CreatedDate is null AND 
				ISNULL(@CreatedDateStart, '') = '' AND
				ISNULL(@CreatedDateEnd, '') = ''
			)
		)
		AND
		(
			(dataTemp.StageNo >= ISNULL(@StageNoStart, dataTemp.StageNo) and dataTemp.StageNo <= ISNULL(@StageNoEnd, dataTemp.StageNo)) OR 
			(ISNULL(dataTemp.StageNo, '') = '' and ISNULL(@StageNoStart, '') = '' and ISNULL(@StageNoEnd, '') = '')
		)
		AND
		(
			(dataTemp.EmpNo >= ISNULL(@EmpNoStart, dataTemp.EmpNo) and dataTemp.EmpNo <= ISNULL(@EmpNoEnd, dataTemp.EmpNo)) OR 
			(ISNULL(dataTemp.EmpNo, '') = '' and ISNULL(@EmpNoStart, '') = '' and ISNULL(@EmpNoEnd, '') = '')
		)
		/*  Work: W20180710-015
				Modified By: Mr.Thitipat Sangduen
				Modified Date: 10/07/2018
				Description: เพิ่มเงื่อนไขการค้นหาสถานะเอกสาร(Lead)
				*/ 
		AND
			(
				(dataTemp.LeadNo >= ISNULL(@LeadNoStart, dataTemp.LeadNo) and dataTemp.LeadNo <= ISNULL(@LeadNoEnd, dataTemp.LeadNo)) OR 
				(ISNULL(dataTemp.LeadNo, '') = '' and ISNULL(@LeadNoStart, '') = '' and ISNULL(@LeadNoEnd, '') = '')
			)

ORDER BY dataTemp.EmpNo, dataTemp.DocDate ASC, dataTemp.DocNo,dataTemp.StartTime

END
