using MySql.Data.MySqlClient;
using System.Runtime.Serialization.Formatters.Binary; 
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.CompilerServices;
using Server;

namespace TeamProject
{
    class Program
    {
        //서버 변수
        private static Thread rcvThread;
        private static TcpClient client;
        public static NetworkStream stream;
        static byte[] readBuffer = new byte[1024 * 4];
        static byte[] sendBuffer = new byte[1024 * 4];
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
                Initialize Init = new Initialize();
                int bs = 0;

                while (true)
                {
                    bs = stream.Read(readBuffer, 0, 1024 * 4);
                    Packet packet = (Packet)Packet.Desserialize(readBuffer);

                    // SQLrst(packet);
                    // Init.Type = ;
                    // Init.Data = ;

                    Packet.Serialize(Init).CopyTo(sendBuffer, 0);
                    stream.Write(sendBuffer, 0, sendBuffer.Length);
                    stream.Flush();

                    for(int i = 0; i < sendBuffer.Length; i++)
                    {
                        sendBuffer[i] = 0;
                    }
                    stream.Flush();
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

        static String SQLrst(Packet packet)
        {
            switch ((int)packet.Type)
            {
                case :
                case :
                case :
                case :

            }
        }

    }
}
