using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TeamProject
{
    public enum RegisterResult
    //외국인전용을 신청하지는 않는지 확인
    //자신의 학점을 초과하지는 않는지 확인
    //시간이 겹치는 지는 않는 지 확인
    //동일한 과목을 추가하지는 않는 지 확인
    //과목이 그 사이에 만석되지는 않았는 지 확인
    {
        OK = 0, //신청 성공
        ForeignerOnly = 1, //내국인이 외국인 수강시도
        ExceedsCredit = 2, //최대신청학점 초과
        TimeConflicts = 3, //시간이 겹침
        AlreadyRegistered = 4, // 이미 신청한 과목을 또 신청하려고 할 때
        OverCapacity = 5, //과목조회에는 성공했으나 신청을 누르기 전에 만석 <- '수강인원이 초과되어...' 에 대응
    }

    public enum InquireResult
    {
        OK = 0, //조회 성공
        WrongCourseNumber = 1, //잘못된 학정번호
        AlreadyTaken = 2, //이미 수강한 과목 신청시도(재수강불가)
        AlreadyFull = 3, //과목조회 단계에서 만석 <- '만석입니다' 에 대응
    }

    public enum LoginResult
    {
        OK = 0,
        WrongPassword = 1,
        NotYourDate = 2,
        ServerOff = 3
    }

    public enum FavoritesResult
    {
        OK = 0,
        AlreadyExist = 1 //선택한 즐겨찾기 번호에 이미 다른과목이 있는 경우
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
            try
            {
                string query = $"SELECT * FROM `student_info` WHERE `student_id`={stuID} AND password={pwd}";
                MySqlCommand login = new MySqlCommand(query, conn);

                MySqlDataReader rdr = login.ExecuteReader();
                // ExecuteReader to query the database.
                // ExecuteNonQuery to insert, update, and delete data.

                while (rdr.Read())
                {
                    Console.WriteLine($"ID: {rdr[0]}, Name: {rdr[1]}, Age: {rdr[2]}");
                }
                rdr.Close();


                return LoginResult.OK;
            }
            catch { return LoginResult.WrongPassword; }
        }

        //최초 로그인 시 즐겨찾기 목록 반환 (DB 읽기)
        static object InquireFavorites(string stuID)
        {
            string query = $"SELECT opened_course.course_id, course_name, credit, instructor_name, time" +
               $" FROM `opened_course` INNER JOIN `registerd_favorites`" +
               $" ON registerd_favorites.course_id = opened_course.course_id" +
               $" WHERE student_id={stuID}";
            return true;
        }

        //최초 로그인 시 나의 현재 신청과목 반환 (DB 읽기)
        static object GetMyRegisteredList(string stuID)
        {
            string query = $"SELECT takes_info.course_id, TYPE, course_name, credit, instructor_name, time ,lect_room " +
                $"FROM sugang.`opened_course` INNER JOIN sugang.takes_info " +
                $"ON opened_course.course_id = takes_info.course_id " +
                $"WHERE student_id=2019707005 AND YEAR=year(CURDATE()) AND semester=1";
            return true;
        }

        //즐겨찾기 필드 : 즐겨찾기에 추가 (DB 쓰기)
        static FavoritesResult AddToFavorites(string stuID, string ci, short idx)
        {
            string query = $"";
            // 이미 그 번호에 다른 과목이 있진 않은 지 확인
            try
            {
                query = $" SELECT * FROM registerd_favorites WHERE student_id={stuID} AND idx={idx} ";
            }
            catch
            {
                return FavoritesResult.AlreadyExist; ;
            }

            //즐겨찾기에 최종 추가
            try
            {
                query = $"INSERT INTO `sugang`.`registerd_favorites` (`student_id`, `idx`, `course_id`)" +
                    $"VALUES ('2019707005', '1', '2023100001812801')";
                return FavoritesResult.OK;
            }
            catch { return FavoritesResult.AlreadyExist; }
        }

        //즐겨찾기 필드 : 즐겨찾기에서 삭제 (DB 쓰기)
        static void DeleteFromFavorites(string stuID, short idx)
        {
            try
            {
                //즐겨찾기에서 삭제
                string a = "DELETE FROM `sugang`.`registerd_favorites` WHERE student_id=2019707005 AND idx=1";
            }
            catch
            {

            }

        }

        //즐겨찾기 및 과목선택 필드 : 과목조회 눌렀을때(from 학정번호직접입력 or from 즐겨찾기) (DB 읽기)
        static object InquireCourse(string stuID, string ci)
        {
            string query = "";
            //그런과목 있는지 확인
            //이전에 동일과목 수강했었는지 (과목명동일 or 동일교과목)
            query = $"SELECT * FROM sugang.`takes_info` " +
                $"INNER JOIN sugang.`opened_course` ON opened_course.course_id = takes_info.course_id " +
                $"INNER JOIN same_subject ON opened_course.subject=same_subject.subject " +
                $"WHERE student_id=2019707005 AND (YEAR!=year(CURDATE()) OR semester!=1)";
            //재수강가능한지
            query = "";
            //과목이 만석인 지 확인
            query = "";

            return RegisterResult.OK;
        }

        //과목선택 필드 : 수강신청 눌렀을때 (DB 쓰기)
        static RegisterResult RegisterCourse(string stuID, string ci)
        {
            //외국인전용을 신청하지는 않는지 확인
            //자신의 학점을 초과하지는 않는지 확인
            //시간이 겹치는 지는 않는 지 확인
            //동일한 과목을 추가하지는 않는 지 확인
            //과목이 그 사이에 만석되지는 않았는 지 확인
            string asd = "";
            //수강목록에 추가 쿼리
            string query = $"INSERT INTO `sugang`.`takes_info` (`student_id`, `course_id`) VALUES ('2019707005', '2023100001812801')";
            return RegisterResult.OK;
        }

        //과목선택 필드 : 수강신청 삭제할때 (DB 쓰기)
        static bool DropCourse(string stuID, string ci)
        {
            //수강삭제 쿼리
            string query = $"DELETE FROM `sugang`.`takes_info` WHERE  `student_id`='2019707005' AND `course_id`='2023100001812801' ";
            return true;
        }

        //검색 필드 : 과목검색 눌렀을때 (DB 읽기)
        static void SearchCourse(string var)
        {
            string query = $"SELECT course_id, type, course_name, credit, instructor_name, maximum , time " +
                    $"FROM `opened_course` " +
                    $"WHERE department REGEXP '^' AND " +
                    $"WHERE type='{}' AND " +
                    $"WHERE AND" +
                    $"WHERE course_name REGEXP '*{}*' AND ";
        }

    }
}
