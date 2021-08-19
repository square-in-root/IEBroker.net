using System;

namespace IEBroker.net46.test
{
    class URLCrackerTest
    {
        public void TestMain()
        {
            // General Protocol
            {
                // Ex. iepp://localhost:8010/path.do?param1=1&param2=2&param3=3;JSESSIONID=1B7613A3D23C36061FD53EB9C81CBE2A;
                String url = "iepp://localhost:8010/path.do?param1=1&param2=2&param3=3;JSESSIONID=1B7613A3D23C36061FD53EB9C81CBE2A;";
                
                URLCracker cracker = new URLCracker(url, Constant.DEFAULT_PROTOCOL_GENERAL, Constant.DEFAULT_PROTOCOL_SECURE);
                cracker.DoCrack();

                // Ex. http://localhost:8010/path.do?param1=1&param2=2&param3=3;JSESSIONID=1B7613A3D23C36061FD53EB9C81CBE2A;
                Console.WriteLine(cracker.GetProcessedUrl());
                Console.WriteLine(cracker.GetProcessedUrl(false));

                // Ex. http://localhost:8010/path.do?param1=1&param2=2&param3=3
                Console.WriteLine(cracker.GetProcessedUrl(true));

                // Ex. ftp://localhost:8010/path.do?param1=1&param2=2&param3=3
                Console.WriteLine(cracker.GetProcessedUrl("ftp", true));
            }


            // Secure Protocol
            {
                // Ex. iepps://localhost:8010/path.do?param1=1&param2=2&param3=3;JSESSIONID=1B7613A3D23C36061FD53EB9C81CBE2A;
                String url = "iepps://localhost:8010/path.do?param1=1&param2=2&param3=3;JSESSIONID=1B7613A3D23C36061FD53EB9C81CBE2A;";

                URLCracker cracker = new URLCracker(url, Constant.DEFAULT_PROTOCOL_GENERAL, Constant.DEFAULT_PROTOCOL_SECURE);
                cracker.DoCrack();

                // Ex. https://localhost:8010/path.do?param1=1&param2=2&param3=3;JSESSIONID=1B7613A3D23C36061FD53EB9C81CBE2A;
                Console.WriteLine(cracker.GetProcessedUrl());
                Console.WriteLine(cracker.GetProcessedUrl(false));

                // Ex. https://localhost:8010/path.do?param1=1&param2=2&param3=3
                Console.WriteLine(cracker.GetProcessedUrl(true));

                // Ex. ftp://localhost:8010/path.do?param1=1&param2=2&param3=3
                Console.WriteLine(cracker.GetProcessedUrl("ftp", true));
            }

            Console.ReadLine();
        }
    }
}
