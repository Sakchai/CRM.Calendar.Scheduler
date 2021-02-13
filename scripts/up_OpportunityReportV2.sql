USE [FAAD2]
GO
/****** Object:  StoredProcedure [dbo].[up_OpportunityReportV2]    Script Date: 2/13/2021 11:35:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
  
  
ALTER PROC [dbo].[up_OpportunityReportV2]  
 @StageNoStart nvarchar(max) = null,  
 @StageNoEnd nvarchar(max) = null,  
 @DocDateStart nvarchar(10) = null,  
 @DocDateEnd nvarchar(10) = null,  
 @CloseDateStart nvarchar(10) = null,  
 @CloseDateEnd nvarchar(10) = null,  
 @OppExptCloseDateStart nvarchar(10) = null,  
 @OppExptCloseDateEnd nvarchar(10) = null,  
 @DocNoStart nvarchar(max) = null,  
 @DocNoEnd nvarchar(max) = null,  
 @LeadNoStart nvarchar(max) = null,  
 @LeadNoEnd nvarchar(max) = null,  
 @CustomerNoStart nvarchar(max) = null,  
 @CustomerNoEnd nvarchar(max) = null,  
 @EmpNoStart nvarchar(max) = null,  
 @EmpNoEnd nvarchar(max) = null,  
 @CusSourceEnumNoStart nvarchar(max) = null,  
 @CusSourceEnumNoEnd nvarchar(max) = null,  
 @BranchID nvarchar(max) = null,  
 @Era nvarchar(10) = null,  
 @ReportType nvarchar(max)= 'Owner'  
AS  
BEGIN  
 WITH dataTemp AS (  
  SELECT crmOpp.DocDate,   
  crmOpp.DocNo,   
  crmOpp.OppCloseDate,  
  smCustomer.CustomerNo,   
  crmOpp.CustomerName,   
  crmOpp.OppTopic,   
  Enum1.EnumName AS Rating,   
  Enum2.EnumNo AS CusSourceNo,  
  Enum2.EnumName AS CusSource,   
  crmOpp.OppExptCloseDate,   
  crmOpp.NetAmnt,   
  smOppStage.StageNo,   
  smOppStage.StageName,   
  crmOpp.OppProb,  
  smEmployee.EmpNo,   
  smEmployee.EmpFirstName +' '+ smEmployee.EmpLastName AS EmpName,  
  crmLead.LeadID,  
  crmLead.LeadNo,  
  crmLead.LeadName,
  crmGood.GoodsNo,
  crmGood.GoodsName,
  oppNote.Note
  FROM crmOpp   
  LEFT JOIN (SELECT EnumID,EnumName FROM smEnum WHERE EnumTypeID=N'd8697aa6-34ce-415b-9543-91813afdfc5b') Enum1   
     ON crmOpp.OppRateEnumID = Enum1.EnumID   
  LEFT JOIN (SELECT EnumID,EnumName, EnumNo FROM smEnum WHERE EnumTypeID=N'b95cd3f7-c036-41b5-b98f-aa667905921e') Enum2   
     ON crmOpp.CusSourceEnumID = Enum2.EnumID  
  LEFT JOIN smOppStage ON crmOpp.OppStageID = smOppStage.OppStageID   
  LEFT JOIN smEmployee ON crmOpp.OwnerID = smEmployee.EmpID    
  LEFT JOIN smCustomer ON crmOpp.CustomerID = smCustomer.CustomerID   
  LEFT JOIN dbo.crmLead ON crmOpp.CustomerID = dbo.crmLead.LeadID   
  LEFT JOIN (select [OppID], max(cast(substring([Note],1,400) as varchar(400))) as Note
			 from [dbo].[crmOppNote] group by [OppID]) oppNote on oppNote.[OppID] = crmOpp.OppID
  LEFT JOIN (select crmOppGood.OppID, max(GoodsNo) GoodsNo, max(substring(smGood.GoodsName,1,200)) [GoodsName] from smGood 
			join crmOppGood on smGood.GoodsID = crmOppGood.GoodsID 
			group by crmOppGood.OppID) crmGood on crmGood.OppID = crmOpp.OppID
  WHERE crmOpp.IsDelete=0 AND crmOpp.BranchID = isnull(@BranchID, crmOpp.BranchID)  
 )  
 SELECT dataTemp.DocDate,  
  dbo.fn_DateDisplay(dataTemp.DocDate, @Era) AS StrDocDate,  
  dataTemp.DocNo,  
  dbo.fn_DateDisplay(dataTemp.OppCloseDate, @Era) AS StrOppCloseDate,  
  dataTemp.OppCloseDate,  
  dataTemp.CustomerNo,  
  dataTemp.CustomerName,  
  dataTemp.OppTopic,  
  dataTemp.Rating,  
  dataTemp.LeadID,  
  dataTemp.LeadNo,  
  dataTemp.LeadName,  
  dataTemp.CusSourceNo,  
  dataTemp.CusSource,  
  dataTemp.OppExptCloseDate,  
  dbo.fn_DateDisplay(dataTemp.OppExptCloseDate, @Era) AS StrOppExptCloseDate,  
  dataTemp.NetAmnt,  
  dataTemp.StageNo,  
  dataTemp.StageName,  
  dataTemp.OppProb,  
  dataTemp.EmpNo,  
  dataTemp.EmpName  ,
  dataTemp.GoodsNo,
  dataTemp.GoodsName,
  dataTemp.Note
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
   (  
    DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppCloseDate), 0) >= ISNULL(dbo.fn_ConvertStrDateToSystem(@CloseDateStart, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppCloseDate), 0)) and   
    DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppCloseDate), 0) <= ISNULL(dbo.fn_ConvertStrDateToSystem(@CloseDateEnd, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppCloseDate), 0))   
   ) OR   
   (  
    dataTemp.OppCloseDate is null AND   
    ISNULL(@CloseDateStart, '') = '' AND  
    ISNULL(@CloseDateEnd, '') = ''  
   )  
  )  
  AND  
  (  
   (  
    DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppExptCloseDate), 0) >= ISNULL(dbo.fn_ConvertStrDateToSystem(@OppExptCloseDateStart, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppExptCloseDate), 0)) and   
    DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppExptCloseDate), 0) <= ISNULL(dbo.fn_ConvertStrDateToSystem(@OppExptCloseDateEnd, @Era), DATEADD(dd, DATEDIFF(dd, 0, dataTemp.OppExptCloseDate), 0))   
   ) OR   
   (  
    dataTemp.OppExptCloseDate is null AND   
    ISNULL(@OppExptCloseDateStart, '') = '' AND  
    ISNULL(@OppExptCloseDateEnd, '') = ''  
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
  AND  
  (  
   (dataTemp.StageNo >=  ISNULL(@StageNoStart, dataTemp.StageNo) and dataTemp.StageNo <= ISNULL(@StageNoEnd, dataTemp.StageNo) OR   
   (ISNULL(dataTemp.StageNo, '') = '' and ISNULL(@StageNoStart, dataTemp.StageNo) = '' and ISNULL(@StageNoEnd, dataTemp.StageNo) = ''))  
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
   (dataTemp.CusSourceNo >= ISNULL(@CusSourceEnumNoStart, dataTemp.CusSourceNo) and dataTemp.CusSourceNo <= ISNULL(@CusSourceEnumNoEnd, dataTemp.CusSourceNo)) OR   
   (ISNULL(dataTemp.CusSourceNo, '') = '' and ISNULL(@CusSourceEnumNoStart, '') = '' and ISNULL(@CusSourceEnumNoEnd, '') = '')  
  )  
  ORDER BY  
  CASE WHEN ISNULL(@ReportType, N'DocDate') = N'DocDate' THEN dataTemp.DocDate END,  
  CASE WHEN @ReportType = N'OppCloseDate' THEN dataTemp.StageNo END,  
  CASE WHEN @ReportType = N'Owner' THEN dataTemp.EmpNo END,  
  CASE WHEN @ReportType = N'Customer' THEN dataTemp.CusSourceNo END, dataTemp.DocDate, dataTemp.DocNo  
END  