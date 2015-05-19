// define namespace
Ext.ns('com.cellit.ValidateProvider.V1.');


com.cellit.ValidateProvider.V1.ValidateFunction = function (remote) {

    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('lostFocus', DataField_LostFocus, this);



    function Mask_Open() {
        //Do Nothing
    };

    function Mask_Close() {
        //Do Nothing
    };

    //Event: Vorgangs-Feld Lost Focus
    function DataField_LostFocus(index) {

        ttCallField_LostFocus(index + 200);
    };

    function ttCallField_LostFocus(index) {

        switch (index) {

            //Check Date Script
            case remote.auswahl.dateField:

                var mydate = ttCall4.Hook.DataFields[remote.auswahl.dateField - 200].value.getValue();
                var myresult = remote.auswahl.getCheckResult(remote.auswahl.check, mydate, remote.auswahl.type);
                var mydatefield = ttCall4.Hook.DataFields[remote.auswahl.dateField - 200].getFieldObject();
                var mycheck = remote.auswahl.check;

                if (myresult == true) {

                    mydatefield.setColor(ttCall4.Hook.ttColors.Green);
                    if (remote.auswahl.OKmessage == '') {
                        //Nothing
                    }
                    else {
                        alert(remote.auswahl.OKmessage);
                    }
                }
                else {
                    mydatefield.setColor(ttCall4.Hook.ttColors.Red);
                    if (remote.auswahl.Failmessage == '') {
                        //Nothing

                    }
                    else {
                        alert(remote.auswahl.Failmessage);
                    }
                }
                break;
            //Email Validate
            case remote.auswahl.mailField:

                var mymail = ttCall4.Hook.DataFields[remote.auswahl.mailField - 200].value.getValue();
                var mymailfield = ttCall4.Hook.DataFields[remote.auswahl.mailField - 200].getFieldObject();
                var myresult = remote.auswahl.EmailIsValid(mymail);

                if (myresult == true) {

                    mymailfield.setColor(ttCall4.Hook.ttColors.Green);

                    //Ext.MessageBox.alert('Fehler', 'Das E-Mail feld darf nicht leer sein.');

                }
                else {
                    mymailfield.setColor(ttCall4.Hook.ttColors.Red);

                    Ext.MessageBox.alert('Fehler', 'Diese E-Mail ist nicht Valide.');

                }
                break;
        }

    };
    return this;
}
