using System;
using System.Collections;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    public class DocList : IEnumerable<Doc>//, IDisposable
    {
        public List<Guid> DocIdList { get; private set; }
        protected Guid UserId { get; private set; }

        public IAppServiceProvider Provider { get; private set; }
        //private readonly bool _ownProvider = false;
        //public IDataContext DataContext { get; private set; }
        //private readonly bool _ownDataContext;

        public DocList(IAppServiceProvider provider, List<Guid> list, Guid userId)
        {
            Provider = provider;
            //_ownProvider = false;

            //DataContext = Provider.Get<IDataContext>();

            DocIdList = list;
            UserId = userId;
        }

        /*public DocList(IDataContext dataContext, List<Guid> list, Guid userId)
        {
            /*if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else#1#
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();
            _ownProvider = true;

            /*if (dataContext == null)
                DataContext = Provider.Get<IDataContext>();
            else
                DataContext = dataContext;#1#

            DocIdList = list;
            UserId = userId;
        }*/

        /*public DocList(List<Guid> list) : this(null, list, Guid.Empty) {}
        public DocList(List<Guid> list, Guid userId) : this(null, list, userId) {}*/

        public IEnumerator<Doc> GetEnumerator()
        {
            return new DocListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal class DocListEnumerator: IEnumerator<Doc>
        {
            private readonly DocList _docList;
            private int _index = -1;
            private IDocRepository _docRepo;

            public DocListEnumerator(DocList docList)
            {
                _docList = docList;
                _docRepo = docList.Provider.Get<IDocRepository>();
                    //new DocRepository(docList.DataContext, docList.UserId);
            }

            public void Dispose()
            {
                /*if (_docRepo != null)
                {
                    _docRepo.Dispose();
                    _docRepo = null;
                }*/
            }

            public bool MoveNext()
            {
                var result = _docList != null && _docList.DocIdList != null && _index < (_docList.DocIdList.Count - 1);

                if (result) _index++;

                return result;
            }

            public void Reset()
            {
                _index = -1;
            }

            public Doc Current
            {
                get
                {
                    var docId = _docList.DocIdList[_index];

                    return _docRepo.LoadById(docId);
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            /*~DocListEnumerator()
            {
                if (_docRepo != null)
                    _docRepo.Dispose();
            }*/
        }

        /*public void Dispose()
        {
            try
            {
                if (_ownDataContext && DataContext != null)
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
            }
            catch(Exception e)
            {
                Logger.OutputLog(e, "DocList.Dispose");
                throw;
            }
        }

        ~DocList()
        {
            if (_ownDataContext && DataContext != null)
               try
               {
                   DataContext.Dispose();
               }
               catch (Exception e)
               {
                   Logger.OutputLog(e, "DocList.Finalize");
                   throw;
               }
        }*/
    }
}
