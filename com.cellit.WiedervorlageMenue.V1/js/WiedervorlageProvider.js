// define namespace
Ext.ns('com.cellit.WiedervorlageMenue.V1');

com.cellit.WiedervorlageMenue.Provider = function (remote) {

    grid = new ttFramework.templates.editorGrid({
        appTitle: 'Wiedervorlagen-Übersicht',
        allowAdd: false,
        allowDelete: false,
        // Spalten definieren
        columns: [
              {
                 dataIndex: 'url',
                 width: 28,
                 align: 'center',
                 renderer: function (value) {
                     return '<div style="width:100%;height:16px;background-image:url('+value+');background-position:center center;background-repeat:no-repeat;">&nbsp;</div>';
                 }
             },
			 {
			    header: "Wiedervorlage am", dataIndex: 'WV_am', width: 120, sortable: true,
			    renderer: function (value) {
			        return value.toLocaleDateString() + " " + value.toLocaleTimeString();
			    }
			 },
             { header: "Kunde", dataIndex: 'WV_Kunde', width: 150, sortable: true, editor: new Ext.form.TextField({ editable: false }) },
             { header: "Bemerkung", dataIndex: 'WV_Bemerkung', width: 200, sortable: false, height: 100, editor: new Ext.form.TextField({ editable: false }) },
             {
                header: "Aufgerufen", dataIndex: 'WV_aufgerufen', width: 120, sortable: true, editor: new Ext.form.TextField({ editable: false }),
                renderer: function (value) {
                    switch (value) {
                        case true:
                            return "ja";
                        case false:
                            return "nein";
                        default:
                            return "";
                    }
                },
             },
             { header: "Projekt", dataIndex: 'Projekt', width: 120, sortable: true, editor: new Ext.form.TextField({ editable: false }) },
             { header: "Kampagne", dataIndex: 'Campaign_Name', width: 120, sortable: true, editor: new Ext.form.TextField({ editable: false }) },
        ],
        idField: 'Wiedervorlagen',
        // Function für den Abruf der Daten
        getDatas: {
            fn: remote.GetWv,
            params: []
        },
    });

    return grid;
}
