/*!
* Dieses Beispiel verwendet die breits seitens ttCall 4 geladene Ext JS Library 3.4.0
* Copyright(c) 2006-2011 Sencha Inc. (http://www.sencha.com/license)
* Für die Verwendung benötigen Sie eine entsprechende Lizenz. 
* Weitere Beispiele sowie eine Dokumentation zur Ext JS Library 3.4.0 finden Sie unter:  
* http://dev.sencha.com/deploy/ext-3.4.0/examples/  
* http://docs.sencha.com/extjs/3.4.0/#!/api  
*/

// define namespace
Ext.ns('com.cellit.Mask.Extention.V1.');

com.cellit.Mask.Extention.V1.SimplePlayer = function (options) {

    var window = new Ext.Window({
        title: 'Player',
        width: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 550 : 315,
        height: (Ext.isIE || (navigator.userAgent.match(/Trident\/[6]/i)) || (navigator.userAgent.match(/Trident\/[7]/i))) ? 110 : 63,
        minWidth: 275,
        minHeight: 50,
        layout: 'anchor',
        modal: true,
        items: [
            new Ext.Panel({
                html: '<audio controls autoplay style=\"audioWidth: 450px; audioHeight: 70px \"><source src="' + options.url + '" type="audio/mpeg">Your browser does not support the audio element.</audio>'
            })
        ]
    });

    return window;
}