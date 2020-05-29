DROP INDEX [Ix_Text_Attribute_Def_Expired_Doc_Inc_Upper_Value] ON [dbo].[Text_Attributes]
--  ([Def_Id], [Expired], [Document_Id])
--INCLUDE ([Upper_Value])
--ON [Index2FileGroup]
GO

DROP INDEX [Ix_Text_Attribute_Def_Expired_UValue_Doc] ON [dbo].[Text_Attributes]
--  ([Def_Id], [Expired], [Upper_Value], [Document_Id])
--ON [Index2FileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Text_Attribute_Doc_Expired_Inc_Def_Value_Created] ON [dbo].[Text_Attributes]
--  ([Document_Id], [Expired])
--INCLUDE ([Def_Id], [Value], [Created])
--ON [Index2FileGroup]
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Text_Attribute_Doc_Expired_Def_UValue_Inc_Value] ON [dbo].[Text_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Upper_Value] ASC
)
INCLUDE ([Value])
ON [Index2FileGroup] 
GO

CREATE NONCLUSTERED INDEX [Ix_Text_Attribute_Def_Expired_UValue_Doc_Inc_Value] ON [dbo].[Text_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Upper_Value] ASC,
	[Document_Id] ASC
)
INCLUDE ([Value])
ON [Index2FileGroup] 
GO


