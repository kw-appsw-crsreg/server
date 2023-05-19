using System.Data;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    //사용을 위해서는 Package 관리자에서
    //Install-Package Newtonsoft.Json
    //입력
    internal class DatasetToJson
    {
        public static string SerializeToJSON(DataSet argDs)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(argDs);
            return JSONString;
        }

        //public DataSet DeSerializeFromJSON(string arg)
    }
}
