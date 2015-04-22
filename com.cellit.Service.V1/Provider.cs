using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using System.Data;

namespace com.cellit.Service.V1
{
    [Provider(DisplayName = "ttFramework Service1-Provider", Description = "Common Provider", Tags = "", Category = "Com.cellit")]
    public class TestService : IService
    {
        private int _looptime;
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
        [ConfigSetting(Frame = "Einstellung", Label = "Looptime in sec", FieldType = FieldType.SliderField , MinValue="1",MaxValue="3600",Default="30")]
        public int looptime
        {
            get { return _looptime / 1000; }
            set { _looptime = value * 1000; }

        }
        private smsGateway _currentsmsgateway;
        [ConfigSetting(Frame = "Einstellung", Label = "SMs Anbieter",Filter="SMS.Anbieter")]
        public ISubProvider CurrentSmsgateway
        {
            get { return _currentsmsgateway as ISubProvider; }
            set { _currentsmsgateway = value as smsGateway; }
        }

        private IProvider  _currentsmsgatewayExt;
        [ConfigSetting(Frame = "Einstellung", Label = "SMs AnbieterExt", Filter = "SMS.Anbieter2")]
        public IProvider CurrentSmsgatewayExt
        {
            get { return _currentsmsgatewayExt;}
            set { _currentsmsgatewayExt=value; }
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
                Name="myService"

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
            while(!isStoping)
            {
                //Programm toDo
                DataTable test = _currentsmsgateway.GetData();
                DataTable test2 = (DataTable)_currentsmsgatewayExt.CallbyName("GetData",null);
                object server=new object();
                string serverText =     (_currentsmsgatewayExt.TryGetValue("smsServer", server)).ToString();
                System.Threading.Thread.Sleep(_looptime);
            }
        }

        [SubProvider(DisplayName="SMS Anbieter eins",Tags="SMS.Anbieter")]
        public class smsGateway : ISubProvider
        {
            #region Runtime-Settings
            // Werte, die bei der Verwendung Auswahl) des Providers für die jeweilige Instanz gesetzt werden können  

            [RuntimeSetting(Frame = "Einstellungen", Label = "SMS Server")]
            public string smsServer;

            [RuntimeSetting(Frame = "Einstellungen", Label = "SMS Port")]
            public string smsPort;

          


            #endregion

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

            public DataTable GetData()
            {
                string sql = "SELECT *  From Dat_000121_Aufrufe (nolock)";
                System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);

                return ds.Tables[0];

            }
        }
        
    }
}
