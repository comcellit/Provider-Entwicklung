//define namespace
Ext.ns('com.cellit.MailProvider.V1.');

com.cellit.MailProvider.V1.MailProvider = function (remote) {
   
    var getTransField;
    

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
                
                var myVtgRef = remote.transaktion - 200;
                if (myVtgRef < 10)
                {
                    myVtgRef = "0" + myVtgRef;
                   
                }
                    
                    var mailTo = ttCall4.Hook.DataFields[remote.mailfield - 200].value.getValue();
                    if (mailTo == null) {
                        Ext.MessageBox.alert('Fehler', 'Das E-Mail feld darf nicht leer sein.');
                    } //Prüft ob Empfänger leer ist
                    else {

                        getProgress();//Anzeige Mail Versand Status
                        var mytransaktion = remote.GetTrasaktionID();//Speicher TransaktionId
                        var newBody = remote.ReplaceBody(ttCall4.Hook.CustomerFields[1].value.getValue(), ttCall4.Hook.CustomerFields[4].value.getValue(), ttCall4.Hook.CustomerFields[2].value.getValue(), mytransaktion);//Ersetze Body Variablen
                        remote.SendMail(mailTo, newBody); //Mail Versand 


                        getTransField = ttCall4.Hook.DataFields[remote.transaktion - 200].value.getValue();
                        if (getTransField == null) {
                            ttCall4.Hook.DataFields[remote.transaktion - 200].value.setValue(mytransaktion);
                        }
                        else {
                            ttCall4.Hook.DataFields[remote.transaktion - 200].value.setValue(getTransField + ',' + mytransaktion);
                        }

                        remote.SetTransaktion(ttFramework.providers.ttCall4.CallJob.Customer.CustomerID, mailTo, mytransaktion, newBody, myVtgRef);
                    }
                
                break;

            default:
                break;
        }
                
    }
    function getProgress(){
        Ext.MessageBox.show({
            title: 'Please wait',
            msg: 'Send Message...',
            progressText: 'Sending...',
            width: 300,
            progress: true,
            closable: false,
        });

        // this hideous block creates the bogus progress
        var f = function (v) {
            return function () {
                if (v == 12) {
                    Ext.MessageBox.hide();
                } else {
                    var i = v / 11;
                    Ext.MessageBox.updateProgress(i, Math.round(100 * i) + '% completed');
                }
            };
        };
        for (var i = 1; i < 13; i++) {
            setTimeout(f(i), i * 50);
        }
        return this;
    }
    return this;
}