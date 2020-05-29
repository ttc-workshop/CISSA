using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;

namespace Intersoft.CISSA.DataAccessLayer.Model.Misc
{
    public interface IComboBoxEnumProvider
    {
        List<EnumValue> GetEnumDocumentValues(BizComboBox combo, DocDef detailDocDef);
        List<EnumValue> GetEnumDocumentValues(BizComboBox combo, AttrDef attrDef);
        List<EnumValue> GetEnumDocumentValues(AttrDef attrDef, params string[] attrNames);
        List<EnumValue> GetEnumUserValues();
        List<EnumValue> GetEnumOrganizationValues(Guid? orgTypeId);

        // New 2015-07-14
        IList<EnumValue> GetFormComboBoxValues(BizForm form, BizComboBox comboBox);
        IList<EnumValue> GetFormComboBoxValues(BizComboBox comboBox, AttrDef attrDef);
        IList<EnumValue> GetFormComboBoxValues(BizForm form, BizComboBox comboBox, AttrDef attrDef);

        string GetComboBoxDetailValue(BizForm form, BizComboBox comboBox);
        string GetComboBoxDetailValue(BizComboBox comboBox, AttrDef attrDef);
    }
}