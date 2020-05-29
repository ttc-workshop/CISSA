using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    public class ControlFactory : IControlFactory
    {
        private readonly IDataContext _dataContext;
        private IDataContext DataContext { get { return _dataContext; } }

        // private Guid UserId { get; set; }

        // private readonly IDocRepository _docRepo;
        // private readonly IEnumRepository _enumRepo;
        private readonly IDocDefRepository _docDefRepo;
        // private readonly IAttributeRepository _attrRepo;
        private readonly IPermissionRepository _permissionRepo;
        //private readonly IComboBoxEnumProvider _comboBoxEnumProv;

        private readonly IQueryRepository _queryRepo;

        private ControlFactory(IDataContext dataContext, /*Guid userId,*/ IDocDefRepository docDefRepo,
            /*IEnumRepository enumRepo,*/ IPermissionRepository permissionRepo)
        {
            _dataContext = dataContext;

            // UserId = userId;

            // _docRepo = docRepo ?? new DocRepository(dataContext);
            _docDefRepo = docDefRepo ?? new DocDefRepository(dataContext);
            // _enumRepo = enumRepo ?? new EnumRepository(dataContext);
            // _attrRepo = attrRepo ?? new AttributeRepository(dataContext);
            _permissionRepo = permissionRepo ?? new PermissionRepository(dataContext);
            // _comboBoxEnumProv = new ComboBoxEnumProvider(DataContext, UserId);
        }

        public ControlFactory(IDataContext dataContext/*, Guid userId*/) : this(dataContext, /*userId, null,*/ null, null) { }

        public ControlFactory(IAppServiceProvider provider, IDataContext dataContext)
        {
            _dataContext = dataContext; //provider.Get<IDataContext>();
            
            // UserId = provider.GetCurrentUserId(); //userDataProvider.UserId;

            // _enumRepo = provider.Get<IEnumRepository>();
            _docDefRepo = provider.Get<IDocDefRepository>();
            _permissionRepo = provider.Get<IPermissionRepository>();
            // _comboBoxEnumProv = provider.Get<IComboBoxEnumProvider>();
            _queryRepo = provider.Get<IQueryRepository>();
        }

        public BizControl Create(Control control)
        {
            var methods = typeof(ControlFactory).GetMethods().Where(m => m.IsPublic && !m.IsStatic);

            return (
                from m in methods
                where m.ReturnType.IsSubclassOf(typeof (BizControl))
                let param = m.GetParameters().FirstOrDefault(p => p.ParameterType == control.GetType())
                where param != null
                let parameters = new object[] {control, null}
                select (BizControl) m.Invoke(this, parameters)).FirstOrDefault();
        }

        public BizText CreateText(Text text, DocDef def)
        {
            var result = new BizText();

            InitControl(result, text);
            return result;
        }

        public BizDataControl CreateEditor(Editor editor, DocDef def)
        {
            BizDataControl result = null;
            AttrDef attrDef = null;

            if (def != null)
            {
                if (editor.Attribute_Id != null)
                    attrDef = def.Attributes.FirstOrDefault(a => a.Id == editor.Attribute_Id);
                if (attrDef == null && !String.IsNullOrEmpty(editor.Attribute_Name) && editor.Attribute_Name[0] != '&')
                    attrDef =
                        def.Attributes.FirstOrDefault(
                            a => String.Equals(a.Name, editor.Attribute_Name, StringComparison.OrdinalIgnoreCase));
            }
            if (attrDef != null)
            {
                result = CreateTypeEditor(editor, attrDef);
            }
            /*else
                {
                    if (!editor.Attribute_DefsReference.IsLoaded) editor.Attribute_DefsReference.Load();
                    if (editor.Attribute_Defs == null) return null;

                    result = CreateTypeEditor(editor.Attribute_Defs);
                }*/

            else
            {
                var attrName = editor.Attribute_Name ?? String.Empty;

                SystemIdent attrIdent;
                if (SystemIdentConverter.TryConvert(attrName, out attrIdent))
                    switch (attrIdent)
                    {
                        case SystemIdent.OrgId:
                        case SystemIdent.OrgName:
                        case SystemIdent.OrgCode:
                        case SystemIdent.State:
//                        case SystemIdent.InState:
                            result = new BizEditText {Ident = attrIdent, ReadOnly = true, AttributeName = attrName};
                            break;
                        case SystemIdent.Id:
                        case SystemIdent.UserId:
                        case SystemIdent.UserName:
                            result = new BizEditText {Ident = attrIdent, ReadOnly = true, AttributeName = attrName};
                            break;
                        case SystemIdent.Created:
                        case SystemIdent.Modified:
                        case SystemIdent.StateDate:
                            result = new BizEditDateTime { Ident = attrIdent, ReadOnly = true, AttributeName = attrName };
                            break;
                    }
            }

            AddChildren(result, editor, def);
            if (result is BizEdit) InitEditor((BizEdit)result, editor);
            if (result is BizComboBox) InitComboBox(result as BizComboBox, editor, attrDef);

            return result;
        }

        public BizComboBox CreateComboBox(Combo_Box combo, DocDef def)
        {
            var result = new BizComboBox();

            AddChildren(result, combo, def);
            InitComboBox(result, combo, def);

            return result;
        }

        public BizPanel CreatePanel(Panel panel, DocDef def)
        {
            var result = new BizPanel
            {
                LayoutId = panel.Layout_Id ?? (short) LayoutType.Default,
                ReadOnly = panel.Read_Only ?? false,
                //IsHorizontal = panel.Is_Horizontal ?? false,
                Children = new List<BizControl>()
            };

            AddChildren(result, panel, def);
            InitControl(result, panel);
            return result;
        }

        public BizTabControl CreateTabControl(Tab_Control control, DocDef def)
        {
            var result = new BizTabControl
            {
                Children = new List<BizControl>()
            };

            AddChildren(result, control, def);
            InitControl(result, control);
            return result;
        }

        public BizDocumentControl CreateDocumentControl(DocumentControl docControl, DocDef def)
        {
//            if (!docControl.Attribute_DefsReference.IsLoaded) docControl.Attribute_DefsReference.Load();
//            if (docControl.Attribute_Defs == null) return null;

            AttrDef attrDef = null;
            if (def != null)
            {
                if (docControl.Attribute_Id != null)
                    attrDef = def.Attributes.FirstOrDefault(a => a.Id == docControl.Attribute_Id);
                else
                    if (!String.IsNullOrEmpty(docControl.Attribute_Name))
                        attrDef =
                            def.Attributes.FirstOrDefault(
                                a => String.Equals(a.Name, docControl.Attribute_Name ?? String.Empty, StringComparison.OrdinalIgnoreCase));
            }

            var result = new BizDocumentControl
                             {
                                 AttributeDefId = attrDef != null ? attrDef.Id : (Guid?) null, //docControl.Attribute_Defs.Id,
                                 AttributeName = attrDef != null ? attrDef.Name : String.Empty, // AttributeName = docControl.Attribute_Name,
                                 FormId = docControl.Form_Id
                             };

            if (attrDef != null && attrDef.DocDefType != null)
            {
                var nestDef = _docDefRepo.DocDefById(attrDef.DocDefType.Id);

                AddChildren(result, docControl, nestDef);
            }
            InitControl(result, docControl);

            return result;
        }

        public BizDocumentListForm CreateDocumentListForm(DocumentList_Form docListForm, DocDef def)
        {
            AttrDef attrDef = null;
            
            if (def != null)
            {
                if (docListForm.Attribute_Id != null)
                    attrDef = def.Attributes.FirstOrDefault(a => a.Id == docListForm.Attribute_Id);
                else
                    if (!String.IsNullOrEmpty(docListForm.Attribute_Name))
                        attrDef = 
                            def.Attributes.FirstOrDefault(
                                a => String.Equals(a.Name, docListForm.Attribute_Name ?? String.Empty, StringComparison.OrdinalIgnoreCase));
            }

            var result = new BizDocumentListForm
                             {
                                 AttributeDefId = attrDef != null ? attrDef.Id : docListForm.Attribute_Id,
                                 AttributeName = attrDef != null ? attrDef.Name : docListForm.Attribute_Name ?? String.Empty,
                                 FormId = docListForm.Form_Id,
                                 FormAttributeDefId = docListForm.Form_Attribute_Id
                             };

            DocDef nestDef = null;

            if (attrDef != null && attrDef.DocDefType != null)
            {
                nestDef = _docDefRepo.DocDefById(attrDef.DocDefType.Id);
            }
            if (attrDef == null && result.FormId != null && result.FormAttributeDefId != null)
            {
                var form = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Table_Form>().FirstOrDefault(f => f.Id == result.FormId);

                if (form != null && form.Document_Id != null)
                {
                    var docDefId = (Guid) form.Document_Id;

                    nestDef = _docDefRepo.DocDefById(docDefId);
                }
            }

            if (nestDef != null) AddChildren(result, docListForm, nestDef);
            InitControl(result, docListForm);

            return result;
        }

        public BizDynamicDocumentListForm CreateDynamicDocumentListForm(DynamicDocumentList dynDocList, DocDef def)
        {
            var result = new BizDynamicDocumentListForm
            {
                ScritpId = dynDocList.ScriptId,
                FormId = dynDocList.Form_Id
            };

            AddChildren(result, dynDocList, def);
            InitControl(result, dynDocList);

            return result;
        }

        public BizDetailForm CreateDetailForm(Form form, DocDef def)
        {
            var result = new BizDetailForm
            {
                DocumentDefId = form.Document_Id,
                LayoutId = form.Layout_Id ?? (short)LayoutType.Detail,
                Children = new List<BizControl>(),
                CanEdit = !(form.Ban_Edit ?? false) & !(form.Read_Only ?? false),
                CanDelete = form.Can_Delete ?? false
            };

            if (result.DocumentDefId != null)
                def = _docDefRepo.DocDefById((Guid)result.DocumentDefId);

            AddChildren(result, form, def);
            InitControl(result, form);

            return result;
        }

        public BizTableForm CreateTableForm(Table_Form form, DocDef def)
        {
            var result = new BizTableForm
            {
                DocumentDefId = form.Document_Id,
                FormId = form.Form_Id,
                FilterFormId = form.Filter_Form_Id,
                Children = new List<BizControl>(),
                AddNewPermissionId = form.Add_New_Permission_Id,
                OpenPermissionId = form.Open_Permission_Id,
                CanAddNew = form.Can_Add_New ?? false,
                CanEdit = form.Can_Edit ?? false,
                CanDelete = form.Can_Delete ?? false,
                PageSize = form.Rows ?? 10
            };

            if (result.DocumentDefId != null)
                def = _docDefRepo.DocDefById((Guid) result.DocumentDefId);

            AddChildren(result, form, def);
            InitControl(result, form);

            return result;
        }

        public BizButton CreateButton(Button button, DocDef def)
        {
            var result = new BizButton
                             {
                                 ActionId = button.Action_Id ?? Guid.Empty,
                                 ProcessId = button.Process_Id,
                                 UserActionId = button.User_Action_Id,
                                 ActionType = (BizButtonActionType?) button.Action_Type ?? BizButtonActionType.Form,
                                 ButtonType = (BizButtonType?) button.Button_Type ?? BizButtonType.Button
                             };

            InitControl(result, button);

            return result;
        }

        public BizGrid CreateGrid(Grid grid, DocDef def)
        {
            var result = new BizGrid
                             {
                                 DocumentDefId = grid.Document_Id ?? Guid.Empty,
                                 IsDetail = grid.Is_Detail ?? true,
                                 Children = new List<BizControl>()
                             };

            AddChildren(result, grid, def);
            InitControl(result, grid);

            return result;
        }

        public BizControl CreateImage(Image image, DocDef def)
        {
            if (image.Attribute_Id == null && image.Image_Data != null)
            {
                var staticImage = new BizImage
                {
                    ImageBytes = image.Image_Data,
                    Height = 0,
                    Width = 0
                };
                InitControl(staticImage, image);

                return staticImage;
            }

            if (image.Attribute_Id == null) return null;

            var docImage = new BizDataImage
            {
                Value = image.Image_Data,
                AttributeDefId = (Guid)image.Attribute_Id,
                Height = 0,
                Width = 0
            };
            InitControl(docImage, image);

            return docImage;
        }

        public BizDataControl CreateTableColumn(Table_Column column, DocDef def)
        {
            AttrDef attrDef = null;
            if (def == null) return null;

            if (column.Attribute_Id != null)
                attrDef = def.Attributes.FirstOrDefault(a => a.Id == column.Attribute_Id);
            if (attrDef == null && column.Attribute_Name != null)
                attrDef =
                    def.Attributes.FirstOrDefault(
                        a => String.Equals(a.Name, column.Attribute_Name, StringComparison.OrdinalIgnoreCase));
            
//            else
//            {
//                if (!column.Attribute_DefsReference.IsLoaded)
//                    column.Attribute_DefsReference.Load();
//            }
            if (attrDef == null) return null;

            if (attrDef.Type.Id == (int)CissaDataType.Doc)
            {
                var result = new BizTableColumn
                                 {
                                     AttributeDefId = attrDef.Id,
                                     AttributeName = attrDef.Name,
                                     Children = new List<BizControl>()
                                 };

                var nestDef = _docDefRepo.DocDefById(attrDef.DocDefType.Id);

                AddChildren(result, column, nestDef);
                InitControl(result, column);

                return result;
            }
            else
            {
                var result = CreateTypeEditor(column, attrDef);

                AddChildren(result, column, def);
                if (result is BizEdit) InitEditor(result as BizEdit, column);
                if (result is BizComboBox) InitComboBox(result as BizComboBox, column, attrDef);
                    
                return result;
            }
        }

        private void InitControl(BizControl control, Control controlData)
        {
            control.Id = controlData.Id;
            control.Caption = controlData.Full_Name;
            control.DefaultCaption = controlData.Full_Name;
            control.Name = controlData.Name;
            control.Style = controlData.Style;
            control.Title = controlData.Title;
            control.LanguageId = 0;
            control.Invisible = controlData.Invisible ?? false;
            if (control is BizDataControl)
                ((BizDataControl) control).ReadOnly = controlData.Read_Only ?? false;
            if (controlData.Sort_Type == null)
                control.SortType = SortType.None;
            else if (controlData.Sort_Type == 0)
                control.SortType = SortType.Ascending;
            else 
                control.SortType = SortType.Descending;

            if (controlData.Compare_Operation != null)
                control.Operation = (CompareOperation) controlData.Compare_Operation;
            else 
                control.Operation = CompareOperation.Equal;

            //var permissionRepo = new PermissionRepository(DataContext);
            control.Permissions = _permissionRepo.GetObjectDefPermissions(controlData.Id);
        }

        private void InitEditor(BizEdit control, Editor controlData)
        {
            control.Rows = controlData.Rows ?? 0;
            control.Cols = controlData.Cols ?? 0;
//            control.AttributeDefId = controlData.Attribute_Id;
//            control.AttributeName = controlData.Attribute_Name ?? String.Empty;
            control.ReadOnly = controlData.Read_Only ?? false;
            control.NotNull = controlData.Not_Null ?? false;
            control.EditMask = controlData.Edit_Mask ?? String.Empty;
            control.Format = controlData.Format ?? String.Empty;
            control.ProcessId = controlData.Process_Id;

            InitControl(control, controlData);
        }

        private void InitEditor(BizEdit control, Table_Column controlData)
        {
//            control.AttributeDefId = controlData.Attribute_Id;
//            control.AttributeName = controlData.Attribute_Name ?? String.Empty;

            InitControl(control, controlData);
        }

        private void InitComboBox(BizComboBox control, Editor controlData, AttrDef attrDef)
        {
            control.Rows = controlData.Rows ?? 0;
//            control.AttributeDefId = controlData.Attribute_Id;
//            control.AttributeName = controlData.Attribute_Name ?? String.Empty;

//            if (!controlData.Attribute_DefsReference.IsLoaded) controlData.Attribute_DefsReference.Load();
/*
            if (attrDef == null)
                InitComboBoxItems(control, null);
            else
                InitComboBox(control, attrDef /*controlData.Attribute_Defs#1#);*/

            InitControl(control, controlData);
            AddQueryItems(control, controlData);
        }

        private void InitComboBox(BizComboBox control, Combo_Box controlData, DocDef def)
        {
            AttrDef attrDef = null;
            if (def != null)
            {
                if (controlData.Attribute_Id != null)
                    attrDef = def.Attributes.FirstOrDefault(a => a.Id == controlData.Attribute_Id);
                if (attrDef == null && controlData.Attribute_Name != null)
                    attrDef =
                        def.Attributes.FirstOrDefault(a => String.Equals(a.Name, controlData.Attribute_Name, StringComparison.OrdinalIgnoreCase));
            }
            control.Rows = controlData.Rows ?? 0;
            control.IsRadio = controlData.Is_Radio ?? false;
            if (attrDef != null)
            {
                control.AttributeDefId = attrDef.Id;
                control.AttributeName = attrDef.Name;
            }
            else
            {
                var attrName = controlData.Attribute_Name ?? String.Empty;
                SystemIdent attrIdent;
                if (SystemIdentConverter.TryConvert(attrName, out attrIdent))
                {
                    control.Ident = attrIdent;
                    control.AttributeName = attrName;
                    //InitComboBox(control, attrIdent);
                }
            }
            control.DetailAttributeId = controlData.Detail_Attribute_Id;
            control.DetailAttributeName = controlData.Detail_Attribute_Name;

            /*if (attrDef != null)
            {
                //if (!controlData.Attribute_DefsReference.IsLoaded) controlData.Attribute_DefsReference.Load();

                InitComboBox(control, attrDef /*controlData.Attribute_Defs#1#);
            } */

            InitControl(control, controlData);
            // AddQueryItems(control, controlData); // Вызывает ошибку дублирования QueryItems
        }

        private void InitComboBox(BizComboBox control, Table_Column controlData, AttrDef attrDef)
        {
            control.Rows = 0;
            control.IsRadio = false;
            //control.AttributeDefId = controlData.Attribute_Id;

            /*if (control.AttributeDefId != null)
            {
                //if (!controlData.Attribute_DefsReference.IsLoaded) controlData.Attribute_DefsReference.Load();

                InitComboBox(control, attrDef /*controlData.Attribute_Defs#1#);
            }*/
            InitControl(control, controlData);
            // AddQueryItems(control, controlData); // Вызывает ошибку дублирования QueryItems
        }

        /*private void InitComboBox(BizComboBox combo, AttrDef attr)
        {
            if (attr == null) return;

            //InitComboBoxItems(combo, attr);
        }*/

        /*private void InitComboBox(BizComboBox combo, SystemIdent ident)
        {
            switch (ident)
            {
                case SystemIdent.OrgId:
                case SystemIdent.OrgCode:
                case SystemIdent.OrgName:
                    combo.Items = GetComboBoxOrganizations(combo.DetailAttributeId);
                    break;
                default:
                    combo.Items = new List<EnumValue>();
                    break;
            }
        }*/

        /*public void InitComboBoxItems(BizComboBox combo, AttrDef attr)
        {
            if (attr == null)
            {
                switch (combo.Ident)
                {
                    case SystemIdent.OrgId:
                    case SystemIdent.OrgCode:
                    case SystemIdent.OrgName:
                        combo.Items = GetComboBoxOrganizations(combo.DetailAttributeId);
                        break;
                    case SystemIdent.UserId:
                    case SystemIdent.UserName:
                        combo.Items = GetComboBoxUsers();
                        break;
                    default:
                        combo.Items = new List<EnumValue>();
                        break;
                }
                return;
            }

            switch (attr.Type.Id)
            {
                case (short) CissaDataType.Enum:
                    if (attr.EnumDefType != null)
                        combo.Items =
                            new List<EnumValue>(
                                _enumRepo.GetEnumItems(attr.EnumDefType.Id));
                    break;
                case (short) CissaDataType.Doc:
                    combo.Items = GetComboBoxDocuments(combo, attr.DocDefType);
                    break;
                case (short) CissaDataType.Organization:
                    combo.Items = GetComboBoxOrganizations(attr.OrgTypeId);
                    break;
                case (short) CissaDataType.User:
                    combo.Items = GetComboBoxUsers();
                    break;
            }
        }*/

        /*public List<EnumValue> GetComboBoxOrganizations(Guid? orgTypeId)
        {
            //var provider = new ComboBoxEnumProvider(DataContext, UserId);
            return _comboBoxEnumProv.GetEnumOrganizationValues(orgTypeId);
        }

        public List<EnumValue> GetComboBoxUsers()
        {
            //var provider = new ComboBoxEnumProvider(DataContext, UserId);
            return _comboBoxEnumProv.GetEnumUserValues();
        }

        public List<EnumValue> GetComboBoxDocuments(BizComboBox combo, DocDef detailDocDef)
        {
            //var provider = new ComboBoxEnumProvider(DataContext, UserId);
            return _comboBoxEnumProv.GetEnumDocumentValues(combo, detailDocDef);
/*
            var list = new List<EnumValue>();

            if (detailDocDef == null) return list;

            var docDef = _docDefRepo.DocDefById(detailDocDef.Id);

            AttrDef detailAttrDef;
            if (combo.DetailAttributeId != null)
                detailAttrDef = docDef.Attributes.FirstOrDefault(ad => ad.Id == combo.DetailAttributeId);
            else
            {
                detailAttrDef =
                    docDef.Attributes.FirstOrDefault(
                        ad => String.Compare(ad.Name, combo.DetailAttributeName, StringComparison.OrdinalIgnoreCase) == 0);
                if (detailAttrDef == null)
                    detailAttrDef =
                        docDef.Attributes.FirstOrDefault(
                            ad => ad.Type.Id == (int) CissaDataType.Text);
            }

            if (detailAttrDef == null) return list;

            using (var query = new SqlQuery(DataContext, docDef, UserId))
            {
                query.AddAttribute("&Id");
                query.AddAttribute(detailAttrDef.Id);
                query.AddOrderAttribute(detailAttrDef.Id);
                using (var reader = new SqlQueryReader(DataContext, query))
                {
                    while (reader.Read())
                    {
                        var detail = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;

                        list.Add(new EnumValue
                        {
                            Id = reader.GetGuid(0),
                            Value = detail,
                            DefaultValue = detail
                        });
                    }
                }
            }

            return list;
#1#
        }*/

        /*[Obsolete("Устаревший метод")]
        private void InitComboBox(BizComboBox control, Attribute_Def def)
        {
            if (def == null) return;

            control.AttributeDefId = def.Id;

            if (def.Type_Id == (short)CissaDataType.Enum && def.Enum_Id != null)
            {
                control.Items =
                    new List<EnumValue>(
                        _enumRepo.GetEnumItems((Guid) def.Enum_Id));
            }
            else if (def.Type_Id == (short)CissaDataType.Doc)
            {
                var en = DataContext.Entities;

                var docDef = _docDefRepo.DocDefById(def.Document_Id ?? Guid.Empty);
                //GetDocumentAttributes(def.Document_Id ?? Guid.Empty, Guid.Empty);

                AttrDef detailAttrDef;
                if (control.DetailAttributeId != null)
                    detailAttrDef = docDef.Attributes.FirstOrDefault(ad => ad.Id == control.DetailAttributeId);
                else
                {
                    detailAttrDef =
                        docDef.Attributes.FirstOrDefault(
                            ad => String.Compare(ad.Name, control.DetailAttributeName, StringComparison.OrdinalIgnoreCase) == 0);
                }

                if (detailAttrDef == null) return;

                var list = new List<EnumValue>();

                // DONE: Изменить доступ к документам в ComboBox
                foreach (var doc in en.Documents
                    .Where(d => d.Def_Id == def.Document_Id && (d.Deleted == null || d.Deleted == false)))
                {
                    var document = _docRepo.LoadById(doc.Id);
                    var attr = _attrRepo.GetAttributeById(detailAttrDef.Id, document);

                    list.Add(new EnumValue
                                 {
                                     Id = doc.Id,
                                     Value = (attr != null ? (string) attr.ObjectValue : ""),
                                     DefaultValue = (attr != null ? (string) attr.ObjectValue : "")
                                 });
                }
                control.Items = list;
            }
        }*/

        private void AddChildren(BizControl control, Control controlData, DocDef def)
        {
            if (control == null) return;

            if (control.Children == null) control.Children = new List<BizControl>();

//            if (!controlData.Children.IsLoaded) controlData.Children.Load();

            var en = DataContext.GetEntityDataContext().Entities;

            /*            var children = en.CreateQuery<Guid>(
                                "SELECT od FROM Object_Defs AS od WHERE od.Parent_Id = @PID AND (od.Deleted IS NULL OR od.Deleted = False) " +
                                "ORDER BY od.Order_Index", new ObjectParameter("PID", controlData.Id));*/
            //            children.MergeOption = MergeOption.NoTracking;
            /*.Where(c => c.Parent_Id == controlData.Id && (c.Deleted == null || c.Deleted == false))
                .OrderBy(c => c.Order_Index).Select(od => od.Id));*/

            var children = en.Object_Defs_View
                .Where(o => o.Parent_Id == controlData.Id && (o.Deleted == null || o.Deleted == false))
                .OrderBy(o => o.Order_Index).Select(o => o.Id).ToList();

            foreach (var childId in children)
            {
                //Object_Defs_View childView = child;
                var childViewId = childId;

                var editor = en.Object_Defs.OfType<Editor>().FirstOrDefault(c => c.Id == childViewId);
                if (editor != null)
                {
                    var sub = CreateEditor(editor, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var button = en.Object_Defs.OfType<Button>().FirstOrDefault(c => c.Id == childViewId);
                if (button != null)
                {
                    var sub = CreateButton(button, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var combo = en.Object_Defs.OfType<Combo_Box>().FirstOrDefault(c => c.Id == childViewId);
                if (combo != null)
                {
                    var sub = CreateComboBox(combo, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var text = en.Object_Defs.OfType<Text>().FirstOrDefault(c => c.Id == childViewId);
                if (text != null)
                {
                    var sub = CreateText(text, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var panel = en.Object_Defs.OfType<Panel>().FirstOrDefault(c => c.Id == childViewId);
                if (panel != null)
                {
                    var sub = CreatePanel(panel, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var docControl = en.Object_Defs.OfType<DocumentControl>().FirstOrDefault(c => c.Id == childViewId);
                if (docControl != null)
                {
                    var sub = CreateDocumentControl(docControl, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var docList = en.Object_Defs.OfType<DocumentList_Form>().FirstOrDefault(c => c.Id == childViewId);
                if (docList != null)
                {
                    var sub = CreateDocumentListForm(docList, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var grid = en.Object_Defs.OfType<Grid>().FirstOrDefault(c => c.Id == childViewId);
                if (grid != null)
                {
                    var sub = CreateGrid(grid, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var menu = en.Object_Defs.OfType<Menu>().FirstOrDefault(c => c.Id == childViewId);
                if (menu != null)
                {
                    var sub = CreateMenu(menu);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var column = en.Object_Defs.OfType<Table_Column>().FirstOrDefault(c => c.Id == childViewId);
                if (column != null)
                {
                    var sub = CreateTableColumn(column, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var dynamic = en.Object_Defs.OfType<DynamicDocumentList>().FirstOrDefault(c => c.Id == childViewId);
                if (dynamic != null)
                {
                    var sub = CreateDynamicDocumentListForm(dynamic, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var tabControl = en.Object_Defs.OfType<Tab_Control>().FirstOrDefault(c => c.Id == childViewId);
                if (tabControl != null)
                {
                    var sub = CreateTabControl(tabControl, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                var image = en.Object_Defs.OfType<Image>().FirstOrDefault(c => c.Id == childViewId);
                if (image != null)
                {
                    var sub = CreateImage(image, def);
                    if (sub != null) control.Children.Add(sub);
                    continue;
                }

                if (_queryRepo != null)
                {
                    var querySource = _queryRepo.FindJoinDef(childViewId);
                    if (querySource != null)
                    {
                        if (control.QueryItems == null) control.QueryItems = new List<QueryItemDefData>();
                        control.QueryItems.Add(querySource);
                        continue;
                    }
                    
                    var queryCondition = _queryRepo.FindConditionDef(childViewId);
                    if (queryCondition != null)
                    {
                        if (control.QueryItems == null) control.QueryItems = new List<QueryItemDefData>();
                        control.QueryItems.Add(queryCondition);
                    }
                }
            }
        }
        private void AddQueryItems(BizControl control, Control controlData)
        {
            if (control == null) return;
            if (_queryRepo == null) return;

            // if (control.Children == null) control.Children = new List<BizControl>();

            var en = DataContext.GetEntityDataContext().Entities;

            var children = en.Object_Defs_View
                .Where(o => o.Parent_Id == controlData.Id && (o.Deleted == null || o.Deleted == false))
                .OrderBy(o => o.Order_Index).Select(o => o.Id).ToList();

            foreach (var childId in children)
            {
                var childViewId = childId;
                var querySource = _queryRepo.FindJoinDef(childViewId);
                if (querySource != null)
                {
                    if (control.QueryItems == null) control.QueryItems = new List<QueryItemDefData>();
                    control.QueryItems.Add(querySource);
                    continue;
                }

                var queryCondition = _queryRepo.FindConditionDef(childViewId);
                if (queryCondition != null)
                {
                    if (control.QueryItems == null) control.QueryItems = new List<QueryItemDefData>();
                    control.QueryItems.Add(queryCondition);
                }
            }
        }
        private BizControl CreateMenu(Menu menu)
        {
            var result = new BizMenu
            {
                ProcessId = menu.Process_Id,
                FormId = menu.Form_Id,
                DocStateId = menu.State_Type_Id
            };

            InitControl(result, menu);

            return result;
        }

        /*[Obsolete("Устаревший метод")]
        private static BizDataControl CreateTypeEditor(Attribute_Def def)
        {
            BizDataControl result = null;

            switch (def.Type_Id)
            {
                case (short)CissaDataType.Int:
                    result = new BizEditInt
                                 {
                                     MaxValue = Convert.ToInt32(def.Max_Value),
                                     MinValue = Convert.ToInt32(def.Min_Value),
                                     MaxLength = (uint?)def.Max_Length ?? 0
                                 };
                    break;
                case (short)CissaDataType.Float:
                    result = new BizEditFloat
                                 {
                                     MaxValue = Convert.ToInt32(def.Max_Value),
                                     MinValue = Convert.ToInt32(def.Min_Value),
                                     MaxLength = (uint?)def.Max_Length ?? 0
                                 };
                    break;
                case (short)CissaDataType.Currency:
                    result = new BizEditCurrency
                                 {
                                     MaxValue = Convert.ToInt32(def.Max_Value),
                                     MinValue = Convert.ToInt32(def.Min_Value),
                                     MaxLength = (uint?)def.Max_Length ?? 0
                                 };
                    break;
                case (short)CissaDataType.Text:
                    result = new BizEditText
                                 {
                                     MaxLength = (uint?)def.Max_Length ?? 0
                                 };
                    break;
                case (short)CissaDataType.DateTime:
                    result = new BizEditDateTime();
                    break;
                case (short)CissaDataType.Bool:
                    result = new BizEditBool();
                    break;
                case (short)CissaDataType.Enum:
                    result = new BizComboBox();
                    break;
                case (short)CissaDataType.Organization:
                    result = new BizEditText {MaxLength = 1000, ReadOnly = true};
                    break;
                case (short)CissaDataType.DocumentState:
                    result = new BizEditText { MaxLength = 1000, ReadOnly = true};
                    break;
            }

            return result;
        }*/

        private static BizDataControl CreateTypeEditor(Control control, AttrDef def)
        {
            if (def == null) return null;

            BizDataControl result = null;

            switch (def.Type.Id)
            {
                case (short) CissaDataType.Int:
                    result = new BizEditInt
                                 {
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name,
                                     MaxValue = Convert.ToInt32(def.MaxValue),
                                     MinValue = Convert.ToInt32(def.MinValue),
                                     MaxLength = (uint) def.MaxLength
                                 };
                    break;
                case (short)CissaDataType.Float:
                    result = new BizEditFloat
                                 {
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name,
                                     MaxValue = Convert.ToInt32(def.MaxValue),
                                     MinValue = Convert.ToInt32(def.MinValue),
                                     MaxLength = (uint) def.MaxLength
                                 };
                    break;
                case (short)CissaDataType.Currency:
                    result = new BizEditCurrency
                                 {
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name,
                                     MaxValue = Convert.ToInt32(def.MaxValue),
                                     MinValue = Convert.ToInt32(def.MinValue),
                                     MaxLength = (uint) def.MaxLength
                                 };
                    break;
                case (short)CissaDataType.Text:
                    result = new BizEditText
                                 {
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name,
                                     MaxLength = (uint) def.MaxLength
                                 };
                    break;
                case (short)CissaDataType.DateTime:
                    result = new BizEditDateTime
                                 {
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name
                                 };
                    break;
                case (short)CissaDataType.Bool:
                    result = new BizEditBool
                                 {
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name
                                 };
                    break;
                case (short)CissaDataType.Enum:
                    result = new BizComboBox
                                 {
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name
                                 };
                    break;
                case (short)CissaDataType.Organization:
                    if (control.Read_Only ?? false)
                        result = new BizEditText
                                     {
                                         MaxLength = 800,
                                         ReadOnly = true,
                                         AttributeDefId = def.Id,
                                         AttributeName = def.Name
                                     };
                    else
                        result = new BizComboBox
                        {
                            AttributeDefId = def.Id,
                            AttributeName = def.Name
                        };
                    break;
                case (short)CissaDataType.DocumentState:
                    result = new BizEditText
                                 {
                                     MaxLength = 800,
                                     ReadOnly = true,
                                     AttributeDefId = def.Id,
                                     AttributeName = def.Name
                                 };
                    break;
                case (short)CissaDataType.Blob:
                    result = new BizEditFile
                    {
                        ReadOnly = control.Read_Only ?? false,
                        AttributeDefId = def.Id,
                        AttributeName = def.Name
                    };
                    break;
                case (short)CissaDataType.User:
                    result = new BizComboBox
                    {
                        ReadOnly = control.Read_Only ?? false,
                        AttributeDefId = def.Id,
                        AttributeName = def.Name
                    };
                    break;
                case (short)CissaDataType.ClassOfDocument:
                case (short) CissaDataType.AuthorOfDocument:
                case (short) CissaDataType.OrganizationOfDocument:
                case (short) CissaDataType.OrgUnitOfDocument:
                case (short)CissaDataType.StateOfDocument:
                case (short) CissaDataType.DocumentId:
                    result = new BizEditText
                    {
                        MaxLength = 800,
                        ReadOnly = true,
                        AttributeDefId = def.Id,
                        AttributeName = def.Name
                    };
                    break;
                case (short)CissaDataType.CreateTimeOfDocument:
                    result = new BizEditDateTime
                    {
                        ReadOnly = true,
                        AttributeDefId = def.Id,
                        AttributeName = def.Name
                    };
                    break;
            }

            return result;
        }
    }
}
