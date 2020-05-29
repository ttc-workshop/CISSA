using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Languages;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface ILanguageRepository //: IDisposable
    {
        IList<LanguageType> Load();
        string GetTranslation(Guid defId, int languageId);
    }
}