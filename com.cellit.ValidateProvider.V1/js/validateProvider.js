// define namespace
Ext.ns('com.cellit.ValidateProvider.V1.');


com.cellit.ValidateProvider.V1.ValidateFunction = function (remote) {

    //ttCall4 Mask-Eventhandler registirerien:
    
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    ttCall4.Hook.DataFields.on('lostFocus', DataField_LostFocus, this);
    //ttCall4.Hook.DataFields.on('keyPress', DataField_KeyPress, this);

    var myDate;
    var myDateResult;
    var mydatefield;
    var mycheck;
    var myMail;
    var mymailfield;
    var myMailResult;
    var myText;
    var myTextField;
    var myReplaceResult;

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
    //Event changeValue
    ttCall4.Hook.DataFields.on('changeValue', function (index, newValue) {
        if (index == (remote.auswahl.textField - 200)) {
            myText = null;
            myReplaceResult = null;
            myText = ttCall4.Hook.DataFields[remote.auswahl.textField - 200].value.getValue();
            myReplaceResult = remote.auswahl.Replace(myText);
            //alert(  myReplaceResult+'  '+ myText);

            if ((myText == null) || (myReplaceResult == myText)) {
                //Do Nothing
            }
            else {
                ttCall4.Hook.DataFields[remote.auswahl.textField - 200].value.setValue(myReplaceResult);
                Ext.MessageBox.alert('Fehler', unescape("Textinhalt enth%E4lt nicht zul%E4ssige zeichen die entfernt wurden"));

            }
        }
    }, this);
        
    function ttCallField_LostFocus(index) {

        switch (index) {

            //Check Date Script
            case remote.auswahl.dateField:

                myDate = ttCall4.Hook.DataFields[remote.auswahl.dateField - 200].value.getValue();
                myDateResult = remote.auswahl.getCheckResult(remote.auswahl.check, myDate, remote.auswahl.type);
                mydatefield = ttCall4.Hook.DataFields[remote.auswahl.dateField - 200].getFieldObject();
                mycheck = remote.auswahl.check;

                if (myDateResult == true) {

                    mydatefield.setColor(ttCall4.Hook.ttColors.Green);
                    if (remote.auswahl.OKmessage == '') {
                        //Nothing
                    }
                    else {
                        Ext.MessageBox.alert('OK', remote.auswahl.OKmessage);
                    }
                }
                else {
                    mydatefield.setColor(ttCall4.Hook.ttColors.Red);
                    if (remote.auswahl.Failmessage == '') {
                        //Nothing

                    }
                    else {
                        Ext.MessageBox.alert('Fehler',remote.auswahl.Failmessage);
                    }
                }
                break;
            //Email Validate
            case remote.auswahl.mailField:

                myMail = ttCall4.Hook.DataFields[remote.auswahl.mailField - 200].value.getValue();
                mymailfield = ttCall4.Hook.DataFields[remote.auswahl.mailField - 200].getFieldObject();
                myMailResult = remote.auswahl.EmailIsValid(myMail);
                
                if (myMail == null) {
                    //Do Nothing
                }
                else {
                    if (myMailResult == true) {

                        mymailfield.setColor(ttCall4.Hook.ttColors.Green);

                        //Ext.MessageBox.alert('Fehler', 'Das E-Mail feld darf nicht leer sein.');

                    }
                    else {
                        mymailfield.setColor(ttCall4.Hook.ttColors.Red);

                        Ext.MessageBox.alert('Fehler', 'Diese E-Mail ist nicht Valide.');

                    }
                }
                break;
            //Text Sonderzeichen entfernen
            //case remote.auswahl.textField:

            //    myText = null;
            //    myReplaceResult = null;
            //    myText = ttCall4.Hook.DataFields[remote.auswahl.textField - 200].value.getValue();
            //    myReplaceResult = remote.auswahl.Replace(myText);
            //    //alert(  myReplaceResult+"="+myText);
                
            //    if ((myText == null) || (myReplaceResult == myText)) {
            //        //Do Nothing
            //    }
            //    else {
            //        ttCall4.Hook.DataFields[remote.auswahl.textField - 200].value.setValue(myReplaceResult);
            //        Ext.MessageBox.alert('Fehler', unescape("Textinhalt enth%E4lt nicht zul%E4ssige zeichen die entfernt wurden"));

            //    }

            //    break;
        }

    };
    return this;
}
