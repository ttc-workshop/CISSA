using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.Models.Application;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Repository
{
    public class FormRepository : IFormRepository
    {
//        private PresentationManagerClient _manager = null;
//        private IPresentationManager Manager
//        {
//            get { return _manager /*?? (_manager = BizConnection.PresentationManager)*/; }
//        }

        private IContext Context { get; set; }

        public FormRepository(IContext context)
        {
//            _manager = pm;
            Context = context;
        }

        [Obsolete("Не используется")]
        public BizForm Get(Guid formId)
        {
            var pm = Context.GetPresentationProxy();
            {
                var bizForm = pm.Proxy.GetDetailForm(formId, 0);

                return bizForm;
            }
        }

        public List<BizControl> CheckFormFields(BizControl form)
        {
            var result = new List<BizControl>();

            if (form != null && form.Children != null)
                foreach (var control in form.Children)
                {
                    if (control is BizDataControl)
                    {
                        var dataControl = (BizDataControl) control;

                        if ((dataControl.DocNotNull || dataControl.FormNotNull) && !dataControl.ReadOnly)
                        {
                            if (dataControl is BizEditBool)
                            {
                                if (((BizEditBool) dataControl).Value == null)
                                    result.Add(dataControl);
                            }
                            else if (dataControl is BizEditInt)
                            {
                                if (((BizEditInt) dataControl).Value == null)
                                    result.Add(dataControl);
                            }
                            else if (dataControl is BizEditFloat)
                            {
                                if (((BizEditFloat) dataControl).Value == null)
                                    result.Add(dataControl);
                            }
                            else if (dataControl is BizEditCurrency)
                            {
                                if (((BizEditCurrency) dataControl).Value == null)
                                    result.Add(dataControl);
                            }
                            else if (dataControl is BizEditText)
                            {
                                if (((BizEditText) dataControl).Value == null)
                                    result.Add(dataControl);
                            }
                            else if (dataControl is BizEditDateTime)
                            {
                                if (((BizEditDateTime) dataControl).Value == null)
                                    result.Add(dataControl);
                            }
                            else if (dataControl is BizComboBox)
                            {
                                if (((BizComboBox) dataControl).Value == null)
                                    result.Add(dataControl);
                            }
                        }
                    }
                    if (control.Children != null && control.Children.Count > 0)
                        result.AddRange(CheckFormFields(control));
                }

            return result;
        }

        public BizForm SetDoc(BizForm form, Doc document)
        {
            var pm = Context.GetPresentationProxy();
            return pm.Proxy.SetFormDoc(form, document);
        }

        public Doc GetDoc(BizForm form, Doc document)
        {
            var pm = Context.GetPresentationProxy();
            return pm.Proxy.GetFormDoc(form, document);
        }

/*
        public BizForm Get(Guid formId, Guid docId)
        {
            using (var pm = Context.GetPresentationProxy())
            {
                var bizForm = pm.Proxy.GetForm(formId, docId);

                return bizForm;
            }
        }
*/

        public static readonly string FieldCannotBeNull = Resources.Form.FieldCannotBeenEmpty;
        //"Данное поле не может быть пустым! Введите значение.";

        public bool AddFieldValue(out string message, BizForm bizForm, Guid controlId, object value, bool onlyCheck)
        {
            //if (value == null) return;
            message = "";
            var ok = true;

            BizControl control = FindControlById(controlId, bizForm.Children);

            if (control is BizEditInt)
            {
                var edit = (BizEditInt)control;
                if (value == null || String.IsNullOrEmpty(value.ToString()))
                {
                    if (!onlyCheck) edit.Value = null;
                    if (!onlyCheck && (edit.DocNotNull || edit.FormNotNull))
                    {
                        message = FieldCannotBeNull;
                        ok = false;
                    }
                }
                else
                {
                    int result;
                    if (Int32.TryParse(value.ToString(), out result))
                    {
                        if (!onlyCheck) edit.Value = result;
                        if (!onlyCheck && (edit.DocNotNull || edit.FormNotNull))
                        {
                            message = FieldCannotBeNull;
                            ok = false;
                        }
                    }
                    else
                    {
                        message = Resources.Form.InvalidNumberFormat /*"Число передано в неверном формате"*/;
                        ok = false;
                    }
                }
            }

            else if (control is BizEditCurrency)
            {
                var edit = (BizEditCurrency)control;
                //edit.Attribute.Value = decimal.Parse(value.ToString());
                if (value == null || String.IsNullOrEmpty(value.ToString()))
                {
                    if (!onlyCheck) edit.Value = null;
                    if (!onlyCheck && (edit.DocNotNull || edit.FormNotNull))
                    {
                        message = FieldCannotBeNull;
                        ok = false;
                    }
                }
                else
                {
                    decimal result;
                    if (decimal.TryParse(value.ToString(), out result))
                    {
                        if (!onlyCheck)
                        {
                            edit.Value = result;
                        }
                    }
                    else
                    {
                        message = Resources.Form.InvalidNumberFormat /*"Число передано в неверном формате"*/;
                        ok = false;
                    }
                }
            }

            else if (control is BizEditFloat)
            {
                var edit = (BizEditFloat)control;
                //edit.Attribute.Value = float.Parse(value.ToString());
                if (value == null || String.IsNullOrEmpty(value.ToString()))
                {
                    if (!onlyCheck) edit.Value = null;
                    if (!onlyCheck && (edit.DocNotNull || edit.FormNotNull))
                    {
                        message = FieldCannotBeNull;
                        ok = false;
                    }
                }
                else
                {
                    float result;
                    if (float.TryParse(value.ToString(), out result))
                    {
                        //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                        if (!onlyCheck) edit.Value = result;
                    }
                    else
                    {
                        message = Resources.Form.InvalidNumberFormat /*"Число передано в неверном формате"*/;
                        ok = false;
                    }
                }
            }

            else if (control is BizEditText)
            {
                var edit = (BizEditText)control;

                //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                if (!onlyCheck)
                    edit.Value = value != null ? value.ToString() : null;
                if (!onlyCheck && String.IsNullOrEmpty(edit.Value) && (edit.DocNotNull || edit.FormNotNull))
                {
                    message = FieldCannotBeNull;
                    ok = false;
                }
            }

            else if (control is BizEditDateTime)
            {
                var edit = (BizEditDateTime)control;
                //edit.Attribute.Value = decimal.Parse(value.ToString());
                if (value == null || String.IsNullOrEmpty(value.ToString()))
                {
                    if (!onlyCheck) edit.Value = null;
                    if (!onlyCheck && (edit.DocNotNull || edit.FormNotNull))
                    {
                        message = FieldCannotBeNull;
                        ok = false;
                    }
                }
                else
                {
                    DateTime result;
                    if (DateTime.TryParse(value.ToString(), out result))
                    {
                        //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                        if (!onlyCheck)
                        {
                            edit.Value = result;
                        }
                    }
                    else
                    {
                        message = Resources.Form.InvalidDateTimeFormat /*"Дата передана в неверном формате"*/;
                        ok = false;
                    }
                }
            }

            else if (control is BizEditBool)
            {
                var edit = (BizEditBool)control;
                //edit.Attribute.Value = decimal.Parse(value.ToString());
                if (value == null || String.IsNullOrEmpty(value.ToString()))
                {
                    if (!onlyCheck) edit.Value = null;
                    if (!onlyCheck && (edit.DocNotNull || edit.FormNotNull))
                    {
                        message = FieldCannotBeNull;
                        ok = false;
                    }
                }
                else
                {
                    bool result;
                    if (value.ToString() == "true,false")
                    {
                        string temp = value.ToString();
                        value = temp.Substring(0, 4);
                    }

                    if (bool.TryParse(value.ToString(), out result))
                    {
                        //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                        if (!onlyCheck)
                        {
                            edit.Value = result;
                        }
                    }
                    else
                    {
                        message = Resources.Form.InvalidFormat /*"Значение передано в неверном формате"*/;
                        ok = false;
                    }
                }
            }

            else if (control is BizComboBox)
            {
                var combo = (BizComboBox)control;
                if (value == null || String.IsNullOrEmpty(value.ToString()))
                {
                    if (!onlyCheck) combo.Value = null;
                    if (!onlyCheck && (combo.DocNotNull || combo.FormNotNull))
                    {
                        message = FieldCannotBeNull;
                        ok = false;
                    }
                }
                else
                {
                    Guid result;
                    if (Guid.TryParse(value.ToString(), out result))
                        if (!onlyCheck) combo.Value = result;
                        else
                        {
                            message = Resources.Form.InvalidFormat /*"Значение передано в неверном формате"*/;
                            ok = false;
                        }
                }
            }

            //TODO: Дописать метод обновления для других обновляемых полей
            return ok;
        }

        public ManagedTableForm GetTableForm(Guid formId)
        {
//            var bizTableForm = Manager.GetGridForm(formId);
//
//            var dm = _docManager;
//
//            return new ManagedTableForm(bizTableForm, dm);
            throw new NotImplementedException("Процедура закоммичена");
        }

        public TControl Find<TControl>(BizControl form, Guid id) where TControl : BizControl
        {
            if (form != null && form.Id == id)
            {
                return form as TControl;
            }
            if (form != null && form.Children != null)
            {
                var control = FindControlById(id, form.Children);

                return control as TControl;
            }
            return null;
        }

        public static BizControl FindControlById(Guid controlId, IEnumerable<BizControl> controls)
        {
            if (controls != null)
                foreach (BizControl bizControl in controls)
                {
                    if (bizControl.Id == controlId)
                        return bizControl;

                    if (bizControl.Children != null)
                    {
                        var controlInChildren = FindControlById(controlId, bizControl.Children);
                        if (controlInChildren != null) return controlInChildren;
                    }
                }
            return null;
        }
    }
}