using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using com.cellit.SMSGatewayService.V1;
using ACD.Interface.V1;

namespace com.cellit.SendSMSProvider.V1
{
    [Provider(DisplayName = "Com.cellit Mask Send SMS", Description = "SMS Versand", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit Mask")]
    public class SendSMSProvider : IProvider
    {
        private static int ttCallProjektID;
        ICampaign currentcampaign;
        public static bool _onlySend;
        public static int smstransfer;
        public static int smsRequestDate;
        public static int smsRequestText;
        private int _send;
        private int _phone;
        private string _text;
        private string _from;

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
        [ConfigSetting(Frame = "Einstellungen", Label = "SMS Gateway", Filter = "SMS.Gateway", AllowBlank = false)]
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
        public int send
        {
            get { return _send; }
            set { _send = value; }
        }


        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "Kunden Handynummer", FieldType = FieldType.ComboBox, Values = "GetDataFields", AllowBlank = false)]
        public int phone
        {
            get { return _phone; }
            set { _phone = value; }
        }


        [RuntimeSetting(Frame = "Einstellungen", Label = "Absenderadresse(Für Inbound immer 01771787827)", FieldType = FieldType.TextField, AllowBlank = false, MaxLength = 11)]
        public string from
        {
            get { return _from; }
            set { _from = value; }
        }


        private IProvider _anliegen;
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Einstellungen", Label = "Versandart", Filter = "SMS.Anliegen", AllowBlank = false)]
        public ISubProvider anliegen
        {
            get { return _anliegen as ISubProvider; }
            set { _anliegen = value; }
        }


        [RuntimeSetting(Frame = "Inhalt SMS Text", Label = "SMS Text", FieldType = FieldType.TextArea, Height = 100, AllowBlank = false)]
        public string text
        {
            get { return _text; }
            set { _text = value; }
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

        #region Provider Code
        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        public void Initialize(object args)
        {
            SMSGatewayService.V1.OnEvent.Inbound += OnEvent_Inbound;//Eingehende SMS Event Registrieren
            currentcampaign = (ICampaign)this.GetParentProvider().GetParentProvider();
            if (currentcampaign.GetProviderDatas().IsInitialized == true)
            {
                ttCallProjektID = GetprojektID(currentcampaign.ID);

            }
            else
            {
                currentcampaign.GetProviderEvents().Initialized += campagnInitialized;
            }


        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
            SMSGatewayService.V1.OnEvent.Inbound -= OnEvent_Inbound;
        }

        //Projekt ID auslesen 
        private int GetprojektID(int campaignID)
        {
            string sql = "SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString();
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);

            return Convert.ToInt32(ds.Tables[0].Rows[0]["Campaign_Reference"]);
        }

        //Falls die Campagn noch nicht initialisiert war wird darauf gewartet
        private void campagnInitialized(object sender, EventArgs e)
        {
            currentcampaign.GetProviderEvents().Initialized -= campagnInitialized;
            ttCallProjektID = GetprojektID(currentcampaign.ID);
        }

        //Datenfelder für Runtimesettings
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

        //Datenfelder/Ergebnisfelder für Runtimesettings
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

        [ScriptVisible]
        public event EventHandler ReceiveMessage;//Event für Java bereitstellen

        [ScriptVisible]
        public string SendSmS(string phonenumber)
        {
            string batchid = null;
            try
            {
                batchid = _smsGateway.CallbyName("SendSMS", phonenumber, text, from, _onlySend, ttCallProjektID.ToString(), smstransfer - 200, smsRequestText - 200, smsRequestDate - 200).ToString();
            }
            catch (Exception ex)
            {
                this.Log(LogType.Error, ex);
            }
            return batchid;
        }//SMS Versand

        //SMS Eingehend Event wird Gestartet und zum Agenten geleitet Pro SMS
        void OnEvent_Inbound(object sender, EventArgs e)
        {
            int i = 0;
            string sql = "select KundeAntwort as msg, Antwortam as empfang, BatchID, Phonenumber from Provider_SmSTransfer where KundeAntwort is not null and SmSEnd=0";
            System.Data.DataSet dataset = this.GetDefaultDatabaseConnection().Select(sql);
            foreach (System.Data.DataRow dataRow in dataset.Tables[0].Rows)//Für jede SMS in Datenbank wird das Event gestartet
            {
                receive(new object[] { dataset.Tables[0].Rows[i]["BatchID"].ToString(), dataset.Tables[0].Rows[i]["Phonenumber"].ToString(), dataset.Tables[0].Rows[i]["empfang"].ToString(), dataset.Tables[0].Rows[i]["msg"].ToString() });
                // Antwort verzögern
                System.Threading.Thread.Sleep(1000);
                i++;
            }

        }

        //Empfangs Event in der Maske auslösen
        private void receive(object args)
        {
            // Registrierte Handler prüfen
            EventHandler receiveMessage = this.ReceiveMessage;
            if (receiveMessage != null)
            {
                // Daten ermitteln
                object[] data = (object[])args;
                string batchID = data[0].ToString();
                string from = data[1].ToString();
                string date = data[2].ToString();
                string msg = data[3].ToString();
                // Event auslösen
                receiveMessage(this, new ParamArrayEventArgs(batchID, from, date, msg));

            }
        }

        //SMS Zuordnung beenden aufruf aus Maske
        [ScriptVisible]
        public void SetSmsEnd(string batchID)
        {
            try
            {
                this.GetDefaultDatabaseConnection().Execute("update Provider_SmSTransfer set SmSEnd='true' where KundeAntwort is not null and BatchID='" + batchID + "'");
            }
            catch (Exception e)
            {
                this.Log(LogType.Error, e);
            }

        }

        //Verpasste SMS Abrufen aufruf aus Maske
        [ScriptVisible]
        public bool GetAllOpenSmS(string batchId)
        {
            System.Data.DataSet old = this.GetDefaultDatabaseConnection().Select("select count(BatchID) as old from Provider_SmSTransfer where SmSEnd=0 and KundeAntwort is not null and BatchId='" + batchId + "'");
            if (Convert.ToInt32(old.Tables[0].Rows[0]["old"].ToString()) > 0)
            {
                OnEvent_Inbound(this, EventArgs.Empty);
                return true;
            }
            else
            {
                return false;
            }

        }

        //Handynummer säubern
        [ScriptVisible]
        public string ReplaceString(string telefon)
        {
            return System.Text.RegularExpressions.Regex.Replace(telefon, @"[^0-9]", string.Empty);
        }

        #endregion


        //SMS SubProvider Nur Senden
        [SubProvider(DisplayName = "Nur SMS Versand", Tags = "SMS.Anliegen")]
        public class SmSOnly : ISubProvider
        {

            #region Runtime-Settings
            // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            public static bool onlySend
            {
                get { return SendSMSProvider._onlySend; }
                set { SendSMSProvider._onlySend = value; }
            }


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
                SendSMSProvider._onlySend = true;
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

            #endregion

        }
        //SMS SubProvider Senden mit Antwort
        [SubProvider(DisplayName = "SMS mit Antwort", Tags = "SMS.Anliegen")]
        public class SmSRequest : ISubProvider
        {

            #region Runtime-Settings
            // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Einstellungen", Label = "Feld für TransferID", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int smstransfer
            {
                get { return SendSMSProvider.smstransfer; }
                set { SendSMSProvider.smstransfer = value; }
            }

            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Einstellungen", Label = "Feld für Datum Antwort", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int smsRequestDate
            {
                get { return SendSMSProvider.smsRequestDate; }
                set { SendSMSProvider.smsRequestDate = value; }
            }

            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Einstellungen", Label = "Feld für Kunden Antwort", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int smsRequestText
            {
                get { return SendSMSProvider.smsRequestText; }
                set { SendSMSProvider.smsRequestText = value; }
            }

            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Einstellungen", Label = "Verpasste SMS abrufen", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int smsOlder;


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
                SendSMSProvider._onlySend = false;
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

            #endregion

        }
    }

}
