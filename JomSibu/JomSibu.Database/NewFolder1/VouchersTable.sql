CREATE TABLE [dbo].[VouchersTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Title] NVARCHAR(MAX) NULL, 
    [Description] NVARCHAR(MAX) NULL, 
    [ExpiryDate] NVARCHAR(MAX) NULL, 
    [ImagePath] NVARCHAR(MAX) NULL
)
