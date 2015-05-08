using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using System.Reflection;

namespace com.cellit.SendSMSProvider.V1
{
    [Provider(DisplayName = "Com.cellit Mask SendSMSProvider", Description = "SMS Versand", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit.Provider")]
    public class SendSMSProvider : IProvider
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
  
        //Das Gateway verbinden 
        private IProvider _smsGateway;
        [ConfigSetting(Frame = "Einstellungen", Label = "SMS Gateway", Filter = "SMS.Gateway", AllowBlank=false)]
        public IProvider SmSGateway
        {
            get { return _smsGateway; }
            set { _smsGateway = value; }
        }
        

        #endregion

        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "Feld zum versenden", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
        public int send;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "Kunden Handynummer", FieldType = FieldType.ComboBox, Values = "GetDataFields", AllowBlank = false)]
        public int phone;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "SMs nur Senden", FieldType = FieldType.CheckBox, AllowBlank = false)]
        public bool onlysend;

        [RuntimeSetting(Frame = "Einstellungen", Label = "Absenderadresse", FieldType = FieldType.TextField, AllowBlank = false, MaxLength = 11)]
        public string from;

        [RuntimeSetting(Frame = "Einstellungen", Label = "SMS Text", FieldType = FieldType.TextArea, Height=100, AllowBlank = false)]
        public string text;



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
                scripts.Add(this.GetProviderDatas().UrlDirectory + "js/SMSProvider.js?V=1");
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
                scripts.Add("com.cellit.SendSMSProvider.V1.ProviderInstance = new com.cellit.SendSMSProvider.V1.SMSProvider(%this%);");
                return scripts;
            }
        }

        #endregion


        // --------------------------------------- Provider-Code --------------------------------------------
        public EventHandler inbound;
        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        public void Initialize(object args)
        {
        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
            
        }

        public static object GetDataFields(Dictionary<string, object> settings)
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
                //for (int i = 1; i <= 50; i++)
                //{
                //    result.Add(new object[] { i, "Ergebnisfeld " + i.ToString() });
                //}
            }

            return result;
        }

        [ScriptVisible]
        public void SendSmS(string phonenumber)
        {
            
            try
            {
                _smsGateway.CallbyName("SendSMS", phonenumber, text, from);
            }
            catch (Exception)
            {
                //Do Nothing
            }
            
        }

        
       
    }
    
}
