﻿//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class DBConnect
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
        string dbName = "sugang";
        string dbUId ;
        string dbPW;
        string dbConnStr;
        public MySqlConnection conn;
        public DBConnect(string uid, string pw)
        {
            this.dbUId = uid;
            this.dbPW = pw;
            dbConnStr = $"server={dbServer};user={dbUId};database={dbName};port={dbPortNum};password={dbPW}";
        }

        public MySqlConnection Connect()
        {
                conn = new MySqlConnection(dbConnStr);
                Console.WriteLine("DB연결중...");
                conn.Open();
                Console.WriteLine("DB연결성공!!");
                return conn;
        }

        public MySqlConnection GetConnection()
        {
            return conn;
        }
    }
}
