using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ttFramework.Provider;

namespace com.cellit.WiedervorlageMenue.V1
{
    [Provider(DisplayName = "Com.Cellit Wiedervorlage Menue", Description = "Anzeige Wiedervorlagen Agenten", Tags = "ttCall4.Application.Extention", Category = "Com.Cellit Application.Extention", SingletonConfiguration = true, ConfigurationKey = "Com.CellitWiedervorlageMenue")]
    public class WiedervorlageMenue : IProvider
    {

        #region Script-Integration in ttCall 4

        // Diese Funktionen werden seitens des Parent-Providers aufgerunfen, 
        // um ggf. Script-Ressourcen hinterlegen und/oder ausführen zu können.  

        // zu ladende Script-Dateinen 
        public List<string> LoadScripts
        {
            get
            {
                return new List<string>
				{
					Extension.GetProviderDatas(this).UrlDirectory + "js/WiedervorlageProvider.js?v=2"
				};
            }
        }

        public List<string> RunScripts
        {
            get
            {
                return new List<string>
				{
					"try {com.cellit.WiedervorlageMenue.V1.ProviderInstance = new com.cellit.WiedervorlageMenue.Provider(%this%);} catch(e){}",
					"try {ttFramework.MainMask.addContent('Wiedervorlagen', 'Übersicht', '" + Extension.GetProviderDatas(this).UrlDirectory + "res/WV.png', com.cellit.WiedervorlageMenue.V1.ProviderInstance);} catch(e){}"
				};
            }
        }

        #endregion

        // --------------------------------------- Provider-Code --------------------------------------------

        // wird augerufen, wenn der Provider vollständig geladen und alle Settings gesetzt wurden
        public void Initialize(object args)
        {

        }
        public void Dispose()
        {
        }

        [ScriptVisible]
        public object GetWv()
        {
            string select = string.Empty;
            select += "SELECT ";
            select += "Global_Wiedervorlagen.WV_am, ";
            select += "Global_Wiedervorlagen.WV_MitarbeiterID, ";
            select += "Global_Mitarbeiter.Mi_Vorname AS WV_Vorname, ";
            select += "Global_Mitarbeiter.Mi_Nachname AS WV_Nachname,";
            select += "Global_Mitarbeiter.Mi_Benutzer, ";
            select += "Global_Wiedervorlagen.WV_Kunde,";
            select += "Global_Wiedervorlagen.WV_Bemerkung,";
            select += "Global_Wiedervorlagen.WV_aufgerufen,";
            select += "Global_Wiedervorlagen.WV_KundenID,";
            select += "Global_Wiedervorlagen.WV_ProjektID,";
            select += "Global_Projekte.Projekt_Kurzname as Projekt,";
            select += "Global_Wiedervorlagen.WV_Campaign,";
            select += "Campaigns.Campaign_Name,";
            select += "case when (Global_Wiedervorlagen.WV_am<=GETDATE()) then 'http://agent.cellit-gruppe.de/ttframework/comCellitImg/redSmiley.png'";
            select += "when (Global_Wiedervorlagen.WV_am>= getdate() AND DATEDIFF(hour,Global_Wiedervorlagen.WV_am,GETDATE())>=-2) then 'http://agent.cellit-gruppe.de/ttframework/comCellitImg/yellowSmiley.png'";
            select += "when (Global_Wiedervorlagen.WV_am>=GETDATE()) then 'http://agent.cellit-gruppe.de/ttframework/comCellitImg/greenSmiley.png'";
            select += "end as url";
            select += " FROM  Global_Wiedervorlagen (nolock)";
            select += "    left JOIN Global_Mitarbeiter (nolock) ON Global_Wiedervorlagen.WV_MitarbeiterID = Global_Mitarbeiter.Mi_ID";
            select += "    Inner JOIN Global_Projekte (Nolock) ON Global_Wiedervorlagen.WV_ProjektID = Global_Projekte.Projekt_ID";
            select += "    left JOIN Campaigns (nolock) ON Global_Wiedervorlagen.WV_Campaign = Campaigns.Campaign_Id ";
            select += "WHERE Global_Wiedervorlagen.WV_MitarbeiterID=" + Extension.GetCallingUser(this).ID + " ";
            select += "ORDER BY WV_am ASC;";

            return this.GetDefaultDatabaseConnection().Select(select);
        }

    }
}
