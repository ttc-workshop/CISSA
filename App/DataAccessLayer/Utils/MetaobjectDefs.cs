using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;
// ReSharper disable InconsistentNaming

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public static class MetaobjectDefs
    {
        // Metaobject Document Defs
        public static readonly Guid Activity_Links_DefId = new Guid("B6EB5459-D2A8-4D5B-A4D4-3C24AD4ACB04");
        public static readonly Guid Attribute_Defs_DefId = new Guid("55E84E98-0E2F-487E-A199-07C964233AE6");
        public static readonly Guid Buttons_DefId = new Guid("03712A5C-DBE2-406E-8B78-2ECB081E3BFE");
        public static readonly Guid Combo_Boxes_DefId = new Guid("E3C4D8C5-E6FD-46B0-8BB3-8D8EE0774028");
        public static readonly Guid Compare_Operations_DefId = new Guid("06D95011-5207-4F3D-BE9C-67E2B3CB95AE");
        public static readonly Guid Controls_DefId = new Guid("2C063F9D-C326-4D29-85FD-6AC6803F85E8");
        public static readonly Guid Data_Transfer_Doc_Errors_DefId = new Guid("99D7C624-7092-4F2D-8835-1A70F9459C1C");
        public static readonly Guid Data_Transfer_Operations_DefId = new Guid("E375FD2B-DDE1-41E9-B1FF-4D6987B3612C");
        public static readonly Guid Data_Transfers_DefId = new Guid("3169AC22-180A-4740-8D6C-C78D825B4274");
        public static readonly Guid Data_Types_DefId = new Guid("D0FC8E1F-1AC6-4BEE-8129-69708F23E2F6");
        public static readonly Guid Document_Activities_DefId = new Guid("9A6CB43D-CE6C-4512-ADC5-B889EC4AC092");
        public static readonly Guid Document_Attributes_DefId = new Guid("184D8C21-0B7F-4107-ADD5-E607E23413B0");
        public static readonly Guid Document_Defs_DefId = new Guid("41F945AD-D66F-478F-B074-100BD401126C");
        public static readonly Guid Document_Route_Transits_DefId = new Guid("360EBD14-B77E-4C44-8187-35B7A45A7FE7");
        public static readonly Guid Document_Routes_DefId = new Guid("EF049AAC-B489-44E1-B6E1-AB2CF666C5BE");
        public static readonly Guid Document_State_Activities_DefId = new Guid("CF9CC7A4-E78D-41D2-A26E-C2C5D8D29ED8");
        public static readonly Guid Document_State_Types_DefId = new Guid("A11A3CBB-8693-48B1-9BB7-AB68CB1C0FD4");
        public static readonly Guid Document_Visibilities_DefId = new Guid("BC317BD8-2948-4437-B3E1-B8DDD4E28564");
        public static readonly Guid DocumentActivityType_DefId = new Guid("4F0F8E43-1C68-4A1B-8377-6DD4CC91F846");
        public static readonly Guid DocumentControl_DefId = new Guid("0C859839-913A-4D2D-B970-F0074F205BC2");
        public static readonly Guid DocumentList_Forms_DefId = new Guid("8C23C063-FA6B-4565-BA03-FEC2628CBB68");
        public static readonly Guid DynamicDocumentList_DefId = new Guid("F1ECF7D0-8737-4518-A104-39BF20E1A587");
        public static readonly Guid Editors_DefId = new Guid("F50F2307-17E5-436F-9C4B-DAB2FC7A6E35");
        public static readonly Guid Enum_Defs_DefId = new Guid("855C2FBD-487D-4F0E-9486-7394563EBE73");
        public static readonly Guid Enum_Items_DefId = new Guid("4450F6AC-F262-4C30-88E7-3D59A35B5D51");
        public static readonly Guid File_Templates_DefId = new Guid("B7CC2116-7C9C-4912-8A76-2113A1229F56");
        public static readonly Guid Finish_Activities_DefId = new Guid("55F2419B-1520-4FBD-ABFB-B19559462D83");
        public static readonly Guid Folder_Defs_DefId = new Guid("738121CB-0EDA-4BF5-A4EB-47340750A02B");
        public static readonly Guid Forms_DefId = new Guid("12F4C4FC-0E07-495B-8924-819A6A7AA666");
        public static readonly Guid Generators_DefId = new Guid("485D24C7-01BB-44D8-BCB7-0665307CCC9A");
        public static readonly Guid Grids_DefId = new Guid("A10C3E68-8F4C-45B1-A2E9-BD99766534F9");
        public static readonly Guid Images_DefId = new Guid("766D20AC-5395-4323-8FC4-D6BF2F2A1103");
        public static readonly Guid Languages_DefId = new Guid("BB269181-32B3-4E90-BE9A-45EAD206E61B");
        public static readonly Guid Layout_Types_DefId = new Guid("FDC7971D-15FA-42A6-A16D-E9B169727048");
        public static readonly Guid Menus_DefId = new Guid("F513DD9A-0F91-4841-B478-A70CD14355F4");
        public static readonly Guid Object_Def_Translations_DefId = new Guid("5A3340F0-0BC7-40BB-8B04-C0F034437153");
        public static readonly Guid Object_Defs_DefId = new Guid("93C8CA02-87EE-4143-A31D-753D5965D1E1");
        public static readonly Guid Org_Models_DefId = new Guid("38222C0A-1669-49E5-9628-3A5970A64704");
        public static readonly Guid Org_Positions_DefId = new Guid("97E7BA2F-1C61-4F71-A748-8D95BD39603A");
        public static readonly Guid Org_Positions_Roles_DefId = new Guid("69E16A8E-7BDA-4D5B-8CAB-DE1E883D3157");
        public static readonly Guid Org_Unit_Types_DefId = new Guid("7B0B7A41-328F-40A4-9CE8-6F3E4B6297CB");
        public static readonly Guid Org_Units_DefId = new Guid("5A3C27BD-68D8-4267-8F46-81B7B104F91A");
        public static readonly Guid Org_Units_Roles_DefId = new Guid("7891DA13-0FA9-47E5-9F89-EC0E33956C00");
        public static readonly Guid Organizations_DefId = new Guid("F23DDDB3-441F-409F-B30A-EF2E32F17814");
        public static readonly Guid OrgUnits_ObjectDefs_DefId = new Guid("6B2211E5-9BC7-4B15-9327-45D02996D7D9");
        public static readonly Guid Panels_DefId = new Guid("D18B57F5-B543-4FC7-A89F-7D7402BDB054");
        public static readonly Guid Permission_Defs_DefId = new Guid("D82A986F-9032-4E24-9F40-8E364570D1A8");
        public static readonly Guid Permissions_DefId = new Guid("9BA13DCD-6894-45D8-8D2E-DA7DB61203BD");
        public static readonly Guid Persons_DefId = new Guid("FC6779F5-3B6E-459B-ADFC-0883A15FAF38");
        public static readonly Guid Presentation_Activities_DefId = new Guid("2D9EBD4C-8523-4E42-A6E8-AD127A18D663");
        public static readonly Guid Process_Call_Activities_DefId = new Guid("6C573CC2-C546-495B-87AE-3D72F120FAD4");
        public static readonly Guid Project_Folders_DefId = new Guid("F6EF21FA-E403-405D-A6A8-27AE8C065072");
        public static readonly Guid Project_Items_DefId = new Guid("BB1012C7-3D5F-40BF-993A-1623B73FE6C7");
        public static readonly Guid Projects_DefId = new Guid("EFC2F0B6-2531-4379-A42C-DF2E45B1CE2B");
        public static readonly Guid Radio_Items_DefId = new Guid("3DCF1078-A236-45B8-B0BA-5E07F65A2546");
        public static readonly Guid Report_Bands_DefId = new Guid("911A796D-88A5-4605-BA2D-4711E4F9711F");
        public static readonly Guid Report_Editors_DefId = new Guid("12E1FCF6-87A1-449E-B154-5728617E5A23");
        public static readonly Guid Report_Section_Types_DefId = new Guid("C28C6330-A5B7-4706-A5CB-0394FBA8DB6A");
        public static readonly Guid Report_Sections_DefId = new Guid("B8C6D428-B7C2-4693-B85B-B040EED69E6A");
        public static readonly Guid Report_Summary_Types_DefId = new Guid("F7EE895B-83B3-4FE3-826D-BB792A114239");
        public static readonly Guid Report_Texts_DefId = new Guid("1CD2D3AC-E104-4E96-9DA9-43FE69410AD9");
        public static readonly Guid Reports_DefId = new Guid("82E61C14-D5E8-4027-9267-D889D112D84E");
        public static readonly Guid Role_Permissions_DefId = new Guid("BC7AF92F-E885-4FE0-9AF0-65F45C816E08");
        public static readonly Guid Role_Refs_DefId = new Guid("DE94338B-7D39-42FA-A618-06C92642C71E");
        public static readonly Guid Roles_DefId = new Guid("0379892F-865B-403F-AA0E-1779149A6D9E");
        public static readonly Guid Rules_DefId = new Guid("B7012F82-80F3-4452-B10F-5A0FFC9F2A1B");
        public static readonly Guid Script_Activities_DefId = new Guid("A5AB49C7-8439-44B1-98A0-A7A41431E90F");
        public static readonly Guid Scripts_DefId = new Guid("C4AFDCFD-8C50-4431-9A67-BC5BE9D20EE9");
        public static readonly Guid Security_Objects_DefId = new Guid("0C04D78B-81E9-40B2-84C5-31FB6A907C21");
        public static readonly Guid Start_Activities_DefId = new Guid("910B0AFF-B3D6-4BA8-994E-336DB4630A3D");
        public static readonly Guid Subjects_DefId = new Guid("86B60434-EFCA-4B13-B18C-FD3BD15B43E5");
        public static readonly Guid Tab_Controls_DefId = new Guid("6E80170A-70DC-42FC-9A71-99C4DFB33A6E");
        public static readonly Guid Table_Columns_DefId = new Guid("EE4D00FB-C8ED-4124-BDF4-A7BDA793C90A");
        public static readonly Guid Table_Forms_DefId = new Guid("0FF4BE6F-D978-4C96-85DC-B1CCC2F57819");
        public static readonly Guid Table_Reports_DefId = new Guid("0BF26860-96CB-45E1-8F54-64468AD08512");
        public static readonly Guid Texts_DefId = new Guid("32C8BAD7-42F8-4186-8737-333B0143B5B3");
        public static readonly Guid Titles_DefId = new Guid("DD1BB2A0-E4A8-4AA4-85FE-B2567BA62F0E");
        public static readonly Guid User_Actions_DefId = new Guid("67603020-6EBE-4DCB-B158-3B8B151ABC8C");
        public static readonly Guid Worker_Roles_DefId = new Guid("264A31F0-900A-473E-8750-C97C0519E58B");
        public static readonly Guid Workers_DefId = new Guid("CBAB558C-2D86-4C49-8BF1-7B0DCEFE1F08");
        public static readonly Guid Workflow_Activities_DefId = new Guid("891DC00B-4528-44C5-A585-CFACBD37ECEE");
        public static readonly Guid Workflow_Def_Rules_DefId = new Guid("2BBA6557-FA28-418A-A24F-B50979E67FD1");
        public static readonly Guid Workflow_Defs_DefId = new Guid("AED05FD5-2407-4E8A-9D05-97D7D39383D7");
        public static readonly Guid Workflow_Processes_DefId = new Guid("13C76FC6-76CD-4F04-8A4E-4E35E7B61CC4");


        // Metaobject attribute defs
        public static readonly Guid Activity_Links_Condition_DefId = new Guid("441F68FF-5BAA-4CB9-9F16-2BA69106D459");
        public static readonly Guid Activity_Links_Id_DefId = new Guid("7EEF8727-F87E-402E-A582-0DCC1224A568");
        public static readonly Guid Activity_Links_Is_Exception_Handler_DefId = new Guid("BB38C086-D1CA-490E-8200-75ECA66E3519");
        public static readonly Guid Activity_Links_Source_Id_DefId = new Guid("CF09070F-8AE9-4C7E-8B18-031D102A6886");
        public static readonly Guid Activity_Links_Target_Id_DefId = new Guid("9096382A-A076-41EA-8065-9331A2122B80");
        public static readonly Guid Activity_Links_User_Action_Id_DefId = new Guid("538AD4EB-64DA-4514-BD60-D44223E7AE53");
        public static readonly Guid Attribute_Defs_Type_Id_DefId = new Guid("7380758A-1351-4DB2-83BF-B46AE306627E");
        public static readonly Guid Attribute_Defs_Org_Type_Id_DefId = new Guid("CB5745A3-4AFB-4EB1-AF50-4FBDF683ACA7");
        public static readonly Guid Attribute_Defs_Id_DefId = new Guid("19DA07CC-A557-4104-8AAC-13B94A11E4C2");
        public static readonly Guid Attribute_Defs_Is_Not_Null_DefId = new Guid("7F5A5DE1-F471-4BF9-ADDB-D77CC48BF132");
        public static readonly Guid Attribute_Defs_Is_Unique_DefId = new Guid("0273B5F5-FB6A-4537-9BAD-0928F69DA89E");
        public static readonly Guid Attribute_Defs_Max_Length_DefId = new Guid("5414DDE9-F487-4000-A7B9-9C11324D8D69");
        public static readonly Guid Attribute_Defs_Max_Value_DefId = new Guid("99C4EF9D-B4B0-4C4B-A782-64BCFF37626E");
        public static readonly Guid Attribute_Defs_Min_Value_DefId = new Guid("5580C618-7715-4B5E-B74F-4D1A71226CC1");
        public static readonly Guid Attribute_Defs_BlobIsImage_DefId = new Guid("EC37D4A6-AAB7-4A84-A045-A88BD453B850");
        public static readonly Guid Attribute_Defs_BlobMaxHeight_DefId = new Guid("226B12E8-DF82-485A-8F75-FB3C757FEC83");
        public static readonly Guid Attribute_Defs_BlobMaxSizeBytes_DefId = new Guid("CE455034-8109-4536-83C3-DF82A5647D38");
        public static readonly Guid Attribute_Defs_BlobMaxWidth_DefId = new Guid("89A34F96-745A-4669-AFD2-96D955BF8E43");
        public static readonly Guid Attribute_Defs_CalculateScript_DefId = new Guid("6ABAD70F-729F-4C9E-807F-128EE3E2638F");
        public static readonly Guid Attribute_Defs_Default_Value_DefId = new Guid("0CA7C02C-8102-4365-8AAF-C3F1EF2EA6A6");
        public static readonly Guid Attribute_Defs_Document_Id_DefId = new Guid("39E65A9A-057B-46BA-A07F-79D00EDB5D2A");
        public static readonly Guid Attribute_Defs_Enum_Id_DefId = new Guid("75A690E4-1D38-4908-937E-1C3D03D323E2");
        public static readonly Guid Buttons_Button_Type_DefId = new Guid("A68791D1-B17C-4619-9FAB-99C99DC51771");
        public static readonly Guid Buttons_Action_Id_DefId = new Guid("4721F288-6DA7-444A-BE16-225067AE3287");
        public static readonly Guid Buttons_Action_Type_DefId = new Guid("E146D30F-67C7-443C-93DF-C71622901A94");
        public static readonly Guid Buttons_Id_DefId = new Guid("0144F540-68B9-4EA0-8F77-47024148DEF4");
        public static readonly Guid Buttons_User_Action_Id_DefId = new Guid("92E253EC-5B4B-412F-BA62-26E28171199C");
        public static readonly Guid Buttons_Process_Id_DefId = new Guid("B4A3CDAC-ED69-4B91-8CEC-4ADDC65BF1E6");
        public static readonly Guid Combo_Boxes_Rows_DefId = new Guid("7A938361-0558-4510-8D0C-210C1BE8DD55");
        public static readonly Guid Combo_Boxes_Is_Radio_DefId = new Guid("1DFD707A-B35F-4E5A-B743-40104754AB05");
        public static readonly Guid Combo_Boxes_Id_DefId = new Guid("6686B947-A243-4807-AEF3-6B6A4CFECC47");
        public static readonly Guid Combo_Boxes_Attribute_Id_DefId = new Guid("961BB265-E9C3-4DAC-8BB5-5D8B56FE886D");
        public static readonly Guid Combo_Boxes_Attribute_Name_DefId = new Guid("8732B125-5046-4598-B0D4-78F05B6173B8");
        public static readonly Guid Combo_Boxes_Detail_Attribute_Id_DefId = new Guid("B5AFA2EF-801B-45AB-B807-FA1B451FE232");
        public static readonly Guid Combo_Boxes_Detail_Attribute_Name_DefId = new Guid("3A4B1790-2C0C-4D76-B0B8-5BB3904653F9");
        public static readonly Guid Compare_Operations_Name_DefId = new Guid("0C2BEF0D-21C1-4E51-83E9-5BC77E8C4DE9");
        public static readonly Guid Compare_Operations_Id_DefId = new Guid("D426CAF4-83EB-4718-BCC4-5E93183FDF0A");
        public static readonly Guid Controls_Invisible_DefId = new Guid("231AB5BF-47F7-4F84-8ABC-2A6857B0BE4D");
        public static readonly Guid Controls_Not_Null_DefId = new Guid("F2750576-83F8-4699-A6EC-26E853895AA7");
        public static readonly Guid Controls_Id_DefId = new Guid("4F423796-18AB-4C23-9F27-3021AE504185");
        public static readonly Guid Controls_Compare_Operation_DefId = new Guid("75A0741F-90BE-4EFA-B3ED-81A6CBBFE39D");
        public static readonly Guid Controls_Sort_Type_DefId = new Guid("0F3CF735-D091-49CF-ABBB-19B9AC6C3885");
        public static readonly Guid Controls_Title_DefId = new Guid("B6652E5A-9907-4C0E-B1D7-DE87CEC47037");
        public static readonly Guid Controls_Style_DefId = new Guid("9E738D11-7C13-4C07-94CC-2ADFBF15F098");
        public static readonly Guid Controls_Read_Only_DefId = new Guid("39675A54-7E01-46BE-BCC5-4F9CADC16F34");
        public static readonly Guid Controls_VisibleInListView_DefId = new Guid("75C4793D-E9CF-4DA2-B8B8-33440904CFF7");
        public static readonly Guid Data_Transfer_Doc_Errors_Document_Id_DefId = new Guid("71A58D55-56FB-43AF-86E4-87AD33E4DA89");
        public static readonly Guid Data_Transfer_Doc_Errors_Type_Id_DefId = new Guid("FB67C2AF-DFBF-4B71-B12B-3C01F92D1478");
        public static readonly Guid Data_Transfer_Doc_Errors_Operation_Id_DefId = new Guid("31B79E44-E01C-46F6-ABE3-47F4E7FA4C1C");
        public static readonly Guid Data_Transfer_Operations_Start_Date_DefId = new Guid("66452780-F4F2-466F-B27A-E159628E28CB");
        public static readonly Guid Data_Transfer_Operations_Source_Date_DefId = new Guid("742F9B72-CD6C-4069-888F-C30A7BA9783F");
        public static readonly Guid Data_Transfer_Operations_Source_File_DefId = new Guid("803B2743-49F8-4229-B167-C0B978D75386");
        public static readonly Guid Data_Transfer_Operations_Source_Id_DefId = new Guid("3DB46BCB-B534-4574-802D-35E460CE2BCC");
        public static readonly Guid Data_Transfer_Operations_Direction_Id_DefId = new Guid("8B9CEAF4-598D-48B8-AA10-579375D52F38");
        public static readonly Guid Data_Transfer_Operations_Finish_Date_DefId = new Guid("679D6A9E-20BB-4951-8240-D138404E68C9");
        public static readonly Guid Data_Transfer_Operations_Id_DefId = new Guid("5387712F-F694-4638-8310-B800A8D77ACD");
        public static readonly Guid Data_Transfers_Id_DefId = new Guid("FBB8E1CC-09E4-4C61-9408-D984A0D048A3");
        public static readonly Guid Data_Transfers_Def_Id_DefId = new Guid("C28B9494-DF26-41B0-987F-253AE065A45E");
        public static readonly Guid Data_Transfers_Expired_DefId = new Guid("181EEF80-1BEC-4DF2-9E77-8F6377D3E2CD");
        public static readonly Guid Data_Transfers_Created_DefId = new Guid("103B095F-2150-4F27-B2E7-D9506E710FC5");
        public static readonly Guid Data_Transfers_Operation_Id_DefId = new Guid("E682590D-A45F-466C-A837-88063534F09B");
        public static readonly Guid Data_Transfers_Value_DefId = new Guid("78E00298-D8F9-4FD4-B4D2-4A57926A4F95");
        public static readonly Guid Data_Types_Id_DefId = new Guid("F8D55174-1E83-4FE3-B890-D44E6CA755DE");
        public static readonly Guid Data_Types_Name_DefId = new Guid("45F8D346-DC98-48C1-9164-795B736A29EB");
        public static readonly Guid Document_Activities_Document_Id_DefId = new Guid("03B6B640-4A19-4CC2-90B3-A63C3E0DCDDF");
        public static readonly Guid Document_Activities_DocActivityTypeId_DefId = new Guid("A3AAB8D8-4A4B-42A3-AAE1-FB80456DD66A");
        public static readonly Guid Document_Activities_Id_DefId = new Guid("DB15623D-3514-4DD7-B282-B40F86ABB68E");
        public static readonly Guid Document_Defs_Id_DefId = new Guid("E97628D3-C9CE-45F0-88D0-EB94EE8508EA");
        public static readonly Guid Document_Defs_Is_Inline_DefId = new Guid("DD0362D7-101F-43F4-93B2-4D1B205AFEBA");
        public static readonly Guid Document_Defs_Is_Public_DefId = new Guid("109A0939-8612-43D9-A227-220135E0C5B9");
        public static readonly Guid Document_Defs_Def_Data_DefId = new Guid("9322EBB5-667D-4A5C-BCBF-06AEE3291777");
        public static readonly Guid Document_Defs_Create_Permission_Id_DefId = new Guid("28475E36-836B-4339-90D0-6A4CFC6DD6D8");
        public static readonly Guid Document_Defs_Ancestor_Id_DefId = new Guid("B376F77D-4412-42D2-9518-8B6957057166");
        public static readonly Guid Document_Defs_Delete_Permission_Id_DefId = new Guid("DBFDD6B7-899B-4D28-B0E8-0085A331202D");
        public static readonly Guid Document_Defs_Update_Permission_Id_DefId = new Guid("9D3FDBC0-E6F6-488D-8A5D-43C12F072030");
        public static readonly Guid Document_Defs_WithHistory_DefId = new Guid("D586E986-DA55-4277-90B1-8A81C127F54D");
        public static readonly Guid Document_Defs_Select_Permission_Id_DefId = new Guid("4D870A0D-CFC6-454C-8B54-562BB1A73028");
        public static readonly Guid Document_Route_Transits_Source_State_Id_DefId = new Guid("80B1A7B1-31E5-43DB-B669-61770F682584");
        public static readonly Guid Document_Route_Transits_Target_State_Id_DefId = new Guid("181B85FF-B88E-423F-8E10-73E39C7B0173");
        public static readonly Guid Document_Route_Transits_Permission_Id_DefId = new Guid("663913C4-868C-4FC2-A09D-DE9229FD49C1");
        public static readonly Guid Document_Route_Transits_Id_DefId = new Guid("A20088BF-3E1C-497D-A97A-7634F207D32D");
        public static readonly Guid Document_Routes_Id_DefId = new Guid("621383E1-148F-4675-BB1E-CE0EE2A54018");
        public static readonly Guid Document_Routes_Document_Def_Id_DefId = new Guid("DD06E50B-5D4E-4F83-B81E-5FEF62F1B41F");
        public static readonly Guid Document_State_Activities_Id_DefId = new Guid("FB7E5A83-E710-47B1-AA40-B815CA056888");
        public static readonly Guid Document_State_Activities_State_Type_Id_DefId = new Guid("710AE892-7A0B-4D4F-8A3C-29A32BA66FCC");
        public static readonly Guid Document_State_Activities_State_Name_DefId = new Guid("A61D8301-0C84-456F-8F65-7BBD4925BB1F");
        public static readonly Guid Document_State_Types_Read_Only_DefId = new Guid("06018182-F9B2-476F-87C2-F38ECB6A20F9");
        public static readonly Guid Document_State_Types_Id_DefId = new Guid("CAF9A6C6-161B-4109-A9FC-113AD896F4AF");
        public static readonly Guid Document_Visibilities_Organization_Id_DefId = new Guid("C6518DE3-5E04-4E27-8608-F199314CB0AF");
        public static readonly Guid Document_Visibilities_Document_Id_DefId = new Guid("A87FF4D9-FB38-40B4-8B61-151E5255EA55");
        public static readonly Guid DocumentActivityType_DocActivityTypeId_DefId = new Guid("AAEC7189-19FF-46B0-9D19-445E2CAE5FF4");
        public static readonly Guid DocumentActivityType_Name_DefId = new Guid("39A66DA1-4B6A-4A67-B9E6-9356C0DA1DA5");
        public static readonly Guid DocumentControl_Form_Id_DefId = new Guid("5B53CDDF-6A4D-433C-840B-4B6C8CBE9455");
        public static readonly Guid DocumentControl_Id_DefId = new Guid("00EB89F6-5C2D-4778-91A8-33686BFEF444");
        public static readonly Guid DocumentControl_Attribute_Name_DefId = new Guid("695A81C8-CC68-45B4-A81D-B99DE737D820");
        public static readonly Guid DocumentControl_Attribute_Id_DefId = new Guid("2643D553-F512-49CC-928F-88BD14AD2496");
        public static readonly Guid DocumentList_Forms_Form_Attribute_Id_DefId = new Guid("F7D917A8-F9A1-4637-9AB7-7D978F5DFF2B");
        public static readonly Guid DocumentList_Forms_Form_Id_DefId = new Guid("D281DA82-6B6F-47EA-8DCB-86634D6AC363");
        public static readonly Guid DocumentList_Forms_Id_DefId = new Guid("19A3E938-D9B5-4465-A2E4-12C9D94087D9");
        public static readonly Guid DocumentList_Forms_Attribute_Name_DefId = new Guid("5455097C-A94D-46C4-ABB8-9F96950EDE6C");
        public static readonly Guid DocumentList_Forms_Attribute_Id_DefId = new Guid("02C70DDB-EDC7-43B3-BC28-CCFFE9CAA4F9");
        public static readonly Guid DynamicDocumentList_ScriptId_DefId = new Guid("04FB6A6D-CB51-41F0-81D6-E1DE80DBE8D3");
        public static readonly Guid DynamicDocumentList_Form_Id_DefId = new Guid("14D41100-A685-4732-96BD-72244EB3B9FC");
        public static readonly Guid DynamicDocumentList_Id_DefId = new Guid("8B12481A-263F-4A0C-AE5C-3C8AA736940F");
        public static readonly Guid Editors_Id_DefId = new Guid("2A5B7F12-67F2-4197-AD27-2C5CEFE9310E");
        public static readonly Guid Editors_Format_DefId = new Guid("2F6B3310-9AFB-4943-AFF6-6F00107FA781");
        public static readonly Guid Editors_Edit_Mask_DefId = new Guid("864A2171-2CF7-4A5A-882A-7458BBCBCE8B");
        public static readonly Guid Editors_Cols_DefId = new Guid("FFC144CF-B649-4071-ADB7-790DEA93BC38");
        public static readonly Guid Editors_Attribute_Name_DefId = new Guid("3A61DB3C-A703-4F19-AAB7-B897F681A46D");
        public static readonly Guid Editors_Attribute_Id_DefId = new Guid("66491899-6251-4186-A2C1-81C2FEA6C552");
        public static readonly Guid Editors_Rows_DefId = new Guid("6CB911F7-C156-4E89-91D8-71FED4929905");
        public static readonly Guid Enum_Defs_Id_DefId = new Guid("2F6F94D7-4A4B-4E70-BB47-464494BEC14A");
        public static readonly Guid Enum_Items_Id_DefId = new Guid("354ED614-CE96-44E5-84A4-109FF56D1A83");
        //public static readonly Guid Enum_Items_Name_DefId = new Guid("D5435204-DBF1-4B74-9A53-EE63CBBCD2AD");
        public static readonly Guid File_Templates_Id_DefId = new Guid("C1981E14-2EB8-4B89-92FE-1E7AA376C3C9");
        public static readonly Guid File_Templates_File_Name_DefId = new Guid("ED358EB7-08AB-4446-AB2F-8EE916883841");
        public static readonly Guid Finish_Activities_Form_Id_DefId = new Guid("02AC2843-7EFE-419C-8680-F2AB78B04DD5");
        public static readonly Guid Finish_Activities_Message_DefId = new Guid("91B882B5-3F2E-4A61-A2BF-E8F3C071CE6B");
        public static readonly Guid Finish_Activities_Id_DefId = new Guid("B79876ED-3041-4BA8-A918-80D69072FB7A");
        public static readonly Guid Folder_Defs_Id_DefId = new Guid("4B2F2FE1-F625-40B1-B607-E13992E20646");
        public static readonly Guid Forms_Id_DefId = new Guid("AFF10200-3872-4006-9A7E-ED60190614A1");
        public static readonly Guid Forms_Layout_Id_DefId = new Guid("A83C48F9-D6D8-4ACF-A283-E720DDFCE11C");
        public static readonly Guid Forms_Def_Data_DefId = new Guid("4720D1B3-5C8D-4397-9164-38B916019912");
        public static readonly Guid Forms_Ban_Edit_DefId = new Guid("4F32F2C9-D60F-4825-9B76-AAE610A2A3E6");
        public static readonly Guid Forms_Can_Delete_DefId = new Guid("EE79EE3A-A876-4B9D-B576-10427DCE60EE");
        public static readonly Guid Forms_Document_Id_DefId = new Guid("7E2F93D6-ECFD-4E45-ABF3-BD087AACF6B4");
        public static readonly Guid Forms_Script_DefId = new Guid("E9660B88-A691-4DB8-A85E-7968B3C9FDC0");
        public static readonly Guid Generators_Organization_Id_DefId = new Guid("355ECEF0-BA67-47F7-89AB-06DD673193ED");
        public static readonly Guid Generators_Value_DefId = new Guid("369AE583-49CC-428F-9C8C-03B23174A659");
        public static readonly Guid Generators_Document_Def_Id_DefId = new Guid("AB39716B-2D4E-4A92-9A88-5489936C7D9F");
        public static readonly Guid Grids_Document_Id_DefId = new Guid("162561A1-6759-470D-B945-1F90E81B38D5");
        public static readonly Guid Grids_Is_Detail_DefId = new Guid("27CFF711-C258-47F1-BFB1-ADBD066D1603");
        public static readonly Guid Grids_Id_DefId = new Guid("F50A985E-BA28-43FF-B69E-AE5D5637E9D9");
        public static readonly Guid Images_Id_DefId = new Guid("C54B29EB-01E6-478C-9E42-CD004BFC7AA9");
        public static readonly Guid Images_Image_Data_DefId = new Guid("C935DF47-B05B-4690-B4BF-2C182D00987C");
        public static readonly Guid Languages_Culture_Name_DefId = new Guid("B1D3F43C-E5DC-4B71-80B5-92AB390BD796");
        public static readonly Guid Languages_Name_DefId = new Guid("FB362E33-F03B-448D-A979-DBC1A26F7461");
        public static readonly Guid Languages_Id_DefId = new Guid("87D8C828-BF36-4CD1-8805-DB2D1B0B0BB8");
        public static readonly Guid Layout_Types_Id_DefId = new Guid("ADEAF5C4-88D4-43DF-ABB4-5B90E5B1ECC0");
        public static readonly Guid Layout_Types_Name_DefId = new Guid("7083EA00-84FC-4AFE-85A9-B1B982E78B40");
        public static readonly Guid Menus_Id_DefId = new Guid("15777600-A95E-4C8B-AF25-8309B9E89EA4");
        public static readonly Guid Menus_Form_Id_DefId = new Guid("3B6F26CE-9DC3-4E3C-9ED2-DE8055AC9446");
        public static readonly Guid Menus_Def_Data_DefId = new Guid("6440841E-7270-47AE-A2F0-D537F67B20EC");
        public static readonly Guid Menus_Process_Id_DefId = new Guid("077B6A9E-8DA8-469E-95EA-838CF1042560");
        public static readonly Guid Menus_State_Type_Id_DefId = new Guid("01E41485-5F6E-4BEC-8929-16D0D918CD1D");
        public static readonly Guid Object_Def_Translations_Def_Id_DefId = new Guid("DDAE9BD5-CBA2-4D66-814E-67FF034604DF");
        public static readonly Guid Object_Def_Translations_Data_Text_DefId = new Guid("F437CFC4-F0B0-4706-A4B1-E7D5098FA377");
        public static readonly Guid Object_Def_Translations_Language_Id_DefId = new Guid("F88B17CD-9A49-47C3-8207-65179DF8A7E0");
        public static readonly Guid Object_Defs_Name_DefId = new Guid("9F1F2577-0C98-4F27-8BA6-1465AD406A5F");
        public static readonly Guid Object_Defs_Full_Name_DefId = new Guid("7705FF06-A811-43E0-B345-7BA557A6AB29");
        public static readonly Guid Object_Defs_Id_DefId = new Guid("9F5587F3-64E8-453A-945C-0ADEA2A499F0");
        public static readonly Guid Object_Defs_Created_DefId = new Guid("987A545F-9D48-472C-92CD-5CFEFF313A59");
        public static readonly Guid Object_Defs_Deleted_DefId = new Guid("64674256-A54E-40DA-A037-B3B0C2CBE775");
        public static readonly Guid Object_Defs_Description_DefId = new Guid("482456C3-24FF-47E6-AB7B-843FC0797384");
        public static readonly Guid Object_Defs_Order_Index_DefId = new Guid("BC33BBF4-EBA6-42DD-97D2-1C7447B00191");
        public static readonly Guid Object_Defs_Parent_Id_DefId = new Guid("BD9F301B-BA3C-4558-B2FF-85A73E7427E6");
        public static readonly Guid Org_Models_Id_DefId = new Guid("946A8C38-3188-4090-A42F-4BCF311AA486");
        public static readonly Guid Org_Models_Is_Active_DefId = new Guid("15E4BDB7-3A1D-402A-B867-F20F2C00DDA4");
        public static readonly Guid Org_Positions_Id_DefId = new Guid("4771246C-4A17-4FBB-870F-EB87477AB1CC");
//        public static readonly Guid Org_Positions_Name_DefId = new Guid("F4E91BD5-22F4-4988-ADF7-83D7EFAE4A2F");
        public static readonly Guid Org_Positions_Title_Id_DefId = new Guid("694CA69F-C2CA-43D4-993E-63AF1E0CE7A9");
        public static readonly Guid Org_Positions_Roles_Org_Position_Id_DefId = new Guid("6A08DC30-70CB-46BC-8598-1CD2E1127E7E");
        public static readonly Guid Org_Positions_Roles_Role_Id_DefId = new Guid("2D54FAFD-A406-4498-B229-26CBF1679BEC");
//        public static readonly Guid Org_Unit_Types_Name_DefId = new Guid("24467AF8-D48A-4D6C-979F-BFCC4566388D");
        public static readonly Guid Org_Unit_Types_Id_DefId = new Guid("D18D666F-A638-4FB3-A5F2-F10E47E342C5");
//        public static readonly Guid Org_Units_Full_Name_DefId = new Guid("3A429797-5B6B-490F-8DF3-4E402B0C967C");
        //public static readonly Guid Org_Units_Name_DefId = new Guid("4A1BB4A0-F42F-43D9-9E9C-3815F296A838");
        public static readonly Guid Org_Units_Id_DefId = new Guid("5D00413F-C977-45CD-B032-410F59C08887");
        public static readonly Guid Org_Units_INN_DefId = new Guid("2A3A6D8B-0E67-4513-A5A1-CA6B95F24EB3");
        public static readonly Guid Org_Units_Type_Id_DefId = new Guid("D0EE89F1-BBD7-4C93-9901-E4D236D8D73F");
        public static readonly Guid Org_Units_Roles_Role_Id_DefId = new Guid("829100F3-5727-45BC-AA79-1FBB19888CCF");
        public static readonly Guid Org_Units_Roles_Org_Unit_Id_DefId = new Guid("FC36DB50-63B4-4160-A1A4-2DE6A3806F80");
        public static readonly Guid Organizations_Type_Id_DefId = new Guid("CF9B3A90-BEB0-4E56-ADA1-7657D77141E2");
        public static readonly Guid Organizations_Id_DefId = new Guid("FCA7ACE9-C5DB-4F57-99E6-7E598A28D197");
        public static readonly Guid Organizations_Code_DefId = new Guid("5D0FA1B3-3D13-4C2B-9F1F-511256DD5953");
        public static readonly Guid OrgUnits_ObjectDefs_ObjDef_Id_DefId = new Guid("8E27256C-9183-4B5F-90DB-1F8A1A36D657");
        public static readonly Guid OrgUnits_ObjectDefs_OrgUnit_Id_DefId = new Guid("896833F3-BE56-470A-920C-3A9754E57903");
        public static readonly Guid Panels_Layout_Id_DefId = new Guid("344F61AB-7983-4A6D-ACD4-C93DCCFF940E");
        public static readonly Guid Panels_Id_DefId = new Guid("B3F96C2F-6850-4969-B853-6D910D9A9237");
        public static readonly Guid Panels_Is_Horizontal_DefId = new Guid("E7CEBC8A-7B51-4EC1-B0D1-9CF859E39A72");
        public static readonly Guid Permission_Defs_Is_Disabled_DefId = new Guid("6B6CCC37-D0F0-4ACA-B130-330000435045");
        public static readonly Guid Permission_Defs_AllowDelete_DefId = new Guid("778A2960-4F85-4066-9343-878D79159857");
        public static readonly Guid Permission_Defs_AllowInsert_DefId = new Guid("AB3494B2-E09A-4FF0-B053-0C1A92039559");
        public static readonly Guid Permission_Defs_AllowSelect_DefId = new Guid("0D66E58D-12E3-45F4-B076-C90871C90815");
        public static readonly Guid Permission_Defs_AllowUpdate_DefId = new Guid("46B39B35-2A67-461E-B131-2452888CCFED");
        public static readonly Guid Permission_Defs_Access_Type_DefId = new Guid("0544F375-8596-4B34-BD00-0E43D53D2C4D");
        public static readonly Guid Permission_Defs_Def_Id_DefId = new Guid("5AD4C661-AE56-44C7-863F-61FCA919234C");
        public static readonly Guid Permission_Defs_Permission_Id_DefId = new Guid("0130A17B-F7E0-45F6-A7F3-E39AAA7F2829");
        public static readonly Guid Permissions_Id_DefId = new Guid("59C41492-8BBC-457A-83B5-990B36A70648");
        public static readonly Guid Persons_Id_DefId = new Guid("39ABFF67-80D3-4F2F-9F07-4501D21B28AE");
        public static readonly Guid Persons_First_Name_DefId = new Guid("17CCA5CA-EC8D-4BFA-AB9F-9C8534CF0B84");
        public static readonly Guid Persons_INN_DefId = new Guid("52081D08-2204-4DD5-B0C6-9D5EDE19B7E8");
        public static readonly Guid Persons_Last_Name_DefId = new Guid("FA64DAAA-E53A-422B-AF36-7BC7C0E14689");
        public static readonly Guid Persons_Middle_Name_DefId = new Guid("649D0531-2A26-4FEF-8D96-CD14E31D383F");
        public static readonly Guid Persons_Birth_Date_DefId = new Guid("9961F872-131E-4DD1-9874-6B297B3D7108");
        public static readonly Guid Persons_Passport_Date_DefId = new Guid("BE0AD185-557F-480E-8095-4805992D814B");
        public static readonly Guid Persons_Passport_No_DefId = new Guid("24290D04-EE95-4B3A-B382-DA8D5D45C5B3");
        public static readonly Guid Persons_Passport_Org_DefId = new Guid("E073335C-8076-4B00-88D6-173D36298AA0");
        public static readonly Guid Persons_Sex_DefId = new Guid("2B9557BB-27B2-4A27-A000-D370B9E5F07B");
        public static readonly Guid Presentation_Activities_Message_DefId = new Guid("95789E94-AAC7-48EB-BEB4-09398493F779");
        public static readonly Guid Presentation_Activities_Is_Exception_DefId = new Guid("1D95D05B-D434-45E7-B9EA-26A2ACF24441");
        public static readonly Guid Presentation_Activities_Id_DefId = new Guid("486B9C8A-1E99-4301-9F05-E9FA72AA85AB");
        public static readonly Guid Presentation_Activities_Form_Id_DefId = new Guid("1BABA423-5907-4B7D-B8FE-204CA8944895");
        public static readonly Guid Process_Call_Activities_Id_DefId = new Guid("7EED8D96-191A-49EC-9272-2352EBD16DEC");
        public static readonly Guid Process_Call_Activities_Process_Id_DefId = new Guid("2C51D8AB-F1DA-436F-9830-6818C6B34804");
        public static readonly Guid Project_Folders_Id_DefId = new Guid("4024337C-171F-4A9E-B713-BC7F0EE946F3");
        public static readonly Guid Project_Items_Id_DefId = new Guid("0368103F-2698-4810-A707-6B9B3F7F468B");
        public static readonly Guid Projects_Id_DefId = new Guid("1CB4A20A-3817-454A-9D14-470E881730CA");
        public static readonly Guid Radio_Items_Id_DefId = new Guid("36A30E69-2A51-4594-B694-E716CE802216");
        public static readonly Guid Radio_Items_Enum_Id_DefId = new Guid("F82B4CE7-C2A4-4657-BA71-7EF7D4945D88");
        public static readonly Guid Report_Bands_Id_DefId = new Guid("277DEDA9-2527-4087-8C9D-F49372B46B99");
        public static readonly Guid Report_Editors_Id_DefId = new Guid("D3085469-45C4-4C2B-8A90-BA6E4ED32DC1");
        public static readonly Guid Report_Editors_Attribute_Id_DefId = new Guid("3666A4B2-F223-47B5-AE56-40AC079A9FDB");
        public static readonly Guid Report_Editors_Summary_Id_DefId = new Guid("22425C1A-255B-4448-954E-5832349BEDF8");
        public static readonly Guid Report_Editors_Width_DefId = new Guid("95D7508E-8D45-4325-BB01-B4358A609742");
        public static readonly Guid Report_Section_Types_Id_DefId = new Guid("7C19034E-DAC4-4025-BFE8-22715C1D65A5");
        public static readonly Guid Report_Section_Types_Name_DefId = new Guid("7E2087BD-3178-4120-ADB0-A9B2BF657D72");
        public static readonly Guid Report_Sections_Id_DefId = new Guid("D4C80E85-BBD8-4222-8ECE-90622D7EBA33");
        public static readonly Guid Report_Sections_Type_Id_DefId = new Guid("C441A343-56F3-42E4-BAEB-6AE8B3BA479D");
        public static readonly Guid Report_Summary_Types_Id_DefId = new Guid("66FB63E7-32D2-4BFE-97D6-DBB608F89EA7");
        public static readonly Guid Report_Summary_Types_Name_DefId = new Guid("CE9D1C95-5099-4F8F-8EC5-B2202E3F3930");
        public static readonly Guid Report_Texts_Id_DefId = new Guid("1D785F2C-EBCA-4486-A4AC-7F9B4CD5113D");
        public static readonly Guid Reports_Id_DefId = new Guid("BC390383-7FFE-4483-A575-99A4BCD33F18");
        public static readonly Guid Reports_Document_Id_DefId = new Guid("0764A0F6-0E23-4405-BE76-74D7AD08AF94");
        public static readonly Guid Role_Permissions_Permission_Id_DefId = new Guid("8CB55520-839E-4A6E-B4EC-4FEE9A674354");
        public static readonly Guid Role_Permissions_Role_Id_DefId = new Guid("0E4A31AC-9F54-4939-A718-BD0050279966");
        public static readonly Guid Role_Refs_Role_Id_DefId = new Guid("43C9C317-5F10-48F8-A45E-40FB5B341996");
        public static readonly Guid Role_Refs_Def_Id_DefId = new Guid("31777D5D-2E7B-43E5-88C2-A761C3001F3E");
        public static readonly Guid Role_Refs_Is_Disabled_DefId = new Guid("97B60588-4CE2-463D-ABBF-4C050ACC517E");
        public static readonly Guid Roles_Id_DefId = new Guid("867EBEA4-69F1-4096-89B7-89402363F72C");
        public static readonly Guid Rules_Id_DefId = new Guid("384067DF-CC17-4702-B83E-9CF2187D1F56");
        public static readonly Guid Rules_Script_DefId = new Guid("C33F456C-5002-4BC8-A597-D1B48477DDBB");
        public static readonly Guid Script_Activities_Script_DefId = new Guid("C8A8EC4C-992D-48E6-8D18-88C0DAEFDA54");
        public static readonly Guid Script_Activities_Id_DefId = new Guid("16A85275-012A-4194-9748-566081792348");
        public static readonly Guid Scripts_Id_DefId = new Guid("86F4A98A-A678-42DB-9B74-D17DBFCE86D7");
        public static readonly Guid Scripts_Script_Text_DefId = new Guid("353E2968-4570-4FBC-8CF2-7B27F3ADF224");
        public static readonly Guid Security_Objects_Name_DefId = new Guid("40148BD3-0547-46F3-90A5-83941260387E");
        public static readonly Guid Security_Objects_Id_DefId = new Guid("6BD4DA8C-C752-4D09-A4A8-E0931ED086E8");
        public static readonly Guid Start_Activities_Id_DefId = new Guid("A1B8190A-E7A4-42D5-8230-52D18B18B42C");
        public static readonly Guid Subjects_Id_DefId = new Guid("0C63A3CC-A900-4517-AC77-4012560D35EF");
        public static readonly Guid Subjects_Address_DefId = new Guid("820148DD-F675-4EE3-9660-636323D681D1");
        public static readonly Guid Subjects_Email_DefId = new Guid("4551C973-5AFF-465B-A61B-5981BDDAEFDF");
        public static readonly Guid Subjects_Phone_DefId = new Guid("D029AF37-06A3-4C07-A02A-28E2448384EE");
        public static readonly Guid Tab_Controls_Id_DefId = new Guid("02076C38-0000-41D5-94DA-164E864F913B");
        public static readonly Guid Table_Columns_Id_DefId = new Guid("ABFFFAB7-7CC7-4641-9283-223FFF21A12F");
        public static readonly Guid Table_Columns_Attribute_Id_DefId = new Guid("AADBF5B2-E5D3-47A0-8702-930582BBAED0");
        public static readonly Guid Table_Columns_Attribute_Name_DefId = new Guid("83FE6AED-1239-41AC-A50D-2FC3C4D7B4E7");
        public static readonly Guid Table_Forms_Can_Delete_DefId = new Guid("91214E34-8BB9-411A-A058-F75E4618053C");
        public static readonly Guid Table_Forms_Can_Edit_DefId = new Guid("7D9393F8-D531-4CB1-AE4B-D63EDC8AF3D4");
        public static readonly Guid Table_Forms_Can_Add_New_DefId = new Guid("82E80112-888C-4E65-ACDD-7CECE55C0B25");
        public static readonly Guid Table_Forms_Add_New_Permission_Id_DefId = new Guid("1753EFD6-A0A5-46BB-A3CC-E9E0F16A52F0");
        public static readonly Guid Table_Forms_Def_Data_DefId = new Guid("67218AF1-21F4-431F-B963-B8E3D99C6A11");
        public static readonly Guid Table_Forms_Document_Id_DefId = new Guid("1885C525-7443-460B-B81B-0AD0538DA042");
        public static readonly Guid Table_Forms_Form_Id_DefId = new Guid("E3578DD6-3494-4442-9033-354DDE64C11A");
        public static readonly Guid Table_Forms_Filter_Form_Id_DefId = new Guid("56F86B0C-0EF3-4DE9-87D0-3DD1FDDB5833");
        public static readonly Guid Table_Forms_Id_DefId = new Guid("D06F7FDB-F790-4098-8C4C-95C32BF55824");
        public static readonly Guid Table_Forms_Open_Permission_Id_DefId = new Guid("3316A410-C01B-4151-9CBC-852E08C53409");
        public static readonly Guid Table_Forms_Script_DefId = new Guid("A544CE29-7202-4482-90D8-0B4B2E5B6D41");
        public static readonly Guid Table_Reports_Is_Portret_DefId = new Guid("5907AD50-0FBE-4306-8A60-4C454F5CBC94");
        public static readonly Guid Table_Reports_Id_DefId = new Guid("94AFAFF4-2B26-41E8-8AFC-A4B94DE26907");
        public static readonly Guid Table_Reports_Def_Data_DefId = new Guid("A7606E9C-0CE2-4C23-A7FE-B6CB85DC6878");
        public static readonly Guid Texts_Id_DefId = new Guid("418B9ED3-F217-4354-9F44-98D9512E53C9");
        public static readonly Guid Titles_Id_DefId = new Guid("FA2799C5-B06A-45C9-B41E-1F9AC6537104");
        public static readonly Guid Titles_Name_DefId = new Guid("A882F6E6-0B91-40BB-99D0-D0286F4188FA");
        public static readonly Guid User_Actions_Id_DefId = new Guid("7D852831-B233-4AA8-AB29-8B41650D9D0D");
        public static readonly Guid Worker_Roles_Worker_Id_DefId = new Guid("A8E19480-EBD2-4646-B59B-E9084A7361E8");
        public static readonly Guid Worker_Roles_Role_Id_DefId = new Guid("49EDED59-ABE0-4268-ADED-11453511A336");
        public static readonly Guid Workers_OrgPosition_Id_DefId = new Guid("E4697C44-F598-45EC-B203-AC90D299D89A");
        public static readonly Guid Workers_User_Name_DefId = new Guid("DB303A92-0F6F-426F-86EF-BEFCD7CE2525");
        public static readonly Guid Workers_User_Password_DefId = new Guid("9384DFD8-E12F-4419-8CA7-D21C4E2DCF28");
        public static readonly Guid Workers_Language_Id_DefId = new Guid("6961CC25-36AE-44EE-8AE2-EB0CF7F3FBEE");
        public static readonly Guid Workers_Id_DefId = new Guid("1023BB5F-79A7-4EE8-AB9D-A884070A0B7B");
        public static readonly Guid Workflow_Activities_Id_DefId = new Guid("4E7491DA-F424-4DC4-BA46-D085CCAC3829");
        public static readonly Guid Workflow_Activities_Type_Id_DefId = new Guid("8277C975-CA75-44E5-94E8-56471540C29A");
        public static readonly Guid Workflow_Def_Rules_Workflow_Id_DefId = new Guid("034B5E41-0D8B-4465-B0DC-670C1D64994D");
        public static readonly Guid Workflow_Def_Rules_Rule_Id_DefId = new Guid("F92FA8A6-094A-4686-9FA5-02D9AD9DA51A");
        public static readonly Guid Workflow_Defs_Id_DefId = new Guid("7CF239C4-4DA2-4C81-B30F-4CB5B04E50AE");
        public static readonly Guid Workflow_Processes_Id_DefId = new Guid("1807A733-E6D5-431A-A7E6-23FDC735FE11");
        public static readonly Guid Workflow_Processes_Diagram_Data_DefId = new Guid("CC573308-8892-4543-9170-D74318B2F2F0");
        public static readonly Guid Workflow_Processes_Script_DefId = new Guid("57D500C6-3059-4DEB-90E6-5B83BB69247D");

        private static readonly List<DocDef> _metaobjectDocDefs = new List<DocDef>();
        public static List<DocDef> GetMetaobjectDocDefs()
        {
            if (_metaobjectDocDefs.Count > 0) return _metaobjectDocDefs;

            var list = _metaobjectDocDefs;

            // Creation of the Metaobjects DocDef
            var Activity_Links = new DocDef
            {
                Id = Activity_Links_DefId,
                Name = "Activity_Links",
                Caption = "Activity_Links",
                Attributes = new List<AttrDef>()
            };
            list.Add(Activity_Links);
            var Attribute_Defs = new DocDef
            {
                Id = Attribute_Defs_DefId,
                AncestorId = Object_Defs_DefId,
                Name = "Attribute_Defs",
                Caption = "Attribute_Defs",
                Attributes = new List<AttrDef>()
            };
            list.Add(Attribute_Defs);
            var Buttons = new DocDef
            {
                Id = Buttons_DefId,
                Name = "Buttons",
                Caption = "Buttons",
                Attributes = new List<AttrDef>()
            };
            list.Add(Buttons);
            var Combo_Boxes = new DocDef
            {
                Id = Combo_Boxes_DefId,
                Name = "Combo_Boxes",
                Caption = "Combo_Boxes",
                Attributes = new List<AttrDef>()
            };
            list.Add(Combo_Boxes);
            var Compare_Operations = new DocDef
            {
                Id = Compare_Operations_DefId,
                Name = "Compare_Operations",
                Caption = "Compare_Operations",
                Attributes = new List<AttrDef>()
            };
            list.Add(Compare_Operations);
            var Controls = new DocDef
            {
                Id = Controls_DefId,
                Name = "Controls",
                Caption = "Controls",
                Attributes = new List<AttrDef>()
            };
            list.Add(Controls);
            var Data_Transfer_Doc_Errors = new DocDef
            {
                Id = Data_Transfer_Doc_Errors_DefId,
                Name = "Data_Transfer_Doc_Errors",
                Caption = "Data_Transfer_Doc_Errors",
                Attributes = new List<AttrDef>()
            };
            list.Add(Data_Transfer_Doc_Errors);
            var Data_Transfer_Operations = new DocDef
            {
                Id = Data_Transfer_Operations_DefId,
                Name = "Data_Transfer_Operations",
                Caption = "Data_Transfer_Operations",
                Attributes = new List<AttrDef>()
            };
            list.Add(Data_Transfer_Operations);
            var Data_Transfers = new DocDef
            {
                Id = Data_Transfers_DefId,
                Name = "Data_Transfers",
                Caption = "Data_Transfers",
                Attributes = new List<AttrDef>()
            };
            list.Add(Data_Transfers);
            var Data_Types = new DocDef
            {
                Id = Data_Types_DefId,
                Name = "Data_Types",
                Caption = "Data_Types",
                Attributes = new List<AttrDef>()
            };
            list.Add(Data_Types);
            var Document_Activities = new DocDef
            {
                Id = Document_Activities_DefId,
                Name = "Document_Activities",
                Caption = "Document_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_Activities);
            var Document_Attributes = new DocDef
            {
                Id = Document_Attributes_DefId,
                Name = "Document_Attributes",
                Caption = "Document_Attributes",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_Attributes);
            var Document_Defs = new DocDef
            {
                Id = Document_Defs_DefId,
                AncestorId = Object_Defs_DefId,
                Name = "Document_Defs",
                Caption = "Document_Defs",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_Defs);
            var Document_Route_Transits = new DocDef
            {
                Id = Document_Route_Transits_DefId,
                Name = "Document_Route_Transits",
                Caption = "Document_Route_Transits",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_Route_Transits);
            var Document_Routes = new DocDef
            {
                Id = Document_Routes_DefId,
                Name = "Document_Routes",
                Caption = "Document_Routes",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_Routes);
            var Document_State_Activities = new DocDef
            {
                Id = Document_State_Activities_DefId,
                Name = "Document_State_Activities",
                Caption = "Document_State_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_State_Activities);
            var Document_State_Types = new DocDef
            {
                Id = Document_State_Types_DefId,
                Name = "Document_State_Types",
                Caption = "Document_State_Types",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_State_Types);
            var Document_Visibilities = new DocDef
            {
                Id = Document_Visibilities_DefId,
                Name = "Document_Visibilities",
                Caption = "Document_Visibilities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Document_Visibilities);
            var DocumentActivityType = new DocDef
            {
                Id = DocumentActivityType_DefId,
                Name = "DocumentActivityType",
                Caption = "DocumentActivityType",
                Attributes = new List<AttrDef>()
            };
            list.Add(DocumentActivityType);
            var DocumentControl = new DocDef
            {
                Id = DocumentControl_DefId,
                Name = "DocumentControl",
                Caption = "DocumentControl",
                Attributes = new List<AttrDef>()
            };
            list.Add(DocumentControl);
            var DocumentList_Forms = new DocDef
            {
                Id = DocumentList_Forms_DefId,
                Name = "DocumentList_Forms",
                Caption = "DocumentList_Forms",
                Attributes = new List<AttrDef>()
            };
            list.Add(DocumentList_Forms);
            var DynamicDocumentList = new DocDef
            {
                Id = DynamicDocumentList_DefId,
                Name = "DynamicDocumentList",
                Caption = "DynamicDocumentList",
                Attributes = new List<AttrDef>()
            };
            list.Add(DynamicDocumentList);
            var Editors = new DocDef
            {
                Id = Editors_DefId,
                Name = "Editors",
                Caption = "Editors",
                Attributes = new List<AttrDef>()
            };
            list.Add(Editors);
            var Enum_Defs = new DocDef
            {
                Id = Enum_Defs_DefId,
                Name = "Enum_Defs",
                Caption = "Enum_Defs",
                Attributes = new List<AttrDef>()
            };
            list.Add(Enum_Defs);
            var Enum_Items = new DocDef
            {
                Id = Enum_Items_DefId,
                Name = "Enum_Items",
                Caption = "Enum_Items",
                Attributes = new List<AttrDef>()
            };
            list.Add(Enum_Items);
            var File_Templates = new DocDef
            {
                Id = File_Templates_DefId,
                Name = "File_Templates",
                Caption = "File_Templates",
                Attributes = new List<AttrDef>()
            };
            list.Add(File_Templates);
            var Finish_Activities = new DocDef
            {
                Id = Finish_Activities_DefId,
                Name = "Finish_Activities",
                Caption = "Finish_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Finish_Activities);
            var Folder_Defs = new DocDef
            {
                Id = Folder_Defs_DefId,
                Name = "Folder_Defs",
                Caption = "Folder_Defs",
                Attributes = new List<AttrDef>()
            };
            list.Add(Folder_Defs);
            var Forms = new DocDef
            {
                Id = Forms_DefId,
                Name = "Forms",
                Caption = "Forms",
                Attributes = new List<AttrDef>()
            };
            list.Add(Forms);
            var Generators = new DocDef
            {
                Id = Generators_DefId,
                Name = "Generators",
                Caption = "Generators",
                Attributes = new List<AttrDef>()
            };
            list.Add(Generators);
            var Grids = new DocDef
            {
                Id = Grids_DefId,
                Name = "Grids",
                Caption = "Grids",
                Attributes = new List<AttrDef>()
            };
            list.Add(Grids);
            var Images = new DocDef
            {
                Id = Images_DefId,
                Name = "Images",
                Caption = "Images",
                Attributes = new List<AttrDef>()
            };
            list.Add(Images);
            var Languages = new DocDef
            {
                Id = Languages_DefId,
                Name = "Languages",
                Caption = "Languages",
                Attributes = new List<AttrDef>()
            };
            list.Add(Languages);
            var Layout_Types = new DocDef
            {
                Id = Layout_Types_DefId,
                Name = "Layout_Types",
                Caption = "Layout_Types",
                Attributes = new List<AttrDef>()
            };
            list.Add(Layout_Types);
            var Menus = new DocDef
            {
                Id = Menus_DefId,
                Name = "Menus",
                Caption = "Menus",
                Attributes = new List<AttrDef>()
            };
            list.Add(Menus);
            var Object_Def_Translations = new DocDef
            {
                Id = Object_Def_Translations_DefId,
                Name = "Object_Def_Translations",
                Caption = "Object_Def_Translations",
                Attributes = new List<AttrDef>()
            };
            list.Add(Object_Def_Translations);
            var Object_Defs = new DocDef
            {
                Id = Object_Defs_DefId,
                Name = "Object_Defs",
                Caption = "Object_Defs",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Object_Defs);
            var Org_Models = new DocDef
            {
                Id = Org_Models_DefId,
                Name = "Org_Models",
                Caption = "Org_Models",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Org_Models);
            var Org_Positions = new DocDef
            {
                Id = Org_Positions_DefId,
                Name = "Org_Positions",
                Caption = "Org_Positions",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Org_Positions);
            var Org_Positions_Roles = new DocDef
            {
                Id = Org_Positions_Roles_DefId,
                Name = "Org_Positions_Roles",
                Caption = "Org_Positions_Roles",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Org_Positions_Roles);
            var Org_Unit_Types = new DocDef
            {
                Id = Org_Unit_Types_DefId,
                Name = "Org_Unit_Types",
                Caption = "Org_Unit_Types",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Org_Unit_Types);
            var Org_Units = new DocDef
            {
                Id = Org_Units_DefId,
                Name = "Org_Units",
                Caption = "Org_Units",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Org_Units);
            var Org_Units_Roles = new DocDef
            {
                Id = Org_Units_Roles_DefId,
                Name = "Org_Units_Roles",
                Caption = "Org_Units_Roles",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Org_Units_Roles);
            var Organizations = new DocDef
            {
                Id = Organizations_DefId,
                AncestorId = Subjects_DefId,
                Name = "Organizations",
                Caption = "Organizations",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Organizations);
            var OrgUnits_ObjectDefs = new DocDef
            {
                Id = OrgUnits_ObjectDefs_DefId,
                Name = "OrgUnits_ObjectDefs",
                Caption = "OrgUnits_ObjectDefs",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(OrgUnits_ObjectDefs);
            var Panels = new DocDef
            {
                Id = Panels_DefId,
                Name = "Panels",
                Caption = "Panels",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Panels);
            var Permission_Defs = new DocDef
            {
                Id = Permission_Defs_DefId,
                Name = "Permission_Defs",
                Caption = "Permission_Defs",
                Attributes = new List<AttrDef>()
            };
            list.Add(Permission_Defs);
            var Permissions = new DocDef
            {
                Id = Permissions_DefId,
                Name = "Permissions",
                Caption = "Permissions",
                Attributes = new List<AttrDef>()
            };
            list.Add(Permissions);
            var Persons = new DocDef
            {
                Id = Persons_DefId,
                AncestorId = Subjects_DefId,
                Name = "Persons",
                Caption = "Persons",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Persons);
            var Presentation_Activities = new DocDef
            {
                Id = Presentation_Activities_DefId,
                Name = "Presentation_Activities",
                Caption = "Presentation_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Presentation_Activities);
            var Process_Call_Activities = new DocDef
            {
                Id = Process_Call_Activities_DefId,
                Name = "Process_Call_Activities",
                Caption = "Process_Call_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Process_Call_Activities);
            var Project_Folders = new DocDef
            {
                Id = Project_Folders_DefId,
                Name = "Project_Folders",
                Caption = "Project_Folders",
                Attributes = new List<AttrDef>()
            };
            list.Add(Project_Folders);
            var Project_Items = new DocDef
            {
                Id = Project_Items_DefId,
                Name = "Project_Items",
                Caption = "Project_Items",
                Attributes = new List<AttrDef>()
            };
            list.Add(Project_Items);
            var Projects = new DocDef
            {
                Id = Projects_DefId,
                Name = "Projects",
                Caption = "Projects",
                Attributes = new List<AttrDef>()
            };
            list.Add(Projects);
            var Radio_Items = new DocDef
            {
                Id = Radio_Items_DefId,
                Name = "Radio_Items",
                Caption = "Radio_Items",
                Attributes = new List<AttrDef>()
            };
            list.Add(Radio_Items);
            var Report_Bands = new DocDef
            {
                Id = Report_Bands_DefId,
                Name = "Report_Bands",
                Caption = "Report_Bands",
                Attributes = new List<AttrDef>()
            };
            list.Add(Report_Bands);
            var Report_Editors = new DocDef
            {
                Id = Report_Editors_DefId,
                Name = "Report_Editors",
                Caption = "Report_Editors",
                Attributes = new List<AttrDef>()
            };
            list.Add(Report_Editors);
            var Report_Section_Types = new DocDef
            {
                Id = Report_Section_Types_DefId,
                Name = "Report_Section_Types",
                Caption = "Report_Section_Types",
                Attributes = new List<AttrDef>()
            };
            list.Add(Report_Section_Types);
            var Report_Sections = new DocDef
            {
                Id = Report_Sections_DefId,
                Name = "Report_Sections",
                Caption = "Report_Sections",
                Attributes = new List<AttrDef>()
            };
            list.Add(Report_Sections);
            var Report_Summary_Types = new DocDef
            {
                Id = Report_Summary_Types_DefId,
                Name = "Report_Summary_Types",
                Caption = "Report_Summary_Types",
                Attributes = new List<AttrDef>()
            };
            list.Add(Report_Summary_Types);
            var Report_Texts = new DocDef
            {
                Id = Report_Texts_DefId,
                Name = "Report_Texts",
                Caption = "Report_Texts",
                Attributes = new List<AttrDef>()
            };
            list.Add(Report_Texts);
            var Reports = new DocDef
            {
                Id = Reports_DefId,
                Name = "Reports",
                Caption = "Reports",
                Attributes = new List<AttrDef>()
            };
            list.Add(Reports);
            var Role_Permissions = new DocDef
            {
                Id = Role_Permissions_DefId,
                Name = "Role_Permissions",
                Caption = "Role_Permissions",
                Attributes = new List<AttrDef>()
            };
            list.Add(Role_Permissions);
            var Role_Refs = new DocDef
            {
                Id = Role_Refs_DefId,
                Name = "Role_Refs",
                Caption = "Role_Refs",
                Attributes = new List<AttrDef>()
            };
            list.Add(Role_Refs);
            var Roles = new DocDef
            {
                Id = Roles_DefId,
                Name = "Roles",
                Caption = "Roles",
                Attributes = new List<AttrDef>()
            };
            list.Add(Roles);
            var Rules = new DocDef
            {
                Id = Rules_DefId,
                Name = "Rules",
                Caption = "Rules",
                Attributes = new List<AttrDef>()
            };
            list.Add(Rules);
            var Script_Activities = new DocDef
            {
                Id = Script_Activities_DefId,
                Name = "Script_Activities",
                Caption = "Script_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Script_Activities);
            var Scripts = new DocDef
            {
                Id = Scripts_DefId,
                Name = "Scripts",
                Caption = "Scripts",
                Attributes = new List<AttrDef>()
            };
            list.Add(Scripts);
            var Security_Objects = new DocDef
            {
                Id = Security_Objects_DefId,
                Name = "Security_Objects",
                Caption = "Security_Objects",
                Attributes = new List<AttrDef>()
            };
            list.Add(Security_Objects);
            var Start_Activities = new DocDef
            {
                Id = Start_Activities_DefId,
                Name = "Start_Activities",
                Caption = "Start_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Start_Activities);
            var Subjects = new DocDef
            {
                Id = Subjects_DefId,
                AncestorId = Object_Defs_DefId,
                Name = "Subjects",
                Caption = "Subjects",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Subjects);
            var Tab_Controls = new DocDef
            {
                Id = Tab_Controls_DefId,
                Name = "Tab_Controls",
                Caption = "Tab_Controls",
                Attributes = new List<AttrDef>()
            };
            list.Add(Tab_Controls);
            var Table_Columns = new DocDef
            {
                Id = Table_Columns_DefId,
                Name = "Table_Columns",
                Caption = "Table_Columns",
                Attributes = new List<AttrDef>()
            };
            list.Add(Table_Columns);
            var Table_Forms = new DocDef
            {
                Id = Table_Forms_DefId,
                Name = "Table_Forms",
                Caption = "Table_Forms",
                Attributes = new List<AttrDef>()
            };
            list.Add(Table_Forms);
            var Table_Reports = new DocDef
            {
                Id = Table_Reports_DefId,
                Name = "Table_Reports",
                Caption = "Table_Reports",
                Attributes = new List<AttrDef>()
            };
            list.Add(Table_Reports);
            var Texts = new DocDef
            {
                Id = Texts_DefId,
                Name = "Texts",
                Caption = "Texts",
                Attributes = new List<AttrDef>()
            };
            list.Add(Texts);
            var Titles = new DocDef
            {
                Id = Titles_DefId,
                Name = "Titles",
                Caption = "Titles",
                Attributes = new List<AttrDef>()
            };
            list.Add(Titles);
            var User_Actions = new DocDef
            {
                Id = User_Actions_DefId,
                Name = "User_Actions",
                Caption = "User_Actions",
                Attributes = new List<AttrDef>()
            };
            list.Add(User_Actions);
            var Worker_Roles = new DocDef
            {
                Id = Worker_Roles_DefId,
                Name = "Worker_Roles",
                Caption = "Worker_Roles",
                Attributes = new List<AttrDef>()
            };
            list.Add(Worker_Roles);
            var Workers = new DocDef
            {
                Id = Workers_DefId,
                AncestorId = Persons_DefId,
                Name = "Workers",
                Caption = "Workers",
                IsPublic = true,
                Attributes = new List<AttrDef>()
            };
            list.Add(Workers);
            var Workflow_Activities = new DocDef
            {
                Id = Workflow_Activities_DefId,
                Name = "Workflow_Activities",
                Caption = "Workflow_Activities",
                Attributes = new List<AttrDef>()
            };
            list.Add(Workflow_Activities);
            var Workflow_Def_Rules = new DocDef
            {
                Id = Workflow_Def_Rules_DefId,
                Name = "Workflow_Def_Rules",
                Caption = "Workflow_Def_Rules",
                Attributes = new List<AttrDef>()
            };
            list.Add(Workflow_Def_Rules);
            var Workflow_Defs = new DocDef
            {
                Id = Workflow_Defs_DefId,
                Name = "Workflow_Defs",
                Caption = "Workflow_Defs",
                Attributes = new List<AttrDef>()
            };
            list.Add(Workflow_Defs);
            var Workflow_Processes = new DocDef
            {
                Id = Workflow_Processes_DefId,
                Name = "Workflow_Processes",
                Caption = "Workflow_Processes",
                Attributes = new List<AttrDef>()
            };
            list.Add(Workflow_Processes);

            // Set DocumentDef inheritence
            Controls.AncestorId = Object_Defs.Id;
            Document_Route_Transits.AncestorId = Object_Defs.Id;
            Document_Routes.AncestorId = Object_Defs.Id;
            Document_State_Types.AncestorId = Object_Defs.Id;
            DocumentControl.AncestorId = Controls.Id;
            DocumentList_Forms.AncestorId = Controls.Id;
            DynamicDocumentList.AncestorId = Controls.Id;
            Enum_Items.AncestorId = Object_Defs.Id;
            File_Templates.AncestorId = Object_Defs.Id;
            Finish_Activities.AncestorId = Workflow_Activities.Id;
            Folder_Defs.AncestorId = Object_Defs.Id;
            Menus.AncestorId = Controls.Id;
            Org_Models.AncestorId = Object_Defs.Id;
            Org_Positions.AncestorId = Subjects.Id;
            Org_Units.AncestorId = Subjects.Id;
            Organizations.AncestorId = Subjects.Id;
            Permissions.AncestorId = Security_Objects.Id;
            Persons.AncestorId = Subjects.Id;
            Project_Items.AncestorId = Object_Defs.Id;
            Radio_Items.AncestorId = Controls.Id;
            Report_Bands.AncestorId = Controls.Id;
            Report_Editors.AncestorId = Controls.Id;
            Report_Sections.AncestorId = Controls.Id;
            Report_Texts.AncestorId = Controls.Id;
            Roles.AncestorId = Security_Objects.Id;
            Script_Activities.AncestorId = Workflow_Activities.Id;
            Scripts.AncestorId = Object_Defs.Id;
            Security_Objects.AncestorId = Object_Defs.Id;
            Start_Activities.AncestorId = Workflow_Activities.Id;
            Subjects.AncestorId = Object_Defs.Id;
            Tab_Controls.AncestorId = Controls.Id;
            Table_Columns.AncestorId = Controls.Id;
            Table_Forms.AncestorId = Controls.Id;
            Table_Reports.AncestorId = Reports.Id;
            Workers.AncestorId = Persons.Id;
            Workflow_Defs.AncestorId = Object_Defs.Id;
            Panels.AncestorId = Controls.Id;
            Workflow_Activities.AncestorId = Workflow_Defs.Id;
            Document_Activities.AncestorId = Workflow_Activities.Id;
            Presentation_Activities.AncestorId = Workflow_Activities.Id;
            Document_State_Activities.AncestorId = Workflow_Activities.Id;
            Activity_Links.AncestorId = Workflow_Defs.Id;
            User_Actions.AncestorId = Workflow_Defs.Id;
            Process_Call_Activities.AncestorId = Workflow_Activities.Id;
            Document_Defs.AncestorId = Object_Defs.Id;
            Editors.AncestorId = Controls.Id;
            Forms.AncestorId = Controls.Id;
            Combo_Boxes.AncestorId = Controls.Id;
            Attribute_Defs.AncestorId = Object_Defs.Id;
            Buttons.AncestorId = Controls.Id;
            Texts.AncestorId = Controls.Id;
            Images.AncestorId = Controls.Id;
            Grids.AncestorId = Controls.Id;
            Reports.AncestorId = Controls.Id;
            Enum_Defs.AncestorId = Object_Defs.Id;
            Projects.AncestorId = Project_Items.Id;
            Project_Folders.AncestorId = Project_Items.Id;
            Workflow_Processes.AncestorId = Workflow_Defs.Id;


            // Creation of metaobject attribute defs
            Activity_Links.Attributes.Add(new AttrDef
            {
                Id = Activity_Links_Condition_DefId,
                Name = "Condition",
                Caption = "Condition",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Activity_Links.Attributes.Add(new AttrDef
            {
                Id = Activity_Links_Is_Exception_Handler_DefId,
                Name = "Is_Exception_Handler",
                Caption = "Is_Exception_Handler",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Activity_Links.Attributes.Add(new AttrDef
            {
                Id = Activity_Links_Source_Id_DefId,
                Name = "Source_Id",
                Caption = "Source_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Activity_Links.Attributes.Add(new AttrDef
            {
                Id = Activity_Links_Target_Id_DefId,
                Name = "Target_Id",
                Caption = "Target_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Activity_Links.Attributes.Add(new AttrDef
            {
                Id = Activity_Links_User_Action_Id_DefId,
                Name = "User_Action_Id",
                Caption = "User_Action_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_BlobIsImage_DefId,
                Name = "BlobIsImage",
                Caption = "BlobIsImage",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_BlobMaxHeight_DefId,
                Name = "BlobMaxHeight",
                Caption = "BlobMaxHeight",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_BlobMaxSizeBytes_DefId,
                Name = "BlobMaxSizeBytes",
                Caption = "BlobMaxSizeBytes",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_BlobMaxWidth_DefId,
                Name = "BlobMaxWidth",
                Caption = "BlobMaxWidth",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_CalculateScript_DefId,
                Name = "CalculateScript",
                Caption = "CalculateScript",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Default_Value_DefId,
                Name = "Default_Value",
                Caption = "Default_Value",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Document_Id_DefId,
                Name = "Document_Id",
                Caption = "Document_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Enum_Id_DefId,
                Name = "Enum_Id",
                Caption = "Enum_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Is_Not_Null_DefId,
                Name = "Is_Not_Null",
                Caption = "Is_Not_Null",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Is_Unique_DefId,
                Name = "Is_Unique",
                Caption = "Is_Unique",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Max_Length_DefId,
                Name = "Max_Length",
                Caption = "Max_Length",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Max_Value_DefId,
                Name = "Max_Value",
                Caption = "Max_Value",
                Type = new TypeDef {Id = (short) CissaDataType.Currency, Name = "Currency"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Min_Value_DefId,
                Name = "Min_Value",
                Caption = "Min_Value",
                Type = new TypeDef {Id = (short) CissaDataType.Currency, Name = "Currency"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Org_Type_Id_DefId,
                Name = "Org_Type_Id",
                Caption = "Org_Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Attribute_Defs.Attributes.Add(new AttrDef
            {
                Id = Attribute_Defs_Type_Id_DefId,
                Name = "Type_Id",
                Caption = "Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Buttons.Attributes.Add(new AttrDef
            {
                Id = Buttons_Action_Id_DefId,
                Name = "Action_Id",
                Caption = "Action_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Buttons.Attributes.Add(new AttrDef
            {
                Id = Buttons_Action_Type_DefId,
                Name = "Action_Type",
                Caption = "Action_Type",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Buttons.Attributes.Add(new AttrDef
            {
                Id = Buttons_Button_Type_DefId,
                Name = "Button_Type",
                Caption = "Button_Type",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Buttons.Attributes.Add(new AttrDef
            {
                Id = Buttons_Process_Id_DefId,
                Name = "Process_Id",
                Caption = "Process_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Buttons.Attributes.Add(new AttrDef
            {
                Id = Buttons_User_Action_Id_DefId,
                Name = "User_Action_Id",
                Caption = "User_Action_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Combo_Boxes.Attributes.Add(new AttrDef
            {
                Id = Combo_Boxes_Attribute_Id_DefId,
                Name = "Attribute_Id",
                Caption = "Attribute_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Combo_Boxes.Attributes.Add(new AttrDef
            {
                Id = Combo_Boxes_Attribute_Name_DefId,
                Name = "Attribute_Name",
                Caption = "Attribute_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Combo_Boxes.Attributes.Add(new AttrDef
            {
                Id = Combo_Boxes_Detail_Attribute_Id_DefId,
                Name = "Detail_Attribute_Id",
                Caption = "Detail_Attribute_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Combo_Boxes.Attributes.Add(new AttrDef
            {
                Id = Combo_Boxes_Detail_Attribute_Name_DefId,
                Name = "Detail_Attribute_Name",
                Caption = "Detail_Attribute_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Combo_Boxes.Attributes.Add(new AttrDef
            {
                Id = Combo_Boxes_Is_Radio_DefId,
                Name = "Is_Radio",
                Caption = "Is_Radio",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Combo_Boxes.Attributes.Add(new AttrDef
            {
                Id = Combo_Boxes_Rows_DefId,
                Name = "Rows",
                Caption = "Rows",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Compare_Operations.Attributes.Add(new AttrDef
            {
                Id = Compare_Operations_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_Compare_Operation_DefId,
                Name = "Compare_Operation",
                Caption = "Compare_Operation",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_Invisible_DefId,
                Name = "Invisible",
                Caption = "Invisible",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_Not_Null_DefId,
                Name = "Not_Null",
                Caption = "Not_Null",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_Read_Only_DefId,
                Name = "Read_Only",
                Caption = "Read_Only",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_Sort_Type_DefId,
                Name = "Sort_Type",
                Caption = "Sort_Type",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_Style_DefId,
                Name = "Style",
                Caption = "Style",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_Title_DefId,
                Name = "Title",
                Caption = "Title",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Controls.Attributes.Add(new AttrDef
            {
                Id = Controls_VisibleInListView_DefId,
                Name = "VisibleInListView",
                Caption = "VisibleInListView",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Data_Transfer_Doc_Errors.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Doc_Errors_Document_Id_DefId,
                Name = "Document_Id",
                Caption = "Document_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Data_Transfer_Doc_Errors.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Doc_Errors_Operation_Id_DefId,
                Name = "Operation_Id",
                Caption = "Operation_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Data_Transfer_Doc_Errors.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Doc_Errors_Type_Id_DefId,
                Name = "Type_Id",
                Caption = "Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Data_Transfer_Operations.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Operations_Direction_Id_DefId,
                Name = "Direction_Id",
                Caption = "Direction_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Data_Transfer_Operations.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Operations_Finish_Date_DefId,
                Name = "Finish_Date",
                Caption = "Finish_Date",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Data_Transfer_Operations.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Operations_Source_Date_DefId,
                Name = "Source_Date",
                Caption = "Source_Date",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Data_Transfer_Operations.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Operations_Source_File_DefId,
                Name = "Source_File",
                Caption = "Source_File",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Data_Transfer_Operations.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Operations_Source_Id_DefId,
                Name = "Source_Id",
                Caption = "Source_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Data_Transfer_Operations.Attributes.Add(new AttrDef
            {
                Id = Data_Transfer_Operations_Start_Date_DefId,
                Name = "Start_Date",
                Caption = "Start_Date",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Data_Transfers.Attributes.Add(new AttrDef
            {
                Id = Data_Transfers_Created_DefId,
                Name = "Created",
                Caption = "Created",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Data_Transfers.Attributes.Add(new AttrDef
            {
                Id = Data_Transfers_Def_Id_DefId,
                Name = "Def_Id",
                Caption = "Def_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Data_Transfers.Attributes.Add(new AttrDef
            {
                Id = Data_Transfers_Expired_DefId,
                Name = "Expired",
                Caption = "Expired",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Data_Transfers.Attributes.Add(new AttrDef
            {
                Id = Data_Transfers_Operation_Id_DefId,
                Name = "Operation_Id",
                Caption = "Operation_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Data_Transfers.Attributes.Add(new AttrDef
            {
                Id = Data_Transfers_Value_DefId,
                Name = "Value",
                Caption = "Value",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Data_Types.Attributes.Add(new AttrDef
            {
                Id = Data_Types_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Document_Activities.Attributes.Add(new AttrDef
            {
                Id = Document_Activities_DocActivityTypeId_DefId,
                Name = "DocActivityTypeId",
                Caption = "DocActivityTypeId",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Document_Activities.Attributes.Add(new AttrDef
            {
                Id = Document_Activities_Document_Id_DefId,
                Name = "Document_Id",
                Caption = "Document_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Ancestor_Id_DefId,
                Name = "Ancestor_Id",
                Caption = "Ancestor_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Create_Permission_Id_DefId,
                Name = "Create_Permission_Id",
                Caption = "Create_Permission_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Def_Data_DefId,
                Name = "Def_Data",
                Caption = "Def_Data",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Delete_Permission_Id_DefId,
                Name = "Delete_Permission_Id",
                Caption = "Delete_Permission_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Is_Inline_DefId,
                Name = "Is_Inline",
                Caption = "Is_Inline",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Is_Public_DefId,
                Name = "Is_Public",
                Caption = "Is_Public",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Select_Permission_Id_DefId,
                Name = "Select_Permission_Id",
                Caption = "Select_Permission_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_Update_Permission_Id_DefId,
                Name = "Update_Permission_Id",
                Caption = "Update_Permission_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Defs.Attributes.Add(new AttrDef
            {
                Id = Document_Defs_WithHistory_DefId,
                Name = "WithHistory",
                Caption = "WithHistory",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Document_Route_Transits.Attributes.Add(new AttrDef
            {
                Id = Document_Route_Transits_Permission_Id_DefId,
                Name = "Permission_Id",
                Caption = "Permission_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Route_Transits.Attributes.Add(new AttrDef
            {
                Id = Document_Route_Transits_Source_State_Id_DefId,
                Name = "Source_State_Id",
                Caption = "Source_State_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Route_Transits.Attributes.Add(new AttrDef
            {
                Id = Document_Route_Transits_Target_State_Id_DefId,
                Name = "Target_State_Id",
                Caption = "Target_State_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Routes.Attributes.Add(new AttrDef
            {
                Id = Document_Routes_Document_Def_Id_DefId,
                Name = "Document_Def_Id",
                Caption = "Document_Def_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_State_Activities.Attributes.Add(new AttrDef
            {
                Id = Document_State_Activities_State_Name_DefId,
                Name = "State_Name",
                Caption = "State_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Document_State_Activities.Attributes.Add(new AttrDef
            {
                Id = Document_State_Activities_State_Type_Id_DefId,
                Name = "State_Type_Id",
                Caption = "State_Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_State_Types.Attributes.Add(new AttrDef
            {
                Id = Document_State_Types_Read_Only_DefId,
                Name = "Read_Only",
                Caption = "Read_Only",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Document_Visibilities.Attributes.Add(new AttrDef
            {
                Id = Document_Visibilities_Document_Id_DefId,
                Name = "Document_Id",
                Caption = "Document_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Document_Visibilities.Attributes.Add(new AttrDef
            {
                Id = Document_Visibilities_Organization_Id_DefId,
                Name = "Organization_Id",
                Caption = "Organization_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            DocumentActivityType.Attributes.Add(new AttrDef
            {
                Id = DocumentActivityType_DocActivityTypeId_DefId,
                Name = "DocActivityTypeId",
                Caption = "DocActivityTypeId",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            DocumentActivityType.Attributes.Add(new AttrDef
            {
                Id = DocumentActivityType_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            DocumentControl.Attributes.Add(new AttrDef
            {
                Id = DocumentControl_Attribute_Id_DefId,
                Name = "Attribute_Id",
                Caption = "Attribute_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            DocumentControl.Attributes.Add(new AttrDef
            {
                Id = DocumentControl_Attribute_Name_DefId,
                Name = "Attribute_Name",
                Caption = "Attribute_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            DocumentControl.Attributes.Add(new AttrDef
            {
                Id = DocumentControl_Form_Id_DefId,
                Name = "Form_Id",
                Caption = "Form_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            DocumentList_Forms.Attributes.Add(new AttrDef
            {
                Id = DocumentList_Forms_Attribute_Id_DefId,
                Name = "Attribute_Id",
                Caption = "Attribute_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            DocumentList_Forms.Attributes.Add(new AttrDef
            {
                Id = DocumentList_Forms_Attribute_Name_DefId,
                Name = "Attribute_Name",
                Caption = "Attribute_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            DocumentList_Forms.Attributes.Add(new AttrDef
            {
                Id = DocumentList_Forms_Form_Attribute_Id_DefId,
                Name = "Form_Attribute_Id",
                Caption = "Form_Attribute_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            DocumentList_Forms.Attributes.Add(new AttrDef
            {
                Id = DocumentList_Forms_Form_Id_DefId,
                Name = "Form_Id",
                Caption = "Form_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            DynamicDocumentList.Attributes.Add(new AttrDef
            {
                Id = DynamicDocumentList_Form_Id_DefId,
                Name = "Form_Id",
                Caption = "Form_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            DynamicDocumentList.Attributes.Add(new AttrDef
            {
                Id = DynamicDocumentList_ScriptId_DefId,
                Name = "ScriptId",
                Caption = "ScriptId",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Editors.Attributes.Add(new AttrDef
            {
                Id = Editors_Attribute_Id_DefId,
                Name = "Attribute_Id",
                Caption = "Attribute_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Editors.Attributes.Add(new AttrDef
            {
                Id = Editors_Attribute_Name_DefId,
                Name = "Attribute_Name",
                Caption = "Attribute_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Editors.Attributes.Add(new AttrDef
            {
                Id = Editors_Cols_DefId,
                Name = "Cols",
                Caption = "Cols",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Editors.Attributes.Add(new AttrDef
            {
                Id = Editors_Edit_Mask_DefId,
                Name = "Edit_Mask",
                Caption = "Edit_Mask",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Editors.Attributes.Add(new AttrDef
            {
                Id = Editors_Format_DefId,
                Name = "Format",
                Caption = "Format",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Editors.Attributes.Add(new AttrDef
            {
                Id = Editors_Rows_DefId,
                Name = "Rows",
                Caption = "Rows",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            /*Enum_Items.Attributes.Add(new AttrDef
            {
                Id = Enum_Items_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });*/
            File_Templates.Attributes.Add(new AttrDef
            {
                Id = File_Templates_File_Name_DefId,
                Name = "File_Name",
                Caption = "File_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Finish_Activities.Attributes.Add(new AttrDef
            {
                Id = Finish_Activities_Form_Id_DefId,
                Name = "Form_Id",
                Caption = "Form_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Finish_Activities.Attributes.Add(new AttrDef
            {
                Id = Finish_Activities_Message_DefId,
                Name = "Message",
                Caption = "Message",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Forms.Attributes.Add(new AttrDef
            {
                Id = Forms_Ban_Edit_DefId,
                Name = "Ban_Edit",
                Caption = "Ban_Edit",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Forms.Attributes.Add(new AttrDef
            {
                Id = Forms_Can_Delete_DefId,
                Name = "Can_Delete",
                Caption = "Can_Delete",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Forms.Attributes.Add(new AttrDef
            {
                Id = Forms_Def_Data_DefId,
                Name = "Def_Data",
                Caption = "Def_Data",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Forms.Attributes.Add(new AttrDef
            {
                Id = Forms_Document_Id_DefId,
                Name = "Document_Id",
                Caption = "Document_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Forms.Attributes.Add(new AttrDef
            {
                Id = Forms_Layout_Id_DefId,
                Name = "Layout_Id",
                Caption = "Layout_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Forms.Attributes.Add(new AttrDef
            {
                Id = Forms_Script_DefId,
                Name = "Script",
                Caption = "Script",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Generators.Attributes.Add(new AttrDef
            {
                Id = Generators_Document_Def_Id_DefId,
                Name = "Document_Def_Id",
                Caption = "Document_Def_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Generators.Attributes.Add(new AttrDef
            {
                Id = Generators_Organization_Id_DefId,
                Name = "Organization_Id",
                Caption = "Organization_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Generators.Attributes.Add(new AttrDef
            {
                Id = Generators_Value_DefId,
                Name = "Value",
                Caption = "Value",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Grids.Attributes.Add(new AttrDef
            {
                Id = Grids_Document_Id_DefId,
                Name = "Document_Id",
                Caption = "Document_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Grids.Attributes.Add(new AttrDef
            {
                Id = Grids_Is_Detail_DefId,
                Name = "Is_Detail",
                Caption = "Is_Detail",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Images.Attributes.Add(new AttrDef
            {
                Id = Images_Image_Data_DefId,
                Name = "Image_Data",
                Caption = "Image_Data",
                Type = new TypeDef {Id = (short) CissaDataType.Blob, Name = "Blob"}
            });
            Languages.Attributes.Add(new AttrDef
            {
                Id = Languages_Culture_Name_DefId,
                Name = "Culture_Name",
                Caption = "Culture_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Languages.Attributes.Add(new AttrDef
            {
                Id = Languages_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Layout_Types.Attributes.Add(new AttrDef
            {
                Id = Layout_Types_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Menus.Attributes.Add(new AttrDef
            {
                Id = Menus_Def_Data_DefId,
                Name = "Def_Data",
                Caption = "Def_Data",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Menus.Attributes.Add(new AttrDef
            {
                Id = Menus_Form_Id_DefId,
                Name = "Form_Id",
                Caption = "Form_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Menus.Attributes.Add(new AttrDef
            {
                Id = Menus_Process_Id_DefId,
                Name = "Process_Id",
                Caption = "Process_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Menus.Attributes.Add(new AttrDef
            {
                Id = Menus_State_Type_Id_DefId,
                Name = "State_Type_Id",
                Caption = "State_Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Object_Def_Translations.Attributes.Add(new AttrDef
            {
                Id = Object_Def_Translations_Data_Text_DefId,
                Name = "Data_Text",
                Caption = "Data_Text",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Object_Def_Translations.Attributes.Add(new AttrDef
            {
                Id = Object_Def_Translations_Def_Id_DefId,
                Name = "Def_Id",
                Caption = "Def_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Object_Def_Translations.Attributes.Add(new AttrDef
            {
                Id = Object_Def_Translations_Language_Id_DefId,
                Name = "Language_Id",
                Caption = "Language_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Object_Defs.Attributes.Add(new AttrDef
            {
                Id = Object_Defs_Created_DefId,
                Name = "Created",
                Caption = "Created",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Object_Defs.Attributes.Add(new AttrDef
            {
                Id = Object_Defs_Deleted_DefId,
                Name = "Deleted",
                Caption = "Deleted",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Object_Defs.Attributes.Add(new AttrDef
            {
                Id = Object_Defs_Description_DefId,
                Name = "Description",
                Caption = "Description",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Object_Defs.Attributes.Add(new AttrDef
            {
                Id = Object_Defs_Full_Name_DefId,
                Name = "Full_Name",
                Caption = "Full_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Object_Defs.Attributes.Add(new AttrDef
            {
                Id = Object_Defs_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Object_Defs.Attributes.Add(new AttrDef
            {
                Id = Object_Defs_Order_Index_DefId,
                Name = "Order_Index",
                Caption = "Order_Index",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Object_Defs.Attributes.Add(new AttrDef
            {
                Id = Object_Defs_Parent_Id_DefId,
                Name = "Parent_Id",
                Caption = "Parent_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Org_Models.Attributes.Add(new AttrDef
            {
                Id = Org_Models_Is_Active_DefId,
                Name = "Is_Active",
                Caption = "Is_Active",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            /*Org_Positions.Attributes.Add(new AttrDef
            {
                Id = Org_Positions_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });*/
            Org_Positions.Attributes.Add(new AttrDef
            {
                Id = Org_Positions_Title_Id_DefId,
                Name = "Title_Id",
                Caption = "Title_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Org_Positions_Roles.Attributes.Add(new AttrDef
            {
                Id = Org_Positions_Roles_Org_Position_Id_DefId,
                Name = "Org_Position_Id",
                Caption = "Org_Position_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Org_Positions_Roles.Attributes.Add(new AttrDef
            {
                Id = Org_Positions_Roles_Role_Id_DefId,
                Name = "Role_Id",
                Caption = "Role_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            /*Org_Unit_Types.Attributes.Add(new AttrDef
            {
                Id = Org_Unit_Types_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });*/
            /*Org_Units.Attributes.Add(new AttrDef
            {
                Id = Org_Units_Full_Name_DefId,
                Name = "Full_Name",
                Caption = "Full_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });*/
            Org_Units.Attributes.Add(new AttrDef
            {
                Id = Org_Units_INN_DefId,
                Name = "INN",
                Caption = "INN",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            /*Org_Units.Attributes.Add(new AttrDef
            {
                Id = Org_Units_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });*/
            Org_Units.Attributes.Add(new AttrDef
            {
                Id = Org_Units_Type_Id_DefId,
                Name = "Type_Id",
                Caption = "Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Org_Units_Roles.Attributes.Add(new AttrDef
            {
                Id = Org_Units_Roles_Org_Unit_Id_DefId,
                Name = "Org_Unit_Id",
                Caption = "Org_Unit_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Org_Units_Roles.Attributes.Add(new AttrDef
            {
                Id = Org_Units_Roles_Role_Id_DefId,
                Name = "Role_Id",
                Caption = "Role_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Organizations.Attributes.Add(new AttrDef
            {
                Id = Organizations_Code_DefId,
                Name = "Code",
                Caption = "Code",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Organizations.Attributes.Add(new AttrDef
            {
                Id = Organizations_Type_Id_DefId,
                Name = "Type_Id",
                Caption = "Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"},
                DocDefType = new DocDef
                {
                    Id = MetaobjectDefs.Org_Units_DefId,
                    Name = "Org_Units",
                    Caption= "Org_Unit"
                }
            });
            OrgUnits_ObjectDefs.Attributes.Add(new AttrDef
            {
                Id = OrgUnits_ObjectDefs_ObjDef_Id_DefId,
                Name = "ObjDef_Id",
                Caption = "ObjDef_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            OrgUnits_ObjectDefs.Attributes.Add(new AttrDef
            {
                Id = OrgUnits_ObjectDefs_OrgUnit_Id_DefId,
                Name = "OrgUnit_Id",
                Caption = "OrgUnit_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Panels.Attributes.Add(new AttrDef
            {
                Id = Panels_Is_Horizontal_DefId,
                Name = "Is_Horizontal",
                Caption = "Is_Horizontal",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Panels.Attributes.Add(new AttrDef
            {
                Id = Panels_Layout_Id_DefId,
                Name = "Layout_Id",
                Caption = "Layout_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_Access_Type_DefId,
                Name = "Access_Type",
                Caption = "Access_Type",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_AllowDelete_DefId,
                Name = "AllowDelete",
                Caption = "AllowDelete",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_AllowInsert_DefId,
                Name = "AllowInsert",
                Caption = "AllowInsert",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_AllowSelect_DefId,
                Name = "AllowSelect",
                Caption = "AllowSelect",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_AllowUpdate_DefId,
                Name = "AllowUpdate",
                Caption = "AllowUpdate",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_Def_Id_DefId,
                Name = "Def_Id",
                Caption = "Def_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_Is_Disabled_DefId,
                Name = "Is_Disabled",
                Caption = "Is_Disabled",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Permission_Defs.Attributes.Add(new AttrDef
            {
                Id = Permission_Defs_Permission_Id_DefId,
                Name = "Permission_Id",
                Caption = "Permission_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_Birth_Date_DefId,
                Name = "Birth_Date",
                Caption = "Birth_Date",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_First_Name_DefId,
                Name = "First_Name",
                Caption = "First_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_INN_DefId,
                Name = "INN",
                Caption = "INN",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_Last_Name_DefId,
                Name = "Last_Name",
                Caption = "Last_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_Middle_Name_DefId,
                Name = "Middle_Name",
                Caption = "Middle_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_Passport_Date_DefId,
                Name = "Passport_Date",
                Caption = "Passport_Date",
                Type = new TypeDef {Id = (short) CissaDataType.DateTime, Name = "DateTime"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_Passport_No_DefId,
                Name = "Passport_No",
                Caption = "Passport_No",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_Passport_Org_DefId,
                Name = "Passport_Org",
                Caption = "Passport_Org",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Persons.Attributes.Add(new AttrDef
            {
                Id = Persons_Sex_DefId,
                Name = "Sex",
                Caption = "Sex",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Presentation_Activities.Attributes.Add(new AttrDef
            {
                Id = Presentation_Activities_Form_Id_DefId,
                Name = "Form_Id",
                Caption = "Form_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Presentation_Activities.Attributes.Add(new AttrDef
            {
                Id = Presentation_Activities_Is_Exception_DefId,
                Name = "Is_Exception",
                Caption = "Is_Exception",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Presentation_Activities.Attributes.Add(new AttrDef
            {
                Id = Presentation_Activities_Message_DefId,
                Name = "Message",
                Caption = "Message",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Process_Call_Activities.Attributes.Add(new AttrDef
            {
                Id = Process_Call_Activities_Process_Id_DefId,
                Name = "Process_Id",
                Caption = "Process_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Radio_Items.Attributes.Add(new AttrDef
            {
                Id = Radio_Items_Enum_Id_DefId,
                Name = "Enum_Id",
                Caption = "Enum_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Report_Editors.Attributes.Add(new AttrDef
            {
                Id = Report_Editors_Attribute_Id_DefId,
                Name = "Attribute_Id",
                Caption = "Attribute_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Report_Editors.Attributes.Add(new AttrDef
            {
                Id = Report_Editors_Summary_Id_DefId,
                Name = "Summary_Id",
                Caption = "Summary_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Report_Editors.Attributes.Add(new AttrDef
            {
                Id = Report_Editors_Width_DefId,
                Name = "Width",
                Caption = "Width",
                Type = new TypeDef {Id = (short) CissaDataType.Float, Name = "Float"}
            });
            Report_Section_Types.Attributes.Add(new AttrDef
            {
                Id = Report_Section_Types_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Report_Sections.Attributes.Add(new AttrDef
            {
                Id = Report_Sections_Type_Id_DefId,
                Name = "Type_Id",
                Caption = "Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Report_Summary_Types.Attributes.Add(new AttrDef
            {
                Id = Report_Summary_Types_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Reports.Attributes.Add(new AttrDef
            {
                Id = Reports_Document_Id_DefId,
                Name = "Document_Id",
                Caption = "Document_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Role_Permissions.Attributes.Add(new AttrDef
            {
                Id = Role_Permissions_Permission_Id_DefId,
                Name = "Permission_Id",
                Caption = "Permission_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Role_Permissions.Attributes.Add(new AttrDef
            {
                Id = Role_Permissions_Role_Id_DefId,
                Name = "Role_Id",
                Caption = "Role_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Role_Refs.Attributes.Add(new AttrDef
            {
                Id = Role_Refs_Def_Id_DefId,
                Name = "Def_Id",
                Caption = "Def_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Role_Refs.Attributes.Add(new AttrDef
            {
                Id = Role_Refs_Is_Disabled_DefId,
                Name = "Is_Disabled",
                Caption = "Is_Disabled",
                Type = new TypeDef {Id = (short) CissaDataType.Bool, Name = "Bool"}
            });
            Role_Refs.Attributes.Add(new AttrDef
            {
                Id = Role_Refs_Role_Id_DefId,
                Name = "Role_Id",
                Caption = "Role_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Rules.Attributes.Add(new AttrDef
            {
                Id = Rules_Script_DefId,
                Name = "Script",
                Caption = "Script",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Script_Activities.Attributes.Add(new AttrDef
            {
                Id = Script_Activities_Script_DefId,
                Name = "Script",
                Caption = "Script",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Scripts.Attributes.Add(new AttrDef
            {
                Id = Scripts_Script_Text_DefId,
                Name = "Script_Text",
                Caption = "Script_Text",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Security_Objects.Attributes.Add(new AttrDef
            {
                Id = Security_Objects_Name_DefId,
                Name = "Name",
                Caption = "Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Subjects.Attributes.Add(new AttrDef
            {
                Id = Subjects_Address_DefId,
                Name = "Address",
                Caption = "Address",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Subjects.Attributes.Add(new AttrDef
            {
                Id = Subjects_Email_DefId,
                Name = "Email",
                Caption = "Email",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Subjects.Attributes.Add(new AttrDef
            {
                Id = Subjects_Phone_DefId,
                Name = "Phone",
                Caption = "Phone",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Worker_Roles.Attributes.Add(new AttrDef
            {
                Id = Worker_Roles_Role_Id_DefId,
                Name = "Role_Id",
                Caption = "Role_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Worker_Roles.Attributes.Add(new AttrDef
            {
                Id = Worker_Roles_Worker_Id_DefId,
                Name = "Worker_Id",
                Caption = "Worker_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Workers.Attributes.Add(new AttrDef
            {
                Id = Workers_Language_Id_DefId,
                Name = "Language_Id",
                Caption = "Language_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Workers.Attributes.Add(new AttrDef
            {
                Id = Workers_OrgPosition_Id_DefId,
                Name = "OrgPosition_Id",
                Caption = "OrgPosition_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"},
                DocDefType = new DocDef
                {
                    Id = Org_Positions_DefId,
                    Caption= "Org_Positions",
                    Name = "Org_Positions"
                }
            });
            Workers.Attributes.Add(new AttrDef
            {
                Id = Workers_User_Name_DefId,
                Name = "User_Name",
                Caption = "User_Name",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Workers.Attributes.Add(new AttrDef
            {
                Id = Workers_User_Password_DefId,
                Name = "User_Password",
                Caption = "User_Password",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Workflow_Activities.Attributes.Add(new AttrDef
            {
                Id = Workflow_Activities_Type_Id_DefId,
                Name = "Type_Id",
                Caption = "Type_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Int, Name = "Int"}
            });
            Workflow_Def_Rules.Attributes.Add(new AttrDef
            {
                Id = Workflow_Def_Rules_Rule_Id_DefId,
                Name = "Rule_Id",
                Caption = "Rule_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Workflow_Def_Rules.Attributes.Add(new AttrDef
            {
                Id = Workflow_Def_Rules_Workflow_Id_DefId,
                Name = "Workflow_Id",
                Caption = "Workflow_Id",
                Type = new TypeDef {Id = (short) CissaDataType.Doc, Name = "Doc"}
            });
            Workflow_Processes.Attributes.Add(new AttrDef
            {
                Id = Workflow_Processes_Diagram_Data_DefId,
                Name = "Diagram_Data",
                Caption = "Diagram_Data",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });
            Workflow_Processes.Attributes.Add(new AttrDef
            {
                Id = Workflow_Processes_Script_DefId,
                Name = "Script",
                Caption = "Script",
                Type = new TypeDef {Id = (short) CissaDataType.Text, Name = "Text"}
            });

            return list;
        }

        public static List<DocumentTableMap> GetMetaobjectTableMaps()
        {
            var list = new List<DocumentTableMap>();

            // DocumentTableMaps
            var Activity_Links = new DocumentTableMap(Activity_Links_DefId, "Activity_Links") {IsVirtual = true };
            list.Add(Activity_Links);
            var Attribute_Defs = new DocumentTableMap(Attribute_Defs_DefId, "Attribute_Defs") { IsVirtual = true };
            list.Add(Attribute_Defs);
            var Buttons = new DocumentTableMap(Buttons_DefId, "Buttons") { IsVirtual = true };
            list.Add(Buttons);
            var Combo_Boxes = new DocumentTableMap(Combo_Boxes_DefId, "Combo_Boxes") { IsVirtual = true };
            list.Add(Combo_Boxes);
            var Compare_Operations = new DocumentTableMap(Compare_Operations_DefId, "Compare_Operations");
            list.Add(Compare_Operations);
            var Controls = new DocumentTableMap(Controls_DefId, "Controls") { IsVirtual = true };
            list.Add(Controls);
            var Data_Transfer_Doc_Errors = new DocumentTableMap(Data_Transfer_Doc_Errors_DefId,
                "Data_Transfer_Doc_Errors");
            list.Add(Data_Transfer_Doc_Errors);
            var Data_Transfer_Operations = new DocumentTableMap(Data_Transfer_Operations_DefId,
                "Data_Transfer_Operations");
            list.Add(Data_Transfer_Operations);
            var Data_Transfers = new DocumentTableMap(Data_Transfers_DefId, "Data_Transfers");
            list.Add(Data_Transfers);
            var Data_Types = new DocumentTableMap(Data_Types_DefId, "Data_Types");
            list.Add(Data_Types);
            var Document_Activities = new DocumentTableMap(Document_Activities_DefId, "Document_Activities");
            list.Add(Document_Activities);
            var Document_Attributes = new DocumentTableMap(Document_Attributes_DefId, "Document_Attributes");
            list.Add(Document_Attributes);
            var Document_Defs = new DocumentTableMap(Document_Defs_DefId, "Document_Defs");
            list.Add(Document_Defs);
            var Document_Route_Transits = new DocumentTableMap(Document_Route_Transits_DefId, "Document_Route_Transits");
            list.Add(Document_Route_Transits);
            var Document_Routes = new DocumentTableMap(Document_Routes_DefId, "Document_Routes");
            list.Add(Document_Routes);
            var Document_State_Activities = new DocumentTableMap(Document_State_Activities_DefId,
                "Document_State_Activities");
            list.Add(Document_State_Activities);
            var Document_State_Types = new DocumentTableMap(Document_State_Types_DefId, "Document_State_Types");
            list.Add(Document_State_Types);
            var Document_Visibilities = new DocumentTableMap(Document_Visibilities_DefId, "Document_Visibilities");
            list.Add(Document_Visibilities);
            var DocumentActivityType = new DocumentTableMap(DocumentActivityType_DefId, "DocumentActivityType");
            list.Add(DocumentActivityType);
            var DocumentControl = new DocumentTableMap(DocumentControl_DefId, "DocumentControl");
            list.Add(DocumentControl);
            var DocumentList_Forms = new DocumentTableMap(DocumentList_Forms_DefId, "DocumentList_Forms");
            list.Add(DocumentList_Forms);
            var DynamicDocumentList = new DocumentTableMap(DynamicDocumentList_DefId, "DynamicDocumentList");
            list.Add(DynamicDocumentList);
            var Editors = new DocumentTableMap(Editors_DefId, "Editors");
            list.Add(Editors);
            var Enum_Defs = new DocumentTableMap(Enum_Defs_DefId, "Enum_Defs") { IsVirtual = true };
            list.Add(Enum_Defs);
            var Enum_Items = new DocumentTableMap(Enum_Items_DefId, "Enum_Items") { IsVirtual = true };
            list.Add(Enum_Items);
            var File_Templates = new DocumentTableMap(File_Templates_DefId, "File_Templates");
            list.Add(File_Templates);
            var Finish_Activities = new DocumentTableMap(Finish_Activities_DefId, "Finish_Activities");
            list.Add(Finish_Activities);
            var Folder_Defs = new DocumentTableMap(Folder_Defs_DefId, "Folder_Defs") { IsVirtual = true };
            list.Add(Folder_Defs);
            var Forms = new DocumentTableMap(Forms_DefId, "Forms") { IsVirtual = true };
            list.Add(Forms);
            var Generators = new DocumentTableMap(Generators_DefId, "Generators");
            list.Add(Generators);
            var Grids = new DocumentTableMap(Grids_DefId, "Grids");
            list.Add(Grids);
            var Images = new DocumentTableMap(Images_DefId, "Images");
            list.Add(Images);
            var Languages = new DocumentTableMap(Languages_DefId, "Languages");
            list.Add(Languages);
            var Layout_Types = new DocumentTableMap(Layout_Types_DefId, "Layout_Types");
            list.Add(Layout_Types);
            var Menus = new DocumentTableMap(Menus_DefId, "Menus") { IsVirtual = true };
            list.Add(Menus);
            var Object_Def_Translations = new DocumentTableMap(Object_Def_Translations_DefId, "Object_Def_Translations");
            list.Add(Object_Def_Translations);
            var Object_Defs = new DocumentTableMap(Object_Defs_DefId, "Object_Defs") { IsVirtual = true };
            list.Add(Object_Defs);
            var Org_Models = new DocumentTableMap(Org_Models_DefId, "Org_Models");
            list.Add(Org_Models);
            var Org_Positions = new DocumentTableMap(Org_Positions_DefId, "Org_Positions") { IsVirtual = true };
            list.Add(Org_Positions);
            var Org_Positions_Roles = new DocumentTableMap(Org_Positions_Roles_DefId, "Org_Positions_Roles");
            list.Add(Org_Positions_Roles);
            var Org_Unit_Types = new DocumentTableMap(Org_Unit_Types_DefId, "Org_Unit_Types") { IsVirtual = true };
            list.Add(Org_Unit_Types);
            var Org_Units = new DocumentTableMap(Org_Units_DefId, "Org_Units") { IsVirtual = true };
            list.Add(Org_Units);
            var Org_Units_Roles = new DocumentTableMap(Org_Units_Roles_DefId, "Org_Units_Roles");
            list.Add(Org_Units_Roles);
            var Organizations = new DocumentTableMap(Organizations_DefId, "Organizations") { IsVirtual = true };
            list.Add(Organizations);
            var OrgUnits_ObjectDefs = new DocumentTableMap(OrgUnits_ObjectDefs_DefId, "OrgUnits_ObjectDefs");
            list.Add(OrgUnits_ObjectDefs);
            var Panels = new DocumentTableMap(Panels_DefId, "Panels");
            list.Add(Panels);
            var Permission_Defs = new DocumentTableMap(Permission_Defs_DefId, "Permission_Defs");
            list.Add(Permission_Defs);
            var Permissions = new DocumentTableMap(Permissions_DefId, "Permissions");
            list.Add(Permissions);
            var Persons = new DocumentTableMap(Persons_DefId, "Persons") { IsVirtual = true };
            list.Add(Persons);
            var Presentation_Activities = new DocumentTableMap(Presentation_Activities_DefId, "Presentation_Activities");
            list.Add(Presentation_Activities);
            var Process_Call_Activities = new DocumentTableMap(Process_Call_Activities_DefId, "Process_Call_Activities");
            list.Add(Process_Call_Activities);
            var Project_Folders = new DocumentTableMap(Project_Folders_DefId, "Project_Folders");
            list.Add(Project_Folders);
            var Project_Items = new DocumentTableMap(Project_Items_DefId, "Project_Items");
            list.Add(Project_Items);
            var Projects = new DocumentTableMap(Projects_DefId, "Projects");
            list.Add(Projects);
            var Radio_Items = new DocumentTableMap(Radio_Items_DefId, "Radio_Items");
            list.Add(Radio_Items);
            var Report_Bands = new DocumentTableMap(Report_Bands_DefId, "Report_Bands");
            list.Add(Report_Bands);
            var Report_Editors = new DocumentTableMap(Report_Editors_DefId, "Report_Editors");
            list.Add(Report_Editors);
            var Report_Section_Types = new DocumentTableMap(Report_Section_Types_DefId, "Report_Section_Types");
            list.Add(Report_Section_Types);
            var Report_Sections = new DocumentTableMap(Report_Sections_DefId, "Report_Sections");
            list.Add(Report_Sections);
            var Report_Summary_Types = new DocumentTableMap(Report_Summary_Types_DefId, "Report_Summary_Types");
            list.Add(Report_Summary_Types);
            var Report_Texts = new DocumentTableMap(Report_Texts_DefId, "Report_Texts");
            list.Add(Report_Texts);
            var Reports = new DocumentTableMap(Reports_DefId, "Reports");
            list.Add(Reports);
            var Role_Permissions = new DocumentTableMap(Role_Permissions_DefId, "Role_Permissions");
            list.Add(Role_Permissions);
            var Role_Refs = new DocumentTableMap(Role_Refs_DefId, "Role_Refs");
            list.Add(Role_Refs);
            var Roles = new DocumentTableMap(Roles_DefId, "Roles");
            list.Add(Roles);
            var Rules = new DocumentTableMap(Rules_DefId, "Rules");
            list.Add(Rules);
            var Script_Activities = new DocumentTableMap(Script_Activities_DefId, "Script_Activities");
            list.Add(Script_Activities);
            var Scripts = new DocumentTableMap(Scripts_DefId, "Scripts");
            list.Add(Scripts);
            var Security_Objects = new DocumentTableMap(Security_Objects_DefId, "Security_Objects");
            list.Add(Security_Objects);
            var Start_Activities = new DocumentTableMap(Start_Activities_DefId, "Start_Activities");
            list.Add(Start_Activities);
            var Subjects = new DocumentTableMap(Subjects_DefId, "Subjects") { IsVirtual = true };
            list.Add(Subjects);
            var Tab_Controls = new DocumentTableMap(Tab_Controls_DefId, "Tab_Controls");
            list.Add(Tab_Controls);
            var Table_Columns = new DocumentTableMap(Table_Columns_DefId, "Table_Columns");
            list.Add(Table_Columns);
            var Table_Forms = new DocumentTableMap(Table_Forms_DefId, "Table_Forms");
            list.Add(Table_Forms);
            var Table_Reports = new DocumentTableMap(Table_Reports_DefId, "Table_Reports");
            list.Add(Table_Reports);
            var Texts = new DocumentTableMap(Texts_DefId, "Texts");
            list.Add(Texts);
            var Titles = new DocumentTableMap(Titles_DefId, "Titles");
            list.Add(Titles);
            var User_Actions = new DocumentTableMap(User_Actions_DefId, "User_Actions");
            list.Add(User_Actions);
            var Worker_Roles = new DocumentTableMap(Worker_Roles_DefId, "Worker_Roles");
            list.Add(Worker_Roles);
            var Workers = new DocumentTableMap(Workers_DefId, "Workers") { IsVirtual = true };
            list.Add(Workers);
            var Workflow_Activities = new DocumentTableMap(Workflow_Activities_DefId, "Workflow_Activities");
            list.Add(Workflow_Activities);
            var Workflow_Def_Rules = new DocumentTableMap(Workflow_Def_Rules_DefId, "Workflow_Def_Rules");
            list.Add(Workflow_Def_Rules);
            var Workflow_Defs = new DocumentTableMap(Workflow_Defs_DefId, "Workflow_Defs");
            list.Add(Workflow_Defs);
            var Workflow_Processes = new DocumentTableMap(Workflow_Processes_DefId, "Workflow_Processes");
            list.Add(Workflow_Processes);

            // Set Inheritance
            Controls.Ancestor = Object_Defs;
            Document_Route_Transits.Ancestor = Object_Defs;
            Document_Routes.Ancestor = Object_Defs;
            Document_State_Types.Ancestor = Object_Defs;
            DocumentControl.Ancestor = Controls;
            DocumentList_Forms.Ancestor = Controls;
            DynamicDocumentList.Ancestor = Controls;
            Enum_Items.Ancestor = Object_Defs;
            File_Templates.Ancestor = Object_Defs;
            Finish_Activities.Ancestor = Workflow_Activities;
            Folder_Defs.Ancestor = Object_Defs;
            Menus.Ancestor = Controls;
            Org_Models.Ancestor = Object_Defs;
            Org_Positions.Ancestor = Subjects;
            Org_Units.Ancestor = Subjects;
            Organizations.Ancestor = Subjects;
            Permissions.Ancestor = Security_Objects;
            Persons.Ancestor = Subjects;
            Project_Items.Ancestor = Object_Defs;
            Radio_Items.Ancestor = Controls;
            Report_Bands.Ancestor = Controls;
            Report_Editors.Ancestor = Controls;
            Report_Sections.Ancestor = Controls;
            Report_Texts.Ancestor = Controls;
            Roles.Ancestor = Security_Objects;
            Script_Activities.Ancestor = Workflow_Activities;
            Scripts.Ancestor = Object_Defs;
            Security_Objects.Ancestor = Object_Defs;
            Start_Activities.Ancestor = Workflow_Activities;
            Subjects.Ancestor = Object_Defs;
            Tab_Controls.Ancestor = Controls;
            Table_Columns.Ancestor = Controls;
            Table_Forms.Ancestor = Controls;
            Table_Reports.Ancestor = Reports;
            Workers.Ancestor = Persons;
            Workflow_Defs.Ancestor = Object_Defs;
            Panels.Ancestor = Controls;
            Workflow_Activities.Ancestor = Workflow_Defs;
            Document_Activities.Ancestor = Workflow_Activities;
            Presentation_Activities.Ancestor = Workflow_Activities;
            Document_State_Activities.Ancestor = Workflow_Activities;
            Activity_Links.Ancestor = Workflow_Defs;
            User_Actions.Ancestor = Workflow_Defs;
            Process_Call_Activities.Ancestor = Workflow_Activities;
            Document_Defs.Ancestor = Object_Defs;
            Editors.Ancestor = Controls;
            Forms.Ancestor = Controls;
            Combo_Boxes.Ancestor = Controls;
            Attribute_Defs.Ancestor = Object_Defs;
            Buttons.Ancestor = Controls;
            Texts.Ancestor = Controls;
            Images.Ancestor = Controls;
            Grids.Ancestor = Controls;
            Reports.Ancestor = Controls;
            Enum_Defs.Ancestor = Object_Defs;
            Projects.Ancestor = Project_Items;
            Project_Folders.Ancestor = Project_Items;
            Workflow_Processes.Ancestor = Workflow_Defs;


            // AttributeFieldMaps
            Activity_Links.Fields.Add(new AttributeFieldMap(Activity_Links_Condition_DefId, "Condition"));
            Activity_Links.Fields.Add(new AttributeFieldMap(Activity_Links_Is_Exception_Handler_DefId, "Is_Exception_Handler"));
            Activity_Links.Fields.Add(new AttributeFieldMap(Activity_Links_Source_Id_DefId, "Source_Id"));
            Activity_Links.Fields.Add(new AttributeFieldMap(Activity_Links_Target_Id_DefId, "Target_Id"));
            Activity_Links.Fields.Add(new AttributeFieldMap(Activity_Links_User_Action_Id_DefId, "User_Action_Id"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_BlobIsImage_DefId, "BlobIsImage"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_BlobMaxHeight_DefId, "BlobMaxHeight"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_BlobMaxSizeBytes_DefId, "BlobMaxSizeBytes"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_BlobMaxWidth_DefId, "BlobMaxWidth"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_CalculateScript_DefId, "CalculateScript"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Default_Value_DefId, "Default_Value"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Document_Id_DefId, "Document_Id"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Enum_Id_DefId, "Enum_Id"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Is_Not_Null_DefId, "Is_Not_Null"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Is_Unique_DefId, "Is_Unique"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Max_Length_DefId, "Max_Length"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Max_Value_DefId, "Max_Value"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Min_Value_DefId, "Min_Value"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Org_Type_Id_DefId, "Org_Type_Id"));
            Attribute_Defs.Fields.Add(new AttributeFieldMap(Attribute_Defs_Type_Id_DefId, "Type_Id"));
            Buttons.Fields.Add(new AttributeFieldMap(Buttons_Action_Id_DefId, "Action_Id"));
            Buttons.Fields.Add(new AttributeFieldMap(Buttons_Action_Type_DefId, "Action_Type"));
            Buttons.Fields.Add(new AttributeFieldMap(Buttons_Button_Type_DefId, "Button_Type"));
            Buttons.Fields.Add(new AttributeFieldMap(Buttons_Process_Id_DefId, "Process_Id"));
            Buttons.Fields.Add(new AttributeFieldMap(Buttons_User_Action_Id_DefId, "User_Action_Id"));
            Combo_Boxes.Fields.Add(new AttributeFieldMap(Combo_Boxes_Attribute_Id_DefId, "Attribute_Id"));
            Combo_Boxes.Fields.Add(new AttributeFieldMap(Combo_Boxes_Attribute_Name_DefId, "Attribute_Name"));
            Combo_Boxes.Fields.Add(new AttributeFieldMap(Combo_Boxes_Detail_Attribute_Id_DefId, "Detail_Attribute_Id"));
            Combo_Boxes.Fields.Add(new AttributeFieldMap(Combo_Boxes_Detail_Attribute_Name_DefId, "Detail_Attribute_Name"));
            Combo_Boxes.Fields.Add(new AttributeFieldMap(Combo_Boxes_Is_Radio_DefId, "Is_Radio"));
            Combo_Boxes.Fields.Add(new AttributeFieldMap(Combo_Boxes_Rows_DefId, "Rows"));
            Compare_Operations.Fields.Add(new AttributeFieldMap(Compare_Operations_Name_DefId, "Name"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_Compare_Operation_DefId, "Compare_Operation"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_Invisible_DefId, "Invisible"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_Not_Null_DefId, "Not_Null"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_Read_Only_DefId, "Read_Only"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_Sort_Type_DefId, "Sort_Type"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_Style_DefId, "Style"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_Title_DefId, "Title"));
            Controls.Fields.Add(new AttributeFieldMap(Controls_VisibleInListView_DefId, "VisibleInListView"));
            Data_Transfer_Doc_Errors.Fields.Add(new AttributeFieldMap(Data_Transfer_Doc_Errors_Document_Id_DefId, "Document_Id"));
            Data_Transfer_Doc_Errors.Fields.Add(new AttributeFieldMap(Data_Transfer_Doc_Errors_Operation_Id_DefId, "Operation_Id"));
            Data_Transfer_Doc_Errors.Fields.Add(new AttributeFieldMap(Data_Transfer_Doc_Errors_Type_Id_DefId, "Type_Id"));
            Data_Transfer_Operations.Fields.Add(new AttributeFieldMap(Data_Transfer_Operations_Direction_Id_DefId, "Direction_Id"));
            Data_Transfer_Operations.Fields.Add(new AttributeFieldMap(Data_Transfer_Operations_Finish_Date_DefId, "Finish_Date"));
            Data_Transfer_Operations.Fields.Add(new AttributeFieldMap(Data_Transfer_Operations_Source_Date_DefId, "Source_Date"));
            Data_Transfer_Operations.Fields.Add(new AttributeFieldMap(Data_Transfer_Operations_Source_File_DefId, "Source_File"));
            Data_Transfer_Operations.Fields.Add(new AttributeFieldMap(Data_Transfer_Operations_Source_Id_DefId, "Source_Id"));
            Data_Transfer_Operations.Fields.Add(new AttributeFieldMap(Data_Transfer_Operations_Start_Date_DefId, "Start_Date"));
            Data_Transfers.Fields.Add(new AttributeFieldMap(Data_Transfers_Created_DefId, "Created"));
            Data_Transfers.Fields.Add(new AttributeFieldMap(Data_Transfers_Def_Id_DefId, "Def_Id"));
            Data_Transfers.Fields.Add(new AttributeFieldMap(Data_Transfers_Expired_DefId, "Expired"));
            Data_Transfers.Fields.Add(new AttributeFieldMap(Data_Transfers_Operation_Id_DefId, "Operation_Id"));
            Data_Transfers.Fields.Add(new AttributeFieldMap(Data_Transfers_Value_DefId, "Value"));
            Data_Types.Fields.Add(new AttributeFieldMap(Data_Types_Name_DefId, "Name"));
            Document_Activities.Fields.Add(new AttributeFieldMap(Document_Activities_DocActivityTypeId_DefId, "DocActivityTypeId"));
            Document_Activities.Fields.Add(new AttributeFieldMap(Document_Activities_Document_Id_DefId, "Document_Id"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Ancestor_Id_DefId, "Ancestor_Id"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Create_Permission_Id_DefId, "Create_Permission_Id"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Def_Data_DefId, "Def_Data"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Delete_Permission_Id_DefId, "Delete_Permission_Id"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Is_Inline_DefId, "Is_Inline"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Is_Public_DefId, "Is_Public"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Select_Permission_Id_DefId, "Select_Permission_Id"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_Update_Permission_Id_DefId, "Update_Permission_Id"));
            Document_Defs.Fields.Add(new AttributeFieldMap(Document_Defs_WithHistory_DefId, "WithHistory"));
            Document_Route_Transits.Fields.Add(new AttributeFieldMap(Document_Route_Transits_Permission_Id_DefId, "Permission_Id"));
            Document_Route_Transits.Fields.Add(new AttributeFieldMap(Document_Route_Transits_Source_State_Id_DefId, "Source_State_Id"));
            Document_Route_Transits.Fields.Add(new AttributeFieldMap(Document_Route_Transits_Target_State_Id_DefId, "Target_State_Id"));
            Document_Routes.Fields.Add(new AttributeFieldMap(Document_Routes_Document_Def_Id_DefId, "Document_Def_Id"));
            Document_State_Activities.Fields.Add(new AttributeFieldMap(Document_State_Activities_State_Name_DefId, "State_Name"));
            Document_State_Activities.Fields.Add(new AttributeFieldMap(Document_State_Activities_State_Type_Id_DefId, "State_Type_Id"));
            Document_State_Types.Fields.Add(new AttributeFieldMap(Document_State_Types_Read_Only_DefId, "Read_Only"));
            Document_Visibilities.Fields.Add(new AttributeFieldMap(Document_Visibilities_Document_Id_DefId, "Document_Id"));
            Document_Visibilities.Fields.Add(new AttributeFieldMap(Document_Visibilities_Organization_Id_DefId, "Organization_Id"));
            DocumentActivityType.Fields.Add(new AttributeFieldMap(DocumentActivityType_DocActivityTypeId_DefId, "DocActivityTypeId"));
            DocumentActivityType.Fields.Add(new AttributeFieldMap(DocumentActivityType_Name_DefId, "Name"));
            DocumentControl.Fields.Add(new AttributeFieldMap(DocumentControl_Attribute_Id_DefId, "Attribute_Id"));
            DocumentControl.Fields.Add(new AttributeFieldMap(DocumentControl_Attribute_Name_DefId, "Attribute_Name"));
            DocumentControl.Fields.Add(new AttributeFieldMap(DocumentControl_Form_Id_DefId, "Form_Id"));
            DocumentList_Forms.Fields.Add(new AttributeFieldMap(DocumentList_Forms_Attribute_Id_DefId, "Attribute_Id"));
            DocumentList_Forms.Fields.Add(new AttributeFieldMap(DocumentList_Forms_Attribute_Name_DefId, "Attribute_Name"));
            DocumentList_Forms.Fields.Add(new AttributeFieldMap(DocumentList_Forms_Form_Attribute_Id_DefId, "Form_Attribute_Id"));
            DocumentList_Forms.Fields.Add(new AttributeFieldMap(DocumentList_Forms_Form_Id_DefId, "Form_Id"));
            DynamicDocumentList.Fields.Add(new AttributeFieldMap(DynamicDocumentList_Form_Id_DefId, "Form_Id"));
            DynamicDocumentList.Fields.Add(new AttributeFieldMap(DynamicDocumentList_ScriptId_DefId, "ScriptId"));
            Editors.Fields.Add(new AttributeFieldMap(Editors_Attribute_Id_DefId, "Attribute_Id"));
            Editors.Fields.Add(new AttributeFieldMap(Editors_Attribute_Name_DefId, "Attribute_Name"));
            Editors.Fields.Add(new AttributeFieldMap(Editors_Cols_DefId, "Cols"));
            Editors.Fields.Add(new AttributeFieldMap(Editors_Edit_Mask_DefId, "Edit_Mask"));
            Editors.Fields.Add(new AttributeFieldMap(Editors_Format_DefId, "Format"));
            Editors.Fields.Add(new AttributeFieldMap(Editors_Rows_DefId, "Rows"));
            //Enum_Items.Fields.Add(new AttributeFieldMap(Enum_Items_Name_DefId, "Name"));
            File_Templates.Fields.Add(new AttributeFieldMap(File_Templates_File_Name_DefId, "File_Name"));
            Finish_Activities.Fields.Add(new AttributeFieldMap(Finish_Activities_Form_Id_DefId, "Form_Id"));
            Finish_Activities.Fields.Add(new AttributeFieldMap(Finish_Activities_Message_DefId, "Message"));
            Forms.Fields.Add(new AttributeFieldMap(Forms_Ban_Edit_DefId, "Ban_Edit"));
            Forms.Fields.Add(new AttributeFieldMap(Forms_Can_Delete_DefId, "Can_Delete"));
            Forms.Fields.Add(new AttributeFieldMap(Forms_Def_Data_DefId, "Def_Data"));
            Forms.Fields.Add(new AttributeFieldMap(Forms_Document_Id_DefId, "Document_Id"));
            Forms.Fields.Add(new AttributeFieldMap(Forms_Layout_Id_DefId, "Layout_Id"));
            Forms.Fields.Add(new AttributeFieldMap(Forms_Script_DefId, "Script"));
            Generators.Fields.Add(new AttributeFieldMap(Generators_Document_Def_Id_DefId, "Document_Def_Id"));
            Generators.Fields.Add(new AttributeFieldMap(Generators_Organization_Id_DefId, "Organization_Id"));
            Generators.Fields.Add(new AttributeFieldMap(Generators_Value_DefId, "Value"));
            Grids.Fields.Add(new AttributeFieldMap(Grids_Document_Id_DefId, "Document_Id"));
            Grids.Fields.Add(new AttributeFieldMap(Grids_Is_Detail_DefId, "Is_Detail"));
            Images.Fields.Add(new AttributeFieldMap(Images_Image_Data_DefId, "Image_Data"));
            Languages.Fields.Add(new AttributeFieldMap(Languages_Culture_Name_DefId, "Culture_Name"));
            Languages.Fields.Add(new AttributeFieldMap(Languages_Name_DefId, "Name"));
            Layout_Types.Fields.Add(new AttributeFieldMap(Layout_Types_Name_DefId, "Name"));
            Menus.Fields.Add(new AttributeFieldMap(Menus_Def_Data_DefId, "Def_Data"));
            Menus.Fields.Add(new AttributeFieldMap(Menus_Form_Id_DefId, "Form_Id"));
            Menus.Fields.Add(new AttributeFieldMap(Menus_Process_Id_DefId, "Process_Id"));
            Menus.Fields.Add(new AttributeFieldMap(Menus_State_Type_Id_DefId, "State_Type_Id"));
            Object_Def_Translations.Fields.Add(new AttributeFieldMap(Object_Def_Translations_Data_Text_DefId, "Data_Text"));
            Object_Def_Translations.Fields.Add(new AttributeFieldMap(Object_Def_Translations_Def_Id_DefId, "Def_Id"));
            Object_Def_Translations.Fields.Add(new AttributeFieldMap(Object_Def_Translations_Language_Id_DefId, "Language_Id"));
            Object_Defs.Fields.Add(new AttributeFieldMap(Object_Defs_Created_DefId, "Created"));
            Object_Defs.Fields.Add(new AttributeFieldMap(Object_Defs_Deleted_DefId, "Deleted"));
            Object_Defs.Fields.Add(new AttributeFieldMap(Object_Defs_Description_DefId, "Description"));
            Object_Defs.Fields.Add(new AttributeFieldMap(Object_Defs_Full_Name_DefId, "Full_Name"));
            Object_Defs.Fields.Add(new AttributeFieldMap(Object_Defs_Name_DefId, "Name"));
            Object_Defs.Fields.Add(new AttributeFieldMap(Object_Defs_Order_Index_DefId, "Order_Index"));
            Object_Defs.Fields.Add(new AttributeFieldMap(Object_Defs_Parent_Id_DefId, "Parent_Id"));
            Org_Models.Fields.Add(new AttributeFieldMap(Org_Models_Is_Active_DefId, "Is_Active"));
//            Org_Positions.Fields.Add(new AttributeFieldMap(Org_Positions_Name_DefId, "Name"));
            Org_Positions.Fields.Add(new AttributeFieldMap(Org_Positions_Title_Id_DefId, "Title_Id"));
            Org_Positions_Roles.Fields.Add(new AttributeFieldMap(Org_Positions_Roles_Org_Position_Id_DefId, "Org_Position_Id"));
            Org_Positions_Roles.Fields.Add(new AttributeFieldMap(Org_Positions_Roles_Role_Id_DefId, "Role_Id"));
//            Org_Unit_Types.Fields.Add(new AttributeFieldMap(Org_Unit_Types_Name_DefId, "Name"));
//            Org_Units.Fields.Add(new AttributeFieldMap(Org_Units_Full_Name_DefId, "Full_Name"));
            Org_Units.Fields.Add(new AttributeFieldMap(Org_Units_INN_DefId, "INN"));
            //Org_Units.Fields.Add(new AttributeFieldMap(Org_Units_Name_DefId, "Name"));
            Org_Units.Fields.Add(new AttributeFieldMap(Org_Units_Type_Id_DefId, "Type_Id"));
            Org_Units_Roles.Fields.Add(new AttributeFieldMap(Org_Units_Roles_Org_Unit_Id_DefId, "Org_Unit_Id"));
            Org_Units_Roles.Fields.Add(new AttributeFieldMap(Org_Units_Roles_Role_Id_DefId, "Role_Id"));
            Organizations.Fields.Add(new AttributeFieldMap(Organizations_Code_DefId, "Code"));
            Organizations.Fields.Add(new AttributeFieldMap(Organizations_Type_Id_DefId, "Type_Id"));
            OrgUnits_ObjectDefs.Fields.Add(new AttributeFieldMap(OrgUnits_ObjectDefs_ObjDef_Id_DefId, "ObjDef_Id"));
            OrgUnits_ObjectDefs.Fields.Add(new AttributeFieldMap(OrgUnits_ObjectDefs_OrgUnit_Id_DefId, "OrgUnit_Id"));
            Panels.Fields.Add(new AttributeFieldMap(Panels_Is_Horizontal_DefId, "Is_Horizontal"));
            Panels.Fields.Add(new AttributeFieldMap(Panels_Layout_Id_DefId, "Layout_Id"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_Access_Type_DefId, "Access_Type"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_AllowDelete_DefId, "AllowDelete"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_AllowInsert_DefId, "AllowInsert"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_AllowSelect_DefId, "AllowSelect"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_AllowUpdate_DefId, "AllowUpdate"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_Def_Id_DefId, "Def_Id"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_Is_Disabled_DefId, "Is_Disabled"));
            Permission_Defs.Fields.Add(new AttributeFieldMap(Permission_Defs_Permission_Id_DefId, "Permission_Id"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_Birth_Date_DefId, "Birth_Date"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_First_Name_DefId, "First_Name"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_INN_DefId, "INN"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_Last_Name_DefId, "Last_Name"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_Middle_Name_DefId, "Middle_Name"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_Passport_Date_DefId, "Passport_Date"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_Passport_No_DefId, "Passport_No"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_Passport_Org_DefId, "Passport_Org"));
            Persons.Fields.Add(new AttributeFieldMap(Persons_Sex_DefId, "Sex"));
            Presentation_Activities.Fields.Add(new AttributeFieldMap(Presentation_Activities_Form_Id_DefId, "Form_Id"));
            Presentation_Activities.Fields.Add(new AttributeFieldMap(Presentation_Activities_Is_Exception_DefId, "Is_Exception"));
            Presentation_Activities.Fields.Add(new AttributeFieldMap(Presentation_Activities_Message_DefId, "Message"));
            Process_Call_Activities.Fields.Add(new AttributeFieldMap(Process_Call_Activities_Process_Id_DefId, "Process_Id"));
            Radio_Items.Fields.Add(new AttributeFieldMap(Radio_Items_Enum_Id_DefId, "Enum_Id"));
            Report_Editors.Fields.Add(new AttributeFieldMap(Report_Editors_Attribute_Id_DefId, "Attribute_Id"));
            Report_Editors.Fields.Add(new AttributeFieldMap(Report_Editors_Summary_Id_DefId, "Summary_Id"));
            Report_Editors.Fields.Add(new AttributeFieldMap(Report_Editors_Width_DefId, "Width"));
            Report_Section_Types.Fields.Add(new AttributeFieldMap(Report_Section_Types_Name_DefId, "Name"));
            Report_Sections.Fields.Add(new AttributeFieldMap(Report_Sections_Type_Id_DefId, "Type_Id"));
            Report_Summary_Types.Fields.Add(new AttributeFieldMap(Report_Summary_Types_Name_DefId, "Name"));
            Reports.Fields.Add(new AttributeFieldMap(Reports_Document_Id_DefId, "Document_Id"));
            Role_Permissions.Fields.Add(new AttributeFieldMap(Role_Permissions_Permission_Id_DefId, "Permission_Id"));
            Role_Permissions.Fields.Add(new AttributeFieldMap(Role_Permissions_Role_Id_DefId, "Role_Id"));
            Role_Refs.Fields.Add(new AttributeFieldMap(Role_Refs_Def_Id_DefId, "Def_Id"));
            Role_Refs.Fields.Add(new AttributeFieldMap(Role_Refs_Is_Disabled_DefId, "Is_Disabled"));
            Role_Refs.Fields.Add(new AttributeFieldMap(Role_Refs_Role_Id_DefId, "Role_Id"));
            Rules.Fields.Add(new AttributeFieldMap(Rules_Script_DefId, "Script"));
            Script_Activities.Fields.Add(new AttributeFieldMap(Script_Activities_Script_DefId, "Script"));
            Scripts.Fields.Add(new AttributeFieldMap(Scripts_Script_Text_DefId, "Script_Text"));
            Security_Objects.Fields.Add(new AttributeFieldMap(Security_Objects_Name_DefId, "Name"));
            Subjects.Fields.Add(new AttributeFieldMap(Subjects_Address_DefId, "Address"));
            Subjects.Fields.Add(new AttributeFieldMap(Subjects_Email_DefId, "Email"));
            Subjects.Fields.Add(new AttributeFieldMap(Subjects_Phone_DefId, "Phone"));
            Worker_Roles.Fields.Add(new AttributeFieldMap(Worker_Roles_Role_Id_DefId, "Role_Id"));
            Worker_Roles.Fields.Add(new AttributeFieldMap(Worker_Roles_Worker_Id_DefId, "Worker_Id"));
            Workers.Fields.Add(new AttributeFieldMap(Workers_Language_Id_DefId, "Language_Id"));
            Workers.Fields.Add(new AttributeFieldMap(Workers_OrgPosition_Id_DefId, "OrgPosition_Id"));
            Workers.Fields.Add(new AttributeFieldMap(Workers_User_Name_DefId, "User_Name"));
            Workers.Fields.Add(new AttributeFieldMap(Workers_User_Password_DefId, "User_Password"));
            Workflow_Activities.Fields.Add(new AttributeFieldMap(Workflow_Activities_Type_Id_DefId, "Type_Id"));
            Workflow_Def_Rules.Fields.Add(new AttributeFieldMap(Workflow_Def_Rules_Rule_Id_DefId, "Rule_Id"));
            Workflow_Def_Rules.Fields.Add(new AttributeFieldMap(Workflow_Def_Rules_Workflow_Id_DefId, "Workflow_Id"));
            Workflow_Processes.Fields.Add(new AttributeFieldMap(Workflow_Processes_Diagram_Data_DefId, "Diagram_Data"));
            Workflow_Processes.Fields.Add(new AttributeFieldMap(Workflow_Processes_Script_DefId, "Script"));

            return list;
        }
    }
}
