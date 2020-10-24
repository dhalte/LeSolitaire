using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeSolitaireMySQL;

namespace Tests
{
  [TestClass]
  public class DB
  {
    [TestMethod]
    public void TestCreateDB()
    {
      LeSolitaireMySQL.Tests tests = new LeSolitaireMySQL.Tests();
      tests.Create_DB_table_insert_select();
    }

    [TestMethod]
    public void DBExists()
    {
      bool b = LeSolitaireMySQL.Common.DBexists("test");
      Assert.IsFalse(b);
    }
  }
}
