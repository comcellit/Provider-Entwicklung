//define namespace
Ext.ns('com.cellit.Mask.Extention.V1');

com.cellit.Mask.Extention.V1.SampleProvider = function (remote) {
    //Variabeln
    var container = null;

    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('click', DataField_Click, this);
    ttCall4.Hook.ResultFields.on('click', ResultField_Click, this);

    //Event: Bearbeitungsmaske geöffnet
    function Mask_Open() {
        //Remote-Events registrieren:
        //remote.ReceiveMessage.addEventHandler(ReceiveMessage, this);
    }

    //Event: Beitungsmaske geschlossen
    function Mask_Close() {
        //Remote-Events deregistrieren:
        //remote.ReceiveMessage.removeEventHandler(ReceiveMessage);
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
            //Info anzeigen
            //case remote.infoField:
            //    GetUserInfo(ttCall4.Hook.agentID.getValue());
            //    break;
            ////Nachricht senden
            //case remote.sendField:
            //    var simpleForm = new ttCall4.Mask.Extention.Sample.Provider.V1.SimpleForm({
            //        to: ttCall4.Hook.CustomerFields[ttCall4.Hook.ttCustomerField.ttEmail].value.getValue(),
            //        subject: 'Wichtige Informationen',
            //        msg: ttCall4.Hook.ResultFields[ttCall4.Hook.ttResultField.ttRemark].value.getValue(),
            //        handler: SendMessage,
            //        scope: this
            //    });
            //    simpleForm.show();
            //    break;
            //Maske anzeigen
            //case remote.showField:
            //    //Callback-function setzen
            //    ttCall4.Hook.setCallbackFunction(maskCallback);

            //    //Url festlegen (Beispiel-HTML-Sample aus Ressource)
            //    var url = ttCall4.Hook.getRootPath() + remote.SampleRessourcePath + 'sample.html';

            //    //Parameter hinzufügen
            //    url += '?id=' + encodeURIComponent(ttCall4.Hook.CustomerFields.id.getValue());
            //    url += '&gender=' + encodeURIComponent(ttCall4.Hook.CustomerFields[ttCall4.Hook.ttCustomerField.ttSalutation].value.getValue());
            //    url += '&firstName=' + encodeURIComponent(ttCall4.Hook.CustomerFields[ttCall4.Hook.ttCustomerField.ttFirstName].value.getValue());
            //    url += '&lastName=' + encodeURIComponent(ttCall4.Hook.CustomerFields[ttCall4.Hook.ttCustomerField.ttLastName].value.getValue());

            //    //Callback-Url abrufen und übergeben
            //    url += '&callback=' + encodeURIComponent(ttCall4.Hook.getCallbackUrl());

            //    //Maske anzeigen                
            //    container = new ttCall4.Mask.Extention.Sample.Provider.V1.SimpleContainer({
            //        url: url
            //    });
            //    container.show();
            //    break;
            ////Sound abspielen
            case remote.playField:
                //Soundfile abrufen
                debugger;
                var url = remote.GetSoundFile(remote.GetVoicepfad(ttCall4.Hook.CustomerFields.id.getValue()));
                

                //Player anzeigen                
                container = new com.cellit.Mask.Extention.V1.SimplePlayer({
                    url: url
                });
                container.show();
                break;
            default:
                break;
        }
    }

    //Beispiel Datenbankabruf
    function GetUserInfo(id) {
        var rs = remote.GetUserInfo(id);
        var result = "";
        for (var i = 0; i < rs.columns.length; i++) {
            result += rs.columns[i].name + ' : ' + rs.rows[0].columns[i] + '<br>';
        }
        Ext.MessageBox.alert('Info', result);
    }

    //Callback-Funktion für Nachricht senden
    function SendMessage(to, subject, msg) {
        try {
            //C#-Funktion aufrufen
            remote.SendMessage(to, subject, msg);
            //Bestätigung ausgeben
            new Ext.ux.SystrayMessage({
                iconCls: 'x-icon-information',
                title: 'Nachricht senden',
                html: 'Nachricht gesendet.',
                autoDestroy: true,
                hideDelay: 2500
            }).show(document);
            return true;
        } catch (e) {
            //Fehlermeldung ausgeben
            Ext.Msg.show({
                title: 'Nachricht senden',
                msg: e,
                buttons: Ext.MessageBox.OK,
                icon: Ext.MessageBox.ERROR
            })
            return false;
        }
    }

    //Callback-Function für externe Maske:
    function maskCallback(params) {
        //Werte zurückschreiben
        ttCall4.Hook.CustomerFields[ttCall4.Hook.ttCustomerField.ttSalutation].value.setValue(params.gender);
        ttCall4.Hook.CustomerFields[ttCall4.Hook.ttCustomerField.ttFirstName].value.setValue(params.firstName);
        ttCall4.Hook.CustomerFields[ttCall4.Hook.ttCustomerField.ttLastName].value.setValue(params.lastName);

        //Maske schließen
        container.hide();
    }

    //Remote-Event: Nachricht erhalten
    function ReceiveMessage(sender, args) {
        //Empfänger prüfen
        if (args.Params[0] == ttCall4.Hook.agentID.getValue()) {
            Ext.MessageBox.alert('Eingehende Nachricht', 'Von: ' + args.Params[1] + '<br>Betreff: ' + args.Params[2] + '<br><br>' + args.Params[3]);
        }
    }

    return this;
}