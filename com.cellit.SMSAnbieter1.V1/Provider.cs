using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;

namespace com.cellit.SMSAnbieter1.V1
{
    [Provider(DisplayName = "SMS Anbieter Als NormalProvider ", Description = "Common Provider", Tags = "SMS.Anbieter2", Category = "")]
    public class SMSGateWayExt : IProvider
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

        //[ConfigSetting(Frame = "Datenbank", Label = "Verbindung")]
        //public IDatabaseConnection ConfigConnection = null;

        [ConfigSetting(Frame = "Einstellungen", Label = "SMS Server")]
        public string smsServer;


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

        public System.Data.DataTable GetData()
        {
            string sql = "SELECT *  From Dat_000121_Aufrufe (nolock)";
            System.Data.DataSet ds = this.GetDefaultDatabaseConnection().Select(sql);

            return ds.Tables[0];

        }

    }
    
}
