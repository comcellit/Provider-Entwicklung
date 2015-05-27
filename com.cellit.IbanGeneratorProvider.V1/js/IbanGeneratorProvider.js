// define namespace
Ext.ns('com.cellit.IbanBicGeneratorProvider.V1.');

com.cellit.IbanBicGeneratorProvider.V1.IbanBicProvider = function (remote) {


    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('lostFocus', DataField_LostFocus, this);



    function Mask_Open() {
        //Do Nothing
    }

    function Mask_Close() {
        //Do Nothing
    }

    //Event: Vorgangs-Feld geklickt
    function DataField_LostFocus(index) {

        ttCallField_LostFocus(index + 200);
    }



    //Globale Prüfung bei Field-Click
    function ttCallField_LostFocus(index) {


        switch (index) {

            case remote.accountNumber:
                alert("Test");
                var myaccountNumber = ttCall4.Hook.DataFields[remote.accountNumber - 200].value.getValue();
                var mybankIdent = ttCall4.Hook.DataFields[remote.bankIdent - 200].value.getValue();
                var myiban = remote.GenerateGermanIban(mybankIdent, myaccountNumber);
                var mybic = remote.GetGermanBic(myiban);
                ttCall4.Hook.DataFields[remote.ibanNumber - 200].value.setValue(myiban);
                ttCall4.Hook.DataFields[remote.bicIdent - 200].value.setValue(mybic);
                break;

            case remote.bankIdent:
                var myaccountNumber = ttCall4.Hook.DataFields[remote.accountNumber - 200].value.getValue();
                var mybankIdent = ttCall4.Hook.DataFields[remote.bankIdent - 200].value.getValue();
                var myiban = remote.GenerateGermanIban(mybankIdent, myaccountNumber);
                var mybic = remote.GetGermanBic(myiban);
                ttCall4.Hook.DataFields[remote.ibanNumber - 200].value.setValue(myiban);
                ttCall4.Hook.DataFields[remote.bicIdent - 200].value.setValue(mybic);
                break;
        }


    }
    return this;
}
