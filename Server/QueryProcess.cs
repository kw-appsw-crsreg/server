using MySql.Data.MySqlClient;
using System;
using Server;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class QueryDao
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
