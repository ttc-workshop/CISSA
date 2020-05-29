using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;
using Intersoft.CISSA.UserApp.Utils;

namespace Intersoft.CISSA.UserApp.Models
{
    public class ManagedForm 
    {
        public BizForm BizForm { get; private set; }

        public ManagedForm(Guid formId, Guid docId)
        {
            var client = Presentation.Manager;
            BizForm = client.GetForm(formId, docId);
        }

        public void AddFieldValue(Guid controlId, object value, bool onlyCheck)
        {
            BizControl control = FindControlById(controlId, BizForm.Children);
            
            if (control is BizEditInt)
            {
                var edit = (BizEditInt) control;
                int result;
                if (Int32.TryParse(value.ToString(), out result))
                {
                    //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                    if (!onlyCheck)
                    {
                        edit.Attribute.Value = result;
                    }
                }
                else
                {
                    throw new ApplicationException("Число передано в неверном формате");
                } 
            }

            if (control is BizEditCurrency)
            {
                var edit = (BizEditCurrency)control;
                //edit.Attribute.Value = decimal.Parse(value.ToString());
                decimal result;
                if (decimal.TryParse(value.ToString(), out result))
                {
                    //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                    if (!onlyCheck)
                    {
                        edit.Attribute.Value = result;
                    }
                }
                else
                {
                    throw new ApplicationException("Число передано в неверном формате");
                } 
            }

            if (control is BizEditFloat)
            {
                var edit = (BizEditFloat)control;
                //edit.Attribute.Value = float.Parse(value.ToString());
                float result;
                if (float.TryParse(value.ToString(), out result))
                {
                    //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                    if (!onlyCheck)
                    {
                        edit.Attribute.Value = result;
                    }
                }
                else
                {
                    throw new ApplicationException("Число передано в неверном формате");
                } 
            }

            if (control is BizEditText)
            {
                var edit = (BizEditText)control;
                
                //TODO: дописить тут различные проверки для аттрибута (ограничения загружены в сам аттрибут)

                if (!onlyCheck)
                {
                    edit.Attribute.Value = value.ToString();
                }
            }

            if (control is BizComboBox)
            {
                var combo = (BizComboBox)control;
                combo.Attribute.Value = Guid.Parse(value.ToString());
            }

            //TODO: Дописать метод обновления для других обновляемых полей
        }

        private static BizControl FindControlById(Guid controlId, IEnumerable<BizControl> controls)
        {
            if(controls != null)
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

        public void Save()
        {
            var client = Presentation.Manager;
            BizForm = client.SaveForm(BizForm);
        }
    }
}