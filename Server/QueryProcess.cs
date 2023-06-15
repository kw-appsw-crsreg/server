//using MySql.Data.MySqlClient;
using AppswPacket;
using MySqlConnector;
using System;
using System.Data;

namespace Server
{
    internal static class QueryProcess
    {
        static public MySqlConnection conn;

        // TESTED : 서버 로그인 결과 (DB 읽기)
        // 학번, 비번
        // GetMyRegisteredList + InquireFavorites + GetDepartments합친기능!!
        public static Packet DBLogin(IUser user)
        {
            Initialize init = new Initialize();
            DataSet ds = new DataSet();
            string query = "";

            try//GetDepartments의 기능 통합됨 -> department_str
            {
                query = $"SELECT department_str, `name` FROM `student_info` WHERE `student_id`='{user.GetStuID()}' AND password='{user.GetPwd()}' ;";
                Console.WriteLine(query);

                MySqlDataAdapter l = new MySqlDataAdapter(query, conn);

                try { l.Fill(ds, "student_info"); } catch { Console.WriteLine("Error!"); }
                if (ds.Tables["student_info"].Rows.Count < 1)
                    init.Type = (int)LoginResult.WrongPassword;
                else init.Type = (int)LoginResult.OK;
            }
            catch { init.Type = (int)LoginResult.WrongPassword; }

            //로그인결과 맞다면 즐겨찾기목록, 현재신청과목, 소속학과도 같이 넣어반환
            //아니라면 LoginResult만 반환
            if (init.Type == (int)LoginResult.OK)
            {
                //InquireFavorites에서의 기능
                query = $"SELECT idx, opened_course.course_id, course_name, credit, instructor_name, `time`" +
               $" FROM `opened_course` INNER JOIN `registerd_favorites`" +
               $" ON registerd_favorites.course_id = opened_course.course_id" +
               $" WHERE student_id={user.GetStuID()}";
                //한번에 읽어와서 Dataset에 저장
                try
                {
                    MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                    adpt.Fill(ds, "favorites_list");
                }
                catch
                {
                    init.Type = (int)LoginResult.OK;
                }

                //GetMyRegisteredList 에서의 기능
                query = $"SELECT takes_info.course_id, opened_course.`type`, course_name, credit, instructor_name, time ,lect_room " +
                $"FROM sugang.`opened_course` INNER JOIN sugang.takes_info " +
                $"ON opened_course.course_id = takes_info.course_id " +
                $"WHERE student_id='{user.GetStuID()}' AND YEAR=year(CURDATE()) AND semester=1";
                //한번에 읽어와서 Dataset에 저장
                try
                {
                    MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                    adpt.Fill(ds, "registered_list");
                }
                catch
                {
                    init.Type = (int)LoginResult.OK;
                }
                //순번 학정번호 구분 과목명 학점 담당교수 시간 강의실
                init.ds = DatasetToJson.SerializeToJSON(ds);
                Console.WriteLine(init.ds);
                return init;
            }
            else return init;
        }

