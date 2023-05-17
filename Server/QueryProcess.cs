using MySql.Data.MySqlClient;
using System;
using Server;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Management;

namespace Server
{
    internal class QueryProcess
    {
        static private MySqlConnection conn;

        // TESTED : 서버 로그인 결과 (DB 읽기)
        public static LoginResult DBLogin(User user)
        {
            try
            {
                string query = $"SELECT * FROM `student_info` WHERE `student_id`={user.GetStuID()} AND password={user.GetPwd()}";
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
        public static Packet InquireFavorites(User user)
        {
            string query = $"SELECT opened_course.course_id, course_name, credit, instructor_name, time" +
               $" FROM `opened_course` INNER JOIN `registerd_favorites`" +
               $" ON registerd_favorites.course_id = opened_course.course_id" +
               $" WHERE student_id={user.GetStuID()}";

            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
            Initialize init = new Initialize();

            //한번에 읽어와서 Dataset에 저장
            try
            {
                adpt.Fill(ds);
                init.ds = ds;
                init.Type = (int)FavoritesResult.OK;
            }
            catch
            {
                Console.WriteLine("Error!");
                init.Type = (int)FavoritesResult.FAIL;
                return init;
            }

            int i = 0;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.Write(i.ToString());
                Console.WriteLine(r["course_name"]);
                i++;
            }

            return init;
        }

        // TESTED : 최초 로그인 시 나의 현재 신청과목 반환 (DB 읽기)
        public static Packet GetMyRegisteredList(User user)
        {
            //현재학기 수강신청정보와 개설과목 정보를 JOIN
            //학정번호,이수구분,과목명,학점,교수명,시간,강의실 반환
            //이수구분은 기본적으로 과목에 따라서 분류
            //전공과목의 경우 -> 개설학과가 신청자 소속과 다르면 일반으로 분류될것임
            string query = $"SELECT takes_info.course_id, takes_info.TYPE, course_name, credit, instructor_name, time ,lect_room " +
                $"FROM sugang.`opened_course` INNER JOIN sugang.takes_info " +
                $"ON opened_course.course_id = takes_info.course_id " +
                $"WHERE student_id='{user.GetStuID()}' AND YEAR=year(CURDATE()) AND semester=1";

            Initialize init = new Initialize();
            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try
            {
                adpt.Fill(ds);
                init.ds = ds;
                init.Type = (int)RegisterResult.OK;
            }
            catch
            {
                Console.WriteLine("Error!");
                init.Type = (int)RegisterResult.Error;
                return init;
            }

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine(r["course_name"].ToString() + " " + r["instructor_name"].ToString());
            }

            return init;
        }

        // TESTED : 최초 로그인 시 과목 이수구분들 뭐뭐있는지  반환 (DB 읽기) : TESTED
        public static Packet GetTypes()
        {
            string query = "SELECT DISTINCT `type` FROM opened_course ORDER BY `type` ASC";

            Initialize init = new Initialize();
            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try
            {
                adpt.Fill(ds);
                init.ds = ds;
                init.Type = (int)First_ProcessResult.OK;
            }
            catch
            {
                Console.WriteLine("Error!");
                init.Type = (int)First_ProcessResult.Error;
                return init;
            }

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine(r["type"].ToString());
            }

            return init;
        }

        // TESTED : 최초 로그인 시 학생의 소속학과 반환 : TESTED
        public static Packet GetDepartments(User user)
        {
            string query = $"SELECT department_str FROM `student_info` WHERE `student_id`={user.GetStuID()}";
            Initialize init = new Initialize();
            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try
            {
                adpt.Fill(ds);
                init.ds = ds;
                init.Type = (int)First_ProcessResult.OK;
            }
            catch
            {
                Console.WriteLine("Error!");
                init.Type = (int)First_ProcessResult.Error;
                return init;
            }

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine(r["department_str"]);
            }

