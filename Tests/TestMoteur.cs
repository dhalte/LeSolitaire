﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using LeSolitaireLogique;
using System.Diagnostics;
using System.IO;

namespace Tests
{
  [TestClass]
  public class TestMoteur:Feedback
  {
    public void Feedback(FeedbackHint hint, string msg)
    {
      Debug.Print($"{hint} {msg}");
    }

    [TestMethod]
    public void TestInitialisation()
    {
      DirectoryInfo tmp = new DirectoryInfo("tmp");
      if (tmp.Exists)
      {
        tmp.Delete(true);
      }
      Moteur moteur = new Moteur(tmp.FullName, this);
      string descriptionPlateau;
      descriptionPlateau =@"
  xxx   
 xxxxx  
xxxxxxx
xxxxxxx
xxxxxxx
 xxxxx  
  xxx   
";
      moteur.Initialise(descriptionPlateau);
    }

    [TestMethod]
    public void TestMouvementsOriginels()
    {
      DirectoryInfo tmp = new DirectoryInfo("tmp");
      if (tmp.Exists)
      {
        tmp.Delete(true);
      }
      Moteur moteur = new Moteur(tmp.FullName, this);
      string descriptionPlateau;
      descriptionPlateau = @"
  xxx   
  xxx   
xxxxxxx
xxxxxxx
xxxxxxx
  xxx   
  xxx   
";
      moteur.Initialise(descriptionPlateau);
      Stopwatch sw = Stopwatch.StartNew();
      moteur.TestMouvementsOriginels();
      sw.Stop();
      Trace.WriteLine($"durée : {sw.ElapsedMilliseconds} ms");
    }
  }
}
