/*
Missing Index Details from SQLQuery2.sql - (local).FAAD2 (sa (63))
The Query Processor estimates that implementing the following index could improve the query cost by 47.6584%.
*/


USE [FAAD2]
GO
CREATE NONCLUSTERED INDEX IDX_crmOpp_01
ON [dbo].[crmOpp] ([IsDelete])
INCLUDE ([CustomerID],[OppStageID],[OwnerID],[OppRateEnumID],[CusSourceEnumID],[DocNo],[DocDate],[CustomerName],[OppTopic],[OppProb],[OppCloseDate],[NetAmnt],[OppExptCloseDate],[BranchID])
GO

