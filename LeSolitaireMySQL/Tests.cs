using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace LeSolitaireMySQL
{
  public class Tests
  {
    private const string connectionStringName = "LeSolitaireDB";

    // Assure l'ouverture de la connexion à la DB
    // A utiliser dans une construction using()
    private class MyContext : IDisposable
    {
      public MySqlConnection cx;
      public MyContext(string connectionString)
      {
        cx = new MySqlConnection(connectionString);
        try
        {
          cx.Open();
        }
        catch (Exception ex)
        {
          Debug.Print($"{ex}");
          throw;
        }
      }
      public void Dispose()
      {
        ((IDisposable)cx).Dispose();
      }
    }

    // Création d'une base de données, d'une table, insertions, lectures, recherches
    // On n'a pas le choix de l'endroit où la DB est créée : 
    // c'est un sous-répertoire de 
    // C:\ProgramData\MySQL\MySQL Server 8.0\Data
    // Les paramètres généraux de fonctionnement de MySQL sont dans des fichiers my.ini
    // C:\ProgramData\MySQL\MySQL Server 8.0\my.ini
    // C:\Users\All Users\MySQL\MySQL Server 8.0\my.ini
    // (All Users est un raccourci sur c:\ProgramData)
    // Peut-on avoir un my.ini individuel ?
    // Dans my.ini, le réglage qui nous importe ici est : 
    // datadir=C:/ProgramData/MySQL/MySQL Server 8.0/Data
    // Avec le moteur MyISAM, une DB a un nom, et est représentée par un répertoire de même nom dans $datadir
    //
    public void Create_DB_table_insert_select()
    {
      string connectionString = GetConnectionString();
      using (MyContext myContext = new MyContext(connectionString))
      {
        // Création DB de nom test
        TestCreateDB(myContext.cx);
        int nbPierres = 13;
        int nbInsert = 10;
        // (suppression et ) création table tbl
        TestDropAndCreateTbl(myContext.cx, nbPierres);
        // boucle création lignes distinctes
        TestInsert(myContext.cx, nbPierres, nbInsert);
        // relecture de toute la table
        TestLecture(myContext.cx, nbPierres);
        // relecture individuelle de tbl pour chaque bytes inséré auparavant
        TestRecherche(myContext.cx, nbPierres, nbInsert);
      }
    }

    // Lit dans les settings la connection string de nom connectionStringName 
    private string GetConnectionString()
    {
      ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;
      if (settings == null)
      {
        throw new ApplicationException("Aucune connexion string n'a été trouvée");
      }
      ConnectionStringSettings cs = settings[connectionStringName];
      if (cs == null)
      {
        throw new ApplicationException($"La connexion string de nom '{connectionStringName}' est introuvable");
      }
      return cs.ConnectionString;
    }

    private void TestCreateDB(MySqlConnection cx)
    {
      string cmdString = "create database if not exists test";
      using (MySqlCommand cmd = new MySqlCommand(cmdString, cx))
      {
        int n = cmd.ExecuteNonQuery();
        Debug.Print($"résultat create database : {n}");
      }
    }

    private void TestDropAndCreateTbl(MySqlConnection cx, int nbPierres)
    {
      // On enchaine les deux instructions dans une même opération
      // drop de la table si elle existe
      // création table tbl avec tableau de bytes indexé, et test des techniques de insert, select
      string cmdString = $@"drop table if exists tbl;
CREATE TABLE `tbl` (
`pierres` binary({nbPierres}) NOT NULL,
`si` tinyint NOT NULL,
PRIMARY KEY (`pierres`)
) ENGINE=MyISAM;";
      using (MySqlCommand cmd = new MySqlCommand(cmdString, cx))
      {
        int n = cmd.ExecuteNonQuery();
        Debug.Print($"résultat drop/create table : {n}");
      }
    }

    private void TestInsert(MySqlConnection cx, int nbPierres, int nbInsert)
    {
      using (MySqlCommand cmd = new MySqlCommand("insert into tbl (pierres,si)values(@pierres,@si)", cx))
      {
        cmd.Parameters.Add("@pierres", MySqlDbType.Binary, nbPierres);
        cmd.Parameters.Add("@si", MySqlDbType.Byte);
        byte[] pierres = new byte[13];
        byte si;
        for (int j = 0; j < nbInsert; j++)
        {
          si = (byte)j;
          for (int i = 0; i < nbPierres; i++)
          {
            pierres[i] = ((byte)(i + 'a' + j));
          }
          cmd.Parameters[0].Value = pierres;
          cmd.Parameters[1].Value = si;
          int n = cmd.ExecuteNonQuery();
          Debug.Print($"résultat insert : {n}");
        }
      }
    }

    private void TestLecture(MySqlConnection cx, int nbPierres)
    {
      using (MySqlCommand cmd = new MySqlCommand("select pierres,si from tbl order by pierres;", cx))
      {
        using (MySqlDataReader mySqlDataReader = cmd.ExecuteReader())
        {
          byte[] pierres = new byte[nbPierres];
          byte si;
          while (mySqlDataReader.Read())
          {
            mySqlDataReader.GetBytes(0, 0, pierres, 0, nbPierres);
            si = mySqlDataReader.GetByte(1);
            string s = string.Empty;
            for (int i = 0; i < nbPierres; i++)
            {
              s += $"{pierres[i]:x2}";
            }
            s += $" {si:x2}";
            Debug.Print(s);
          }
        }
      }
    }

    private void TestRecherche(MySqlConnection cx, int nbPierres, int nbInsert)
    {
      using (MySqlCommand cmd = new MySqlCommand("select si from tbl where pierres = @pierres", cx))
      {
        cmd.Parameters.Add("@pierres", MySqlDbType.Binary, nbPierres);
        byte[] pierres = new byte[nbPierres];
        byte si;
        for (int j = 0; j < nbInsert; j++)
        {
          // Attention : reconstruire le tableau pierres de la même manière que lors de l'insertion
          for (int i = 0; i < nbPierres; i++)
          {
            pierres[i] = ((byte)(i + 'a' + j));
          }
          cmd.Parameters[0].Value = pierres;
          using (MySqlDataReader mySqlDataReader = cmd.ExecuteReader())
          {
            while (mySqlDataReader.Read())
            {
              si = mySqlDataReader.GetByte(0);
              string s = $"j={j} ";
              for (int i = 0; i < nbPierres; i++)
              {
                s += $"{pierres[i]:x2}";
              }
              s += $" {si:x2}";
              Debug.Print(s);
            }
          }
        }
      }
    }

  }
}
