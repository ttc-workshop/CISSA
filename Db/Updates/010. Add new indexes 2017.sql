CREATE NONCLUSTERED INDEX [Ix_Text_Attribute_Def_Expired_Value_Doc_2] ON [dbo].[Text_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Upper_Value] ASC,
	[Document_Id] ASC
)
INCLUDE ( 
[Value]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO