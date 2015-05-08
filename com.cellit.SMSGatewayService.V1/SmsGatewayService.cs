using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using com.esendex.sdk.messaging;
using System.Reflection;

namespace com.cellit.SMSGatewayService.V1
{
    [Provider(DisplayName = "Com.cellit SMSGatewayService", Description = "SMS Dienste Bereitstellen für den SMS Provider", Tags = "SMS.Gateway", Category = "Com.cellit.Provider", SingletonConfiguration = true, ConfigurationKey = "SMSGatewayService")]
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

        [ConfigSetting(Frame = "Service Einstellung", Label = "Looptime in sec", FieldType = FieldType.SliderField, MinValue = "30", MaxValue = "3600", Default = "300")]
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

        //[RuntimeSetting(Frame = "Datenbank", Label = "Verbindung")]
        //public IDatabaseConnection ConfigConnection = null;

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

        public event EventHandler ServiceStarted;

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

                System.Threading.Thread.Sleep(_looptime);
            }
        }


        public void SendSMS(string phone, string text, string from)
        {
            var messagingService = new MessagingService(_user, _pass);
            messagingService.SendMessage(new SmsMessage(phone, text, _accountRef, from));
        }

    }
}
