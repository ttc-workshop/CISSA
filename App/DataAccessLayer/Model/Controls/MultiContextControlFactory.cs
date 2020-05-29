using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    // TODO: Нужен ли MultiContextControlFactory?
    public class MultiContextControlFactory : IControlFactory
    {
        private IMultiDataContext DataContext { get; set; }

        private readonly IList<IControlFactory> _repositories = new List<IControlFactory>();

        public MultiContextControlFactory(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new ControlFactory(provider, context));
            }
        }
        public BizControl Create(Control control)
        {
            return _repositories.Select(repo => repo.Create(control)).FirstOrDefault(c => c != null);
        }

        /*public void InitComboBoxItems(BizComboBox combo, AttrDef attr)
        {
            _repositories.ForEach(repo => repo.InitComboBoxItems(combo, attr));
        }*/
    }
}