            return init;
        }

        // TESTED : 즐겨찾기 필드 : 즐겨찾기에 추가 (DB 쓰기)
        public static FavoritesResult AddToFavorites(User user)
        {
            MySqlCommand fav;
            string query = $"";
            // 이미 그 번호에 다른 과목이 있진 않은 지 확인
            try
            {
                // Console.WriteLine($"REQ : {stuID} {ci} {idx}");
                query = $" SELECT COUNT(*) FROM registerd_favorites WHERE student_id={user.GetStuID()} AND idx={user.GetIdx()} ";
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
                    $"VALUES ('{user.GetStuID()}', {user.GetIdx()}, {user.GetCourseID()})";
                fav = new MySqlCommand(query, conn);
                fav.ExecuteNonQuery();
                return FavoritesResult.OK;
            }
            catch { return FavoritesResult.AlreadyExist; }
        }

        // TESTED : 즐겨찾기 필드 : 즐겨찾기에서 삭제 (DB 쓰기)
        public static FavoritesResult DeleteFromFavorites(User user)
        {
            MySqlCommand fav;
            string query = $"";
            try
            {
                //즐겨찾기에서 삭제
                query = $"DELETE FROM `sugang`.`registerd_favorites` WHERE student_id={user.GetStuID()} AND idx={user.GetIdx()}";
                fav = new MySqlCommand(query, conn);
                fav.ExecuteNonQuery();
            }
            catch
            {
                return FavoritesResult.FAIL;
            }
            return FavoritesResult.OK;
        }

        //일부  TESTED : 즐겨찾기 및 과목선택 필드 : 과목조회 눌렀을때(from 학정번호직접입력 or from 즐겨찾기) (DB 읽기)
        public static object InquireCourse(User user)
        {
            string query = "";
            MySqlDataAdapter adpt;
            DataSet ds = new DataSet();

            //요청교과목정보 가져오기
            query = $"SELECT * FROM `opened_course` WHERE course_id='{user.GetCourseID()}'";
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
                $"WHERE student_id='{user.GetStuID()}' AND (YEAR!=year(CURDATE()) OR semester!=1) " + //이전학기 수강으로 한정
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
        public static RegisterResult RegisterCourse(User user)
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
                query = $"INSERT INTO `sugang`.`takes_info` (`student_id`, `course_id`) VALUES ('{user.GetStuID()}', '{user.GetCourseID()}')";
                reg = new MySqlCommand(query, conn);
                reg.ExecuteNonQuery();
                return RegisterResult.OK;
            }
            catch
            {

            }
            return RegisterResult.OK;
        }

        // TESTED : 과목선택 필드 : 수강신청 삭제할때 (DB 쓰기)
        public static First_ProcessResult DropCourse(User user)
        {
            //수강삭제 쿼리
            string query = $"DELETE FROM `sugang`.`takes_info` WHERE  `student_id`='{user.GetStuID()}' AND `course_id`='{user.GetCourseID()}' ";
            MySqlCommand fav;
            try
            {
                //수강삭제 실행
                fav = new MySqlCommand(query, conn);
                fav.ExecuteNonQuery();
            }
            catch
            {
                return First_ProcessResult.Error;
            }
            return First_ProcessResult.OK;
        }

        //검색 필드 : 과목검색 눌렀을때 (DB 읽기)
        public static Packet SearchCourse(User user)
        {
            string query = $"SELECT course_id, type, course_name, credit, instructor_name, maximum , time " +
                 $"FROM `opened_course` " +
                 $"WHERE department REGEXP '^' AND " +
                 $"WHERE type='전선' AND " +
                 $"WHERE AND" +
                 $"WHERE course_name REGEXP '*{user.GetVar()}*' AND ";

            Initialize init = new Initialize();
            DataSet searchResult = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try
            {
                adpt.Fill(searchResult);
                init.ds = searchResult;
                init.Type = (int)First_ProcessResult.OK;
            }
            catch
            {
                Console.WriteLine("Error!");
                init.Type = (int)First_ProcessResult.Error;
                return init;
            }

            int i = 0;
            foreach (DataRow r in searchResult.Tables[0].Rows)
            {
                Console.Write(i.ToString());
                Console.WriteLine(r["course_name"]);
                i++;
            }
            return init;
        }

    }
}
