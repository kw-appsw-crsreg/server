using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal interface User
    {
        void SetStuID(string stuID);
        void SetPwd(String pwd);
        void SetCourseID(String ci);
        void SetIdx(short idx);
        void SetVar(String var);


        String GetStuID();
        String GetPwd();
        String GetCourseID();
        short GetIdx();
        String GetVar();

    }


    public class user : User
    {
        String stuid;
        String pwd;
        String ci;
        short idx;
        String var;


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
    }
}