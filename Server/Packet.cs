using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data;

namespace Server
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
        Error = 6
    }

    public enum InquireResult
    {
        OK = 7, //조회 성공
        WrongCourseNumber = 8, //잘못된 학정번호
        AlreadyTaken = 9, //이미 수강한 과목 신청시도(재수강불가)
        AlreadyFull = 10, //과목조회 단계에서 만석 <- '만석입니다' 에 대응
        Error = 11
    }

    public enum LoginResult
    {
        OK = 12,
        WrongPassword = 13,
        NotYourDate = 14,
        ServerOff = 15
    }

    public enum FavoritesResult
    {
        OK = 16,
        FAIL=17,
        AlreadyExist = 18 //선택한 즐겨찾기 번호에 이미 다른과목이 있는 경우
    }

    public enum First_ProcessResult
    {
        OK = 19,
        Error = 20
    }

    public enum Packet_Type
    {
        GoLogin = 0, // DBLogin()

        GetFavoirtes = 1, // InquireFavorites()
        AddToFavorites = 2, // AddToFavorites()
        DeleteFromFavorites = 4, // DeleteFromFavorites()

        GoRegister = 5, // RegisterCourse()
        GoInquire = 6, // InquireCourse()
        GetRegisterCourses = 7, // GetMyRegisteredList()
        DropCourse = 8, // DropCourse()
        SearchCouse = 9, // SearchCouse()

        GetTypes = 10,
        GetDepartments = 11
    }


    public enum PacketSendERROR
    {
        NOERROR = 0,
        ERROR
    }

    [Serializable]
    public class Packet
    {
        public int Type;

        public Packet()
        {
            this.Type = 0;
        }

        public static byte[] Serialize(Object o)
        {
            MemoryStream ms = new MemoryStream(1024 * 4); // OS DATA CONTROL BASIC SIZE 4KB
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, o);
            return ms.ToArray();
        }

        public static Object Desserialize(byte[] bt)
        {
            MemoryStream ms = new MemoryStream(1024 * 4); // OS DATA CONTROL BASIC SIZE 4KB
            foreach (byte b in bt)
            {
                ms.WriteByte(b);
            }

            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            bf.Binder = new AllowAllAssembly();
            Object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }

    }

    [Serializable]
    public class Initialize : Packet
    {
        public string stuID;
        public String ds;
    }

    [Serializable]
    public class Login : Packet
    {
        public string stuID;
        public string pwd;
        public String ds;
    }

    [Serializable]
    public class Register : Packet
    {
        public string stuID;
        public string ci;
        public string var;
        public String ds;
    }

    [Serializable]
    public class inquire : Packet
    {
        public string stuID;
        public string ci;
        public String ds;
    }

    [Serializable]
    public class Favorites : Packet
    {
        public string stuID;
        public string ci;
        public short idx;
        public String ds;
    }

    // Serializationexception : Cant find Assembly 해결용
    sealed class AllowAllAssembly : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type type = null;

            String currentAssem = System.Reflection.Assembly.GetExecutingAssembly().FullName;
            type = Type.GetType(String.Format("{0},{1}", typeName, currentAssem));

            return type;
        }
    }
}
