﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace com.cellit.ValidateProvider.V1
{
    [Provider(DisplayName = "Com.cellit Mask Field Validator", Description = "Prüfen von Feldinhalten auf Valide Inhalte", Tags = "ttCall4.Mask.Extention" , Category = "Com.cellit Mask")]
    public class ValidateProvider : IProvider
    {

        #region Add/Remove-Provider

        // wird augerufen, wenn der Provider hinzugefügt wird
        public static void ProviderAdded(Dictionary<string, object> settings)
        {
            //get unique directory 
            //string targetDir = Extension.GetProviderDatas((IProvider)settings["this"]).AssemblyDirectory + Extension.GetMandatorKey((IProvider)settings["this"]) + "\\" + Extension.GetProviderDatas((IProvider)settings["this"]).Key;

            //get compiled ressource
            //byte[] ressource = Extension.GetRessource((IProvider)settings["this"], "myRessource");
        }

        // wird augerufen, wenn der Provider gelöscht wird
        public static void ProviderRemoved(Dictionary<string, object> settings)
        {
        }

        #endregion

        #region Config-Settings
        // Werte, die bei der Konfiguration des Providers für alle Instanzen gesetzt werden können  

        //[ConfigSetting(Frame = "Datenbank", Label = "Verbindung")]
        //public IDatabaseConnection ConfigConnection = null;

        #endregion

        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
        private IProvider _auswahl;
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "Prüfe Feldinhalt auf", Filter = "Validate.Block")]
        public ISubProvider auswahl
        {
            get { return _auswahl as ISubProvider; }
            set { _auswahl = value; }
        }
        

        #endregion

        #region Script-Integration in ttCall 4

        public List<string> LoadScripts
        {
            get
            {
                List<string> scripts = new List<string>();
                scripts.Add(this.GetProviderDatas().UrlDirectory + "js/validateProvider.js");
                return scripts;
            }
        }
         public List<string> RunScripts
        {
            get
            {
                List<string> scripts = new List<string>();
                scripts.Add("com.cellit.ValidateProvider.V1.ValidateFunction(%this%);");
                return scripts;
            }
        }

        #endregion

        // --------------------------------------- Provider-Code --------------------------------------------
        
        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        public void Initialize(object args)
        {
        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }
        

    }
    //Check Date Subprovider
    [SubProvider(DisplayName = "Valides datum", Tags = "Validate.Block")]
    public class CheckDate : ISubProvider
    {
        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Datum prüfen", Label = "Feld Datum", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int dateField;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Datum prüfen", Label = "Prüfe auf ", FieldType = FieldType.ComboBox, Values = "GetValue")]
        public string type;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Datum prüfen", Label = "Prüf Ziffer", FieldType = FieldType.NumberField)]
        public int check;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Datum prüfen", Label = "Textnachricht bei Erfolg", FieldType = FieldType.TextField)]
        public string OKmessage;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Datum prüfen", Label = "Textnachricht bei Fehler", FieldType = FieldType.TextField)]
        public string Failmessage;
        

        #endregion

        #region Provider Code

        public IProvider OwnerProvider
        {
            set {  }
        }

        public void Dispose()
        {

            //throw new NotImplementedException();
        }

        public void Initialize(object args)
        {

            //throw new NotImplementedException();
        }

        //Combobox für Prüf einstellung
        public static object GetValue(Dictionary<string, object> settings)
        {
            List<object[]> value = new List<object[]>();
            value.Add(new object[] { "Jahre", "Jahre" });
            value.Add(new object[] { "Monate", "Monate" });
            value.Add(new object[] { "Tage", "Tage" });

            return value;
        }

        // Felder für Einstellung( NUR Datenfelder!)
        public static object GetFields(Dictionary<string, object> settings)
        {
            object campaignID = Extension.GetProviderDatas((IProvider)settings["this"]).OwnerID;
            List<object[]> result = new List<object[]>();
            bool isExtended = false;
            if (campaignID != null)
            {
                string sql = "SELECT * FROM Prog_Vtg_Bez_Art (Nolock) WHERE Vtg_Bez_Art_ProjektID IN (SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString() + ");" + "\r\n";
                sql += "SELECT Projekt_DBVersion FROM Global_Projekte (Nolock) WHERE Projekt_ID IN (SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString() + ");";
                using (System.Data.DataSet ds = Extension.GetDefaultDatabaseConnection((IProvider)settings["this"]).Select(sql))
                {
                    isExtended = (ds.Tables[1].Rows.Count == 1 && Convert.ToInt32(ds.Tables[1].Rows[0]["Projekt_DBVersion"]) == 3);

                    if (ds.Tables[0].Rows.Count == 1)
                    {
                        for (int i = 1; i <= 50; i++)
                        {
                            if (ds.Tables[0].Rows[0][i] != null && ds.Tables[0].Rows[0][i].ToString().Length > 0)
                            {
                                result.Add(new object[] { i + 200, ds.Tables[0].Rows[0][i].ToString() });
                            }
                            else
                            {
                                result.Add(new object[] { i + 200, "Datenfeld " + i.ToString() });
                            }
                        }
                        if (isExtended)
                        {
                            if (ds.Tables[0].Rows[0]["Vtg_Extended"].ToString().Length > 0)
                            {
                                object[] ExtFields = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(ds.Tables[0].Rows[0]["Vtg_Extended"].ToString()) as object[];

                                for (int i = 1; i <= ExtFields.Length; i++)
                                {
                                    string fieldCaption = ((object[])ExtFields[i - 1])[0].ToString();
                                    if (fieldCaption == "")
                                    {
                                        result.Add(new object[] { i + 250, "Datenfeld " + (i + 50).ToString() });
                                    }
                                    else
                                    {
                                        result.Add(new object[] { i + 250, fieldCaption });
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i <= 150; i++)
                                {
                                    result.Add(new object[] { i + 250, "Datenfeld " + (i + 50).ToString() });
                                }
                            }
                            //for (int i = 1; i <= 200; i++)
                            //{
                            //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            //}
                        }
                        else
                        {
                            //for (int i = 1; i <= 50; i++)
                            //{
                            //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            //}
                        }

                    }
                }
            }
            else
            {
                for (int i = 1; i <= 50; i++)
                {
                    result.Add(new object[] { i + 200, "Datenfeld " + i.ToString() });
                }
                //for (int i = 1; i <= 50; i++)
                //{
                //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                //}
            }

            return result;
        }

        [ScriptVisible]
        public static bool getCheckResult(int check, string date, string type)
        {
            DateTime now = DateTime.Now;
            DateTime checkbirth = Convert.ToDateTime(date);

            TimeSpan span = now - checkbirth;

            DateTime ResultAge = DateTime.MinValue + span;


            int Years = ResultAge.Year - 1;
            int Months = ResultAge.Month - 1;
            int Days = ResultAge.Day - 1;

            if (date == null)
            {
                return false;
            }
            else
            {

                if (type == "Jahre")
                {
                    if (Years >= check)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (type == "Monate")
                {
                    if (Months >= check)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (type == "Tage")
                {
                    if (Days >= check)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

    }
    //Check MailAdress Subprobider
    [SubProvider(DisplayName = "Valide Email Adresse", Tags = "Validate.Block")]
    public class CheckMailAdress : ISubProvider
    {
        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Email Adresse", Label = "Feld Email", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int mailField;



        #endregion

        #region Provider Code

        public IProvider OwnerProvider
        {
            set { }
        }

        public void Dispose()
        {

            //throw new NotImplementedException();
        }

        public void Initialize(object args)
        {

            //throw new NotImplementedException();
        }

        // Felder für Einstellung( NUR Datenfelder!)
        public static object GetFields(Dictionary<string, object> settings)
        {
            object campaignID = Extension.GetProviderDatas((IProvider)settings["this"]).OwnerID;
            List<object[]> result = new List<object[]>();
            bool isExtended = false;
            if (campaignID != null)
            {
                string sql = "SELECT * FROM Prog_Vtg_Bez_Art (Nolock) WHERE Vtg_Bez_Art_ProjektID IN (SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString() + ");" + "\r\n";
                sql += "SELECT Projekt_DBVersion FROM Global_Projekte (Nolock) WHERE Projekt_ID IN (SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString() + ");";
                using (System.Data.DataSet ds = Extension.GetDefaultDatabaseConnection((IProvider)settings["this"]).Select(sql))
                {
                    isExtended = (ds.Tables[1].Rows.Count == 1 && Convert.ToInt32(ds.Tables[1].Rows[0]["Projekt_DBVersion"]) == 3);

                    if (ds.Tables[0].Rows.Count == 1)
                    {
                        for (int i = 1; i <= 50; i++)
                        {
                            if (ds.Tables[0].Rows[0][i] != null && ds.Tables[0].Rows[0][i].ToString().Length > 0)
                            {
                                result.Add(new object[] { i + 200, ds.Tables[0].Rows[0][i].ToString() });
                            }
                            else
                            {
                                result.Add(new object[] { i + 200, "Datenfeld " + i.ToString() });
                            }
                        }
                        if (isExtended)
                        {
                            if (ds.Tables[0].Rows[0]["Vtg_Extended"].ToString().Length > 0)
                            {
                                object[] ExtFields = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(ds.Tables[0].Rows[0]["Vtg_Extended"].ToString()) as object[];

                                for (int i = 1; i <= ExtFields.Length; i++)
                                {
                                    string fieldCaption = ((object[])ExtFields[i - 1])[0].ToString();
                                    if (fieldCaption == "")
                                    {
                                        result.Add(new object[] { i + 250, "Datenfeld " + (i + 50).ToString() });
                                    }
                                    else
                                    {
                                        result.Add(new object[] { i + 250, fieldCaption });
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i <= 150; i++)
                                {
                                    result.Add(new object[] { i + 250, "Datenfeld " + (i + 50).ToString() });
                                }
                            }
                            //for (int i = 1; i <= 200; i++)
                            //{
                            //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            //}
                        }
                        else
                        {
                            //for (int i = 1; i <= 50; i++)
                            //{
                            //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            //}
                        }

                    }
                }
            }
            else
            {
                for (int i = 1; i <= 50; i++)
                {
                    result.Add(new object[] { i + 200, "Datenfeld " + i.ToString() });
                }
                //for (int i = 1; i <= 50; i++)
                //{
                //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                //}
            }

            return result;
        }

        [ScriptVisible]
        public static bool EmailIsValid(string email)
        {
            try
            {
                MailMessage validate = new MailMessage();
                validate.From = new MailAddress(email);
                return true;
            }
            catch (Exception)
            {
                return false; 
               
            }
            //string expresion;
            //expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            //if (Regex.IsMatch(email, expresion))
            //{
            //    if (Regex.Replace(email, expresion, string.Empty).Length == 0)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //else
            //{
            //    return false;
            //}
        }

        #endregion

    }
    //Keine Sonderzeichen Subprovider
    [SubProvider(DisplayName = "Entfernte Sonderzeichen Text", Tags = "Validate.Block")]
    public class ReplaceSonderzeichen : ISubProvider
    {
        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Email Adresse", Label = "Feld zur prüfung", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int textField;



        #endregion

        #region Provider Code

        public IProvider OwnerProvider
        {
            set { }
        }

        public void Dispose()
        {

            //throw new NotImplementedException();
        }

        public void Initialize(object args)
        {

            //throw new NotImplementedException();
        }

        // Felder für Einstellung( NUR Datenfelder!)
        public static object GetFields(Dictionary<string, object> settings)
        {
            object campaignID = Extension.GetProviderDatas((IProvider)settings["this"]).OwnerID;
            List<object[]> result = new List<object[]>();
            bool isExtended = false;
            if (campaignID != null)
            {
                string sql = "SELECT * FROM Prog_Vtg_Bez_Art (Nolock) WHERE Vtg_Bez_Art_ProjektID IN (SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString() + ");" + "\r\n";
                sql += "SELECT Projekt_DBVersion FROM Global_Projekte (Nolock) WHERE Projekt_ID IN (SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString() + ");";
                using (System.Data.DataSet ds = Extension.GetDefaultDatabaseConnection((IProvider)settings["this"]).Select(sql))
                {
                    isExtended = (ds.Tables[1].Rows.Count == 1 && Convert.ToInt32(ds.Tables[1].Rows[0]["Projekt_DBVersion"]) == 3);

                    if (ds.Tables[0].Rows.Count == 1)
                    {
                        for (int i = 1; i <= 50; i++)
                        {
                            if (ds.Tables[0].Rows[0][i] != null && ds.Tables[0].Rows[0][i].ToString().Length > 0)
                            {
                                result.Add(new object[] { i + 200, ds.Tables[0].Rows[0][i].ToString() });
                            }
                            else
                            {
                                result.Add(new object[] { i + 200, "Datenfeld " + i.ToString() });
                            }
                        }
                        if (isExtended)
                        {
                            if (ds.Tables[0].Rows[0]["Vtg_Extended"].ToString().Length > 0)
                            {
                                object[] ExtFields = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(ds.Tables[0].Rows[0]["Vtg_Extended"].ToString()) as object[];

                                for (int i = 1; i <= ExtFields.Length; i++)
                                {
                                    string fieldCaption = ((object[])ExtFields[i - 1])[0].ToString();
                                    if (fieldCaption == "")
                                    {
                                        result.Add(new object[] { i + 250, "Datenfeld " + (i + 50).ToString() });
                                    }
                                    else
                                    {
                                        result.Add(new object[] { i + 250, fieldCaption });
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i <= 150; i++)
                                {
                                    result.Add(new object[] { i + 250, "Datenfeld " + (i + 50).ToString() });
                                }
                            }
                            //for (int i = 1; i <= 200; i++)
                            //{
                            //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            //}
                        }
                        else
                        {
                            //for (int i = 1; i <= 50; i++)
                            //{
                            //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            //}
                        }

                    }
                }
            }
            else
            {
                for (int i = 1; i <= 50; i++)
                {
                    result.Add(new object[] { i + 200, "Datenfeld " + i.ToString() });
                }
                //for (int i = 1; i <= 50; i++)
                //{
                //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                //}
            }

            return result;
        }

        [ScriptVisible]
        public string Replace(string feldinhalt)
        {
            string test = System.Text.RegularExpressions.Regex.Replace(feldinhalt, @"[^0-9a-zA-Z .;.,_-]", string.Empty);
            return System.Text.RegularExpressions.Regex.Replace(feldinhalt, @"[^0-9a-zA-Z .;.,_-]", string.Empty);
        }

        #endregion

    }
    
}