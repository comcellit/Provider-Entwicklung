using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using ACD.Interface.V1;

namespace com.cellit.Mask.Extention.V1
{
    //,SingletonConfiguration=true ,ConfigurationKey="MaskExtentionWeis"
    [Provider(DisplayName = "ttCall4 Masken schulung", Description = "Voicerec Play Provider", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit")]
    public class MaskExtention : IProvider
    {
        ICampaign currentcampaign;

        int ttcallProjektID;

       

        #region Add/Remove-Provider

        // wird augerufen, wenn der Provider hinzugefügt wird
        public static void ProviderAdded(Dictionary<string, object> settings)
        {
            //get unique directory 
            //string targetDir = Extension.GetProviderDatas((IProvider)settings["this"]).AssemblyDirect + Extension.GetMandatorKey((IProvider)settings["this"]) + "\\" + Extension.GetProviderDatas((IProvider)settings["this"]).Key;
            
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
        private IDatabaseConnection currentconnektion;

        [ConfigSetting(Frame = "Datenbank", Label = "Verbindung")]
        public IDatabaseConnection ConfigConnection = null;
        
        

        

        #endregion

        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  

        
        
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "Feld zur Abspielen Voicefile", FieldType = FieldType.ComboBox , Values = "GetFields")]
        public int playField = 0;



        // Hilfsfunktion zum Abrufen der ttCall-Felder
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
                scripts.Add(this.GetProviderDatas().UrlDirectory + "js/SampleProvider.js?V=1");
                scripts.Add(this.GetProviderDatas().UrlDirectory + "js/SimplePlayer.js?V=1");
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
                scripts.Add("com.cellit.Mask.Extention.V1.SampleProviderInstance = new com.cellit.Mask.Extention.V1.SampleProvider(%this%);");
                return scripts;
            }
        }

        #endregion



        // --------------------------------------- Provider-Code --------------------------------------------

        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        public void Initialize(object args)
        {
            currentcampaign = (ICampaign) this.GetParentProvider().GetParentProvider();
            if (currentcampaign.GetProviderDatas().IsInitialized==true)
            {
                ttcallProjektID= GetprojektID(currentcampaign.ID);
                this.Log(LogType.Debug, Convert.ToString(currentcampaign.Name));
            }
            else
            {
                currentcampaign.GetProviderEvents().Initialized += campagnInitialized;
            }
                       
            
            if (this.ConfigConnection== null)
            {
                currentconnektion = this.GetDefaultDatabaseConnection();
            }
            else
            {
                currentconnektion = this.ConfigConnection;
            }
        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }

        private void campagnInitialized(object sender, EventArgs e)
        {
            currentcampaign.GetProviderEvents().Initialized -= campagnInitialized;
            ttcallProjektID= GetprojektID(currentcampaign.ID);
            this.Log(LogType.Debug, Convert.ToString(currentcampaign.Name));
           
        }
        [ScriptVisible]
        public WebRessource GetSoundFile(string name)
        {
            //Souudfile aus Ressource laden (
            //alternativ: 
            byte[] data = System.IO.File.ReadAllBytes(name);
            return new WebRessource("Test", "audio/mpeg", data, 300);
        }
        private int GetprojektID(int campaignID)
        {
            string sql = "SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString() ;
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);
            
            return Convert.ToInt32(ds.Tables[0].Rows[0]["Campaign_Reference"]);

        }
        [ScriptVisible]
        public string GetVoicepfad(int kundenID)
        {
            string sql = "SELECT top(1) Aufrufe_tkAnlage From Dat_000"+ ttcallProjektID.ToString()+"_Aufrufe (nolock) WHERE Aufrufe_Kunden_ID = " + kundenID.ToString() +" and Aufrufe_TkAnlage is not null order by Aufrufe_ID desc";
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);

            return Convert.ToString(ds.Tables[0].Rows[0]["Aufrufe_tkAnlage"]);

        }
        // Kommentare beispiele

    }
}
