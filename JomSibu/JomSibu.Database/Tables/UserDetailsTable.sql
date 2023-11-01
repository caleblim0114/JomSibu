CREATE TABLE [dbo].[UserDetailsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [FullName] NVARCHAR(MAX) NULL, 
    [Email] NVARCHAR(MAX) NULL, 
    [PhoneNumber] NVARCHAR(MAX) NULL, 
    [IsHalal] INT NULL, 
    [IsVegetarian] INT NULL, 
    [IsDeleted] INT NULL, 
    [ImagePath] NVARCHAR(MAX) NULL, 
    [BudgetStatusId] INT NULL, 
    CONSTRAINT [FK_UserDetailsTable_ToBudgetStatusesTable] FOREIGN KEY ([BudgetStatusId]) REFERENCES [BudgetStatusesTable]([Id])
)
