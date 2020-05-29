using System;
using System.IO;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Utils
{
    public class BizConnectionException : ApplicationException
    {
        public BizConnectionException(string message) : base(message) { }
    }

    public class BizConnection
    {
//        public static BizConnection Connection = null;
/*
        public static bool LogOn(string userName, string password)
        {
            try
            {
                if (Connection != null) LogOut();

                var conn = new BizConnection(userName, password);

                if (conn.Connected)
                {
                    Connection = conn;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void LogOut()
        {
            /*if (Connection != null && Connection.Connected)
                Connection._bizService.Close();♥1♥

            Connection = null;
        }*/
/*

        public static bool IsConnected()
        {
            return (Connection != null) && Connection.Connected;
        }

        private static void CheckConnected()
        {
            if (!IsConnected())
                throw new BizConnectionException("Соединение с сервисом бизнес-логики не установлено!");
        }

        public static string GetUserName()
        {
            CheckConnected();
            return Connection.UserName;
        }

        public static Guid GetUserId()
        {
            CheckConnected();
            return Connection.UserId;
        }

        public static UserInfo GetUserInfo()
        {
            CheckConnected();
            return Connection.Info;
        }

        public static IUserManager UserManager
        {
            get
            {
                CheckConnected();

                return Connection._bizService;
            }
        }

        public static IDocManager DocumentManager
        {
            get
            {
                CheckConnected();

                var dm = new DocManagerClient();
                dm.ClientCredentials.UserName.UserName = Connection.UserName;
                dm.ClientCredentials.UserName.Password = Connection.Password;
                dm.Open();

                return dm;
            }
        }

        public static IPresentationManager PresentationManager
        {
            get
            {
                CheckConnected();

                var pm = new PresentationManagerClient();
                pm.ClientCredentials.UserName.UserName = Connection.UserName;
                pm.ClientCredentials.UserName.Password = Connection.Password;
                pm.Open();

                return pm;
            }
        }

        public static IWorkflowManager WorkflowManager
        {
            get
            {
                CheckConnected();

                var wm = new WorkflowManagerClient();
                wm.ClientCredentials.UserName.UserName = Connection.UserName;
                wm.ClientCredentials.UserName.Password = Connection.Password;
                wm.Open();

                return wm;
            }
        }

        public static IReportManager ReportManager
        {
            get
            {
                CheckConnected();

                var rm = new ReportManagerClient();
                rm.ClientCredentials.UserName.UserName = Connection.UserName;
                rm.ClientCredentials.UserName.Password = Connection.Password;
                rm.Open();

                return rm;
            }
        }

        public static IQueryManager QueryManager
        {
            get
            {
                CheckConnected();

                var qm = new QueryManagerClient();
                qm.ClientCredentials.UserName.UserName = Connection.UserName;
                qm.ClientCredentials.UserName.Password = Connection.Password;
                qm.Open();

                return qm;
            }
        }
*/

        public string UserName { get; private set; }
        public string Password { get; private set; }
        public int Code { get; private set; }

        protected internal BizConnection(string userName, string password)
        {
            UserName = userName;
            Password = password;
            Connected = false;
            var r = new Random();
            Code = r.Next();

        }

        public bool Connect()
        {
            var bizService = new UserManagerClient();
            try
            {
                if (bizService.ClientCredentials != null)
                {
                    bizService.ClientCredentials.UserName.UserName = UserName;
                    bizService.ClientCredentials.UserName.Password = Password;
                }
                bizService.Open();

                Connected = bizService.TryConnect();

                if (Connected)
                {
                    Info = bizService.GetUserInfo();
                    UserId = Info.Id;
                }
                return Connected;
            }
            catch(Exception e)
            {
                try
                {
                    using (var writer = new StreamWriter("c:\\distr\\cissa\\ConnectionErrors.log", true))
                    {
                        writer.WriteLine("{0}: \"{1}\"; message: \"{2}\"", DateTime.Now, UserName, e.Message);
                        if (e.InnerException != null)
                            writer.WriteLine("  - inner exception: \"{0}\"", e.InnerException.Message);
                        writer.WriteLine("  -- Stack: {0}", e.StackTrace);
                    }
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                try 
                { 
                    bizService.Close();
                }
                catch
                {
                    bizService.Abort();
                    throw;
                }
            }
            return false;
        }

        public void Disconnect()
        {
        }

        public Guid UserId { get; private set; }
        public UserInfo Info { get; private set; }

        public bool Connected { get; private set; }
    }
}