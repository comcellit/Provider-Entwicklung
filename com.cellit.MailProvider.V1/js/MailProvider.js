//define namespace
Ext.ns('com.cellit.MailProvider.V1.');

com.cellit.MailProvider.V1.MailProvider = function (remote) {
   
    var getTransField;
    var myVtgKonto;
    var myVtgBlz;
    var myVtgIban;
    var myVtgBic;
    var bankArt;
    var mailTo = null;

    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('click', DataField_Click, this);
    ttCall4.Hook.ResultFields.on('click', ResultField_Click, this);

    //Event: Bearbeitungsmaske geöffnet
    function Mask_Open() {
        remote.RecieveMailStatus.addEventHandler(MailStatus, this);
        remote.ReceiveMailMessage.addEventHandler(ReciveMailMessage, this);
    }

    //Event: Beitungsmaske geschlossen
    function Mask_Close() {
        //Remote-Events deregistrieren:
        remote.RecieveMailStatus.removeEventHandler(MailStatus);
        remote.ReceiveMailMessage.removeEventHandler(ReciveMailMessage);
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

            //Email Versand
            case remote.send:

                mailTo = ttCall4.Hook.DataFields[remote.mailfield - 200].value.getValue();
                var valide = remote.EmailIsValid(mailTo)
                if (valide == true) {

                    if (remote.anliegen.isVollmacht == true) {
                        SendVollmacht(mailTo);
                    }
                    else {
                        SendGetBank(mailTo);
                    }
                }
                else
                {
                    Ext.MessageBox.alert('Fehler', 'Die E-Mail Adresse ist nicht Valide');
                }

                break;

            default:
                break;
        }
                
    }
    //Event Starten Daten Speichern
    function ReciveMailMessage(sender, args)
    {
        var indexof = ttCall4.Hook.DataFields[remote.transaktion - 200].value.getValue();
        if (indexof.indexOf(args.Params[0]) >= 0 )
        {
            Ext.MessageBox.alert('Info', "Eingehende Mail Antwort für transaktion: " + args.Params[0]);
            ttCall4.Hook.DataFields[remote.KundeIPField].value.setValue(args.Params[4]);
            
            if (args.Params[3] == "Vollmacht")
            {
                ttCall4.Hook.DataFields[remote.anliegen.KundeDatumField].value.setValue(args.Params[1]);
                ttCall4.Hook.DataFields[remote.anliegen.KundeUhrzeitField].value.setValue(args.Params[2]);
                ttCall4.Hook.DataFields[remote.anliegen.KundeResultField].value.setValue(args.Params[5]);
            }
            if (args.Params[3] == "Bankdaten")
            {
                if (typeof (remote.anliegen.bank.konto) == "undefined") {
                    ttCall4.Hook.DataFields[remote.anliegen.bank.iban - 200].value.setValue(args.Params[8]);
                    ttCall4.Hook.DataFields[remote.anliegen.bank.bic - 200].value.setValue(args.Params[9]);
                } else {
                    ttCall4.Hook.DataFields[remote.anliegen.bank.konto - 200].value.setValue(args.Params[6]);
                    ttCall4.Hook.DataFields[remote.anliegen.bank.blz - 200].value.setValue(args.Params[7]);
                }
            }
        }
    }

    //Delivery Event Empfangen
    function MailStatus(sender,args1)
    {
        var mymail = ttCall4.Hook.DataFields[remote.mailfield - 200].value.getValue();
        //alert(args1.Params[0]+ ttCall4.Hook.DataFields[remote.mailfield - 200].value.getValue());
        if (args1.Params[0]== mymail && args1.Params[1]==false)
        {
            Ext.MessageBox.alert('Achtung', "E-Mail unzustellbar die E-mail Adresse "+mymail+" ist nicht vergeben");
        }
        if (args1.Params[0] == mymail && args1.Params[1] == true)
        {
            Ext.MessageBox.alert('Info', "E-Mail erfolgreich zugestellt an "+mymail);
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

    function SendVollmacht(mailTo)
    {
        var myVtgRef = remote.transaktion - 200;
        
        if (mailTo == null) {
            Ext.MessageBox.alert('Fehler', 'Das E-Mail feld darf nicht leer sein.');
        } //Prüft ob Empfänger leer ist
        else {

            getProgress();//Anzeige Mail Versand Status
            var mytransaktion = remote.GetTrasaktionID();//Speicher TransaktionId
            var newBody = remote.ReplaceBody(ttCall4.Hook.CustomerFields[1].value.getValue(),
                          ttCall4.Hook.CustomerFields[4].value.getValue(),
                          ttCall4.Hook.CustomerFields[2].value.getValue(),
                          mytransaktion,
                          "Keine",
                          ttCall4.Hook.DataFields[remote.anliegen.produktgeber - 200].value.getValue());//Ersetze Body Variablen

            remote.SendMail(mailTo, newBody); //Mail Versand 

            getTransField = ttCall4.Hook.DataFields[remote.transaktion - 200].value.getValue();//Frühere transaktionen speichern falls vorhanden

            if (getTransField == null) {
                ttCall4.Hook.DataFields[remote.transaktion - 200].value.setValue(mytransaktion);
            }
            else {
                ttCall4.Hook.DataFields[remote.transaktion - 200].value.setValue(getTransField + ', ' + mytransaktion);
            }

            remote.SetTransaktion(ttFramework.providers.ttCall4.CallJob.Customer.CustomerID, mailTo, mytransaktion, newBody, myVtgRef);
        }

    }

    function SendGetBank(mailTo)
    {
        var myVtgRef = remote.transaktion - 200;
        if (typeof (remote.anliegen.bank.konto) == "undefined") {
            myVtgKonto = 0;
            bankArt = "SEPA";
        }
        else {
            myVtgKonto = remote.anliegen.bank.konto - 200;
        }
        if (typeof (remote.anliegen.bank.blz) == "undefined") {
            myVtgBlz = 0;
        }
        else {
            myVtgBlz = remote.anliegen.bank.blz - 200;
        }
        if (typeof (remote.anliegen.bank.iban) == "undefined") {
            myVtgIban = 0;
            bankArt = "Konto";
        }
        else {
            myVtgIban = remote.anliegen.bank.iban - 200;
        }
        if (typeof (remote.anliegen.bank.bic) == "undefined") {
            myVtgBic = 0;
        }
        else {
            myVtgBic = remote.anliegen.bank.bic - 200;
        }
        
        if (mailTo == null) {
            Ext.MessageBox.alert('Fehler', 'Das E-Mail feld darf nicht leer sein.');
        } //Prüft ob Empfänger leer ist
        else {

            getProgress();//Anzeige Mail Versand Status
            var mytransaktion = remote.GetTrasaktionID();//Speicher TransaktionId
            var newBody = remote.ReplaceBody(ttCall4.Hook.CustomerFields[1].value.getValue(),
                          ttCall4.Hook.CustomerFields[4].value.getValue(),
                          ttCall4.Hook.CustomerFields[2].value.getValue(),
                          mytransaktion,
                          bankArt,
                          "Keine");//Ersetze Body Variablen
            remote.SendMail(mailTo, newBody); //Mail Versand 


            getTransField = ttCall4.Hook.DataFields[remote.transaktion - 200].value.getValue();
            if (getTransField == null) {
                ttCall4.Hook.DataFields[remote.transaktion - 200].value.setValue(mytransaktion);
            }
            else {
                ttCall4.Hook.DataFields[remote.transaktion - 200].value.setValue(getTransField + ',' + mytransaktion);
            }
            remote.SetBankTransaktion(ttFramework.providers.ttCall4.CallJob.Customer.CustomerID, mailTo, mytransaktion, newBody, myVtgRef, myVtgKonto, myVtgBlz, myVtgIban, myVtgBic, bankArt);
        }

    }

    return this;
}