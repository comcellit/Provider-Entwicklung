using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using System.IO;
using System.Reflection;
using System.Web;
using ACD.Interface.V1;
using System.Net.Mail;


namespace com.cellit.MailProvider.V1
{
    [Provider(DisplayName = "Com.cellit Mask SendMailProvider", Description = "EMail Versand Provider", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit.Provider")]
    public class MailProvider : IProvider
    {
        //Globale Variablen
        private static string _server;
        private static int _port;
        private static string _username;
        private static string _passwort;
        private static bool _ssl;
        private static string _bcc;
        private static string _subject;
        private static string _body;
        ICampaign currentcampaign;
        private static int ttCallProjektID;
        


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

        [ConfigSetting(Frame = "Kontoeinstellung", Label = "Smtp Server", FieldType = FieldType.TextField)]
        public string server
        {
            get { return _server; }
            set { _server = value; }
        }

        [ConfigSetting(Frame = "Kontoeinstellung", Label = "Smtp Server", FieldType = FieldType.NumberField)]
        public int port
        {
            get { return _port; }
            set { _port = value; }
        }

        [ConfigSetting(Frame = "Kontoeinstellung", Label = "User/Mailadresse", FieldType = FieldType.TextField)]
        public string user
        {
            get { return _username; }
            set { _username = value; }
        }
        [ConfigSetting(Frame = "Kontoeinstellung", Label = "Passwort", FieldType = FieldType.PasswordField)]
        public string passwort
        {
            get { return _passwort; }
            set { _passwort = value; }
        }
        [ConfigSetting(Frame = "Kontoeinstellung", Label = "SSL", FieldType = FieldType.CheckBox)]
        public bool ssl
        {
            get { return _ssl; }
            set { _ssl = value; }
        }

        [ConfigSetting(Frame = "Kontoeinstellung", Label = "Einstellungen testen")]
        public static void CheckConnection(Dictionary<string, object> settings)
        {
                SmtpClient test = new SmtpClient(_server, _port);
                test.Credentials = new System.Net.NetworkCredential(_username, _passwort);
                if (_ssl == true)
                {
                    test.EnableSsl = true;
                }
                else
                {
                    test.EnableSsl = false;
                }
                test.Send(new MailMessage(_username, _username, "test", "Test"));
        }
        

        #endregion

        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "Feld zum versenden", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int send;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "TrasaktionsID Speichern auf Feld", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int transaktion;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "Kunden Email", FieldType = FieldType.ComboBox, Values = "GetFields")]
        public int mailfield;

        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "Bcc an", FieldType = FieldType.TextField)]
        public string bcc
        {
            get { return _bcc; }
            set { _bcc = value; }
        }

        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "Betreff", FieldType = FieldType.TextField)]
        public string subject
        {
            get { return _subject; }
            set { _subject = value; }

        }

        [RuntimeSetting(Frame = "Provider Einstellungen", Label = "E-Mail body html", FieldType = FieldType.TextArea, Height = 500, Width = 500)]
        public string nachricht
        {
            get { return _body; }
            set { _body = value; }
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
                scripts.Add(this.GetProviderDatas().UrlDirectory + "js/MailProvider.js?V=1");
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
                scripts.Add("com.cellit.MailProvider.V1.ProviderInstance = new com.cellit.MailProvider.V1.MailProvider(%this%);");
                return scripts;
            }
        }

        #endregion

        // --------------------------------------- Provider-Code --------------------------------------------

        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        #region Initilize/Dispose

        public void Initialize(object args)
        {
            currentcampaign = (ICampaign)this.GetParentProvider().GetParentProvider();
            if (currentcampaign.GetProviderDatas().IsInitialized == true)
            {
                ttCallProjektID = GetprojektID(currentcampaign.ID);
                this.Log(LogType.Debug, Convert.ToString("MailProvider Initilisiert " + currentcampaign.Name));
                this.Log(LogType.Debug, Convert.ToString(currentcampaign.Name));
            }
            else
            {
                currentcampaign.GetProviderEvents().Initialized += campagnInitialized;
            }

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
                    this.Log(LogType.Debug, Convert.ToString("Test" + e));
                }
                
            }
        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }

        #endregion

        #region Provider Code
        //Fals die Campagn noch nicht initialisiert war wir darauf gewartet
        private void campagnInitialized(object sender, EventArgs e)
        {
            currentcampaign.GetProviderEvents().Initialized -= campagnInitialized;
            ttCallProjektID = GetprojektID(currentcampaign.ID);
            this.Log(LogType.Debug, Convert.ToString("VoiceRecProvider Initilisiert " + currentcampaign.Name));
        }
        //Projekt ID auslesen 
        private int GetprojektID(int campaignID)
        {
            string sql = "SELECT Campaign_Reference From Campaigns (Nolock) WHERE Campaign_Id = " + campaignID.ToString();
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);

            return Convert.ToInt32(ds.Tables[0].Rows[0]["Campaign_Reference"]);
        }
        //Get Field Methode
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
        //Mail Versand
        [ScriptVisible]
        public  void SendMail(string mailTo, string newBody) //E-Mail Versenden
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_username); //Absender
                mail.To.Add(mailTo); //empfänger
                if (_bcc == ""){
                    //Do Nothing
                }
                else{
                    mail.Bcc.Add(_bcc); //Blindkopie an
                }
                mail.Subject = _subject; //Betreff
                mail.IsBodyHtml = true;
                mail.Body = newBody;

                SmtpClient client = new SmtpClient(_server, _port); //SMTP Server und port


                client.Credentials = new System.Net.NetworkCredential(_username, _passwort); //Server Login
                if (_ssl == true)
                {
                    client.EnableSsl = true;
                }
                else
                {
                    client.EnableSsl = false;
                }
                client.Send(mail);
                this.Log(LogType.Debug, Convert.ToString("MailProvider Email erfolgreich versand ") + mailTo);
                
            }
            catch (Exception ex)
            {
                this.Log(LogType.Error, Convert.ToString("MailProvider ERROR ") + ex);
            }
            
        }
        //Transaktion in Sql speichern
        [ScriptVisible]
        public void SetTransaktion(string kundenId,  string kundenmail, string hex, string body)
        {

            string sql = "Insert Into _MailProviderTransaktion (ProjektID,transaktionID,KundenID,VersandDatum,VersandText,VersandUhrzeit,EmpfaengerAdresse,TransaktionEnd) values('" + ttCallProjektID + "','" + hex + "'," + kundenId + ",cast(GETDATE() as DATE),'" + body + "',getdate(),'" + kundenmail + "','false');";
            try
            {
                this.GetDefaultDatabaseConnection().Execute(sql);
            }
            catch (Exception error)
            {
                this.Log(LogType.Error, error);
            }
            
        }
        //Nachrichten Variablen ersetzen
        [ScriptVisible]
        public string ReplaceBody(string anrede,string vorname,string nachname,string transaktion )
        {
            string message= _body;
            message = message.Replace("[Anrede]", anrede);
            message = message.Replace("[Name]", nachname);
            message = message.Replace("[Vorname]", vorname);
            message = message.Replace("[transID]", transaktion);
            return message;
        }
        //Get Transaktion ID
        [ScriptVisible]
        public string GetTrasaktionID()
        {
            Random random = new Random();
            int num = random.Next();
            return num.ToString("X");

        }

        #endregion
    }
    
}
