using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AppswPacket;

namespace Server
{
    class Program
    {
        //서버 변수
        private static Thread rcvThread;
        private static TcpClient client;
        public static NetworkStream stream;
        static HashSet<Packet> set = new HashSet<Packet>();
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
            try { QueryProcess.conn = c.Connect(); } catch (Exception e) { Console.WriteLine("DB연결실패"); Environment.Exit(-1); }

            //테스트용
            while (false)
            {
                Console.Write("사용자 ID입력 >> ");
                string studentID = Console.ReadLine();

                Console.Write("대상학정번호 >> ");
                string stuCourseNum = Console.ReadLine();

                user tst = new user();
                tst.SetStuID(studentID); tst.SetCourseID(stuCourseNum);

                QueryProcess.DropCourse(tst);

                //From String to byte array
                /*
                 * SHA1 sha = SHA1.Create();
                byte[] sourceBytes = Encoding.UTF8.GetBytes(studentPWPlain);
                byte[] hashBytes = sha.ComputeHash(sourceBytes);
                string studentPWHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);

                Console.WriteLine(studentPWHash);

                user tst = new user();
                tst.SetStuID(studentID); tst.SetPwd(studentPWHash);

                if (QueryProcess.DBLogin(tst) == LoginResult.OK)
                    Console.WriteLine("성공");
                else { Console.WriteLine("없음"); }
                 */
            }

            try
            {
                server = new TcpListener(localAddr, port);
                server.Start();
                DateTime t = DateTime.Now; //시간 표시용
                //Listening Loop
                while (true)
                {
                    try
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
                    catch (Exception e)
                    {
                        Console.WriteLine("One Connection lost!!");
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException : {0}", e);
            }
        }

        static void ReceiverThread()
        {
            try
            {
                //Receive From Client
                stream = client.GetStream();
                Packet packet;
                Thread sndThread;
                int bs = 0;

                while (true)
                {
                    byte[] readBuffer = new byte[1024 * 4000];
                    bs = stream.Read(readBuffer, 0, 1024 * 4000);
                    packet = (Packet)Packet.Desserialize(readBuffer);
                    stream.Flush();

                    if (!set.Contains(packet))
                    {
                        set.Add(packet);
                        sndThread = new Thread(new ParameterizedThreadStart(SenderThread));
                        sndThread.Start(packet);
                    }
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

        static void SenderThread(Object packet)
        {
           
                //Send to Client
                Initialize init;
                byte[] sendBuffer = new byte[1024 * 4000];
                init = (Initialize)SQLrst((Packet)packet);

                Packet.Serialize(init).CopyTo(sendBuffer, 0);
                stream.Write(sendBuffer, 0, sendBuffer.Length);
                stream.Flush();
                set.Remove((Packet)packet);
            
        }

        static Packet SQLrst(Packet packet)
        {
            /*
            * client에서 보낸 패킷 타입에 따라서 해당하는 쿼리 작업을 수행함.
            * 쿼리 작업이 수행된 후에 나온 결과물을 패킷에 담아서 반환해줌.
            */
            switch ((int)packet.Type)
            {
                case (int)Packet_Type.GoLogin:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Login)packet).stuID);
                        user.SetPwd(((Login)packet).pwd);

                        return (Initialize)QueryProcess.DBLogin(user);
                    }
                case (int)Packet_Type.GoRegister:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Register)packet).stuID);
                        user.SetCourseID(((Register)packet).ci);

                        init.Type = (int)QueryProcess.RegisterCourse(user); 
                        return init;
                    }
                case (int)Packet_Type.GoInquire:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((inquire)packet).stuID);
                        user.SetCourseID(((inquire)packet).ci);

                        return (Initialize)QueryProcess.InquireCourse(user); 
                    }
                case (int)Packet_Type.GetFavoirtes:
                    {
                        IUser user = new user();
                        user.SetStuID(((inquire)packet).stuID);

                        return (Initialize)QueryProcess.InquireFavorites(user); 
                    }
                case (int)Packet_Type.AddToFavorites:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Favorites)packet).stuID);
                        user.SetCourseID(((Favorites)packet).ci);
                        user.SetIdx(((Favorites)packet).idx);

                        init.Type = (int)QueryProcess.AddToFavorites(user); 
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

                        return (Initialize)QueryProcess.GetMyRegisteredList(user); 
                    }
                case (int)Packet_Type.DropCourse:
                    {
                        Initialize init = new Initialize();
                        IUser user = new user();
                        user.SetStuID(((Register)packet).stuID);
                        user.SetCourseID(((Register)packet).ci);

                        init.Type = (int)QueryProcess.DropCourse(user); 
                        return init;
                    }
                case (int)Packet_Type.SearchCouse:
                    {
                        IUser user = new user();
                        user.SetisOnlyRemaining(((inquire)packet).isOnlyRemaining);
                        user.SetVar(((inquire)packet).var);
                        user.SetCourseTyoe(((inquire)packet).courseType);
                        user.SetDepartment(((inquire)packet).department);

                        return (Initialize)QueryProcess.SearchCourse(user);
                    }
                case (int)Packet_Type.GetTypes:
                    {
                        return (Initialize)QueryProcess.GetTypes(); 
                    }
                case (int)Packet_Type.GetDepartments:
                    {
                        IUser user = new user();
                        user.SetStuID(((Initialize)packet).stuID);

                        return (Initialize)QueryProcess.GetDepartments(user); 
                    }
            }
            return null;
        }

    }
}
