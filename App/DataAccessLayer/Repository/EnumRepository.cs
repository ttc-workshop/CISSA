using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class EnumRepository: IEnumRepository, IDisposable
    {
        public IDataContext DataContext { get; private set; }
        // TODO: Исключить IDisposable, _ownDataContext после изменения cissa-with-children ссылок
        private readonly bool _ownDataContext;

        private ILanguageRepository LangRepo { get; set; }

        public EnumRepository(IDataContext dataContext)
        {
            if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else
                DataContext = dataContext;

            LangRepo = new LanguageRepository(DataContext);
        }

        public EnumRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            DataContext = dataContext; //provider.Get<IDataContext>();
            LangRepo = provider.Get<ILanguageRepository>();
        }

        public EnumRepository() : this(null) {}

        public static readonly ObjectCache<EnumDef> EnumDefCache = new ObjectCache<EnumDef>();

        public EnumDef Find(Guid id)
        {
            var cached = EnumDefCache.Find(id);

            if (cached != null)
                return Clone(cached.CachedObject);

            var dbEnumDef = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Enum_Def>().FirstOrDefault(e => e.Id == id);
            if (dbEnumDef == null) return null;

            var enumDef = new EnumDef
            {
                Id = dbEnumDef.Id,
                Caption = dbEnumDef.Full_Name,
                Description = dbEnumDef.Description,
                Name = dbEnumDef.Name,
                EnumItems = new List<EnumValue>(LoadEnumItems(id))
            };

            EnumDefCache.Add(enumDef, id);

            return Clone(enumDef);
        }

        public EnumDef Get(Guid enumDefId)
        {
            var enumDef = Find(enumDefId);

            if (enumDef == null) throw new ApplicationException(String.Format("Справочник с Id=\"{0}\" не найден!", enumDefId));

            return enumDef;
        }

        public EnumDef Find(string enumDefName)
        {
            if (String.IsNullOrEmpty(enumDefName))
                return null;

            var cached = EnumDefCache./*GetItems().*/FirstOrDefault(i => String.Equals(i.CachedObject.Name, enumDefName, StringComparison.OrdinalIgnoreCase));

            if (cached != null)
                return Clone(cached.CachedObject);

            var enumDefId =
                DataContext.GetEntityDataContext()
                    .Entities.Object_Defs.OfType<Enum_Def>()
                    .Where(e => e.Name != null && e.Name.ToUpper() == enumDefName.ToUpper())
                    .Select(e => e.Id)
                    .FirstOrDefault();

            return enumDefId != Guid.Empty ? Find(enumDefId) : null;
        }

        public EnumDef Get(string enumDefName)
        {
            var enumDef = Find(enumDefName);

            if (enumDef == null) throw new ApplicationException(String.Format("Справочник \"{0}\" не найден!", enumDefName));

            return enumDef;
        }

        /// <summary>
        /// Позволяет проверить действительно ли значение принадлежит данному справочнику
        /// </summary>
        /// <param name="enumId">Идентификатор справочника</param>
        /// <param name="enumValueId">Идентификатор проверяемого значения</param>
        /// <returns>True если значение имеется в справочнике</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        public bool CheckEnumValue(Guid enumId, Guid enumValueId)
        {
            var enumDef = Get(enumId);
            return enumDef.EnumItems.Any(i => i.Id == enumValueId);
        }

        /// <summary>
        /// Позволяет получить идентификатор (значение) элемента перечисления по имени
        /// </summary>
        /// <param name="enumId">Идентификатор перечисления</param>
        /// <param name="enumValue">Имя значения перечисления</param>
        /// <returns>Идентификатор значения справочника</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        public Guid GetEnumValueId(Guid enumId, string enumValue)
        {
            var enumDef = Get(enumId);
            return enumDef.EnumItems.Where(i => String.Equals(i.Value, enumValue, StringComparison.OrdinalIgnoreCase)).Select(i => i.Id).First();
        }

        /// <summary>
        /// Позволяет получить идентификатор (значение) элемента перечисления по имени
        /// </summary>
        /// <param name="enumDefName">Наименование перечисления</param>
        /// <param name="enumValue">Имя значения перечисления</param>
        /// <returns>Идентификатор значения справочника</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        public Guid GetEnumValueId(string enumDefName, string enumValue)
        {
            var enumDef = Get(enumDefName);
            return enumDef.EnumItems.Where(i => String.Equals(i.Value, enumValue, StringComparison.OrdinalIgnoreCase)).Select(i => i.Id).First();
        }

        //[SmartCache(TimeOutSeconds = 600)]
        public string GetEnumValue(Guid enumId, Guid valueId)
        {
            var enumDef = Get(enumId);
            return enumDef.EnumItems.Where(i => i.Id == valueId).Select(i => i.Value).First();
        }

        /// <summary>
        /// Загружает справочник
        /// </summary>
        /// <param name="enumId">Идентификатор справочника</param>
        /// <returns>Список значений справочника</returns>
        //[SmartCache(TimeOutSeconds = 600)]
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

        /// <summary>
        /// Загружает справочник
        /// </summary>
        /// <param name="enumId">Идентификатор справочника</param>
        /// <returns>Список значений справочника</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        protected IList<EnumValue> LoadEnumItems(Guid enumId)
        {
            var query = from item in DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Enum_Item>()
                        where item.Parent_Id == enumId && (item.Deleted == null || item.Deleted == false)
                        orderby item.Order_Index, item.Full_Name
                        select new EnumValue
                        {
                            Id = item.Id,
                            Code = item.Name,
                            Value = item.Full_Name ?? item.Name,
                            DefaultValue = item.Full_Name ?? item.Name
                        };

            return query.ToList();
        }

        /// <summary>
        /// Загружает справочник
        /// </summary>
        /// <param name="enumDefName">Наименование справочника</param>
        /// <returns>Список значений справочника</returns>
        //[SmartCache(TimeOutSeconds = 600)]
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

        public IList<EnumValue> GetEnumItems(string enumDefName, int languageId)
        {
            var def = Get(enumDefName);

            return def.EnumItems.Select(item => new EnumValue
            {
                Id = item.Id,
                Code = item.Code,
                Value = LangRepo.GetTranslation(item.Id, languageId) ?? item.DefaultValue,
                DefaultValue = item.DefaultValue
            }).ToList();
        }

        /// <summary>
        /// Получает идентификатор перечисления
        /// </summary>
        /// <param name="enumDefName">Имя перечисления</param>
        /// <returns>Идентификатор перечисления</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        public Guid GetEnumDefId(string enumDefName)
        {
            var cached =
                EnumDefCache /*.GetItems()*/
                    .FirstOrDefault(
                        i => String.Equals(i.CachedObject.Name, enumDefName, StringComparison.OrdinalIgnoreCase));

            if (cached != null)
                return cached.CachedObject.Id;

            var query = from item in DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Enum_Def>()
                        where (item.Name.ToUpper() == enumDefName.ToUpper() ||
                               item.Full_Name.ToUpper() == enumDefName.ToUpper()) &&
                              (item.Deleted == null || item.Deleted == false)
                        select item.Id;

            if (!query.Any())
                throw new ApplicationException(
                    string.Format("Перечисления с именем '{0}' не существует.", enumDefName));

            return query.First();
        }

        /// <summary>
        /// Вернуть объект перечисления по идентификатору перечисления
        /// </summary>
        /// <param name="enumId">Идентификатор перечисления</param>
        /// <returns>Объект перечисления</returns>
        //[SmartCache(TimeOutSeconds = 3600)]
        public EnumValue GetValue(Guid enumId)
        {
            /*foreach (var cached in EnumDefCache.GetItems())
            {
                var item = cached.CachedObject.EnumItems.FirstOrDefault(i => i.Id == enumId);

                if (item != null) return item;
            }*/
            var cached =
                EnumDefCache.FirstOrDefault(item => item.CachedObject.EnumItems.FirstOrDefault(i => i.Id == enumId) != null);
            if (cached != null)
                return cached.CachedObject.EnumItems.First(i => i.Id == enumId);

            var query = from item in DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Enum_Item>()
                        where item.Id == enumId
                        orderby item.Order_Index, item.Full_Name
                        select new EnumValue
                                   {
                                       Id = item.Id,
                                       Code = item.Name,
                                       Value = item.Full_Name ?? item.Name,
                                       DefaultValue = item.Full_Name ?? item.Name
                                   };

            var enumValue = query.FirstOrDefault();

            if (enumValue == null) return null;

            return new EnumValue
                       {
                           Id = enumValue.Id,
                           Value = enumValue.Value,
                           Code = enumValue.Code,
                           DefaultValue = enumValue.DefaultValue
                       };
        }

        public void TranslateEnumItems(List<EnumValue> items, int languageId)
        {
            if (items == null) return;

            if (languageId != 0)
            {
                foreach (var item in items)
                {
                    item.Value = LangRepo.GetTranslation(item.Id, languageId);
                    if (String.IsNullOrEmpty(item.Value)) item.Value = item.DefaultValue;
                }
            }
            else
                foreach (var item in items)
                {
                    item.Value = item.DefaultValue;
                }
        }

        public EnumDef Clone(EnumDef def)
        {
            return new EnumDef
                              {
                                  Id = def.Id,
                                  Name = def.Name,
                                  Caption = def.Caption,
                                  Description = def.Description,
                                  EnumItems = def.EnumItems.Select(item => new EnumValue
                                                    {
                                                        Id = item.Id,
                                                        Code = item.Code,
                                                        Value = item.Value,
                                                        DefaultValue = item.DefaultValue
                                                    }).ToList()
                              };
        }

        public void Dispose()
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "EnumRepository.Dispose");
                    throw;
                }
            }
        }

        ~EnumRepository()
        {
            if (_ownDataContext && DataContext != null)
                try 
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "EnumRepository.Finalize");
                    throw;
                }
        }
    }
}
