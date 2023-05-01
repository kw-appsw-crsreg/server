using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

namespace TeamProject
{
    class Program
    {
        //데이터베이스 변수
        /* 
         * MariaDB 연동을 위해서는
         * Visual Studio 메뉴에서 보기->다른창->패키지 관리자 콘솔 선택
         * 콘솔에서 다음과 같이 입력 :
         * Install-Package MySql.Data
         * 완료되었다면 솔루션탐색기에서 참조에 MySql.Data 추가되었는지 확인
         * using MySql.Data.MySqlClient; 선언
         */
        string dbServer = "127.0.0.1";
        string dbPortNum = "3306";
        string dbName = "Appsw2023DB";
        string dbUId = "";
        string dbPW = "";
        //string dbConnStr = $"server={dbServer};user={dbUId};database={dbName};port={dbPortNum};password={dbPW}";

        //서버 변수
        private static Thread rcvThread;
        private static TcpClient client;
        public static NetworkStream stream;
        public static StreamReader reader;
        public static StreamWriter writer;
        static void Main(string[] args)
        {
            //TcpListener 생성 및 시작
            TcpListener server = null;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            int port = 12000;
            try
            {
                server = new TcpListener(localAddr, port);
                server.Start();
                DateTime t = DateTime.Now; //시간 표시용

                //Listening Loop
                while (true)
                {
                    Console.WriteLine("Waiting for next Connection...");
                    client = server.AcceptTcpClient();
                    t = DateTime.Now;
                    string currentTime = t.ToString();
                    Console.WriteLine(currentTime + " Connected!");

                    //Thread 시작(Receive from Client)
                    rcvThread = new Thread(new ThreadStart(ReceiverThread));
                    rcvThread.Start();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException : {0}", e);
            }
            finally
            {
                server.Stop();
            }
            Console.WriteLine("\n 서버가 종료됩니다");
        }

        static void ReceiverThread()
        {
            try
            {
                //Send to Client
                stream = client.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                while (true)
                {
                    String order = reader.ReadLine();
                    writer.WriteLine(SQLrst(order));
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : {0}", e);
            }
            finally
            {
                Console.WriteLine("연결중 문제가 발생했습니다");
            }
        }

        static String SQLrst(String order)
        {
            String rst = null;
            return rst;
        }
    }
}
