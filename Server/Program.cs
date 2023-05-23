using MySql.Data.MySqlClient;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Server
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

            //DB 연결
            Console.Write("DB ID입력 >> ");
            string dbID = Console.ReadLine();
            Console.Write("DB PW입력 >> ");
            string dbPW = Console.ReadLine();
            DBConnect c = new DBConnect(dbID, dbPW);
            try { MySqlConnection conn = c.Connect(); } catch (Exception e) { Console.Write("DB연결실패"); }
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
                Initialize init;
                int bs = 0;

                while (true)
                {
                    bs = stream.Read(readBuffer, 0, 1024 * 4);
                    Packet packet = (Packet)Packet.Desserialize(readBuffer);

                    init = (Initialize)SQLrst(packet);

                    Packet.Serialize(init).CopyTo(sendBuffer, 0);
                    stream.Write(sendBuffer, 0, sendBuffer.Length);
                    stream.Flush();

                    for (int i = 0; i < sendBuffer.Length; i++)
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

        static Packet SQLrst(Packet packet)
        {
            switch ((int)packet.Type)
            {
                case (int)Packet_Type.GoLogin:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Login)packet).stuID);
                        user.SetPwd(((Login)packet).pwd);

                        init.Type = (int)QueryProcess.DBLogin(user); //
                        return init;
                    }
                case (int)Packet_Type.GoRegister:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Register)packet).stuID);
                        user.SetCourseID(((Register)packet).ci);

                        init.Type = (int)QueryProcess.RegisterCourse(user); //
                        return init;
                    }
                case (int)Packet_Type.GoInquire:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((inquire)packet).stuID);
                        user.SetCourseID(((inquire)packet).ci);

                        init.Type = (int)QueryProcess.InquireCourse(user); //
                        return init;
                    }
                case (int)Packet_Type.GetFavoirtes:
                    {
                        IUser user = new user();
                        user.SetStuID(((inquire)packet).stuID);

                        return (Initialize)QueryProcess.InquireFavorites(user); //
                    }
                case (int)Packet_Type.AddToFavorites:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Favorites)packet).stuID);
                        user.SetCourseID(((Favorites)packet).ci);
                        user.SetIdx(((Favorites)packet).idx);

                        init.Type = (int)QueryProcess.AddToFavorites(user); //
                        return init;
                    }
                case (int)Packet_Type.DeleteFromFavorites:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Favorites)packet).stuID);
                        user.SetIdx(((Favorites)packet).idx);

                        init.Type = (int)QueryProcess.DeleteFromFavorites(user);
                        return init;
                    }
                case (int)Packet_Type.GetRegisterCourses:
                    {
                        IUser user = new user();
                        user.SetStuID(((Register)packet).stuID);

                        return (Initialize)QueryProcess.GetMyRegisteredList(user); //
                    }
                case (int)Packet_Type.DropCourse:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Register)packet).stuID);
                        user.SetCourseID(((Register)packet).ci);

                        init.Type = (int)QueryProcess.DropCourse(user); //
                        return init;
                    }
                case (int)Packet_Type.SearchCouse:
                    {
                        IUser user = new user();
                        user.SetVar(((Register)packet).var);

                        return (Initialize)QueryProcess.SearchCourse(user);
                    }
                case (int)Packet_Type.GetTypes:
                    {
                        return (Initialize)QueryProcess.GetTypes(); //
                    }
                case (int)Packet_Type.GetDepartments:
                    {
                        IUser user = new user();
                        user.SetStuID(((Initialize)packet).stuID);

                        return (Initialize)QueryProcess.GetDepartments(user); //
                    }
            }
            return null;
        }

    }
}
