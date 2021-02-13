/*
Missing Index Details from SQLQuery6.sql - (local).FAAD2 (sa (51))
The Query Processor estimates that implementing the following index could improve the query cost by 18.9813%.
*/


USE [FAAD2]
GO
CREATE NONCLUSTERED INDEX IDX_crmOpp_02
ON [dbo].[crmOpp] ([IsDelete])
INCLUDE ([CustomerID],[OppStageID],[OwnerID],[OppRateEnumID],[CusSourceEnumID],[DocNo],[DocDate],[CustomerName],[TotalAmnt],[DiscAmnt],[NetAmnt],[OppExptCloseDate],[BranchID])
GO

