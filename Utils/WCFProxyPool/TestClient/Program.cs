using System;
using System.Collections.Generic;
using System.Text;

namespace TestServiceClient
{
    class Program
    {
        static ConsoleColor originalColor = Console.ForegroundColor;
        static bool doContinuousTests = false;

        static void Main(string[] args)
        {
            int count = 0;
            int minutes = 0;
            while (true)
            {
                Console.WriteLine("Do you want to run the tests continuously without prompting?");
                doContinuousTests = UserAnsweredYes();

                count++;
                string msg = string.Format("{0} {1} - Doing series of tests #{2} .... \n\n",DateTime.Now.ToShortDateString(),DateTime.Now.ToShortTimeString(),count);
                Console.WriteLine(msg);
                System.Diagnostics.Debug.WriteLine(msg);

                Console.WriteLine("Want to run a simple Multi-interface test? (10 seconds)");
                if (UserAnsweredYes())
                    DoMultiInterfaceTest();

                DoSpeedTest();

                Console.WriteLine("Do you want to run a TimeDelay Test (15 minutes)?");
                if (UserAnsweredYes())
                    DoTimeDelaytest();

                minutes = 13;
                
                Console.WriteLine("Do you want to wait for {0} minutes before runningmore tests?");
                if (UserAnsweredYes())
                {
                    Console.WriteLine("\nSleeping for {0} minutes\n", minutes);
                    //sleep for 30 minutes befre next loop of tests
                    System.Threading.Thread.Sleep(minutes*60*1000);
                }

                if (!doContinuousTests)
                    break;
            }
        }

        private static void DoTimeDelaytest()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("About to run time delay test....");

            Console.WriteLine("Running 1st service call....");
            using (ClientBasePoolProxy p4 = new ClientBasePoolProxy())
                {
                    p4.Op1("123");
                    p4.Close();
                }

            Console.WriteLine("Waiting for 30 seconds....");
            System.Threading.Thread.Sleep(30000);

            Console.WriteLine("Running 2nd service call....");
            using (ClientBasePoolProxy p5 = new ClientBasePoolProxy())
            {
                p5.Op1("123");
                p5.Close();
            }

            Console.WriteLine("Waiting for 1 minute and 30 seconds....");
            System.Threading.Thread.Sleep(90000);

            Console.WriteLine("Running 3rd service call....");
            using (ClientBasePoolProxy p6 = new ClientBasePoolProxy())
            {
                p6.Op1("123");
                p6.Close();
            }

            Console.WriteLine("Waiting for 11 minutes and 30 seconds....");
            System.Threading.Thread.Sleep(690000);

            Console.WriteLine("Running 4th service call....");
            using (ClientBasePoolProxy p7 = new ClientBasePoolProxy())
            {
                p7.Op1("123");
                p7.Close();
            }

            Console.WriteLine("Done.");
        }

       #region Simple Multi interface calls

