//define namespace
Ext.ns('com.cellit.SendSMSProvider.V1.');

com.cellit.SendSMSProvider.V1.SMSProvider = function (remote) {
   
    var batchid = null;

    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('click', DataField_Click, this);
    ttCall4.Hook.ResultFields.on('click', ResultField_Click, this);

    //Event: Bearbeitungsmaske geöffnet
    function Mask_Open() {
        remote.ReceiveMessage.addEventHandler(ReceiveMessage, this);
        //Remote-Events registrieren:
    }

    //Event: Beitungsmaske geschlossen
    function Mask_Close() {
        remote.ReceiveMessage.removeEventHandler(ReceiveMessage);
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

            //Send SmS
            case remote.send:

                var phonenumber = ttCall4.Hook.DataFields[remote.phone - 200].value.getValue();
                if (phonenumber == null) {
                    Ext.MessageBox.alert('Fehler', 'Die Handynummer darf nicht leer sein.');
                } //Prüft ob Empfänger leer ist
                if (phonenumber.length < 10) {
                    Ext.MessageBox.alert('Fehler', 'Die Handynummer ist zu kurz.');
                }
                else {
                    getProgress();
                    phonenumber = remote.ReplaceString(phonenumber);
                    ttCall4.Hook.DataFields[remote.phone - 200].value.setValue(phonenumber);
                    batchid = remote.SendSmS(phonenumber);
                    if (typeof (remote.anliegen.onlySend) == "undefined") {
                        ttCall4.Hook.DataFields[remote.anliegen.smstransfer - 200].value.setValue(batchid)
                    }
                }

                break;
            case remote.anliegen.smsOlder:

                if (remote.GetAllOpenSmS(ttCall4.Hook.DataFields[remote.anliegen.smstransfer - 200].value.getValue()) == false) {
                    Ext.MessageBox.alert('Fehler', 'Keine SMS erhalten für: ' + '<br>' + ttCall4.Hook.DataFields[remote.anliegen.smstransfer - 200].value.getValue());
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
    //Remote-Event: Nachricht erhalten
    function ReceiveMessage(sender, args) {
        //Empfänger prüfen
        if (args.Params[0] == ttCall4.Hook.DataFields[remote.anliegen.smstransfer - 200].value.getValue()) {
            Ext.MessageBox.alert('Eingehende SMS', 'Von: ' + args.Params[1] + '<br>Datum: ' + args.Params[2] + '<br><br>' + args.Params[3]);
            //Antwort speichern
            ttCall4.Hook.DataFields[remote.anliegen.smsRequestDate - 200].value.setValue(args.Params[2])
            ttCall4.Hook.DataFields[remote.anliegen.smsRequestText - 200].value.setValue(args.Params[3])
            //Transaktion Beendet setzen
            remote.SetSmsEnd(args.Params[0]);
        }
        else {
            //DoNothing
            //Ext.MessageBox.alert('Eingehende Nachricht', 'Von: ' + args.Params[1] + '<br>Datum: ' + args.Params[2] + '<br><br>' + args.Params[3]+args.Params[0]);
        }
        
    }
    
    return this;
}