CREATE TABLE [dbo].[AdvertisementsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Title] NVARCHAR(MAX) NULL, 
    [ImagePath] NVARCHAR(MAX) NULL, 
    [Description] NVARCHAR(MAX) NULL
)
