using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireMySQL
{
  public class Common
  {
    private const string connectionStringName = "LeSolitaireDB";

    public static bool DBexists(string DBname)
    {
      using (MyContext myContext = new MyContext())
      {
        return myContext.DBexists(DBname);
      }
    }

    public static MySqlConnection OpenConnection()
    {
      string cxString = GetConnectionString();
      MySqlConnection cx = new MySqlConnection(cxString);
      cx.Open();
      return cx;
    }

    private static string GetConnectionString()
    {
      return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
    }

    public static string InitDB()
    {
      using (MyContext myContext = new MyContext())
      {
        return myContext.InitDB();
      }
    }

  }
}
