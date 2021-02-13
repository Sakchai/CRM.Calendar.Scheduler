/*
Missing Index Details from SQLQuery6.sql - (local).FAAD2 (sa (51))
The Query Processor estimates that implementing the following index could improve the query cost by 53.7441%.
*/


USE [FAAD2]
GO
CREATE NONCLUSTERED INDEX IDX_crmOppStageHistory_01
ON [dbo].[crmOppStageHistory] ([OppID])
INCLUDE ([OppStageName],[OppProb],[OppExpectedRev],[OppChageDate])
GO

