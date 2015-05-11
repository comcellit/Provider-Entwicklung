//define namespace
Ext.ns('com.cellit.SendSMSProvider.V1.');

com.cellit.SendSMSProvider.V1.SMSProvider = function (remote) {
   
    

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

                var phonenumber= ttCall4.Hook.DataFields[remote.phone - 200].value.getValue();
                if (phonenumber == null) {
                    Ext.MessageBox.alert('Fehler', 'Die Handynummer darf nicht leer sein.');
                } //Prüft ob Empfänger leer ist
                if (phonenumber.length < 10) {
                    Ext.MessageBox.alert('Fehler', 'Die Handynummer ist zu kurz.');
                }
                else {
                    getProgress();
                    var batchid = remote.SendSmS(phonenumber);
                    batchid 
                }

                break;

            default:
                break;
        }
                
    }
    function getProgress(){
        Ext.MessageBox.show({
            title: 'Please wait',
            msg: 'Send SMS...',
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