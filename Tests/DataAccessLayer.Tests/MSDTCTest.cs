using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.Transactions;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class MSDTCTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string connectionString1 =
            "Data Source=195.38.189.100;Initial Catalog=cissa-with-children;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
            string connectionString2 =
            "Data Source=localhost;Initial Catalog=asist_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

            using (TransactionScope transactionScope = new TransactionScope())
            {
                SqlConnection connectionOne = new SqlConnection(connectionString1);
                SqlConnection connectionTwo = new SqlConnection(connectionString2);

                try
                {
                    //2 connections, nested
                    connectionOne.Open();
                    connectionTwo.Open(); // escalates to DTC on 05 and 08
                    connectionTwo.Close();
                    connectionOne.Close();

                    Console.WriteLine("Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                    throw;
                }
                finally
                {
                    connectionOne.Dispose();
                    connectionTwo.Dispose();
                }
            }
        }
    }
}
