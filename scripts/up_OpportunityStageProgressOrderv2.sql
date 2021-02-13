USE [FAAD2]
GO
/****** Object:  StoredProcedure [dbo].[up_OpportunityStageProgressOrderv2]    Script Date: 2/13/2021 11:35:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
  
  
ALTER PROC [dbo].[up_OpportunityStageProgressOrderv2]  
 @ReportType nvarchar(10) = N'Owner',  
 @DocDateStart nvarchar(10) = null,  
 @DocDateEnd nvarchar(10) = null,  
 @DocNoStart nvarchar(max) = null,  
 @DocNoEnd nvarchar(max) = null,  
 @LeadNoStart nvarchar(max) = null,  
 @LeadNoEnd nvarchar(max) = null,  
 @CustomerNoStart nvarchar(max) = null,  
 @CustomerNoEnd nvarchar(max) = null,  
 @EmpNoStart nvarchar(max) = null,  
 @EmpNoEnd nvarchar(max) = null,  
 @StageNoStart nvarchar(max) = null,  
 @StageNoEnd nvarchar(max) = null,  
 @BranchID nvarchar(max) = null,  
 @Era nvarchar(10) = null  
AS  
BEGIN  
 WITH dataTemp AS (  
 SELECT crmOpp.DocDate,   
  crmOpp.DocNo,   
  ISNULL(smCustomer.CustomerNo,crmLead.LeadNo) AS CustomerNo,   
  crmOpp.CustomerName,   
  smEmployee.EmpNo,   
  smEmployee.EmpFirstName + ' ' + smEmployee.EmpLastName AS EmpName,   
  OppRateEnum.EnumName AS OppRateName,   
  crmOpp.OppExptCloseDate,   
  CusSourceEnum.EnumName AS CusSourceName,  
  crmOpp.TotalAmnt,   
  crmOpp.DiscAmnt,   
  crmOpp.NetAmnt,   
  crmOppStageHistory.OppChageDate,   
  crmOppStageHistory.OppStageName,   
  crmOppStageHistory.OppProb,  
  crmOppStageHistory.OppExpectedRev,  
  crmLead.LeadID,  
  crmLead.LeadNo,  
  crmLead.LeadName,  
  smOppStage.StageNo  
 FROM crmOpp   
  LEFT JOIN smEnum AS OppRateEnum ON crmOpp.OppRateEnumID = OppRateEnum.EnumID   
  LEFT JOIN smEnum AS CusSourceEnum ON crmOpp.CusSourceEnumID = CusSourceEnum.EnumID   
  LEFT JOIN crmOppStageHistory ON crmOpp.OppID = crmOppStageHistory.OppID   
  LEFT JOIN smCustomer ON crmOpp.CustomerID = smCustomer.CustomerID   
  LEFT JOIN smEmployee ON crmOpp.OwnerID = smEmployee.EmpID  
  LEFT JOIN dbo.crmLead ON crmOpp.CustomerID = dbo.crmLead.LeadID   
  LEFT JOIN dbo.smOppStage ON crmOpp.OppStageID = smOppStage.OppStageID  
 WHERE crmOpp.IsDelete = 0 AND crmOpp.BranchID = isnull(@BranchID, crmOpp.BranchID)  
 )  
 SELECT dataTemp.DocDate,dbo.fn_DateDisplay(dataTemp.DocDate,@Era) AS StrDocDate,  
  dataTemp.DocNo,  
  dataTemp.CustomerNo,  
  dataTemp.CustomerName,  
  dataTemp.EmpNo,  
  dataTemp.EmpName,  
  dataTemp.OppRateName,  
  dataTemp.OppExptCloseDate,dbo.fn_DateDisplay(dataTemp.OppExptCloseDate,@Era) AS  StrOppExptCloseDate,  
  dataTemp.CusSourceName,  
  dataTemp.TotalAmnt,  
  dataTemp.DiscAmnt,  
  dataTemp.NetAmnt,  
  dataTemp.OppChageDate,dbo.fn_DateDisplay(dataTemp.OppChageDate,@Era) AS StrOppChageDate,  
  dataTemp.OppStageName,  
  dataTemp.OppProb,  
  dataTemp.OppExpectedRev,  
  dataTemp.LeadID,  
  dataTemp.LeadNo,  
  dataTemp.LeadName  
  
 FROM dataTemp  
 WHERE  
  (  
   (  
    DATEADD(dd, DATEDIFF(dd, 0, dataTemp.DocDate), 0) >= ISNULL(dbo.fn_ConvertStrDateToSystem(@DocDateStart, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.DocDate), 0)) and   
    DATEADD(dd, DATEDIFF(dd, 0, dataTemp.DocDate), 0) <= ISNULL(dbo.fn_ConvertStrDateToSystem(@DocDateEnd, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.DocDate), 0))   
   ) OR   
   (  
    dataTemp.DocDate is null AND   
    ISNULL(@DocDateStart, '') = '' AND  
    ISNULL(@DocDateEnd, '') = ''  
   )  
  )  
  AND  
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
   (dataTemp.EmpNo >= ISNULL(@EmpNoStart, dataTemp.EmpNo) and dataTemp.EmpNo <= ISNULL(@EmpNoEnd, dataTemp.EmpNo)) OR   
   (ISNULL(dataTemp.EmpNo, '') = '' and ISNULL(@EmpNoStart, '') = '' and ISNULL(@EmpNoEnd, '') = '')  
  )  
   /*  Work: W20180710-015  
    Modified By: Mr.Thitipat Sangduen  
    Modified Date: 10/07/2018  
    Description: ????????????????????????????????(Lead)  
    */   
  AND  
  (  
   (dataTemp.LeadNo >= ISNULL(@LeadNoStart, dataTemp.LeadNo) and dataTemp.LeadNo <= ISNULL(@LeadNoEnd, dataTemp.LeadNo)) OR   
   (ISNULL(dataTemp.LeadNo, '') = '' and ISNULL(@LeadNoStart, '') = '' and ISNULL(@LeadNoEnd, '') = '')  
  )  
  AND  
  (  
   (dataTemp.StageNo >= ISNULL(@StageNoStart, dataTemp.StageNo) and dataTemp.StageNo <= ISNULL(@StageNoEnd, dataTemp.StageNo)) OR   
   (ISNULL(dataTemp.StageNo, '') = '' and ISNULL(@StageNoStart, '') = '' and ISNULL(@StageNoEnd, '') = '')  
  )  
     
 /* Create By : Miss Artittaya Wongarsa  
  Create Date : 29/03/2019  
  Work No.    : W20200102-001  
  Description : ????????????????  */   
 ORDER BY  
 CASE WHEN ISNULL(@ReportType, N'DocDate') = N'DocDate' THEN dataTemp.DocDate END ASC,   
 CASE WHEN @ReportType = N'Customer' THEN dataTemp.CustomerNo END ASC,   
 CASE WHEN @ReportType = N'Owner' THEN dataTemp.EmpNo END ASC,  
 dataTemp.DocNo ASC,
 dataTemp.DocDate, dataTemp.OppChageDate ASC  
  
  
END  