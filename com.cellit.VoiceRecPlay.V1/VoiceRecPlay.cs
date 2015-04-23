using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using ACD.Interface.V1;
using System.Data.SqlClient;

namespace com.cellit.VoiceRecPlay.V1
{
    [Provider(DisplayName = "Com.cellit.Mask.VoiceRecPlay", Description = "Provider zum Abspielen von Voicerecords aus vorherigen Aufrufen", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit.Provider",SingletonConfiguration=true ,ConfigurationKey="VoicerecPlay")]
    public class VoiceRecPlay : IProvider
    {
        //Globale Variablen
        ICampaign currentcampaign;
        int ttCallProjektID;
        

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


        #endregion

        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "Button zum Abspielen der Voicefile", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int playField = 0;

       
        
        

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
                scripts.Add(this.GetProviderDatas().UrlDirectory + "js/VoiceRecProvider.js?V=1");
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
                scripts.Add("com.cellit.VoiceRecPlay.V1.ProviderInstance = new com.cellit.VoiceRecPlay.V1.VoiceRecProvider(%this%);");
                return scripts;
            }
        }

        #endregion

        // --------------------------------------- Provider-Code --------------------------------------------
        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        #region Initialized/Dispose
        public void Initialize(object args)
        {
            currentcampaign = (ICampaign)this.GetParentProvider().GetParentProvider();
            if (currentcampaign.GetProviderDatas().IsInitialized == true)
            {
                ttCallProjektID = GetprojektID(currentcampaign.ID);
                this.Log(LogType.Debug, Convert.ToString("VoiceRecProvider Initilisiert " + currentcampaign.Name));
                this.Log(LogType.Debug, Convert.ToString(currentcampaign.Name));
            }
            else
            {
                currentcampaign.GetProviderEvents().Initialized += campagnInitialized;
            }
        }
        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }
        #endregion

        #region Provider-Code
           

        //Wenn Campagne Initialisiert Event deregistrieren und Variablen speichern
        private void campagnInitialized(object sender, EventArgs e)
        {
            currentcampaign.GetProviderEvents().Initialized -= campagnInitialized;
            ttCallProjektID = GetprojektID(currentcampaign.ID);
            this.Log(LogType.Debug, Convert.ToString("VoiceRecProvider Initilisiert "+currentcampaign.Name));

        }

        [ScriptVisible]
        public WebRessource GetSoundFile(string name)
        {
            //Souudfile aus Ressource laden 
            byte[] data = System.IO.File.ReadAllBytes(name);
            return new WebRessource("Test", "audio/mpeg", data, 300);
        }
        private int GetprojektID(int campaignID)
        {
            string sql = "SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString();
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);
            

            return Convert.ToInt32(ds.Tables[0].Rows[0]["Campaign_Reference"]);

        }
        [ScriptVisible]
        public int GetVoiceAnzahl(int kundenID)
        {
            int count = 0;
            string sql = "SELECT Count([Aufrufe_ID]) as count From Dat_000" + ttCallProjektID.ToString() + "_Aufrufe (nolock) WHERE Aufrufe_Kunden_ID = " + kundenID.ToString() + " and Aufrufe_TkAnlage is not null ";
            System.Data.DataSet dscount = this.GetDefaultDatabaseConnection().Select(sql);

            count = Convert.ToInt32(dscount.Tables[0].Rows[0]["count"]);

            return count;
        }
        [ScriptVisible]
        public object GetVoicepfad(int kundenID,int count)
        {
            object[,] pfad = new object[count,4];
            

            string sql = "SELECT  Aufrufe_tkAnlage,Aufrufe_AnrufNr,Aufrufe_Datum,cast(Aufrufe_Zeit as time) as Aufrufe_Zeit From Dat_000" + ttCallProjektID.ToString() + "_Aufrufe (nolock) WHERE Aufrufe_Kunden_ID = " + kundenID.ToString() + " and Aufrufe_TkAnlage is not null order by Aufrufe_ID ";
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    pfad[i,0] =  Convert.ToString(ds.Tables[0].Rows[i]["Aufrufe_tkAnlage"]) ;
                    pfad[i,1] =  Convert.ToString(ds.Tables[0].Rows[i]["Aufrufe_AnrufNr"]);
                    pfad[i,2] =  Convert.ToString(ds.Tables[0].Rows[i]["Aufrufe_Datum"]).Substring(0, 10) ;
                    pfad[i,3] =  Convert.ToString(ds.Tables[0].Rows[i]["Aufrufe_Zeit"]);
                }
                
            }
            catch (Exception)
            {
                this.Log(LogType.Debug, Convert.ToString("VoiceRecProvider ERROR Kein VR vorhanden für Kunden ID: "+kundenID.ToString()));
                
            }
            return pfad;
        }
        #endregion
    }
}
