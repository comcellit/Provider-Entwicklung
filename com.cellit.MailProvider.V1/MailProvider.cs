﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using System.IO;
using System.Web;
using ACD.Interface.V1;
using System.Net.Mail;
using System.Collections;
using com.cellit.MailResultService;
using OpenPop;



namespace com.cellit.MailProvider.V1
{
    delegate void AsyncMailCheck(int sleep,string mailTo);//delegate variable für Async Check der zustellung

    [Provider(DisplayName = "Com.cellit Mask Send Mail Provider", Description = "EMail Versand Provider", Tags = "ttCall4.Mask.Extention", Category = "Com.cellit Mask")]
    public class MailProvider : IProvider
    {
        //Variablen
        private static string _server;
        private static int _port;
        private static int _pop3port;
        private static string _username;
        private static string _passwort;
        private static bool _ssl;
        private static string _display;
        private static string _bcc;
        private static string _subject;
        private static string _body;
        public static int _KDatumField;
        public static int _KUhrzeitField;
        public static int _KResultField;
        private static int _KIPField;
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

        [ConfigSetting(Frame = "Kontoeinstellung", Label = "Server Port", FieldType = FieldType.NumberField)]
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

            SmtpClient test = new SmtpClient(settings["server"].ToString(), Convert.ToInt32(settings["port"].ToString()));
            test.Credentials = new System.Net.NetworkCredential(settings["user"].ToString(), settings["passwort"].ToString());
            if (Convert.ToBoolean(settings["ssl"].ToString()) == true)
            {
                test.EnableSsl = true;
            }
            else
            {
                test.EnableSsl = false;
            }
            test.Send(new MailMessage(settings["user"].ToString(), settings["user"].ToString(), "test", "Test"));
        }


        #endregion

        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Feld Einstellung", Label = "Feld für den E-Mail versand", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
        public int send;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Feld Einstellung", Label = "TrasaktionsID Speichern auf Feld", FieldType = FieldType.ComboBox, Values = "GetDataFields", AllowBlank = false)]
        public int transaktion;

        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Feld Einstellung", Label = "Kunden Email", FieldType = FieldType.ComboBox, Values = "GetDataFields", AllowBlank = false)]
        public int mailfield;



        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Provider Feld Einstellung", Label = "Kunden IP", FieldType = FieldType.ComboBox, Values = "GetDataFields", AllowBlank = false)]
        public int KundeIPField
        {
            get { return _KIPField; }
            set { _KIPField = value - 200; }
        }

        private IProvider _anliegen;
        [ScriptVisible(SerializeType = SerializeTypes.Value)]
        [RuntimeSetting(Frame = "Anliegen der Email", Label = "Anliegen der e-Mail", Filter = "EMail.Anliegen", AllowBlank = false)]
        public ISubProvider anliegen
        {
            get { return _anliegen as ISubProvider; }
            set { _anliegen = value; }
        }

        [RuntimeSetting(Frame = "Mail Versand Einstellungen", Label = "Display Name", FieldType = FieldType.TextField)]
        public string display
        {
            get { return _display; }
            set { _display = value; }
        }

        [RuntimeSetting(Frame = "Mail Versand Einstellungen", Label = "Bcc an", FieldType = FieldType.TextField)]
        public string bcc
        {
            get { return _bcc; }
            set { _bcc = value; }
        }

        [RuntimeSetting(Frame = "Mail Versand Einstellungen", Label = "Betreff", FieldType = FieldType.TextField, AllowBlank = false)]
        public string subject
        {
            get { return _subject; }
            set { _subject = value; }

        }

