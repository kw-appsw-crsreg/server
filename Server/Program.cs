using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamProject
{
    class Program
    {
        private static Thread rcvThread;
        private static TcpClient client;
        public static NetworkStream stream;
        public static StreamReader reader;
        public static StreamWriter writer;
        static void Main(string[] args)
        {
            TcpListener server = null;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            int port = 12000;
            try
            {
                server = new TcpListener(localAddr, port);
                server.Start();

                while (true)
                {
                    Console.WriteLine("Waiting for next Connection...");
                    client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

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
