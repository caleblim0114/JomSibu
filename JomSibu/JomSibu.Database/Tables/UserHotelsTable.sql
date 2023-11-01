CREATE TABLE [dbo].[UserHotelsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NULL, 
    [HotelId] INT NULL, 
    [StartDate] DATE NULL, 
    [EndDate] DATE NULL, 
    [IsDeleted] INT NULL, 
    CONSTRAINT [FK_UserHotelsTable_ToUserDetailsTable] FOREIGN KEY ([UserId]) REFERENCES [UserDetailsTable]([Id]), 
    CONSTRAINT [FK_UserHotelsTable_ToHotelsTable] FOREIGN KEY ([HotelId]) REFERENCES [HotelsTable]([Id])
)
