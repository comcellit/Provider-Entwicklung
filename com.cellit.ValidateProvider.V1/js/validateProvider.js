// define namespace
Ext.ns('com.cellit.ValidateProvider.V1.');


com.cellit.ValidateProvider.V1.ValidateFunction = function (remote) {

    //ttCall4 Mask-Eventhandler registirerien:
    ttCall4.Hook.on('dialogStart', Mask_Open, this);
    ttCall4.Hook.on('dialogEnd', Mask_Close, this);
    
    var _comboPLZ;
    var _comboOrt;
    var _comboStrasse;
    var _textFieldHausnummer;
    var fieldObjekt;
    var list;
    var store;
    var combo;
    var myPersoField;
    var myPerso;
    var mydate;
    var myresult;
    var mydatefield;
    var mycheck;
    var mymail;
    var mymailfield;
    var myTextField;
    var mySpaceField;
    

    function Mask_Open() {
        //Do Nothing
    };

    function Mask_Close() {
        //Variablen Löschen
        _comboPLZ = null;
        _comboOrt = null;
        _comboStrasse = null;
        _textFieldHausnummer = null;
        fieldObjekt = null;
        list = null;
        store = null;
        combo = null;
        myPersoField = null;
        myPerso = null;
        mydate = null;
        myresult = null;
        mydatefield = null;
        mycheck = null;
        mymail = null;
        mymailfield = null;
        myTextField = null;
    };
    //ttCall4.Hook.DataFields.on('keyPress', function (index, event) {
    //    if (index == remote.auswahl.plzField - 200) {
    //        GetForm();
    //    }
    //}, this);
    ttCall4.Hook.DataFields.on('gotFocus', function (index) {
        if (index == remote.auswahl.plzField - 200 && (ttCall4.Hook.DataFields[remote.auswahl.plzField - 200].value.getValue() == null || ttCall4.Hook.DataFields[remote.auswahl.plzField - 200].value.getValue() == '')) {
            GetForm();
        }
       
    }, this);

    ttCall4.Hook.DataFields.on('lostFocus', function (index) {
        if (index == remote.auswahl.dateField-200) {
            //Check Date
            mydate = ttCall4.Hook.DataFields[remote.auswahl.dateField - 200].value.getValue();
            myresult = remote.auswahl.getCheckResult(remote.auswahl.check, mydate, remote.auswahl.type);
            mydatefield = ttCall4.Hook.DataFields[remote.auswahl.dateField - 200].getFieldObject();
            mycheck = remote.auswahl.check;

            if (myresult == true) {

                mydatefield.setColor(ttCall4.Hook.ttColors.Green);
                if (remote.auswahl.OKmessage == '') {
                    //Do Nothing
                }
                else {
                    alert(remote.auswahl.OKmessage);
                }
            }
            else {
                mydatefield.setColor(ttCall4.Hook.ttColors.Red);
                if (remote.auswahl.Failmessage == '') {
                    //Do Nothing
                }
                else {
                    Ext.MessageBox.alert('Achtung', remote.auswahl.Failmessage);
                }
            }
        }
        if (index == remote.auswahl.mailField - 200) {
            //Check Email
            mymail = ttCall4.Hook.DataFields[remote.auswahl.mailField - 200].value.getValue();
            mymailfield = ttCall4.Hook.DataFields[remote.auswahl.mailField - 200].getFieldObject();
            myresult = remote.auswahl.EmailIsValid(mymail);

            if (myresult == true) {
                mymailfield.setColor(ttCall4.Hook.ttColors.Green);
            }
            else {
                mymailfield.setColor(ttCall4.Hook.ttColors.Red);
                Ext.MessageBox.alert('Achtung', 'Diese E-Mail ist nicht Valide.');
            }
        }
        if (index == remote.auswahl.textField - 200) {
            //Entferne sonderzeichen
            myTextField = ttCall4.Hook.DataFields[remote.auswahl.textField - 200].value.getValue();
            myresult = remote.auswahl.ReplaceStringValue(myTextField);

            if (myTextField == myresult) {
                //Do Nothing
            }
            else {
                Ext.MessageBox.alert('Achtung', 'nicht erlaubte Sonderzeichen wurden entfernt');
                ttCall4.Hook.DataFields[remote.auswahl.textField - 200].value.setValue(myresult);
            }
        }
        if (index == remote.auswahl.spaceField - 200) {
            //Entfernt Leerzeichen
            mySpaceField = ttCall4.Hook.DataFields[remote.auswahl.spaceField - 200].value.getValue();
            myresult = remote.auswahl.ReplaceSpaceValue(mySpaceField);

            if (mySpaceField == myresult) {
                //Do Nothing
            }
            else {
                Ext.MessageBox.alert('Achtung', 'nicht erlaubte Leerzeichen wurden entfernt');
                ttCall4.Hook.DataFields[remote.auswahl.spaceField - 200].value.setValue(myresult);
            }
        }
        if (index == remote.auswahl.persoField - 200) {
            //Check Personalausweis
            myPersoField = ttCall4.Hook.DataFields[remote.auswahl.persoField - 200].getFieldObject();
            myPerso = ttCall4.Hook.DataFields[remote.auswahl.persoField - 200].value.getValue();
            myresult = remote.auswahl.ValidateAusweis(myPerso);
            if (myresult == true) {
                myPersoField.setColor(ttCall4.Hook.ttColors.Green);
            }
            else {
                myPersoField.setColor(ttCall4.Hook.ttColors.Red);
            }
        }
    }, this);
    
    function GetForm() {
       
        var window = new Ext.Window({
            title: 'Adress Pruefung',
            resizable: false,
            layout: 'table',
            modal: true,
            items: [
                new Ext.FormPanel({

                baseCls: 'x-plain',
                labelWidth: 75, 
                bodyStyle: 'margin: 10px; padding: 5px 3px;',
                frame: true,
                width: 300,
                defaults: { width: 230 },
                defaultType: 'textfield',
                items: [{
                    fieldLabel: 'PLZ',
                    xtype: 'combo',
                    typeAhead: true,
                    enableKeyEvents: true,
                    mode: 'local',
                    hiddenName: 'plz',
                    displayField: 'plz',
                    width: 180,
                    forceSelection: true,
                    autoSelect: false,
                    emptyText: 'PLZ eingeben',
                    editable: true,
                    minChars: 4,
                    selectOnFocus: true,
                    triggerAction: 'all',
                    store: new Ext.data.ArrayStore({
                        id: 0,
                        fields: [
                            'id',
                            'plz'
                        ]
                                , data: []
                    }),
                    listeners: {
                        'keyup': function (field, e) {
                            GetFieldObject(this);
                            if (IsAllowedKey(e.keyCode) == false) {
                                //Do Nothing
                            } else {
                                if (e.keyCode == 13) {
                                    //Do Nothing
                                } else {
                                    if (field.getRawValue().length == 4) {
                                        GetListValue(field, "PLZ");
                                    }
                                }
                            }
                        },

                        'select': function (combo, value, index) {
                            GetListValue(combo, "ORT");

                        }
                    }
                },
                {
                    fieldLabel: 'Ort',
                    xtype: 'combo',
                    typeAhead: true,
                    enableKeyEvents: true,
                    mode: 'local',
                    hiddenName: 'Ort',
                    displayField: 'Ort',
                    width: 180,
                    forceSelection: true,
                    autoSelect: false,
                    emptyText: 'Ort eingeben',
                    editable: true,
                    minChars: 3,
                    selectOnFocus: true,
                    triggerAction: 'all',
                    store: new Ext.data.ArrayStore({
                        id: 0,
                        fields: [
                            'id',
                            'Ort'
                        ]
                                , data: []
                    }),
                    listeners: {
                        'select': function () {
                            _comboStrasse.enable();
                        }
                    }
                }, {
                    fieldLabel: 'Strasse',
                    disabled: true,
                    xtype: 'combo',
                    typeAhead: true,
                    mode: 'local',
                    width: 180,
                    hiddenName: 'strasse',
                    enableKeyEvents: true,
                    emptyText: 'Strasse eingeben',
                    displayField: 'strasse',
                    valueField: 'strasse',
                    forceSelection: true,
                    triggerAction: 'all',
                    store: new Ext.data.ArrayStore({
                        id: 0,
                        fields: [
                            'id',
                            'strasse'
                        ]
                                , data: []
                    }),
                    listeners: {
                        'keyup': function (field, e) {
                            if (IsAllowedKey(e.keyCode) == false) {
                                //Do Nothing
                            } else {
                                if (e.keyCode == 13) {
                                    //Do Nothing
                                } else {
                                    if (field.getRawValue().length == 5) {
                                        GetListValue(field, "STR");
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    fieldLabel: 'Hausnummer',
                    xtype: 'textfield',
                    emptyText: 'Hausnummer eingeben',
                    enableKeyEvents: true,
                    width: 60,
                    allowBlank: false,
                    regext: '[1-9][0-9]?[0-9]?[a-z]?',
                    regexText: "Ungueltig",
                    listeners: {
                        'keyup': function (field, e) {

                            if (field.getRawValue().length >= 1) {
                                window.buttons[0].enable();
                            }
                        },
                    },
                },
                ],

            })],
            buttons: [{
                text: 'Speichern',
                disabled: true,
                handler: function () {
                    try {
                        ttCall4.Hook.DataFields[remote.auswahl.plzField - 200].value.setValue(_comboPLZ.getRawValue());
                        ttCall4.Hook.DataFields[remote.auswahl.ortField - 200].value.setValue(_comboOrt.getRawValue());
                        ttCall4.Hook.DataFields[remote.auswahl.streetField - 200].value.setValue(_comboStrasse.getRawValue())
                        ttCall4.Hook.DataFields[remote.auswahl.numberField - 200].value.setValue(_textFieldHausnummer.getValue())
                    }
                    catch (err) {
                        //Do Nothning
                    }
                    window.close();
                }

            }]

        });
        window.show();
        return window;
    };

    function GetListValue(value, listCase) {

        switch (listCase) {
            case 'PLZ':
                list = remote.auswahl.GetPLZ(value.getRawValue());
                combo = value;
                store = combo.getStore();
                combo.trigger.show();
                if (list.length >= 1) {
                    setTimeout(function () {
                        var recs = [],
                            recType = store.recordType;
                        for (var i = 0; i < list.length; i++) {
                            recs.push(new recType({
                                id: i + 1,
                                plz: list[i]
                            }));

                        }
                        store.add(recs);
                        combo.onLoad();
                        combo.focus();

                    }, 200);
                }
                break;
            case 'ORT':
                list = remote.auswahl.GetOrt(value.getRawValue());
                combo = _comboOrt;
                store = combo.getStore();
                combo.trigger.show();
                if (list.length >= 1) {
                    setTimeout(function () {
                        var recs = [],
                            recType = store.recordType;
                        for (var i = 0; i < list.length; i++) {
                            recs.push(new recType({
                                id: i + 1,
                                Ort: list[i]
                            }));

                        }
                        store.add(recs);
                        combo.focus();
                        combo.onLoad();

                    }, 200);
                }
                break;
            case 'STR':
                list = remote.auswahl.GetStreet(value.getRawValue(), _comboPLZ.getRawValue());
                combo = value;
                store = combo.getStore();
                combo.trigger.show();
                if (list.length >= 1) {

                    var recs = [],
                        recType = store.recordType;
                    for (var i = 0; i < list.length; i++) {
                        recs.push(new recType({
                            id: i + 1,
                            strasse: list[i]
                        }));

                    }
                    store.add(recs);
                    combo.focus();
                    combo.onLoad();

                }
        }
    };

    function IsAllowedKey(key) {
        if (key <= 90 && key > 45) {
            return true;
        } else {
            if (key <= 16) {
                return true;
            } else {
                if (key >= 96 && key <= 105) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    };

    function GetFieldObject(object) {
        if (_comboPLZ == null) {
            _comboPLZ = object.ownerCt.items.items[0];
            _comboOrt = object.ownerCt.items.items[1];
            _comboStrasse = object.ownerCt.items.items[2];
            _textFieldHausnummer = object.ownerCt.items.items[3];
        }
    };
    
    return this;
}
