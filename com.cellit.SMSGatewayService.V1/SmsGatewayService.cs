﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using com.esendex.sdk.messaging;
using com.esendex.sdk;
using com.esendex.sdk.inbox;
using System.Xml.Serialization;
using System.Web;
using System.IO;

namespace com.cellit.SMSGatewayService.V1
{
    [Provider(DisplayName = "Com.cellit SMS Gateway", Description = "SMS Dienste Bereitstellen für die SMS Send Provider", Tags = "SMS.Gateway", Category = "Com.cellit Service", SingletonConfiguration = true, ConfigurationKey = "SMSGatewayService")]
    public class SmsGatewayService : IService
    {
        private int _looptime;
        private string _user;
        private string _pass;
        private string _accountRef;

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

        [ConfigSetting(Frame = "Service Einstellung", Label = "Looptime in sec", FieldType = FieldType.SliderField, MinValue = "10", MaxValue = "3600", Default = "30")]
        public int looptime
        {
            get { return _looptime / 1000; }
            set { _looptime = value * 1000; }

        }


        [ConfigSetting(Frame = "SMS Dienst Login", Label = "Username", FieldType = FieldType.TextField)]
        public string user
        {
            get { return _user; }
            set { _user = value; }
        }

        [ConfigSetting(Frame = "SMS Dienst Login", Label = "Passwort", FieldType = FieldType.PasswordField)]
        public string pass
        {
            get { return _pass; }
            set { _pass = value; }
        }

        [ConfigSetting(Frame = "SMS Dienst Login", Label = "Account Reference", FieldType = FieldType.TextField)]
        public string accountRef
        {
            get { return _accountRef; }
            set { _accountRef = value; }
        }

        private ServiceState _serviceState = ServiceState.StateUnknown;
        private System.Threading.Thread _servicethread;
        private bool isStoping;



        #endregion

        #region Runtime-Settings
        // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  
       

        #endregion

        // --------------------------------------- Provider-Code --------------------------------------------

        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        public void Initialize(object args)
        {
            string sql = "select COUNT(*) as count from INFORMATION_SCHEMA.TABLES where TABLE_NAME='Provider_SmSTransfer'";
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);
            int exists = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);

            if (exists == 1)
            {
                //do Nothing
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
              
        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }
       
        //Service Benötigte Events und Methoden
        public event EventHandler ServiceStarted;

        public static event EventHandler smsInbound;

        public ServiceState ServiceState
        {
            get { return _serviceState; }
        }

        public event EventHandler ServiceStopped;

        public void Start(Dictionary<string, object> args)
        {
            isStoping = false;
            _serviceState = ServiceState.StateStarted;
            _servicethread = new System.Threading.Thread(ServiceLoop)
            {
                Name = "SmSGatewayService"

            };
            _servicethread.Start();
            EventHandler serviceStarted = ServiceStarted;
            if (serviceStarted != null)
            {
                serviceStarted(this, null);
            }

        }

        public void Stopp()
        {
            isStoping = true;
            _serviceState = ServiceState.StateStopped;
            EventHandler serviceStopped = ServiceStopped;
            if (serviceStopped != null)
            {
                serviceStopped(this, null);
            }
        }

        public void ServiceLoop()
        {
            while (!isStoping)
            {
                //Programm toDo
                GetInboundMessage();
                System.Threading.Thread.Sleep(_looptime);
            }
        }

        //SMS Versenden durch aufruf aus dem  Send SMS Providern
        public string SendSMS(string phone, string text, string from,bool onlysend,string progId, int transField, int kundeRequest,int kundeRequestDate)
        {
            string batchid = null;
            // login Daten leerzeichen entfernen
            _accountRef =  System.Text.RegularExpressions.Regex.Replace(_accountRef, @"\s+", string.Empty);
            _pass = System.Text.RegularExpressions.Regex.Replace(_pass, @"\s+", string.Empty);
            _user = System.Text.RegularExpressions.Regex.Replace(_user, @"\s+", string.Empty);

            var messagingService = new MessagingService(_user, _pass);

            batchid = messagingService.SendMessage(new SmsMessage(phone, text,_accountRef, from)).BatchId.ToString();

            if (onlysend == true)
            {
                return "";
            }
            else
            {
                SetTransaktion(phone, batchid, progId,transField,kundeRequest,kundeRequestDate);
                return batchid;
            }

        }

        //Für die antworten Trasaktion in Datenbank speicher
        public void SetTransaktion(string to, string batch, string projektID, int trasRef, int requestText,int smsRequestDate)
        {
            string insert = "Insert Into Provider_SmSTransfer (BatchID,Gesendetam,phonenumber,ProjektID,TrasRef,ResultRef,ResultDateRef) values('" + batch + "', getdate(),'" + to + "','" + projektID + "'," + trasRef + "," + requestText + "," + smsRequestDate + ");";
            this.GetDefaultDatabaseConnection().Execute(insert);
        }

        //SMS Antworten holen und Speichern an zugehörige BatchID und Event schmeißen
        public void GetInboundMessage()
        {
            string update = null;
            int count = 0;
            //EsendexCredentials loginCredentials = new EsendexCredentials(_user, _pass);
            var inboxService = new InboxService(_user,_pass);
            var inboxMessages = inboxService.GetMessages();
            foreach (var inboxMessage in inboxMessages.Messages)
            {
                if (inboxMessage.ReadAt.ToString() == "01.01.0001 00:00:00")
                {
                    update = "update Provider_SmSTransfer set KundeAntwort='" + inboxMessage.Summary + "', Antwortam='" + TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(inboxMessage.ReceivedAt), TimeZoneInfo.Local) + "' where SmSEnd=0 and Phonenumber='" + "0" + inboxMessage.Originator.PhoneNumber.Substring(2, inboxMessage.Originator.PhoneNumber.Length - 2) + "'";
                    this.GetDefaultDatabaseConnection().Execute(update);
                    inboxService.MarkMessageAsRead(inboxMessage.Id);
                    count++;

                }
                var date = DateTime.Now.AddDays(-14);
                if (inboxMessage.ReceivedAt < date)
                {
                    inboxService.DeleteMessage(inboxMessage.Id);
                }
            }
            if (count>0)
            {
                OnInbound();
            }
        }

        public  void OnInbound()
        {
            EventHandler onInbound = smsInbound;
            if (onInbound != null)
            {
                onInbound(this, EventArgs.Empty);

            }
        }
       
    }
    
   

}