       private static void DoMultiInterfaceTest()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Doing MultiInterface test. - Simple Test on different interfaces -");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\nInstantiating service1 interface proxy...");
            ClientBasePoolProxyPart2 p2 = new ClientBasePoolProxyPart2();
            string s = p2.MyOperation1("123");
            p2.Close();
            Console.WriteLine("Finished service1 interface proxy test, result={0}", s);

            Console.WriteLine("\nInstantiating service2 interface proxy..");

            ClientBasePoolProxy p1 = new ClientBasePoolProxy();
            s = p1.Op1("123");
            p1.Close();
            Console.WriteLine("Finished service2 interface proxy test, result={0}", s);

            Console.WriteLine("\nInstantiating another service1 interface proxy...");
            ClientBasePoolProxyPart2 p3 = new ClientBasePoolProxyPart2();
            s = p3.MyOperation1("123");
            p3.Close();
            Console.WriteLine("Finished service1 interface proxy test, result={0}", s);

            Console.WriteLine("\nInstantiating another service1 interface proxy...");
            ClientBasePoolProxy p4 = new ClientBasePoolProxy();
            s = p4.Op1("123");
            p4.Close();
            Console.WriteLine("Finished service2 interface proxy test, result={0}", s);

           // Cleaup as this will affect the other tests
           System.ServiceModel.ChannelPool.ChannelPoolFactory<TestServiceInterface.IService1>.Destroy();
           System.ServiceModel.ChannelPool.ChannelPoolFactory<TestServiceInterface.IService2>.Destroy();

        }

       #endregion

        #region DoSpeedTest

        private static void DoSpeedTest()
        {
            const int MAX = 20000;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("About to run timed tests for {0} iterations....",MAX);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch sw3 = new System.Diagnostics.Stopwatch();

            
            // This is the normal, proxy create execute, then destroy pattern
            int i;

            Console.WriteLine("Want to run some baseline tests using the standard proxy class?");
            if (UserAnsweredYes())
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Doing std tests as a baseline...");

                sw.Start();
                for (i = 0; i < MAX; i++)
                {
                    using (ClientProxy2 p = new ClientProxy2())
                    {
                        p.Open();   // can only do 127 of these at a single time....?
                        p.Op1("123");

                        p.Close();
                    }
                }
                sw.Stop();
            }
            TimeSpan normal = sw.Elapsed;

            Console.WriteLine("Want to run some tests using the ClientPoolBase class?");
            if (UserAnsweredYes())
            {
                Console.WriteLine("Doing ClientPoolBase tests ...");

                if (!System.ServiceModel.ChannelPool.ChannelPoolFactory<TestServiceInterface.IService2>.Initialise())
                    throw new Exception("Error initialising ClientBasePool!");

                // This is the same test as above but using a single proxy only. NO re-creation of any sort. This is the
                // performance GOAL to aim for, although due to additional overhead it will be almost impossible to achieve.
                // However, the closer we get to this figure, the better
                sw2.Start();
                for (i = 0; i < MAX; i++)
                {
                    using (ClientBasePoolProxy p4 = new ClientBasePoolProxy())
                    {
                        p4.Op1("123");
                        p4.Close();
                    }
                }
                sw2.Stop();
            }
            TimeSpan clientPoolBase = sw2.Elapsed;

            // Kill off the pool and associated threads so those running threads dont impact the next tests
            System.ServiceModel.ChannelPool.ChannelPoolFactory<TestServiceInterface.IService2>.Destroy();

            Console.WriteLine("Want to run some tests using one standard proxy class only?");
            if (UserAnsweredYes())
            {
                Console.WriteLine("Doing OneProxy tests ...");

                // This is the same test as above but using a single proxy only. NO re-creation of any sort. This is the
                // performance GOAL to aim for, although due to additional overhead it will be almost impossible to achieve.
                // However, the closer we get to this figure, the better
                ClientProxy2 p3 = new ClientProxy2();
                sw3.Start();
                for (i = 0; i < MAX; i++)
                {
                    p3.Op1("123");
                    //p3.Op2();
                }
                p3.Close();
                sw3.Stop();
            }
            TimeSpan oneProxy = sw3.Elapsed;

            
            // Show the results

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Timed Tests complete.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n\tNormal: {0}:{1}:{2}",normal.Minutes,normal.Seconds, normal.Milliseconds);
            if (clientPoolBase < normal)
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("\tClientPoolBase: {0}:{1}:{2}", clientPoolBase.Minutes, clientPoolBase.Seconds, clientPoolBase.Milliseconds);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\tSingle Proxy: {0}:{1}:{2}",oneProxy.Minutes, oneProxy.Seconds, oneProxy.Milliseconds);

        }

        #endregion

        #region Helper Methods

        private static void WaitForEnter()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n------ Hit <ENTER> -------\n");
            Console.ReadLine();
        }


        private static bool UserAnsweredYes()
        {
            Console.WriteLine(">>> (Yes/Y/No/N) ");
            bool answer = false;
            if (!doContinuousTests)
            {
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    string realAnswer = input.ToUpperInvariant();
                    if (realAnswer == "Y" || realAnswer == "YES")
                        answer = true;
                }
            } else 
            {
                Console.WriteLine("Yes");
                answer = true;
            }

            return answer;
        }
    }

    #endregion
}
