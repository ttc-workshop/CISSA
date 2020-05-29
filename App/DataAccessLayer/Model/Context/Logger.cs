using System;
using System.Data.Entity.Core.EntityClient;
using System.IO;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public static class Logger
    {
        private static string _dbName = String.Empty;

        public static string DatabaseName
        {
            get { return _dbName; }
            set { _dbName = value; }
        }

        private static string GetDatabaseName()
        {
            /*if (String.IsNullOrEmpty(_dbName))
            {
                var conn = new EntityConnection("name=cissaEntities");
                try
                {
                    _dbName = conn.StoreConnection.Database;
                }
                catch
                {
                    // ignored
                }
            }*/
            return _dbName;
        }

        public static string GetLogFileName(string filename)
        {
            var dbName = GetDatabaseName();

            var s = (!String.IsNullOrEmpty(dbName) ? GetDatabaseName() + "-" : "") +
                    DateTime.Today.ToString("yyyy-MM-dd");
            return String.Format("c:\\distr\\cissa\\{1}-{0}.log", s, filename);
        }

        public static void OutputLog(string fileName, string message)
        {
            try
            {
                using (var writer = new StreamWriter(fileName, true))
                {
                    writer.WriteLine("{0}: {1}", DateTime.Now, message);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void OutputLog(string fileName, Exception e, string msg)
        {
            try
            {
                using (var writer = new StreamWriter(fileName, true))
                {
                    writer.WriteLine("{0}: {1} : \"{2}\"", DateTime.Now, msg, e.Message);
                    if (e.InnerException != null)
                    {
                        writer.WriteLine("  InnerException: " + e.InnerException.Message);
                    }
                    writer.WriteLine("   StackTrace: " + e.StackTrace);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static void OutputLog(Exception e, string msg)
        {
            try
            {
                using (var writer = new StreamWriter(GetLogFileName("General"), true))
                {
                    writer.WriteLine("{0}: {1} : \"{2}\"", DateTime.Now, msg, e.Message);
                    if (e.InnerException != null)
                    {
                        writer.WriteLine("  InnerException: " + e.InnerException.Message);
                    }
                    writer.WriteLine("   StackTrace: " + e.StackTrace);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}