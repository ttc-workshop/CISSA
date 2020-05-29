CREATE UNIQUE NONCLUSTERED INDEX [IUX_Document_States_Doc_Expired_State] ON [dbo].[Document_States] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[State_Type_Id] ASC
)
INCLUDE(Created)
GO

CREATE NONCLUSTERED INDEX [IX_Document_States_State_Expired_Doc] ON [dbo].[Document_States] 
(
	[State_Type_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Boolean_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Boolean_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Boolean_Attribute_Def_Expired_Doc_Value] ON [dbo].[Boolean_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Currency_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Currency_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Currency_Attribute_Def_Expired_Doc_Value] ON [dbo].[Currency_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Date_Time_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Date_Time_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Date_Time_Attribute_Def_Expired_Doc_Value] ON [dbo].[Date_Time_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Float_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Float_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Float_Attribute_Def_Expired_Value_Doc] ON [dbo].[Float_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Int_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Int_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Int_Attribute_Def_Expired_Value_Doc] ON [dbo].[Int_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Enum_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Enum_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Enum_Attribute_Def_Expired_Value_Doc] ON [dbo].[Enum_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Document_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Document_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Attribute_Def_Expired_Doc_Value] ON [dbo].[Document_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Org_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Org_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Org_Attribute_Def_Expired_Value_Doc] ON [dbo].[Org_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Doc_State_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Doc_State_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Doc_State_Attribute_Def_Expired_Value_Doc] ON [dbo].[Doc_State_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Image_Attribute_Doc_Expired_Def_Inc_File_Created] ON [dbo].[Image_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC
)
INCLUDE ([File_Name], [Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Image_Attribute_Def_Expired_Doc] ON [dbo].[Image_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Text_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Text_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Upper_Value] ASC
)
INCLUDE ([Created], [Value]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Text_Attribute_Def_Expired_Value_Doc] ON [dbo].[Text_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Upper_Value] ASC
)
INCLUDE ([Created], [Value]) 
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Object_Def_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[Object_Def_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_Object_Def_Attribute_Def_Expired_Value_Doc] ON [dbo].[Object_Def_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_DocumentList_Attribute_Doc_Expired_Def_Value_Inc_Created] ON [dbo].[DocumentList_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
INCLUDE ([Created]) 
GO

CREATE NONCLUSTERED INDEX [Ix_DocumentList_Attribute_Def_Expired_Value_Doc] ON [dbo].[Object_Def_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Id_Def_Org_Deleted_Modified] ON [dbo].[Documents] 
(
	[Id] ASC,
	[Def_Id] ASC,
	[Organization_Id] ASC,
	[Deleted] ASC,
	[Last_Modified] DESC
)
INCLUDE ([UserId], [Org_Position_Id], [Created])
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Org_Deleted_Id] ON [dbo].[Documents] 
(
	[Def_Id] ASC,
	[Organization_Id] ASC,
	[Deleted] ASC,
	[Id] ASC,
	[Last_Modified] DESC
)
INCLUDE ([UserId], [Org_Position_Id], [Created])
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Org_Deleted_Modified] ON [dbo].[Documents] 
(
	[Def_Id] ASC,
	[Organization_Id] ASC,
	[Deleted] ASC,
	[Last_Modified] DESC
)
INCLUDE ([Id], [UserId], [Org_Position_Id], [Created])
GO

