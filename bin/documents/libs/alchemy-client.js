/*
Alchemy Websockets Client Library
Copyright 2011 Olivine Labs, LLC.
http://www.olivinelabs.com
*/

/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

(function() {
  function Alchemy(options) {
    // thanks, John. http://ejohn.org/blog/simple-class-instantiation/
    if (!this instanceof Alchemy) {
      return new Alchemy(options);
    } else {
      if (!options) {
        options = {};
      }

      this._options = MergeDefaults(this._defaultOptions, options);

      if (!options.SocketType) {
        /* Try to autodetect websocket support if we have Modernizr
        loaded. If another lib (like web-sockets-js) is loaded that
        creates a websocket obj where we wouldn't normally have one,
        we'll assume that it's flash.*/
        if (!!Modernizr) {
          if (Modernizr.websockets) {
            this.SocketType = Alchemy.prototype.SocketTypes.WebSocket;
          } else if (!Modernizr.websockets) {
            this.SocketType = Alchemy.prototype.SocketTypes.FlashSocket;
          }
        }
      }

      if (!window.WebSocket) {
        throw 'UNSUPPORTED: Websockets are not supported in this browser!';
      }

      this.SocketState = Alchemy.prototype.SocketStates.Closed;

      this.Connected = this._options.Connected;
      this.Disconnected = this._options.Disconnected;
      this.MessageReceived = this._options.MessageReceived;
    }
  }

  Alchemy.prototype = {
    _socket: null,
    _lastReceive: (new Date()).getTime(),
    _options: {},

    SocketStates: {
      Connecting: 0,
      Open: 1,
      Closing: 2,
      Closed: 3
    },

    SocketState: 3,

    SocketTypes: {
      WebSocket: 'websocket',
      FlashSocket: 'flashsocket'
    },

    Start: function() {
      var server = 'ws://' + this._options.Server + ':' + this._options.Port + '/' + this._options.Action + '/' + this._options.SocketType;
      var ACInstance = this;
      if (this._socket != null) this._socket.close();
      this._socket = new WebSocket(server);

      this._socket.onopen = function() { ACInstance._OnOpen(); };
      this._socket.onmessage = function(data) { ACInstance._OnMessage(data); };
      this._socket.onclose = function() { ACInstance._OnClose(); };
      this._socket.onerror = function(event) { ACInstance._OnError(); }

      if (this._options.BinaryType != undefined)
        this._socket.binaryType = this._options.BinaryType;

      this.SocketState = Alchemy.prototype.SocketStates.Connecting;

      if (this._options.DebugMode) {
        console.log('Server started, connecting to ' + server);
      }
    },

    Send: function(data) {
      if (typeof data === 'object') {
        data = JSON.stringify(data);
      }

      this._socket.send(data);

      if (this._options.DebugMode) {
        console.log('Sent data to server: ' + data);
      }
    },

    Stop: function() {
      this._socket.close();
      this._socket = null;
      if (this._options.DebugMode) {
        console.log('Closed connection.');
      }
    },

    Connected: function() { },
    Disconnected: function() { },
    MessageReceived: function() { },
    Error: function(event) { },

    _OnOpen: function() {
      var instance = this;
      this.SocketState = Alchemy.prototype.SocketStates.Open;

      if (this._options.DebugMode) {
        console.log('Connected.');
      }

      this.Connected();
    },

    _OnMessage: function(event) {
      var instance = this;

      this._lastReceive = (new Date()).getTime();

      if (this._options.DebugMode) {
        console.log('Message received: ' + JSON.stringify(event.data));
      }

      this.MessageReceived(event);
    },

    _OnClose: function() {
      var instance = this;
      if (this._options.DebugMode) {
        console.log('Connection closed.');
      }

      this.SocketState = Alchemy.prototype.SocketStates.Closed;

      this.Disconnected();
    },

    _OnError: function(event) {
      var instance = this;
      if (this._options.DebugMode) {
        console.log('Error: ' + event);
      }

      this.SocketState = Alchemy.prototype.SocketStates.Closed;

      this.Error(event);
    }
  };

  Alchemy.prototype._defaultOptions = {
    Port: 81,
    Server: '',
    Action: '',
    SocketType: Alchemy.prototype.SocketTypes.WebSocket,

    Connected: function() { },
    Disconnected: function() { },
    MessageReceived: function(data) { },

    DebugMode: false
  };

  function MergeDefaults(o1, o2) {
    var o3 = {};
    var p = {};

    for (p in o1) {
      o3[p] = o1[p];
    }

    for (p in o2) {
      o3[p] = o2[p];
    }

    return o3;
  }

  window.Alchemy = Alchemy;
  window.MergeDefaults = MergeDefaults;

  if(window.MozWebSocket){
    window.WebSocket = MozWebSocket;
  }
})(window);