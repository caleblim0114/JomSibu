CREATE TABLE [dbo].[LocationReviewsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [LocationId] INT NULL, 
    [Review] DECIMAL NULL, 
    [Comment] NVARCHAR(MAX) NULL, 
    CONSTRAINT [FK_LocationReviewsTable_ToLocationsTable] FOREIGN KEY ([LocationId]) REFERENCES [LocationsTable]([Id])
)
