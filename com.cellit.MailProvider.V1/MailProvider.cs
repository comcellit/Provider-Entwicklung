using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using System.IO;
using System.Reflection;
using System.Web;
using ACD.Interface.V1;


namespace com.cellit.MailProvider.V1
{
    [Provider(DisplayName = "Com.cellit Mask MailProvider", Description = "EMail Versand Provider", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit.Provider")]
    public class MailProvider : IProvider
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
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Kontoeinstellung", Label = "Smtp Server", FieldType = FieldType.TextField)]
        public string server;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Kontoeinstellung", Label = "User/Mailadresse", FieldType = FieldType.TextField)]
        public string user;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Kontoeinstellung", Label = "Passwort", FieldType = FieldType.PasswordField)]
        public string passwort;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Kontoeinstellung", Label = "SSL", FieldType = FieldType.CheckBox)]
        public bool ssl;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "Feld zum versenden", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int send;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "TrasaktionsID Speichern auf Feld", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int transaktion;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "Kunden Email", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int email;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "Bcc an", FieldType = FieldType.TextField)]
        public string bcc;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "E-Mail Inhalt html", FieldType = FieldType.TextArea , Height=500 , Width=500 )]
        public string Nachricht;
        
        #endregion

        #region Script-Integration in ttCall 4

        // Diese Funktionen werden seitens des Parent-Providers aufgerunfen, 
        // um ggf. Script-Ressourcen hinterlegen und/oder ausführen zu können.  

        // zu ladende Script-Dateinen 
        public List<string> LoadScripts
        {
            get
            {
                List<string> scripts = new List<string>();
                // zu ladende Scripte müssen als Ressourcen eingebettet sein oder im Pfad als Datei vorliegen
                //scripts.Add(this.GetProviderDatas().UrlDirectory + "js/VoiceRecProvider.js?V=1");
                return scripts;
            }
        }

        // Code zum Inistialisieren der Scripte, nachdem alle zuvor angegebenen Scripte geladen wurden.
        // Hinweis: alternativ kann hierfür auch ein zu ladendes Script verwendet werden
        public List<string> RunScripts
        {
            get
            {
                List<string> scripts = new List<string>();
                // der Platzhalter %this% dient als Verweiß für diese Instance der C#-Klasse (siehe ScriptVisible) 
                //scripts.Add("com.cellit.VoiceRecPlay.V1.ProviderInstance = new com.cellit.VoiceRecPlay.V1.VoiceRecProvider(%this%);");
                return scripts;
            }
        }

        #endregion

        // --------------------------------------- Provider-Code --------------------------------------------

        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        #region Initilize/Dispose
        public void Initialize(object args)
        {
            string sql = "select COUNT(*) as count from INFORMATION_SCHEMA.TABLES where TABLE_NAME='_MailProviderTransaktion'";
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);
            int exists = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
            
            if (exists==1)
            {
                //do Nothing
            }
            else
            {
                Assembly _Assembly = Assembly.GetExecutingAssembly();
                StreamReader sr = new StreamReader(_Assembly.GetManifestResourceStream("com.cellit.MailProvider.V1.sql.cmd.txt"));
                string data = sr.ReadLine();
                string insert="";
              
                while (data!=null)
                {
                    insert += " " + data;
                    data = sr.ReadLine();
                }

                try
                {
                    this.GetDefaultDatabaseConnection().Execute(insert);
                }
                catch (Exception e)
                {
                    this.Log(LogType.Debug, Convert.ToString("Test"+ e));
                    //throw;
                }
            }
        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }
        #endregion

        #region Provider Code

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
                            for (int i = 1; i <= 200; i++)
                            {
                                result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            }
                        }
                        else
                        {
                            for (int i = 1; i <= 50; i++)
                            {
                                result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                            }
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
                for (int i = 1; i <= 50; i++)
                {
                    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                }
            }

            return result;
        }

        #endregion
    }
}
