using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace com.cellit.ValidateProvider.V1
{
    [Provider(DisplayName = "Com.cellit Mask Field Validator", Description = "Prüfen von Feldinhalten auf Valide Inhalte oder entfernt sonderzeichen", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit Mask")]
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
        [RuntimeSetting(Frame = "Einstellungen", Label = "Prüfen auf", Filter = "Validate.Block")]
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
        [RuntimeSetting(Frame = "Feld Auswahl", Label = "Feld Email", FieldType = FieldType.ComboBox, Values = "GetFields")]
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
            catch (Exception ex)
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
    //Replace Sonderzeichen Subprobider
    [SubProvider(DisplayName = "Entfernt Sonderzeichen aus String", Tags = "Validate.Block")]
    public class ReplaceString : ISubProvider
    {
        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Feld Auswahl", Label = "Textfeld", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int textField;
        
        #endregion

        #region Provider Code

        public IProvider OwnerProvider
        {
            set { }
        }

        public void Dispose()
        {
        }

        public void Initialize(object args)
        {

            //throw new NotImplementedException();
        }

        // Felder für Einstellung ( NUR Datenfelder!)
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

        //Replace String Sonderzeichen
        [ScriptVisible]
        public string ReplaceStringValue(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text, @"[^0-9a-zA-Z .;.,_-]", string.Empty);
        }

        #endregion

    }
    //Replace Leerzeichen Subprobider
    [SubProvider(DisplayName = "Entfernt Leerzeichen aus String", Tags = "Validate.Block")]
    public class ReplaceSpace : ISubProvider
    {
        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Feld Auswahl", Label = "Textfeld", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int spaceField;


        #endregion

        #region Provider Code

        public IProvider OwnerProvider
        {
            set { }
        }

        public void Dispose()
        {
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

        //Replace String Space
        [ScriptVisible]
        public string ReplaceSpaceValue(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text,@"\s+", string.Empty);
        }
        
        #endregion

    }
    //Validiere Ausweisnummer
    [SubProvider(DisplayName="Überprüfe Ausweisnummer", Tags="Validate.Block")]
    public class ValidatePerso : ISubProvider
    {
        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Feld Auswahl", Label = "Ausweisnummer", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int persoField;


        #endregion

        #region Provider Code

        public IProvider OwnerProvider
        {
            set { }
        }

        public void Dispose()
        {
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

        //Replace String Space
        [ScriptVisible]
        public bool ValidateAusweis(string ausweis)
        {
            string[] number = new string[15];
            int[] multipler = new int[15] { 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1 };
            int pruefziffer = 0;
            int i = 0;
            foreach (char cr in ausweis)
            {
                if (cr == 'A') { number[i] = "10"; } if (cr == 'B') { number[i] = "11"; } if (cr == 'C') { number[i] = "12"; } if (cr == 'F') { number[i] = "15"; }
                if (cr == 'G') { number[i] = "16"; } if (cr == 'H') { number[i] = "17"; } if (cr == 'J') { number[i] = "19"; } if (cr == 'K') { number[i] = "20"; }
                if (cr == 'L') { number[i] = "21"; } if (cr == 'M') { number[i] = "22"; } if (cr == 'N') { number[i] = "23"; } if (cr == 'P') { number[i] = "25"; }
                if (cr == 'R') { number[i] = "27"; } if (cr == 'T') { number[i] = "29"; } if (cr == 'V') { number[i] = "31"; } if (cr == 'W') { number[i] = "32"; }
                if (cr == 'X') { number[i] = "33"; } if (cr == 'Y') { number[i] = "34"; } if (cr == 'Z') { number[i] = "35"; } 
                if (number[i] == null) 
                {
                    number[i] = cr.ToString();
                }
                if (i < ausweis.Length-1)
                {
                    pruefziffer = pruefziffer + Convert.ToInt32(number[i]) * multipler[i];
                }
                i++;
            }
            pruefziffer =Convert.ToInt32( pruefziffer.ToString().Substring((pruefziffer.ToString().Length - 1), 1));
            if (pruefziffer == Convert.ToInt32( number[ausweis.Length-1]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


    }

    
}
