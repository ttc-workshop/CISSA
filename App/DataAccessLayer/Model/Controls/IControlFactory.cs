using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    public interface IControlFactory
    {
        BizControl Create(Control control);
        // void InitComboBoxItems(BizComboBox combo, AttrDef attr);
    }
}