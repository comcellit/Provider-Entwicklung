//define namespace
Ext.ns('com.cellit.GetIban.Provider.V1.');

com.cellit.GetIban.Provider.V1.GetIbanFunction = function (remote) {
   
    var myaccountNumber;
    var mybankIdent;
    var myibanNumber;
    var mybicIdent;
    var myIbanField;
    var myBicField;

    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('lostFocus', DataField_LostFocus, this);

    //Event: Bearbeitungsmaske geöffnet
    function Mask_Open() {
        //Remote-Events registrieren:
        //alert(remote.anliegen.bank.konto);
        //alert(remote.anliegen.bank.iban);
        //remote.anliegen.isVollmacht
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
    function DataField_LostFocus(index) {
        switch (index+200) {

            //Email Versand
            case remote.accountNumber:

                myIbanField = ttCall4.Hook.DataFields[remote.ibanNumber - 200].getFieldObject();
                myBicField = ttCall4.Hook.DataFields[remote.bicIdent - 200].getFieldObject();
                myaccountNumber = ttCall4.Hook.DataFields[remote.accountNumber - 200].value.getValue();
                mybankIdent = ttCall4.Hook.DataFields[remote.bankIdent - 200].value.getValue();
                if (myaccountNumber == null || mybankIdent == null){
                    //Do Nothing
                }
                else{

                    myibanNumber = remote.GenerateGermanIban(mybankIdent, myaccountNumber);
                    mybicIdent = remote.GetGermanBic(myibanNumber);
                    ttCall4.Hook.DataFields[remote.ibanNumber - 200].value.setValue(myibanNumber);
                    ttCall4.Hook.DataFields[remote.bicIdent - 200].value.setValue(mybicIdent);
                    if (myibanNumber=="ERROR")
                    {
                        myIbanField.setColor(ttCall4.Hook.ttColors.Red);
                        myBicField.setColor(ttCall4.Hook.ttColors.Red);
                    }
                    else
                    {
                        myIbanField.setColor(ttCall4.Hook.ttColors.Green);
                        myBicField.setColor(ttCall4.Hook.ttColors.Green);
                    }
                }
                break;

            case remote.bankIdent:

                myIbanField = ttCall4.Hook.DataFields[remote.ibanNumber - 200].getFieldObject();
                myBicField = ttCall4.Hook.DataFields[remote.bicIdent - 200].getFieldObject();
                myaccountNumber = ttCall4.Hook.DataFields[remote.accountNumber - 200].value.getValue();
                mybankIdent = ttCall4.Hook.DataFields[remote.bankIdent - 200].value.getValue();
                if (myaccountNumber == null || mybankIdent == null) {
                    //Do Nothing
                }
                else {

                    myibanNumber = remote.GenerateGermanIban(mybankIdent, myaccountNumber);
                    mybicIdent = remote.GetGermanBic(myibanNumber);
                    ttCall4.Hook.DataFields[remote.ibanNumber - 200].value.setValue(myibanNumber);
                    ttCall4.Hook.DataFields[remote.bicIdent - 200].value.setValue(mybicIdent);
                    if (myibanNumber == "ERROR") {
                        myIbanField.setColor(ttCall4.Hook.ttColors.Red);
                        myBicField.setColor(ttCall4.Hook.ttColors.Red);
                    }
                    else {
                        myIbanField.setColor(ttCall4.Hook.ttColors.Green);
                        myBicField.setColor(ttCall4.Hook.ttColors.Green);
                    }
                }
                break;
            default:
                break;
        }
                
    }
    return this;
}