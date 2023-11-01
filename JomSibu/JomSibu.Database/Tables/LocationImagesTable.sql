CREATE TABLE [dbo].[LocationImagesTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [LocationId] INT NULL, 
    [ImagePath] NVARCHAR(MAX) NULL, 
    CONSTRAINT [FK_LocationImagesTable_ToLocationsTable] FOREIGN KEY ([LocationId]) REFERENCES [LocationsTable]([Id])
)
