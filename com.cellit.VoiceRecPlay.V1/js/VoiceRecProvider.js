//define namespace
Ext.ns('com.cellit.VoiceRecPlay.V1');

com.cellit.VoiceRecPlay.V1.VoiceRecProvider = function (remote) {
    //Variabeln
    var myStore;

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
                        myStore = [
                        [voicepfad[0]]
                        ];
                        GetVoice(voicepfad[0])
                    }
                    else {
                        if (voicecount == 2) {
                            voicepfad = remote.GetVoicepfad(ttCall4.Hook.CustomerFields.id.getValue(), voicecount);
                            myStore = [
                            [voicepfad[0], voicepfad[1], voicepfad[2], voicepfad[3]],
                            [voicepfad[4], voicepfad[5], voicepfad[6], voicepfad[7]]
                            ];
                            GetGrid(myStore);
                        }
                        else {
                            if (voicecount == 3) {
                                voicepfad = remote.GetVoicepfad(ttCall4.Hook.CustomerFields.id.getValue(), voicecount);
                                myStore = [
                                [voicepfad[0], voicepfad[1], voicepfad[2], voicepfad[3]],
                                [voicepfad[4], voicepfad[5], voicepfad[6], voicepfad[7]],
                                [voicepfad[8], voicepfad[9], voicepfad[10], voicepfad[11]]
                                ];
                                GetGrid(myStore);
                            }
                            else {
                                if (voicecount == 4) {
                                    voicepfad = remote.GetVoicepfad(ttCall4.Hook.CustomerFields.id.getValue(), voicecount);
                                    myStore = [
                                    [voicepfad[0], voicepfad[1], voicepfad[2], voicepfad[3]],
                                    [voicepfad[4], voicepfad[5], voicepfad[6], voicepfad[7]],
                                    [voicepfad[8], voicepfad[9], voicepfad[10], voicepfad[11]],
                                    [voicepfad[12], voicepfad[13], voicepfad[14], voicepfad[15]],
                                    ];
                                    GetGrid(myStore);
                                }
                                else {
                                    if (voicecount == 5) {
                                        voicepfad = remote.GetVoicepfad(ttCall4.Hook.CustomerFields.id.getValue(), voicecount);
                                        myStore = [
                                        [voicepfad[0], voicepfad[1], voicepfad[2], voicepfad[3]],
                                        [voicepfad[4], voicepfad[5], voicepfad[6], voicepfad[7]],
                                        [voicepfad[8], voicepfad[9], voicepfad[10], voicepfad[11]],
                                        [voicepfad[12], voicepfad[13], voicepfad[14], voicepfad[15]],
                                        [voicepfad[16], voicepfad[17], voicepfad[18], voicepfad[19]]
                                        ];
                                        GetGrid(myStore);
                                    }
                                }
                            }


                        }
                    }
                }
                break;
            default:
                break;
        }
        //Voicerecording Tabelle Anzeigen
        function GetGrid()
        {

            var window = new Ext.Window({
                width: 300,
                height: 120 + (voicecount * 25),
                layout: 'table',
                modal: true,
                items: [
                         new Ext.Panel({
                             html: '<div id="grid1" ></div>'
                         })]
                    ,
                buttons: [{
                    text: 'Close',
                    handler: function () {
                        window.close();
                    }
                }]

            });
            window.show();
            //Grid
            var store = new Ext.data.ArrayStore({
                fields: [
                    { name: 'url' },
                    { name: 'level', type: 'float' },
                    { name: 'datum'},
                    { name: 'uhrzeit'}
               ]
            });
            //Store Laden
            store.loadData(myStore);
            //grid erstellen
            var grid = new Ext.grid.GridPanel({
                store: store,
                columns: [
                    {id: 'level', header: 'level', width: 50, sortable: true, dataIndex: 'level'},
                    {header: 'Datum', width: 100, sortable: true, dataIndex: 'datum'},
                    {header: 'Uhrzeit', width: 100, sortable: true,dataIndex: 'uhrzeit'},
                    {
                        xtype: 'actioncolumn', width: 25, items: [{
                            icon: 'http://agent.cellit-gruppe.de/ttframework/comCellitImg/play.png',  // Use a URL in the icon config
                            tooltip: 'Viocerec Play',
                            handler: function (grid, rowIndex, colIndex) {
                                var rec = store.getAt(rowIndex);
                                GetVoice(rec.get('url'));
                            }
                        }]
                    }
                ],
                stripeRows: true,
                height: voicecount*100,
                width: 300,
                title: 'VR Auswahl',
                // config options for stateful behavior
                stateful: true,
                stateId: 'grid'
            });
            grid.render('grid1');
        }
        //Loading Voice
        function GetVoice(gridurl)
        {
            var url = remote.GetSoundFile(gridurl);
            //Loading
            //Ext.MessageBox.show({
            //    title: 'Please wait',
            //    msg: 'Loading items...',
            //    progressText: 'Initializing...',
            //    width: 300,
            //    progress: true,
            //    closable: false,
            //});

            // this hideous block creates the bogus progress
            var f = function (v) {
                return function () {
                    if (v == 12) {
                        Ext.MessageBox.hide();
                        Ext.example.msg('Done', 'Your fake items were loaded!');
                    } else {
                        var i = v / 11;
                        Ext.MessageBox.updateProgress(i, Math.round(100 * i) + '% completed');
                    }
                };
            };
            for (var i = 1; i < 13; i++) {
                setTimeout(f(i), i * 500);
            }
            var player = new Ext.Window({
                //title: 'Player',
                header: false,
                width: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 550 : 315,
                height: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 110 : 63,
                minWidth: 275,
                minHeight: 55,
                layout: 'fit',
                //modal: true,
                items: [
                    new Ext.Panel({
                        html: '<audio style="margin-top:5px;" controls autoplay ><source src="' + url + '" type="audio/mpeg">Your browser does not support the audio element.</audio>'
                    })]
            });
            player.show();
            return this;
        }
       
    }
    return this;
}