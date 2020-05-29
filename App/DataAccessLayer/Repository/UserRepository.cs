using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class UserRepository: IUserRepository //, IDisposable
    {
        public IDataContext DataContext { get; private set; }
//        private readonly bool _ownDataContext;

        public IOrgRepository OrgRepo { get; private set; }

        public UserRepository(IDataContext dataContext)
        {
            /*if (dataContext == null)
            {
                var dataContextFactory = DataContextFactoryProvider.GetFactory(); //new DataContext();
                DataContext = dataContextFactory.CreateMultiDc();
                _ownDataContext = true;
            }
            else*/
            DataContext = dataContext;

            OrgRepo = new OrgRepository(DataContext);
        }

        public UserRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            DataContext = dataContext; //provider.Get<IDataContext>();
            OrgRepo = provider.Get<IOrgRepository>();
        }

        // TODO: Remove constructor
        public UserRepository() : this(null) {}

        public static readonly ObjectCache<UserInfo> UserInfoCache = new ObjectCache<UserInfo>(); 

        //[SmartCache(TimeOutSeconds = 3600)]
        public Guid GetUserId(string userName)
        {
            var userInfo =
                UserInfoCache./*GetItems().*/FirstOrDefault(o => o.CachedObject.UserName.ToUpper() == userName.ToUpper());

            if (userInfo != null)
                return userInfo.CachedObject.Id;

            var query = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Worker>()
                .FirstOrDefault(w => w.User_Name.ToUpper() == userName.ToUpper());

            if (query == null)
                throw new ApplicationException(String.Format("Пользователь \"{0}\" не найден", userName));

            return query.Id;
        }

        //[SmartCache(TimeOutSeconds = 3600)]
        public UserInfo FindUserInfo(Guid userId)
        {
            var cached = UserInfoCache.Find(userId);
            if (cached != null) return cached.CachedObject;

            var userData = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Worker>().FirstOrDefault(w => w.Id == userId);

            if (userData == null) return null;

            var userInfo = GetUserInfo(DataContext, userData);

            UserInfoCache.Add(userInfo, userId);
            return userInfo;
        }

        public UserInfo GetUserInfo(Guid userId)
        {
            var userInfo = FindUserInfo(userId);

            if (userInfo == null)
                throw new ApplicationException(String.Format("Пользователь \"{0}\" не найден!", userId));

            return userInfo;
        }

        public UserInfo FindUserInfo(string userName)
        {
            var userInfo =
                UserInfoCache/*.GetItems()*/.FirstOrDefault(o => o.CachedObject.UserName.ToUpper() == userName.ToUpper());

            if (userInfo != null)
                return userInfo.CachedObject;

            var userData = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Worker>()
                .FirstOrDefault(w => w.User_Name.ToUpper() == userName.ToUpper() &&
                    (w.Deleted == null || w.Deleted == false));

            if (userData == null) return null;

            var info = GetUserInfo(DataContext, userData);

            UserInfoCache.Add(info, info.Id);
            return info;
        }

        public UserInfo GetUserInfo(string userName)
        {
            var info = FindUserInfo(userName);

            if (info == null)
                throw new ApplicationException(String.Format("Пользователь \"{0}\" не найден!", userName));

            return info;
        }

        internal UserInfo GetUserInfo(IDataContext context, Worker user)
        {
//            Guid? orgUnitTypeId = null;
//            string orgUnitTypeName = "";
//            string positionName = "";

            var positionId = user.OrgPosition_Id;
/*
            if (!user.Org_PositionsReference.IsLoaded) user.Org_PositionsReference.Load();
            if (user.Org_Positions != null)
            {
                positionName = user.Org_Positions.Full_Name;
                if (user.Org_Positions.Parent_Id != null)
                {
                    var orgUnit = context.Defs<Org_Unit>()
                        .FirstOrDefault(ou => ou.Id == user.Org_Positions.Parent_Id);

                    if (orgUnit != null)
                    {
                        orgUnitTypeId = user.Org_Positions.Parent_Id;
                        orgUnitTypeName = orgUnit.Full_Name;
                    }
                }
//                if (!user.Org_Positions.ParentReference.IsLoaded) user.Org_Positions.ParentReference.Load();
//                if (user.Org_Positions.Parent is Org_Unit)
//                {
//                    orgUnitTypeId = user.Org_Positions.Parent_Id;
//                    orgUnitTypeName = user.Org_Positions.Parent.Full_Name;
//                }
            }
*/

            var orgPosition = positionId != null ? OrgRepo.FindOrgPosition((Guid) positionId) : null;
            var orgUnitType = orgPosition != null && orgPosition.OrgTypeId != null
                                  ? OrgRepo.FindOrgType((Guid) orgPosition.OrgTypeId)
                                  : null;
            /*Organization organization = null;

            if (user.Parent_Id != null)
            {
                organization = context.Defs<Organization>()
                    .FirstOrDefault(o => o.Id == user.Parent_Id);
            }*/
            var orgInfo = user.Parent_Id != null ? OrgRepo.Find((Guid) user.Parent_Id) : null;

            return new UserInfo
                       {
                           Id = user.Id,
                           UserName = user.User_Name,
                           FirstName = user.First_Name ?? "",
                           LastName = user.Last_Name ?? "",
                           Position = orgPosition,
                           PositionId = positionId,
                           PositionName = orgPosition != null ? orgPosition.Name : String.Empty,
                           OrgUnitTypeId = orgPosition != null ? orgPosition.OrgTypeId : null,
                           OrgUnitTypeName = orgUnitType != null ? orgUnitType.Name : String.Empty,
                           Organization = orgInfo,
                           OrganizationId = orgInfo != null ? orgInfo.Id : (Guid?) null,
                           OrganizationTypeId = orgInfo != null ? orgInfo.TypeId : null,
                           OrganizationName = orgInfo != null ? orgInfo.Name : String.Empty,
                           OrganizationCode = orgInfo != null ? orgInfo.Code : String.Empty,
                           LanguageId = user.Language_Id ?? 0
                       };
        }

        public BizResult ChangeUserPassword(string userName, string oldPassword, string newPassword)
        {
            try
            {
                var edc = DataContext.GetEntityDataContext();
                var userInfo = GetUserInfo(userName);

                var user = edc.Entities.Object_Defs.OfType<Worker>().FirstOrDefault(w =>
                                                                     w.Id == userInfo.Id &&
                                                                     w.User_Password == oldPassword);
                if (user == null)
                    return new BizResult {Type = BizResultType.Error, Message = "Неверное имя пользователя или пароль!"};

                user.User_Password = newPassword;
                edc.SaveChanges();
                UserAccountCache.Remove(userName);
            }
            catch
            {
                return new BizResult {Type = BizResultType.Error, Message = "Неверное имя пользователя или пароль!"};
            }

            return new BizResult {Type = BizResultType.Message, Message = "Смена пароля прошла успешно."};
        }

        protected static readonly ObjectKeyCache<String, UserAccount> UserAccountCache = new ObjectKeyCache<String, UserAccount>(3600);

        public static int UserAccountCount { get { return UserAccountCache.Count; } }

        private static UserAccount FindUser(string userName, string password)
        {
            /*lock (UserAccountCache.Lock)
                return (
                    from account in UserAccountCache.GetItems()
                    where
                        String.Equals(account.CachedObject.UserName, userName, StringComparison.OrdinalIgnoreCase) &&
                        account.CachedObject.Password == password
                    select account.CachedObject).FirstOrDefault();*/
            var account =
                UserAccountCache.FirstOrDefault(
                    a => String.Equals(a.CachedObject.UserName, userName, StringComparison.OrdinalIgnoreCase) &&
                         a.CachedObject.Password == password);

            return account != null ? account.CachedObject : null;
        }

        public Guid? FindUserId(string userName, string password)
        {
            foreach (var id in 
                DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Worker>().Where(w =>
                    w.User_Name.ToUpper() == userName.ToUpper() &&
                    w.User_Password == password &&
                    (w.Deleted == null || w.Deleted == false)).Select(w => w.Id))
            {
                return id;
            }
            return null;
        }

        public bool Validate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException();
            }

            if (FindUser(userName, password) != null) return true;

            var userExists = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Worker>().Where(w =>
                                                              w.User_Name.ToUpper() == userName.ToUpper() &&
                                                              w.User_Password == password &&
                                                              (w.Deleted == null || w.Deleted == false)).Select(w => w.Id).Any();

            if (userExists) UserAccountCache.AddOrSet(new UserAccount(userName, password), userName.ToUpper());

            return userExists;
        }

        public static readonly ObjectCache<UserOrgs> UserOrgCache = new ObjectCache<UserOrgs>();

        private IEnumerable<Guid> FindUserAccessOrgs(Guid userId)
        {
            var userInfo = GetUserInfo(userId);

            if (userInfo.OrganizationId != null && userInfo.OrganizationTypeId != null)
                return DataContext.GetEntityDataContext().Entities.ExecuteStoreQuery<Guid>(
                    "SELECT org.[Id] FROM [Organizations] org " +
                        "JOIN [Object_Defs] od ON od.Id = org.Id " +
                    "WHERE (org.[Id] = {0} " +
                    "OR EXISTS(SELECT * FROM [OrgUnits_ObjectDefs] WHERE [OrgUnit_Id] = {1} AND [ObjDef_Id] = org.[Type_Id])) " +
                    "AND (od.Deleted is null OR od.Deleted = 0)",
                    (Guid) userInfo.OrganizationId,
                    (Guid) userInfo.OrganizationTypeId);

            if (userInfo.OrganizationId != null)
                return new[] {(Guid) userInfo.OrganizationId};

            return new List<Guid>();
        }

        public IEnumerable<Guid> GetUserAccessOrgs(Guid userId)
        {
            var cached = UserOrgCache.Find(userId);
            if (cached != null)
                return cached.CachedObject.Orgs;

            var userOrgs = new UserOrgs(userId, FindUserAccessOrgs(userId));

            UserOrgCache.Add(userOrgs, userId);

            return userOrgs.Orgs;
        }

        public void SetUserLanguage(Guid userId, int languageId)
        {
            var edc = DataContext.GetEntityDataContext();
            var user = edc.Entities.Object_Defs.OfType<Worker>().FirstOrDefault(w => w.Id == userId);

            if (user == null) return;

            var langId = user.Language_Id ?? 0;
            if (langId != languageId)
            {
                user.Language_Id = languageId;

                try
                {
                    edc.SaveChanges();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "UserRepository.SetUserLanguage");
                    throw;
                }
                UserInfoCache.Remove(userId);
            }
        }

        public IEnumerable<Guid> GetUserAccessUsers(Guid? userId)
        {
            /*var orgs = "";
            foreach (var orgId in GetUserAccessOrgs(userId))
            {
                if (orgs.Length > 0) orgs += ",";
                orgs += String.Format("'{0}'", orgId);
            }
            if (orgs.Length > 0)
                return DataContext.Entities.ExecuteStoreQuery<Guid>(
                    "SELECT w.[Id] FROM [Workers] w " +
                    "WHERE EXISTS(SELECT * FROM Object_Defs WHERE [Id] = w.[Id] AND [Parent_Id] IN ({0})) ", orgs);*/
            var edc = DataContext.GetEntityDataContext();
            if (userId != null)
            {
                var orgs = GetUserAccessOrgs((Guid) userId).ToList();
                if (orgs.Any())
                    return
                        edc.Entities.Object_Defs.OfType<Worker>()
                            .Where(w => (w.Deleted == null || w.Deleted == false) && orgs.Contains(w.Parent.Id))
                            .Select(w => w.Id);
                return new List<Guid> { (Guid) userId };
            }
            return
                edc.Entities.Object_Defs.OfType<Worker>()
                    .Where(w => w.Deleted == null || w.Deleted == false)
                    .Select(w => w.Id);
        }

        /*public void Dispose()
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
                    Logger.OutputLog(e, "UserRepository.Dispose");
                    throw;
                }
            }
        }

        ~UserRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "UserRepository.Finalize");
                    throw;
                }
        } */

        protected class UserAccount
        {
            public string UserName { get; private set; }
            public string Password { get; private set; }

            public UserAccount(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }
        }

        public class UserOrgs
        {
            public Guid UserId { get; set; }
            private readonly List<Guid> _orgs = new List<Guid>();
            public List<Guid> Orgs { get { return _orgs; } }

            public UserOrgs(Guid userId, IEnumerable<Guid> orgs)
            {
                UserId = userId;
                Orgs.AddRange(orgs);
            }
        }
    }
}
