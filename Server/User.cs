using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server
{
    // 메소드간 필요한 자료형을 매개변수 주입으로 주고 받을 때 사용될 인터페이스
    internal interface IUser
    {
        void SetStuID(string stuID); // 인터페이스를 통해 인자로 전달 할 학생ID를 인터페이스를 구현한 클래스의 변수에 저장합니다.
        void SetPwd(String pwd); // 인터페이스를 통해 인자로 전달 할 학생 비밀번호를 인터페이스를 구현한 클래스의 변수에 저장합니다.
        void SetCourseID(String ci); // 인터페이스를 통해 인자로 전달 할 Course ID를 인터페이스를 구현한 클래스의 변수에 저장합니다.
        void SetIdx(short idx); // 인터페이스를 통해 인자로 전달 할 즐겨찾기를 추가하는 동작을 하는 AddToFavorites() 메소드에 쓰이는 인덱스를 인터페이스를 구현한 클래스의 변수에 저장합니다.
        void SetVar(String var); // 인터페이스를 통해 인자로 전달 할 과목 명칭을 인터페이스를 구현한 클래스의 변수에 저장합니다.
        void SetCourseName(String courseName);
        void SetCourseTyoe(String CourseType);
        void SetsubjectID(String subjectID);
        void SetDepartment(String department);
        void SetisOnlyRemaining(bool isOnlyRemaining);

        String GetStuID(); // 저장된 학생 ID를 반환합니다.
        String GetPwd(); // 저장된 학생 비밀번호를 반환합니다.
        String GetCourseID(); // 저장된 Course ID를 반환합니다.
        short GetIdx(); // 저장된 인덱스를 반환합니다.
        String GetVar(); // 저장된 과목 명칭를 반환합니다.
        String GetCourseName();
        String GetCourseType();
        String GetsubjectID();
        String GetDepartment();
        bool GetisOnlyRemaining();
    }

    // 위의 인터페이스가 구현된 클래스
    public class user : IUser
    {
        String stuid = "";
        String pwd = "";
        String ci = "";
        short idx;
        String var = "";
        String courseName = "";
        String courseType = "";
        String subjectID = "";
        String department = "";
        bool isOnlyRemaining = false;


        public void SetStuID(string stuID)
        {
            this.stuid = stuID;
        }
        public void SetPwd(String pwd)
        {
            this.pwd = pwd;
        }
        public void SetCourseID(String ci)
        {
            this.ci = ci;
        }
        public void SetIdx(short idx)
        {
            this.idx = idx;
        }
        public void SetVar(String var)
        {
            this.var = var;
        }
        public void SetCourseName(String courseName)
        {
            this.courseName = courseName;
        }
        public void SetCourseTyoe(String CourseType)
        {
            this.courseType = CourseType;
        }
        public void SetsubjectID(String subjectID)
        {
            this.subjectID = subjectID;
        }
        public void SetDepartment(String department)
        {
            this.department = department;
        }
        public void SetisOnlyRemaining(bool isOnlyRemaining)
        {
            this.isOnlyRemaining = isOnlyRemaining;
        }


        public String GetStuID()
        {
            return stuid;
        }
        public String GetPwd()
        {
            return pwd;
        }
        public String GetCourseID()
        {
            return ci;
        }
        public short GetIdx()
        {
            return idx;
        }
        public String GetVar()
        {
            return var;
        }
        public String GetCourseName()
        {
            return courseName;
        }
        public String GetCourseType()
        {
            return courseType;
        }
        public String GetsubjectID()
        {
            return subjectID;
        }
        public String GetDepartment()
        {
            return department;
        }
        public bool GetisOnlyRemaining()
        {
            return isOnlyRemaining;
        }
    }
}
