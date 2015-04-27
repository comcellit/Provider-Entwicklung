//define namespace
Ext.ns('com.cellit.MailProvider.V1.');

com.cellit.MailProvider.V1.MailProvider = function (remote) {
   
   
    

    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('click', DataField_Click, this);
    ttCall4.Hook.ResultFields.on('click', ResultField_Click, this);

    //Event: Bearbeitungsmaske geöffnet
    function Mask_Open() {
        //Remote-Events registrieren:
    }

    //Event: Beitungsmaske geschlossen
    function Mask_Close() {
        //Remote-Events deregistrieren:
    }

    //Event: Vorgangs-Feld geklickt
    function DataField_Click(index) {
        // Weiterleitung an globale Prüfung
        ttCallField_Click(index + 200);
    }

    //Event: Ergebnis-Feld geklickt
    function ResultField_Click(index) {
        // Weiterleitung an globale Prüfung
        ttCallField_Click(index);
    }

    //Globale Prüfung bei Field-Click
    function ttCallField_Click(index) {
        switch (index) {

            ////Sound abspielen
            case remote.send:
                if (remote.mailfield > 200)
                {
                    var mailTo = ttCall4.Hook.DataFields[remote.mailfield - 200].value.getValue();
                    remote.SendMail(mailTo);
                }



                break;
            default:
                break;
        }
                
    }
    return this;
}