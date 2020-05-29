using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Raven.Abstractions.Extensions;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextEnumRepository : IEnumRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IEnumRepository> _repositories = new List<IEnumRepository>();

        public MultiContextEnumRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new EnumRepository(provider, context));
            }
        }

        public bool CheckEnumValue(Guid enumId, Guid enumValueId)
        {
            return _repositories.Any(repo => repo.CheckEnumValue(enumId, enumValueId));
        }

        public Guid GetEnumValueId(Guid enumId, string enumValue)
        {
            var enumDef = Get(enumId);
            return enumDef.EnumItems.Where(i => String.Equals(i.Value, enumValue, StringComparison.OrdinalIgnoreCase)).Select(i => i.Id).First();
        }

        public Guid GetEnumValueId(string enumDefName, string enumValue)
        {
            var enumDef = Get(enumDefName);
            return enumDef.EnumItems.Where(i => String.Equals(i.Value, enumValue, StringComparison.OrdinalIgnoreCase)).Select(i => i.Id).First();
        }

        public string GetEnumValue(Guid enumId, Guid valueId)
        {
            var enumDef = Get(enumId);
            return enumDef.EnumItems.Where(i => i.Id == valueId).Select(i => i.Value).First();
        }

        public EnumValue GetValue(Guid valueId)
        {
            return _repositories.Select(repo => repo.GetValue(valueId)).FirstOrDefault(v => v != null);
        }

        public IList<EnumValue> GetEnumItems(Guid enumId)
        {
            var def = Get(enumId);

            return def.EnumItems.Select(item => new EnumValue
            {
                Id = item.Id,
                Code = item.Code,
                Value = item.Value,
                DefaultValue = item.DefaultValue
            }).ToList();
        }

        public IList<EnumValue> GetEnumItems(string enumDefName)
        {
            var def = Get(enumDefName);

            return def.EnumItems.Select(item => new EnumValue
            {
                Id = item.Id,
                Code = item.Code,
                Value = item.Value,
                DefaultValue = item.DefaultValue
            }).ToList();
        }

        public Guid GetEnumDefId(string enumDefName)
        {
            var enumDef = Get(enumDefName);

            return enumDef.Id;
        }

        public EnumDef Find(Guid id)
        {
            return _repositories.Select(repo => repo.Find(id)).FirstOrDefault(def => def != null);
        }

        public EnumDef Get(Guid id)
        {
            var enumDef = Find(id);

            if (enumDef == null) throw new ApplicationException(String.Format("Справочник с Id=\"{0}\" не найден!", id));

            return enumDef;
        }

        public EnumDef Find(string enumDefName)
        {
            return _repositories.Select(repo => repo.Find(enumDefName)).FirstOrDefault(def => def != null);
        }

        public EnumDef Get(string enumDefName)
        {
            var enumDef = Find(enumDefName);

            if (enumDef == null) throw new ApplicationException(String.Format("Справочник \"{0}\" не найден!", enumDefName));

            return enumDef;
        }

        public void TranslateEnumItems(List<EnumValue> items, int languageId)
        {
            _repositories.ForEach(repo => repo.TranslateEnumItems(items, languageId));
        }
    }
}