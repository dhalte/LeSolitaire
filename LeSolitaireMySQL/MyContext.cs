using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace LeSolitaireMySQL
{
  public class MyContext : IDisposable
  {
    public MySqlConnection cx;
    private XmlElement xmlSQL;
    public MyContext(string DBname = null)
    {
      cx = Common.OpenConnection();
      string sqlCommands = Properties.Resources.SQLcommands;
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(sqlCommands);
      xmlSQL = xmlDoc.DocumentElement;
      if (!string.IsNullOrEmpty(DBname))
      {
        using (MySqlCommand mySqlCommand = new MySqlCommand($"use `{DBname}`", cx))
        {
          mySqlCommand.ExecuteNonQuery();
        }
      }
    }

    private string GetSQL(string v)
    {
      XmlElement xml = (XmlElement)xmlSQL.SelectSingleNode($"sql[@name='{v}']");
      return xml.InnerText;
    }

    public bool DBexists(string DBname)
    {
      string sql = GetSQL("showAllSchemas");
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
        while (mySqlDataReader.Read())
        {
          string dbName = mySqlDataReader.GetString(0);
          if (string.Compare(dbName, DBname, true) == 0)
          {
            return true;
          }
        }
      }
      return false;
    }

    public string InitDB()
    {
      string sql = GetSQL("showAllSchemas");
      HashSet<string> DBs = new HashSet<string>();
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        // L'instruction using est très importante, elle ferme le reader, 
        // sans quoi une exception claque à l'instruction suivante.
        // Et ça nous évite un try/finally mySqlDataReader.Close() end try
        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
        {
          while (mySqlDataReader.Read())
          {
            DBs.Add(mySqlDataReader.GetString(0).ToLower());
          }
        }
      }
      string dbName;
      int n = 1;
      MyStringComparer stringComparer = new MyStringComparer();
      for (; ; )
      {
        // En fait, la DB aura un nom tout en minuscules
        dbName = $"LeSolitaire{n++}";
        if (!DBs.Contains(dbName.ToLower()))
        {
          break;
        }
      }
      sql = GetSQL("createDB");
      // l'instruction create schema ne supporte pas les @parametres, alors on a mis un {0}
      sql = string.Format(sql, dbName);
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        mySqlCommand.ExecuteNonQuery();
      }
      return dbName;
    }

    public int VerifieEF()
    {
      if (!VerifieEFexists())
      {
        return -1;
      }
      return sizeEFpierres();
    }

    private bool VerifieEFexists()
    {
      string sql = GetSQL("verifieEFexists");
      bool bExists = false;
      using (MySqlCommand cmd = new MySqlCommand(sql, cx))
      {
        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
          bExists = reader.HasRows;
        }
      }
      return bExists;
    }

    private int sizeEFpierres()
    {
      int n = -1;
      string sql = GetSQL("sizeofEF.pierres");
      MySqlCommand cmd;
      using (cmd = new MySqlCommand(sql, cx))
      {
        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            // les colonnes de l'instruction sont : Field Type Null Key Default Extra
            string type = reader.GetString(1);
            // par exemple type="binary(12)"
            Match match = Regex.Match(type, @"binary\((\d+)\)");
            if (match.Success)
            {
              type = match.Groups[1].Value;
              n = int.Parse(type);
            }
          }
        }
      }
      return n;
    }

    public void CreateEFtmp(int nbPierres)
    {
      string sql = GetSQL("createEFtmp");
      sql = string.Format(sql, nbPierres);
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        mySqlCommand.ExecuteNonQuery();
      }
    }

    public void InsertEFtmp(byte[] pierres, byte flagsSI)
    {
      string sql = GetSQL("insertEFtmp");
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        mySqlCommand.Parameters.AddWithValue("@pierres", pierres);
        mySqlCommand.Parameters.AddWithValue("@sg", flagsSI);
        mySqlCommand.ExecuteNonQuery();
      }
    }

    public void renameEFtmp()
    {
      string sql = GetSQL("renameEFtmp");
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        mySqlCommand.ExecuteNonQuery();
      }
    }

    public void Dispose()
    {
      ((IDisposable)cx).Dispose();
    }

    class MyStringComparer : IEqualityComparer<string>
    {
      public bool Equals(string x, string y)
      {
        return string.Compare(x, y, true) == 0;
      }

      public int GetHashCode(string obj)
      {
        return obj.GetHashCode();
      }
    }
    public IEnumerable<(byte[] situation, byte sg)> ReadAllEF(int tailleSituation)
    {
      string sql = GetSQL("ReadAllEF");
      byte[] situation = new byte[tailleSituation];
      byte sg = 0;
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
        {
          while (mySqlDataReader.Read())
          {
            mySqlDataReader.GetBytes(0, 0, situation, 0, tailleSituation);
            sg = mySqlDataReader.GetByte(1);
            yield return (situation, sg);
          }
        }
      }
    }

    public void UpdateOrInsert(byte[] imagePierres, byte sg)
    {
      string sqlReadOneSituation;
      byte sg2 = sg;
      sqlReadOneSituation = GetSQL("ReadOneSituationFromEFtmp");
      // Recherche de la situation et de ses SG
      using (MySqlCommand mySqlCommandRead = new MySqlCommand(sqlReadOneSituation, cx))
      {
        mySqlCommandRead.Parameters.AddWithValue("@pierres", imagePierres);
        bool bNew;
        using (MySqlDataReader mySqlDataReader = mySqlCommandRead.ExecuteReader())
        {
          if (mySqlDataReader.Read())
          {
            byte sg1 = mySqlDataReader.GetByte(0);
            sg2 |= sg1;
            bNew = false;
          }
          else
          {
            bNew = true;
          }
        }
        // Elle est nouvelle, on l'insère
        if (bNew)
        {
          string sqlInsert = GetSQL("InsertEFtmp");
          using (MySqlCommand mySqlCommandInsert = new MySqlCommand(sqlInsert, cx))
          {
            mySqlCommandInsert.Parameters.AddWithValue("@pierres", imagePierres);
            mySqlCommandInsert.Parameters.AddWithValue("@sg", sg);
            mySqlCommandInsert.ExecuteNonQuery();
          }
        }
        else if(sg2 != sg)
        {
          // Elle est déjà répertoriée, mais adresse de nouvelles SG, on la met à jour
          string sqlUpdate = GetSQL("UpdateEFtmp");
          using (MySqlCommand mySqlCommandUpdate = new MySqlCommand(sqlUpdate, cx))
          {
            mySqlCommandUpdate.Parameters.AddWithValue("@pierres", imagePierres);
            mySqlCommandUpdate.Parameters.AddWithValue("@sg", sg2);
            mySqlCommandUpdate.ExecuteNonQuery();
          }
        }
      }
    }

    public IEnumerable<(byte sg, long nbSF)> DumpSFgroupbySG()
    {
      string sql = GetSQL("SFgroupbySG");
      using (MySqlCommand mySqlCommand = new MySqlCommand(sql, cx))
      {
        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
        {
          while (mySqlDataReader.Read())
          {
            byte sg = mySqlDataReader.GetByte(0);
            long nb = mySqlDataReader.GetInt64(1);
            yield return (sg, nb);
          }
        }
      }
    }
  }
}