        [RuntimeSetting(Frame = "Mail Versand Einstellungen", Label = "E-Mail Body html", FieldType = FieldType.TextArea, Height = 500, Width = 500, AllowBlank = false)]
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
            //Beim hinzufühgen des Provider in die Kampange wird die ProjektID ermittelt
            currentcampaign = (ICampaign)this.GetParentProvider().GetParentProvider();
            if (currentcampaign.GetProviderDatas().IsInitialized == true)
            {
                ttCallProjektID = GetprojektID(currentcampaign.ID);
            }
            else
            {
                currentcampaign.GetProviderEvents().Initialized += campagnInitialized;
            }
            //Prüfung ob die Benötigte Tabelle in der datenbank vorhanden ist
            string sql = "select COUNT(*) as count from INFORMATION_SCHEMA.TABLES where TABLE_NAME='Provider_MailTransaktion'";
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);
            int exists = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);

            //Wenn nicht vorhanden angelegen
            if (exists == 1)
            {
                //Do Nothing
            }
            else
            {

                byte[] file = this.GetRessource("cmd.txt");
                MemoryStream stream = new MemoryStream(file);
                StreamReader sr = new StreamReader(stream);
                string data = sr.ReadLine();
                string insert = "";

                while (data != null)
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
            //MailInbound Event registrieren
            com.cellit.MailResultService.V1.MailResultService.MailInbound += MailResultService_MailInbound;

        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }

        #endregion

        #region Provider Code

        [ScriptVisible]
        public event EventHandler ReceiveMailMessage;//Event für Java bereitstellen

        [ScriptVisible]
        public event EventHandler RecieveMailStatus;

        //Fals die Campagn noch nicht initialisiert war wir darauf gewartet
        private void campagnInitialized(object sender, EventArgs e)
        {
            currentcampaign.GetProviderEvents().Initialized -= campagnInitialized;
            ttCallProjektID = GetprojektID(currentcampaign.ID);
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

        //Nur DatenFelder abrufen
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

        //Mail Versand
        [ScriptVisible]
        public void SendMail(string mailTo, string newBody) //E-Mail Versenden Asyncron
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_username, _display); //Absender
                mail.To.Add(mailTo); //empfänger
                if (_bcc == "")
                {
                    //Do Nothing
                }
                else
                {
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
                client.SendAsync(mail, null);

                AsyncMailCheck check = GetMailStatus;
                IAsyncResult asyncRes = check.BeginInvoke(10000, mailTo, null, null);

            }
            catch (Exception ex)
            {
                this.Log(LogType.Error, Convert.ToString("SendMail :") + ex);
            }

        }

        //Mail Delivery Status?
        public void GetMailStatus(int sleep, string mailTo)
        {
            int nodelivery = 0;
            System.Threading.Thread.Sleep(sleep);
            OpenPop.Pop3.Pop3Client popclient = new OpenPop.Pop3.Pop3Client();
            if (_ssl == true)
            {
                _pop3port = 995;
            }
            else
            {
                _pop3port = 110;
            }
            popclient.Connect(_server, _pop3port, true);
            popclient.Authenticate(_username, _passwort);
            int mailCount = 0;
            int messageCount = popclient.GetMessageCount();
            if (messageCount < 10)
            {
                mailCount = 0;
            }
            else
            {
                mailCount = messageCount - 5;
            }
            for (int i = messageCount; i > mailCount; i--)
            {
                OpenPop.Mime.Message message = popclient.GetMessage(i);
                try
                {
                    OpenPop.Mime.MessagePart messagePart = message.MessagePart.MessageParts[1];
                    var body = messagePart.BodyEncoding.GetString(message.RawMessage);
                    var status = messagePart.ContentType.MediaType.ToString();
                    if (body.Contains(mailTo) && status == "message/delivery-status")
                    {
                        nodelivery++;
                        MailStatus(mailTo, false);
                        popclient.DeleteMessage(i);
                    }

                }
                catch (Exception e)
                {
                    this.Log(LogType.Info, e);
                }
            }
            if (nodelivery == 0)
            {
                MailStatus(mailTo, true);
                this.Log(LogType.Info, Convert.ToString("MailProvider Email erfolgreich versand ") + mailTo);
            }
            popclient.Disconnect();
        }//Asyncroner check ob die Mai angekommen ist asyncron damit die Maske nicht hängt

        //Vollmacht Transaktion in Sql speichern
        [ScriptVisible]
        public void SetTransaktion(string kundenId, string kundenmail, string hex, string body, int vtgTransRef)
        {

            string sql = "Insert Into Provider_MailTransaktion (ProjektID,transaktionID,KundenID,VersandDatum,VersandText,VersandUhrzeit,EmpfaengerAdresse,RequestEnd,vtg_TransRef,vtg_BDatumRef,vtg_BUhrzeitRef,vtg_ErgebnisRef,vtg_IPRef,Anliegen) values('" + ttCallProjektID + "','" + hex + "'," + kundenId + ",cast(GETDATE() as DATE),'" + body + "',getdate(),'" + kundenmail + "','false',+" + vtgTransRef + "," + _KDatumField + "," + _KUhrzeitField + "," + _KResultField + "," + _KIPField + ",'Vollmacht');";
            try
            {
                this.GetDefaultDatabaseConnection().Execute(sql);
            }
            catch (Exception error)
            {
                this.Log(LogType.Error, error);
            }

        }

        //Bank Transaktion Speichern
        [ScriptVisible]
        public void SetBankTransaktion(string kundenId, string kundenmail, string hex, string body, int vtgTransRef, int vtgKontoRef, int vtgBlzRef, int vtgIbanRef, int vtgBicRef, string bankArt)
        {

            string sql = "Insert Into Provider_MailTransaktion (ProjektID,transaktionID,KundenID,VersandDatum,VersandText,VersandUhrzeit,EmpfaengerAdresse,RequestEnd,vtg_TransRef,vtg_IPRef,vtg_KontoRef,vtg_BlzRef,vtg_IbanRef,vtg_BicRef,Anliegen,BankArt) values('" + ttCallProjektID + "','" + hex + "'," + kundenId + ",cast(GETDATE() as DATE),'" + body + "',getdate(),'" + kundenmail + "','false',+" + vtgTransRef + "," + _KIPField + "," + vtgKontoRef + "," + vtgBlzRef + "," + vtgIbanRef + "," + vtgBicRef + ",'Bankdaten','" + bankArt + "');";
            try
            {
                this.GetDefaultDatabaseConnection().Execute(sql);
            }
            catch (Exception error)
            {
                this.Log(LogType.Error, error);
            }

        }

        //E-Mail prüfen
        [ScriptVisible]
        public bool EmailIsValid(string email)
        {
            try
            {
                MailMessage validate = new MailMessage();
                validate.From = new MailAddress(email);
                return true;
            }
            catch (Exception e)
            {
                this.Log(LogType.Info, e);
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

        //Nachrichten Variablen ersetzen
        [ScriptVisible]
        public string ReplaceBody(string anrede, string vorname, string nachname, string transaktion, string bankdatenart, string auftraggeber)
        {
            string message = _body;
            message = message.Replace("[Anrede]", anrede);
            message = message.Replace("[Name]", nachname);
            message = message.Replace("[Vorname]", vorname);
            message = message.Replace("[TransID]", transaktion);
            message = message.Replace("[BankArt]", bankdatenart);
            message = message.Replace("[Produktgeber]", auftraggeber);
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

        //Eingehende Mail Event Starten Werte übergeben
        void MailResultService_MailInbound(object sender, MailResultService.V1.MailEvent e)
        {
            EventHandler reciveMail = this.ReceiveMailMessage;
            if (reciveMail != null)
            {
                reciveMail(this, new ParamArrayEventArgs(e.transaktionID,
                                                        e.resultDate,
                                                        e.resultTime,
                                                        e.auftrag,
                                                        e.resultIP,
                                                        e.resultErg,
                                                        e.resultKonto,
                                                        e.resultBlz,
                                                        e.resultIban,
                                                        e.resultBic));
            }


        }

        //Mail Status Event starten
        void MailStatus(string mailTo, bool Versand)
        {

            EventHandler status = this.RecieveMailStatus;
            if (status != null)
            {
                status(this, new ParamArrayEventArgs(mailTo, Versand));
            }
        }



        #endregion



        #region SubProvider

        //Anliegen SubProvider Vollmacht
        [SubProvider(DisplayName = "Vollmacht Einholen", Tags = "EMail.Anliegen")]
        public class GetVollmacht : ISubProvider
        {

            #region Runtime-Settings
            // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            public bool isVollmacht;

            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Feld für Auftraggeber", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int produktgeber;

            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Kundenreaktions Datum", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int KundeDatumField
            {
                get { return MailProvider._KDatumField; }
                set { MailProvider._KDatumField = value - 200; }
            }
            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Kundenreaktions Uhrzeit", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int KundeUhrzeitField
            {
                get { return MailProvider._KUhrzeitField; }
                set { MailProvider._KUhrzeitField = value - 200; }
            }
            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Kunden Antwort", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int KundeResultField
            {
                get { return MailProvider._KResultField; }
                set { MailProvider._KResultField = value - 200; }
            }

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
                isVollmacht = true;
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
        //Anliegen SubProvider Bankeinzug
        [SubProvider(DisplayName = "Bankeinzug", Tags = "EMail.Anliegen")]
        public class GetBankdata : ISubProvider
        {
            #region Runtime-Settings
            // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
            private IProvider _bank;
            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Welche Art Bankdaten", Filter = "Bank.Anliegen", AllowBlank = false)]
            public ISubProvider bank
            {
                get { return _bank as ISubProvider; }
                set { _bank = value; }

            }



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

            #endregion

        }
        //Bankdaten SubProvider
        [SubProvider(DisplayName = "Kontonummer und BLZ", Tags = "Bank.Anliegen")]
        public class GetBankArt : ISubProvider
        {
            #region Runtime-Settings
            // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  


            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Feld für Kontonummer", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int konto;

            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Feld für BLZ", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int blz;



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

            #endregion

        }
        //Bankdaten SubProvider
        [SubProvider(DisplayName = "IBAN und BIC", Tags = "Bank.Anliegen")]
        public class GetBankArt2 : ISubProvider
        {
            #region Runtime-Settings
            // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  


            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Feld für IBAN", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int iban;

            [ScriptVisible(SerializeType = SerializeTypes.Value)]
            [RuntimeSetting(Frame = "Anliegen der Email", Label = "Feld für BIC", FieldType = FieldType.ComboBox, Values = "GetFields", AllowBlank = false)]
            public int bic;



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

            #endregion

        }

        #endregion
    }
    
}
