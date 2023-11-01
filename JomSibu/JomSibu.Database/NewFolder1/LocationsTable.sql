CREATE TABLE [dbo].[LocationsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(MAX) NULL, 
    [Address] NVARCHAR(MAX) NULL, 
    [OperationDateTime] DATETIME NULL, 
    [RecommendedDateTime] DATETIME NULL, 
    [AverageReview] DECIMAL NULL
)
