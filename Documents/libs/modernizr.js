/*! modernizr 3.1.0 (Custom Build) | MIT *
 * http://modernizr.com/download/?-websockets !*/
!function(e,n,s){function o(e){var n=l.className,s=Modernizr._config.classPrefix||"";if(r&&(n=n.baseVal),Modernizr._config.enableJSClass){var o=new RegExp("(^|\\s)"+s+"no-js(\\s|$)");n=n.replace(o,"$1"+s+"js$2")}Modernizr._config.enableClasses&&(n+=" "+s+e.join(" "+s),r?l.className.baseVal=n:l.className=n)}function a(e,n){return typeof e===n}function t(){var e,n,s,o,t,l,c;for(var r in f){if(e=[],n=f[r],n.name&&(e.push(n.name.toLowerCase()),n.options&&n.options.aliases&&n.options.aliases.length))for(s=0;s<n.options.aliases.length;s++)e.push(n.options.aliases[s].toLowerCase());for(o=a(n.fn,"function")?n.fn():n.fn,t=0;t<e.length;t++)l=e[t],c=l.split("."),1===c.length?Modernizr[c[0]]=o:(!Modernizr[c[0]]||Modernizr[c[0]]instanceof Boolean||(Modernizr[c[0]]=new Boolean(Modernizr[c[0]])),Modernizr[c[0]][c[1]]=o),i.push((o?"":"no-")+c.join("-"))}}var i=[],l=n.documentElement,f=[],c={_version:"3.1.0",_config:{classPrefix:"",enableClasses:!0,enableJSClass:!0,usePrefixes:!0},_q:[],on:function(e,n){var s=this;setTimeout(function(){n(s[e])},0)},addTest:function(e,n,s){f.push({name:e,fn:n,options:s})},addAsyncTest:function(e){f.push({name:null,fn:e})}},Modernizr=function(){};Modernizr.prototype=c,Modernizr=new Modernizr,Modernizr.addTest("websockets","WebSocket"in e&&2===e.WebSocket.CLOSING);var r="svg"===l.nodeName.toLowerCase();t(),o(i),delete c.addTest,delete c.addAsyncTest;for(var u=0;u<Modernizr._q.length;u++)Modernizr._q[u]();e.Modernizr=Modernizr}(window,document);