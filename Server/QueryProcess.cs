using MySql.Data.MySqlClient;
using System;
using Server;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Server
{
    internal class QueryProcess
    {
        static private MySqlConnection conn;

        // TESTED : 서버 로그인 결과 (DB 읽기)
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

        // TESTED : 최초 로그인 시 즐겨찾기 목록 반환 (DB 읽기)
        static object InquireFavorites(string stuID)
        {
            string query = $"SELECT opened_course.course_id, course_name, credit, instructor_name, time" +
               $" FROM `opened_course` INNER JOIN `registerd_favorites`" +
               $" ON registerd_favorites.course_id = opened_course.course_id" +
               $" WHERE student_id={stuID}";

            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try { adpt.Fill(ds); } catch { Console.WriteLine("Error!"); }

            int i = 0;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.Write(i.ToString());
                Console.WriteLine(r["course_name"]);
                i++;
            }

            return true;
        }

        // TESTED : 최초 로그인 시 나의 현재 신청과목 반환 (DB 읽기)
        static object GetMyRegisteredList(string stuID)
        {
            //현재학기 수강신청정보와 개설과목 정보를 JOIN
            //학정번호,이수구분,과목명,학점,교수명,시간,강의실 반환
            //이수구분은 기본적으로 과목에 따라서 분류
            //전공과목의 경우 -> 개설학과가 신청자 소속과 다르면 일반으로 분류될것임
            string query = $"SELECT takes_info.course_id, takes_info.TYPE, course_name, credit, instructor_name, time ,lect_room " +
                $"FROM sugang.`opened_course` INNER JOIN sugang.takes_info " +
                $"ON opened_course.course_id = takes_info.course_id " +
                $"WHERE student_id='{stuID}' AND YEAR=year(CURDATE()) AND semester=1";

            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try { adpt.Fill(ds); } catch { Console.WriteLine("Error!"); }

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine(r["course_name"].ToString() + " " + r["instructor_name"].ToString());
            }

            return true;
        }

        // TESTED : 최초 로그인 시 과목 이수구분들 뭐뭐있는지  반환 (DB 읽기) : TESTED
        static object GetTypes()
        {
            string query = "SELECT DISTINCT `type` FROM opened_course ORDER BY `type` ASC";

            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try { adpt.Fill(ds); } catch { Console.WriteLine("Error!"); }

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine(r["type"].ToString());
            }

            return true;
        }

        // TESTED : 최초 로그인 시 학생의 소속학과 반환 : TESTED
        static object GetDepartments(string stuID)
        {
            string query = $"SELECT department_str FROM `student_info` WHERE `student_id`={stuID}";
            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try { adpt.Fill(ds); } catch { Console.WriteLine("Error!"); }

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine(r["department_str"]);
            }

            return true;
        }

        // TESTED : 즐겨찾기 필드 : 즐겨찾기에 추가 (DB 쓰기)
        static FavoritesResult AddToFavorites(string stuID, string ci, short idx)
        {
            MySqlCommand fav;
            string query = $"";
            // 이미 그 번호에 다른 과목이 있진 않은 지 확인
            try
            {
                // Console.WriteLine($"REQ : {stuID} {ci} {idx}");
                query = $" SELECT COUNT(*) FROM registerd_favorites WHERE student_id={stuID} AND idx={idx} ";
                fav = new MySqlCommand(query, conn);
                int cnt = int.Parse(fav.ExecuteScalar().ToString());
                Console.WriteLine(query);
                Console.WriteLine(cnt);
                if (cnt != 0) return FavoritesResult.AlreadyExist;
            }
            catch
            {
                return FavoritesResult.AlreadyExist;
            }

            //즐겨찾기에 최종 추가
            try
            {
                query = $"INSERT INTO `sugang`.`registerd_favorites` (`student_id`, `idx`, `course_id`)" +
                    $"VALUES ('{stuID}', {idx}, {ci})";
                fav = new MySqlCommand(query, conn);
                fav.ExecuteNonQuery();
                return FavoritesResult.OK;
            }
            catch { return FavoritesResult.AlreadyExist; }
        }

        // TESTED : 즐겨찾기 필드 : 즐겨찾기에서 삭제 (DB 쓰기)
        static void DeleteFromFavorites(string stuID, short idx)
        {
            MySqlCommand fav;
            string query = $"";
            try
            {
                //즐겨찾기에서 삭제
                query = $"DELETE FROM `sugang`.`registerd_favorites` WHERE student_id={stuID} AND idx={idx}";
                fav = new MySqlCommand(query, conn);
                fav.ExecuteNonQuery();
            }
            catch
            {

            }

        }

        //일부  TESTED : 즐겨찾기 및 과목선택 필드 : 과목조회 눌렀을때(from 학정번호직접입력 or from 즐겨찾기) (DB 읽기)
        static object InquireCourse(string stuID, string ci)
        {
            string query = "";
            MySqlDataAdapter adpt;
            DataSet ds = new DataSet();

            //요청교과목정보 가져오기
            query = $"SELECT * FROM `opened_course` WHERE course_id='{ci}'";
            adpt = new MySqlDataAdapter(query, conn);
            try { adpt.Fill(ds, "course_info"); } catch { Console.WriteLine("Error!"); }

            //만석여부 판단 -> 만석이면 False
            DataRow dataRow = ds.Tables["course_info"].Rows[0];
            if (int.Parse(dataRow["remaining_capacity"].ToString()) == 0) return false;

            string courseName = dataRow["course_name"].ToString();
            string subjectID = dataRow["subject"].ToString();

            //이 학생의 이전 수강 교과목 로드
            query = $"SELECT `year`,semester, takes_info.`type`, takes_info.grade, credit, course_name, gpa, subject" +
                $" FROM sugang.`takes_info` " +//현재까지의 누적 수강정보
                $"INNER JOIN sugang.`opened_course` ON opened_course.course_id = takes_info.course_id " + // join해서 나머지정보도 불러오기
                $"INNER JOIN sugang.`reference_grade` ON reference_grade.grade = takes_info.grade " + //성적정보 불러오기
                $"WHERE student_id='{stuID}' AND (YEAR!=year(CURDATE()) OR semester!=1) " + //이전학기 수강으로 한정
                $"AND(course_name='{courseName}' OR subject='{subjectID}' " +//과목명이 똑같거나 과목코드가 똑같거나
                $"OR subject IN (SELECT `same_subject` FROM `same_subject` WHERE subject='{subjectID}' ) )" + //동일교과목 지정과목이거나
                $"ORDER BY `year` desc";//최신순 정렬
            adpt = new MySqlDataAdapter(query, conn);
            try { adpt.Fill(ds, "before_taken"); } catch { Console.WriteLine("Error!"); }

            //재수강가능인지 확인
            dataRow = ds.Tables["before_taken"].Rows[0];
            if (int.Parse(dataRow["gpa"].ToString()) >= 3) return false; // 이전에 B이상이면 재수강불가
            if (ds.Tables["before_taken"].Rows.Count >= 3) return false; //재수강은 학칙상 2번만 가능 : 최초+1번째+2번째

            string beforeYear = dataRow["year"].ToString();
            string beforeSemester = dataRow["semester"].ToString();
            string beforeType = dataRow["type"].ToString(); ;
            string beforeGrade = dataRow["grade"].ToString(); ;
            string beforeCredit = dataRow["credit"].ToString(); ;
            string beforeCourseName = dataRow["course_name"].ToString();

            return RegisterResult.OK;
        }

        //일부 TESTED : 과목선택 필드 : 수강신청 눌렀을때 (DB 쓰기)
        static RegisterResult RegisterCourse(string stuID, string ci)
        {
            //외국인전용을 신청하지는 않는지 확인
            //자신의 학점을 초과하지는 않는지 확인
            //시간이 겹치는 지는 않는 지 확인
            //과목이 그 사이에 만석되지는 않았는 지 확인
            string query = "";
            MySqlCommand reg;

            //즐겨찾기에 최종 추가
            try
            {
                //수강목록에 추가 쿼리
                query = $"INSERT INTO `sugang`.`takes_info` (`student_id`, `course_id`) VALUES ('{stuID}', '{ci}')";
                reg = new MySqlCommand(query, conn);
                reg.ExecuteNonQuery();
                return RegisterResult.OK;
            }
            finally
            {

            }
            return RegisterResult.OK;
        }

        // TESTED : 과목선택 필드 : 수강신청 삭제할때 (DB 쓰기)
        static bool DropCourse(string stuID, string ci)
        {
            //수강삭제 쿼리
            string query = $"DELETE FROM `sugang`.`takes_info` WHERE  `student_id`='{stuID}' AND `course_id`='{ci}' ";
            MySqlCommand fav;
            try
            {
                //수강삭제 실행
                fav = new MySqlCommand(query, conn);
                fav.ExecuteNonQuery();
            }
            catch
            {
                return false;
            }
            return true;
        }

        //검색 필드 : 과목검색 눌렀을때 (DB 읽기)
        static void SearchCourse(string var)
        {
            string query = $"SELECT course_id, type, course_name, credit, instructor_name, maximum , time " +
                 $"FROM `opened_course` " +
                 $"WHERE department REGEXP '^' AND " +
                 $"WHERE type='전선' AND " +
                 $"WHERE AND" +
                 $"WHERE course_name REGEXP '*{var}*' AND ";

            DataSet searchResult = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try { adpt.Fill(searchResult); } catch { Console.WriteLine("Error!"); }

            int i = 0;
            foreach (DataRow r in searchResult.Tables[0].Rows)
            {
                Console.Write(i.ToString());
                Console.WriteLine(r["course_name"]);
                i++;
            }
        }

    }
}
