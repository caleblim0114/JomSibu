CREATE TABLE [dbo].[UserVouchersTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NULL, 
    [VoucherId] INT NULL, 
    [IsUsed] INT NULL, 
    CONSTRAINT [FK_UserVouchersTable_ToUserDetailsTable] FOREIGN KEY ([UserId]) REFERENCES [UserDetailsTable]([Id]), 
    CONSTRAINT [FK_UserVouchersTable_ToVouchersTable] FOREIGN KEY ([VoucherId]) REFERENCES [VouchersTable]([Id]) 
)
