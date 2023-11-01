CREATE TABLE [dbo].[HotelsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(MAX) NULL, 
    [Address] NVARCHAR(MAX) NULL, 
    [BudgetStatusId] INT NULL, 
    CONSTRAINT [FK_HotelsTable_ToBudgetStatusesTable] FOREIGN KEY ([BudgetStatusId]) REFERENCES [BudgetStatusesTable]([Id])
)
