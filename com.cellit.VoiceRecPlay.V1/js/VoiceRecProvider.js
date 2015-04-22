//define namespace
Ext.ns('com.cellit.VoiceRecPlay.V1');

com.cellit.VoiceRecPlay.V1.VoiceRecProvider = function (remote) {
    //Variabeln
    var voiceStore;
    var voicepfad;
    var voicecount;
    

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
            case remote.playField:

                
                voicecount = remote.GetVoiceAnzahl(ttCall4.Hook.CustomerFields.id.getValue());
                //Soundfile Pfad mit prüfung

                if (voicecount == 0) {
                    alert("Kein VoiceFile vorhanden");
                }
                else {
                    if (voicecount == 1) {
                        voicepfad = remote.GetVoicepfad(ttCall4.Hook.CustomerFields.id.getValue(), voicecount);
                        //voiceStore= [
                        //[voicepfad[0]]                        
                        //];
                        GetVoice(voicepfad[0])
                    }
                    else {
                        if (voicecount==2)
                            voicepfad = remote.GetVoicepfad(ttCall4.Hook.CustomerFields.id.getValue(), voicecount);
                            voiceStore= [
                            [voicepfad[0]],
                            [voicepfad[1]]
                            ];
                            GetGrid(voiceStore);
                    }                    
                }
                

                
                    //Soundfile abrufen
                   // var url = remote.GetSoundFile(voicepfad[0]);
                   // //Player anzeigen 
                   // var player = new Ext.Window({
                   //     title: 'Player',
                   //     width: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 550 : 315,
                   //     height: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 110 : 63,
                   //     minWidth: 275,
                   //     minHeight: 50,
                   //     layout: 'fit',
                   //     modal: true,
                   //     items: [
                   //         new Ext.Panel({
                   //             html: '<audio controls autoplay ><source src="' + url + '" type="audio/mpeg">Your browser does not support the audio element.</audio>'
                   //         })]
                   // });
                   //player.show();
                    //container = new com.cellit.VoiceRecPlay.V1.SimplePlayer({
                    //    url: url
                    //});
                    //container.show();
                
                break;
            default:
                break;
        }
        //Voicerecording Tabelle Anzeigen
        function GetGrid()
        {
            var window = new Ext.Window({
                title: 'VR Auswahl',
                width: 800,
                height: 600,
                layout: 'fit',
                modal: true,
                items: [
                         new Ext.Panel({
                             html: '<div id="grid1" ></div><p>' + voicepfad + '</p>'
                         })]
                    ,
                buttons: [{
                    text: 'Submit',
                    disabled: true
                }, {
                    text: 'Close',
                    handler: function () {
                        window.hide();
                    }
                }]

            });
            window.show();
            //Grid
            var store = new Ext.data.ArrayStore({
                fields: [
                    { name: 'company' },
                    { name: 'change', type: 'float' },
                    { name: 'pctChange', type: 'float' },
                    { name: 'lastChange', type: 'date', dateFormat: 'n/j h:ia' }
                ]
            });
            //Store Laden
            store.loadData(voiceStore);
            //grid erstellen
            var grid = new Ext.grid.GridPanel({
                store: store,
                columns: [
                    {
                        id: 'company',
                        header: 'Pfad',
                        width: 160,
                        height: 50,
                        sortable: true,
                        dataIndex: 'company'
                    },
                    {
                        header: 'Change',
                        width: 75,
                        sortable: true,
                        renderer: change,
                        dataIndex: 'change'
                    },
                    {
                        header: '% Change',
                        width: 75,
                        sortable: true,
                        renderer: pctChange,
                        dataIndex: 'pctChange'
                    },
                    {
                        header: 'Last Updated',
                        width: 85,
                        sortable: true,
                        renderer: Ext.util.Format.dateRenderer('m/d/Y'),
                        dataIndex: 'lastChange'
                    },
                    {
                        xtype: 'actioncolumn',
                        width: 150,
                        items: [{
                            icon: 'http://agent.cellit-gruppe.de/ttframework/img/start.gif',  // Use a URL in the icon config
                            tooltip: 'Viocerec Play',
                            handler: function (grid, rowIndex, colIndex) {
                                var rec = store.getAt(rowIndex);
                                GetVoice(rec.get('company'));
                            }
                            //}, {
                            //    getClass: function (v, meta, rec) {          // Or return a class from a function
                            //        if (rec.get('change') < 0) {
                            //            this.items[1].tooltip = 'Do not buy!';
                            //            return 'alert-col';
                            //        } else {
                            //            this.items[1].tooltip = 'Buy stock';
                            //            return 'buy-col';
                            //        }
                            //    },
                            //    handler: function (grid, rowIndex, colIndex) {
                            //        var rec = store.getAt(rowIndex);
                            //        alert("Buy " + rec.get('company'));
                            //    }
                        }]
                    }
                ],
                stripeRows: true,
                autoExpandColumn: 'company',
                height: 350,
                width: 600,
                title: 'Array Grid',
                // config options for stateful behavior
                stateful: true,
                stateId: 'grid'
            });
            grid.render('grid1');
        }
        function change(val) {
            if (val > 0) {
                return '<span style="color:green;">' + val + '</span>';
            } else if (val < 0) {
                return '<span style="color:red;">' + val + '</span>';
            }
            return val;
        }
        function pctChange(val) {
            if (val > 0) {
                return '<span style="color:green;">' + val + '%</span>';
            } else if (val < 0) {
                return '<span style="color:red;">' + val + '%</span>';
            }
            return val;
        }
        function GetVoice(gridurl)
        {
            var url = remote.GetSoundFile(gridurl);            
            var player = new Ext.Window({
                //title: 'Player',
                header: false,
                width: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 550 : 315,
                height: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 110 : 63,
                minWidth: 275,
                minHeight: 50,
                layout: 'fit',
                //modal: true,
                items: [
                    new Ext.Panel({
                        html: '<audio controls autoplay ><source src="' + url + '" type="audio/mpeg">Your browser does not support the audio element.</audio>'
                    })]
            });
            player.show();
            return this;
        }
    }
    return this;
}