using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

namespace TeamProject
{
    public enum RegisterResult
    {
        OK=0, //신청 성공
        AlreadyFull=1, //과목조회 단계에서 만석 <- '만석입니다' 에 대응
        OverCapacity=2, //과목조회에는 성공했으나 신청을 누르기 전에 만석 <- '수강인원이 초과되어...' 에 대응
        WrongCourseNumber=3 //잘못된 학정번호
    }

    public enum LoginResult
    {
        OK=0,
        WrongPassword=1,
        NotYourDate=2,
        ServerOff=3
    }

    public enum FavoritesResult
    {
        OK=0,
        AlreadyExist=1 //선택한 즐겨찾기 번호에 이미 다른과목이 있는 경우
    }

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

        //서버 로그인 결과 (DB 읽기)
        static LoginResult DBLogin(string stuID, string pwd)
        {
            if (true)
            {
                return LoginResult.OK;
            }
            else return LoginResult.LoginFailed;
        }

        //최초 로그인 시 즐겨찾기 목록 반환 (DB 읽기)
        static object InquireFavorites(string stuID)
        {

        }

        //최초 로그인 시 나의 현재 신청과목 반환 (DB 읽기)
        static object GetMyRegisteredList(string stuID)
        {

        }

        //즐겨찾기 필드 : 즐겨찾기에 추가 (DB 쓰기)
        static FavoritesResult AddToFavorites(string stuID, string ci, short idx)
        {
            if (true)
            {
                return FavoritesResult.OK;
            }
            else return FavoritesResult.AlreadyExist;
        }

        //즐겨찾기 필드 : 즐겨찾기에서 삭제 (DB 쓰기)
        static void DeleteFromFavorites(string stuID, short idx)
        {

        }

        //즐겨찾기 및 과목선택 필드 : 과목조회 눌렀을때(from 학정번호직접입력 or from 즐겨찾기) (DB 읽기)
        static RegisterResult InquireCourse(string stuID, string ci) { }

        //과목선택 필드 : 수강신청 눌렀을때 (DB 쓰기)
        static RegisterResult RegisterCourse(string stuID, string ci) { }

        //검색 필드 : 과목검색 눌렀을때 (DB 읽기)
        static void SearchCourse(string var) { }

    }
}
