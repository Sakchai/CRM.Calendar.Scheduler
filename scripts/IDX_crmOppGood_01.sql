/*
Missing Index Details from SQLQuery2.sql - (local).FAAD2 (sa (63))
The Query Processor estimates that implementing the following index could improve the query cost by 50.4191%.
*/


USE [FAAD2]
GO
CREATE NONCLUSTERED INDEX IDX_crmOppGood_01
ON [dbo].[crmOppGood] ([OppID])

GO

