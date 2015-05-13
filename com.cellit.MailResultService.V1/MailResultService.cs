using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;


namespace com.cellit.MailResultService.V1
{
    [Provider(DisplayName = "Com.cellit MailResultService", Description = "EMail Vollmacht/Bankverbindung Result zuordnung und rückspielung an den Kunden", Tags = "", Category = "Com.cellit.Provider",SingletonConfiguration=true ,ConfigurationKey="MailResultProv")]
    public class MailResultService : IService
    {
        //Globale Variablen
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

        [ConfigSetting(Frame = "Einstellung", Label = "Looptime in sec", FieldType = FieldType.SliderField, MinValue = "30", MaxValue = "3600", Default = "300")]
        public int looptime
        {
            get { return _looptime / 1000; }
            set { _looptime = value * 1000; }

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
                Name = "MailResultService"

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
                SendResultData();
                System.Threading.Thread.Sleep(_looptime);
            }
        }
        public void SendResultData()
        {
            int datacount = 0;
            string transRef = null;
            string dateRef = null;
            string timeRef = null;
            string ergRef = null;
            string ipRef = null;

            string sql = "SELECT _MailProviderTransaktion.ProjektID,transaktionID,vtg_TransRef,_MailProviderTransaktion.KundenID,left(BestaetigungDatum,10) as BestaetigungDatum,vtg_BDatumRef,BestaetigungUhrzeit,vtg_BUhrzeitRef,Ergebnis,vtg_ErgebnisRef,KundenIP,vtg_IPRef,TransaktionEnd FROM _MailProviderTransaktion left Join Prog_KundenLogin on  _MailProviderTransaktion.KundenID=Prog_KundenLogin.KundenID and Prog_KundenLogin.ProjektID = _MailProviderTransaktion.ProjektID where TransaktionEnd=1 and RequestEnd=0  and Prog_KundenLogin.KundenID is null";
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);
            //Daten Lesen und schreiben
            try
            {
                foreach (System.Data.DataRow dataRow in ds.Tables[0].Rows)
                {
                    //Trasaktion Vtg Feld=
                    if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_TransRef"]) < 10)
                    {
                        transRef = "Vtg_Wert0" + ds.Tables[0].Rows[datacount]["vtg_TransRef"];
                    }
                    else
                    {
                        if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_TransRef"]) > 50)
                        {
                            transRef = "VTG_ExData" + (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_TransRef"]) + 50);
                        }
                        else
                        {
                            transRef = "Vtg_Wert" + ds.Tables[0].Rows[datacount]["vtg_TransRef"];
                        }
                    }
                    //Datum vtg Feld=
                    if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_BDatumRef"]) < 10)
                    {
                        dateRef = "Vtg_Wert0" + ds.Tables[0].Rows[datacount]["vtg_BDatumRef"];
                    }
                    else
                    {
                        if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_BDatumRef"]) > 50)
                        {
                            dateRef = "VTG_ExData" + (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_BDatumRef"]) + 50);
                        }
                        else
                        {
                            dateRef = "Vtg_Wert" + ds.Tables[0].Rows[datacount]["vtg_BDatumRef"];
                        }
                    }
                    //Uhrzeit vtg Feld=
                    if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_BUhrzeitRef"]) < 10)
                    {
                        timeRef = "Vtg_Wert0" + ds.Tables[0].Rows[datacount]["vtg_BUhrzeitRef"];
                    }
                    else
                    {
                        if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_BUhrzeitRef"]) > 50)
                        {
                            timeRef = "VTG_ExData" + (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_BUhrzeitRef"]) + 50);
                        }
                        else
                        {
                            timeRef = "Vtg_Wert" + ds.Tables[0].Rows[datacount]["vtg_BUhrzeitRef"];
                        }
                    }
                    //Ergebnis vtg Feld=
                    if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_ErgebnisRef"]) < 10)
                    {
                        ergRef = "Vtg_Wert0" + ds.Tables[0].Rows[datacount]["vtg_ErgebnisRef"];
                    }
                    else
                    {
                        if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_ErgebnisRef"]) > 50)
                        {
                            ergRef = "VTG_ExData" + (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_ErgebnisRef"]) + 50);
                        }
                        else
                        {
                            ergRef = "Vtg_Wert" + ds.Tables[0].Rows[datacount]["vtg_ErgebnisRef"];
                        }
                    }
                    //IP vtg Feld=
                    if (Convert.ToInt32(ds.Tables[0].Rows[0]["vtg_IPRef"]) < 10)
                    {
                        ipRef = "Vtg_Wert0" + ds.Tables[0].Rows[datacount]["vtg_IPRef"];
                    }
                    else
                    {
                        if (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_IPRef"]) > 50)
                        {
                            ipRef = "VTG_ExData" + (Convert.ToInt32(ds.Tables[0].Rows[datacount]["vtg_IPRef"]) + 50);
                        }
                        else
                        {
                            ipRef = "Vtg_Wert" + ds.Tables[0].Rows[datacount]["vtg_IPRef"];
                        }
                    }
                    string Insert = "Update Dat_000";
                    Insert += ds.Tables[0].Rows[datacount]["ProjektID"] + "_Vorgang";
                    Insert += " Set " + dateRef + "='" + ds.Tables[0].Rows[datacount]["BestaetigungDatum"] + "'";
                    Insert += " ," + timeRef + "='" + ds.Tables[0].Rows[datacount]["BestaetigungUhrzeit"] + "'";
                    Insert += " ," + ergRef + "='" + ds.Tables[0].Rows[datacount]["Ergebnis"] + "'";
                    Insert += " ," + ipRef + "='" + ds.Tables[0].Rows[datacount]["KundenIP"] + "'";
                    Insert += " where Vtg_KundenID=" + ds.Tables[0].Rows[datacount]["KundenID"];
                    Insert += " and " + transRef + " like '%" + ds.Tables[0].Rows[datacount]["transaktionID"] + "%'";

                    string ResultEnd = "Update _MailProviderTransaktion set RequestEnd='true' where transaktionID='" + ds.Tables[0].Rows[datacount]["transaktionID"] + "'";

                    try
                    {
                        this.GetDefaultDatabaseConnection().Execute(Insert);
                        this.GetDefaultDatabaseConnection().Execute(ResultEnd);
                    }
                    catch (Exception e)
                    {
                        //this.Log(LogType.Error, e);

                    }


                    datacount++;

                }

            }
            catch (Exception ex)
            {
                this.Log(LogType.Debug, ex);
               
            }

        }
    }
}
