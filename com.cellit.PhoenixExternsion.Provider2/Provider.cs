﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;
using ACD.Interface.V1;

namespace com.cellit.PhoenixExtersion.Provider2
{
    [Provider(DisplayName = "ttFramework Common-Provider", Description = "Common Provider", Tags = "Phoenix.Extentions.Outbound", Category = "")]
    public class Provider : IProvider
    {

        IProvider parent;
        ICampaign campaign;

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
            parent = this.GetParentProvider().GetParentProvider();
            if (parent.GetProviderDatas().IsInitialized)
            {
                campaign = (ICampaign)parent;
                campaign.CallJobFinished += campaign_CallJobFinished;
                
            }
            else
            {
                parent.GetProviderEvents().Initialized += campaignInitialized;
            }        
        
        }

        void campaign_CallJobFinished(object sender, CallJob args)
        {
           // throw new NotImplementedException();
        }

        private void campaignInitialized(object sender, EventArgs e)
        {
            try
            {
                campaign = (ICampaign)parent;
                campaign.CallJobFinished += campaign_CallJobFinished;
                parent.GetProviderEvents().Initialized -= campaignInitialized;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        // wird aufgerufen, wenn der Provider nicht mehr benötigt wird
        public void Dispose()
        {
        }

    }
}