        // TESTED : 최초 로그인 시 즐겨찾기 목록 반환 (DB 읽기)
        // 학번
        public static Packet InquireFavorites(IUser user)
        {
            string query = $"SELECT idx, opened_course.course_id, course_name, credit, instructor_name, time" +
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
                init.ds = DatasetToJson.SerializeToJSON(ds);
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
        // 학번
        public static Packet GetMyRegisteredList(IUser user)
        {
            //현재학기 수강신청정보와 개설과목 정보를 JOIN
            //학정번호,이수구분,과목명,학점,교수명,시간,강의실 반환
            //이수구분은 기본적으로 과목에 따라서 분류
            //전공과목의 경우 -> 개설학과가 신청자 소속과 다르면 일반으로 분류하는게 가능은 하지만 여기선 구현x
            string query = $"SELECT takes_info.course_id, opened_course.`type`, course_name, credit, instructor_name, time ,lect_room " +
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
                init.ds = DatasetToJson.SerializeToJSON(ds);
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
                init.ds = DatasetToJson.SerializeToJSON(ds);
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
        // 학번
        public static Packet GetDepartments(IUser user)
        {
            string query = $"SELECT department_str FROM `student_info` WHERE `student_id`={user.GetStuID()}";
            Initialize init = new Initialize();
            DataSet ds = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try
            {
                adpt.Fill(ds);
                init.ds = DatasetToJson.SerializeToJSON(ds);
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
        // 학번, 인덱스, 학정번호
        public static FavoritesResult AddToFavorites(IUser user)
        {
            MySqlCommand fav;
            string query = $"";
            // 이미 그 번호에 다른 과목이 있진 않은 지 확인 -> db무결성검증로직으로 해결
            /*
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
            */

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
        // 학번, 인덱스
        public static FavoritesResult DeleteFromFavorites(IUser user)
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

        // TESTED : 즐겨찾기 및 과목선택 필드 : 과목조회 눌렀을때(from 학정번호직접입력 or from 즐겨찾기) (DB 읽기)
        // 학번, 학정번호
        // 조회 시 조회결과(성공여부)와, 해당과목정보 
        public static Packet InquireCourse(IUser user)
        {
            Initialize init = new Initialize();
            string query = "";
            MySqlDataAdapter adpt;
            DataSet ds = new DataSet(); //여기에 과목정보저장

            //요청교과목정보 가져오기
            query = $"SELECT * FROM `opened_course` WHERE course_id='{user.GetCourseID()}'";
            adpt = new MySqlDataAdapter(query, conn);
            try { adpt.Fill(ds, "course_info"); }
            catch
            {
                Console.WriteLine("잘못된학정번호!");
                init.Type = (int)InquireResult.WrongCourseNumber;
                return init;
            }

            //만석여부 판단 -> 만석이면 False
            DataRow dataRow;
            try { dataRow = ds.Tables["course_info"].Rows[0]; }
            catch
            {
                Console.WriteLine("잘못된학정번호!");
                init.Type = (int)InquireResult.WrongCourseNumber;
                return init;
            }
            init.ds = DatasetToJson.SerializeToJSON(ds);
            if (int.Parse(dataRow["remaining_capacity"].ToString()) == 0)
            {
                Console.WriteLine(ds.Tables["course_info"].Rows[0]["course_name"] + "는 만석!");
                init.Type = (int)InquireResult.AlreadyFull;
                return init;
            }

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
            try { adpt.Fill(ds, "before_taken"); }
            catch
            {
                Console.WriteLine("Error!");
                init.Type = (int)InquireResult.Error;
                return init;
            }

            //이전수강내역이 있는 경우라면?? =====>> 재수강가능인지 확인
            //이전에 B이상이면 재수강불가 (3.0미만이어야)
            //재수강은 학칙상 2번만 가능 : 최초+1번째+2번째 => 3개까지만가능
            if (ds.Tables["before_taken"].Rows.Count != 0)
            {
                dataRow = ds.Tables["before_taken"].Rows[0];
                if (!((float)dataRow["gpa"] < (float)3.0) || ds.Tables["before_taken"].Rows.Count > 3)
                {
                    Console.WriteLine("재수강불가");
                    init.Type = (int)InquireResult.AlreadyTaken;
                    return init;
                }

                init.ds = DatasetToJson.SerializeToJSON(ds); //재수강정보까지 포함한 ds


                string beforeYear = dataRow["year"].ToString();
                string beforeSemester = dataRow["semester"].ToString();
                string beforeType = dataRow["type"].ToString(); ;
                string beforeGrade = dataRow["grade"].ToString(); ;
                string beforeCredit = dataRow["credit"].ToString(); ;
                string beforeCourseName = dataRow["course_name"].ToString();
                Console.WriteLine(beforeYear + beforeSemester + "에들은 " + dataRow["course_name"] + "를 재수강");

            }

            Console.WriteLine(user.GetStuID() + "가 " + ds.Tables["course_info"].Rows[0]["course_name"] + " 조회");
            init.Type = (int)InquireResult.OK;

            return init;
        }

        // TESTED : 과목선택 필드 : 수강신청 눌렀을때 (DB 쓰기)
        // 학번, 학정번호
        public static RegisterResult RegisterCourse(IUser user)
        {
            DataSet ds = new DataSet();
            //학생정보 가져오기
            string query = $"SELECT is_foreigner, registered_times FROM student_info WHERE student_id='{user.GetStuID()}';";
            MySqlDataAdapter fori = new MySqlDataAdapter(query, conn);
            fori.Fill(ds, "student_info");
            bool isStudentForeigner = bool.Parse(ds.Tables["student_info"].Rows[0]["is_foreigner"].ToString());

            //과목정보 가져오기
            query = $"SELECT is_foreignerOnly, `time` FROM opened_course WHERE course_id='{user.GetCourseID()}';";
            fori = new MySqlDataAdapter(query, conn);
            fori.Fill(ds, "course_info");
            bool isCourseForeigner = bool.Parse(ds.Tables["course_info"].Rows[0]["is_foreignerOnly"].ToString());

            //내국인이 외국인전용을 신청하지는 않는지 확인
            if (isStudentForeigner == false && isCourseForeigner == true)
            {
                Console.WriteLine("내국인은 외국인전용 신청불가");
                return RegisterResult.ForeignerOnly;
            }
            ////////////////////////////////////////////////////////////////////////////////////////


            //지금 신청하려는 과목이 학생의 다른 신청과목과 겹치지는 않는지 확인
            string studentTime = ds.Tables["student_info"].Rows[0]["registered_times"].ToString();
            string courseTime = ds.Tables["course_info"].Rows[0]["time"].ToString();

            Console.WriteLine("학생시간 >> " + studentTime);
            Console.WriteLine("과목시간 >> " + courseTime);

            string[] courseTimeS = courseTime.Split('.'); // . 으로 신청하려는 과목의 시간들을 Slicing하여 저장

            //현재 신청하려는 과목의 시간대에 다른 기신청 과목이 있는지 확인
            foreach (string str in courseTimeS)
            {
                if (str == "미지정") break;
                if (str == "") continue;
                if (studentTime.Contains(str) == true)
                {
                    Console.WriteLine("시간이 겹칩니다.");
                    return RegisterResult.TimeConflicts;
                }
            }

            //모든 검증을 완료했다면, 수강목록에 추가
            //학생의 수강목록에도 현재 과목을 추가하는건 DB레벨에서 알아서 해줌(트리거)
            try
            {
                //수강목록에 추가 쿼리문
                query = $"INSERT INTO `sugang`.`takes_info` (`student_id`, `course_id`) VALUES ('{user.GetStuID()}', '{user.GetCourseID()}') ;";
                MySqlCommand timeCheck = new MySqlCommand(query, conn);
                timeCheck.ExecuteNonQuery();
                Console.WriteLine("신청성공!!!!");
                return RegisterResult.OK;
            }
            catch (MySqlException e) //인원초과, 신청가능 최대학점 초과 확인도 DB에서 알아서 해줌(제약조건, 트리거)
            {
                switch (e.Number)
                {
                    case 1690:
                        Console.WriteLine("1690 인원이 초과되어 신청불가!");
                        return RegisterResult.OverCapacity;
                    case 4025:
                        Console.WriteLine("4025 최대학점초과!");
                        return RegisterResult.ExceedsCredit;
                }
                Console.WriteLine(e.Number.ToString());
            }
            return RegisterResult.OK;
        }

        // TESTED : 과목선택 필드 : 수강신청 삭제할때 (DB 쓰기)
        // 학번, 학정번호
        public static RegisterResult DropCourse(IUser user)
        {
            //수강삭제 쿼리
            Console.WriteLine(user.GetCourseID() + user.GetStuID());
            string query = $"DELETE FROM `sugang`.`takes_info` WHERE  `student_id`='{user.GetStuID()}' AND `course_id`='{user.GetCourseID()}'; ";
            
            MySqlCommand fav;
            try
            {
                //수강삭제 실행
                fav = new MySqlCommand(query, conn);
                fav.ExecuteNonQuery();
            }
            catch
            {
                return RegisterResult.Error;
            }
            Console.WriteLine("삭제성공");
            return RegisterResult.OK;
        }

        // TESTED : 검색 필드 : 과목검색 눌렀을때 (DB 읽기)
        public static Packet SearchCourse(IUser user)
        {
            string department = user.GetDepartment(); //전체검색이라면 비워두세요
            string courseType = user.GetCourseType(); //전체검색이라면 비워두세요
            string courseName = user.GetCourseName(); //전체검색이라면 비워두세요
            bool isOnlyRemaining = user.GetisOnlyRemaining();

            //반환항목 : 학정번호,개설구분,과목명,학점,교수명,여석,개설시간
            string query = $"SELECT course_id, type, course_name, credit, instructor_name, remaining_capacity , time " +
                 $"FROM `opened_course` "              
                 + $"WHERE department REGEXP '^{department}' ";//개설학과 또는 개설단과대별 - 예) 소융대전체라면 H, 소융대공통이라면 H000, 소융대+소프트학부라면 H030}
            if (!(String.Equals(user.GetVar(), "")|| String.Equals(user.GetVar(), null))) { query = query + $" AND course_name REGEXP '{user.GetVar()}+' "; } //과목명
            if (!(String.Equals(courseType, null) || String.Equals(courseType, ""))) { query = query + $" AND `type`='{courseType}' "; } //이수구분별
            if (isOnlyRemaining) { query = query + " AND remaining_capacity>0 "; } //여석유무에따라

            Initialize init = new Initialize();
            DataSet searchResult = new DataSet();
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);

            //한번에 읽어와서 Dataset에 저장
            try
            {
                adpt.Fill(searchResult);
                init.ds = DatasetToJson.SerializeToJSON(searchResult);
                init.Type = (int)InquireResult.OK;
            }
            catch
            {
                Console.WriteLine("Error!");
                init.Type = (int)InquireResult.Error;
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
