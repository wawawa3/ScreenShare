function onLoad() {
	var __INFO__ = true;
	var __DEBUG__ = false;

	if (__DEBUG__) {
		var dLabelID = document.createElement("p");
		var dLabelOffer = [];
		var dLabelAns = [];
		var dBody = document.getElementById("_body");
		var dLabelLog = document.createElement("p");
		var dButton = document.createElement("input");
		var dCast = true;
		dButton.type = "button";
		dButton.onclick = function() { dCast = !dCast; }
		dButton.value = "chk";
		dBody.appendChild(dLabelLog);
		dBody.appendChild(dLabelID);
		dBody.appendChild(dButton);

		var debugLog = function(log, col = "black") {
			dLabelLog.innerHTML += "</br><font color="+col+">"+log+"</font>";
		}
	}
	else {
		var debugLog = function(log, col = "black") {}
	}

	// Utility ------------------------------------------------------------
	if (!ArrayBuffer.prototype.slice) {
		ArrayBuffer.prototype.slice = function (start, end) {
			var that = new Uint8Array(this);
			if (end == undefined) 
				end = that.length;

			var result = new ArrayBuffer(end - start);
			var resultArray = new Uint8Array(result);

			for (var i = 0; i < resultArray.length; i++)
			   resultArray[i] = that[i + start];

			return result;
		}
	}

	if (!ArrayBuffer.prototype.segmentation) {
		ArrayBuffer.prototype.segmentation = function(segmentSize) {
			if (this.byteLength <= segmentSize)
				return [this];

			var segments = [];

			for(var i = 0; i * segmentSize < this.byteLength; i++)
				segments.push(this.slice(i * segmentSize, (i + 1) * segmentSize));

			return segments;
		}
	}

	function average(arr) {
		if (arr.length <= 1) 
			return arr.length == 0 ? 0 : arr[0];

		var sum = arr.reduce(function(previousValue, currentValue, index, array) {
			return previousValue + currentValue;
		});
//console.log(sum);
		return sum / arr.length;
	}

	function concatenation(segments) {
		var sumLength = 0;

		for(var i = 0; i < segments.length; i++)
			sumLength += segments[i].byteLength;

		var whole = new Uint8Array(sumLength);
		var pos = 0;

		for(var i = 0; i < segments.length; i++)
		{
			whole.set(new Uint8Array(segments[i]), pos);
			pos += segments[i].byteLength;
		}

		return whole.buffer;
	}

	function clamp(num, min, max) {
		return (num > max ? max : (num < min ? min : num));
	}

	function max(a, b) {
		return a > b ? a : b;
	}

	function clone(src) {
		var dst = {}
		
		for(var k in src)
			dst[k] = src[k];

		return dst;
	}

	function nowTime() {
		var now = new Date();
		return now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds();
	}

	function nowTimeM(date) {
		return date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds() + ":" + date.getMilliseconds();
	}

	// Def ------------------------------------------------------------
	const BrunchCount = 2;
	const ChankSize = 1024 * 32;
	const FrameHeaderLength = 0x0a;
	const AudioHeaderLength = 0x0c;
	const MaxHeaderLength = max(FrameHeaderLength, AudioHeaderLength);
	const TimeOutMillisecond = 2000;
	const CheckPacketIdentifier = "cp";

	const MessageType = {
		"Connected"				: 0x0,
		"UpdateID"				: 0x1,
		"PeerConnection"		: 0x2,
		"SDPOffer"				: 0x3,
		"SDPAnswer"				: 0x4,
		"ICECandidateOffer"		: 0x5,
		"ICECandidateAnswer"	: 0x6,
		"RemoveOffer"			: 0x7,
		"RemoveAnswer"			: 0x8,
		"Settings"				: 0x9,
		"StartCasting"			: 0xa,
		"StopCasting"			: 0xb,
		"RequestReconnect"		: 0xc,
		"Disconnect"			: 0xd,
	};

	const DataType = {
		"FrameBuffer" : 0x0,
		"AudioBuffer" : 0x1,
	};

	const AverageLength = 100;

	const DocumentTitle = "ScreenShare";

	// Application ------------------------------------------------------------
	var urlCreator = (window.URL || window.webkitURL);

	var elm_frame = document.getElementById("_frame");
	var elm_canvas = document.getElementById("_canvas");
	var canvas_ctx = elm_canvas.getContext('2d');
	var elm_audioCheckBox = document.getElementById("_audio");
	//var elm_block = document.getElementById("_block");
	var elm_idview = document.getElementById("_idview");
	var elm_loader = document.getElementById("_loader");
	var elm_phase = document.getElementById("_phase");
	var elm_info = document.getElementById("_nodeid");

	var frameBuffer = new Uint8Array();
	var chanks = [];

	var canvasLocking = false;
	var isCasting = false;
	
	var divisionNum = 1;
	var aspectRatio = 1.5;
	var canvasSize = {};
	var framePerSecond = 10;

	var receivedCount = 0;
	var connectTime;
	var connectedServerDate = null;
	var ntpInitialDelay = 0;
	var serverDelay = new Array(AverageLength);
	var parentDelay = new Array(AverageLength);

	var initializedLocalDate;
	var initializedNetworkTime;

	var delayTimeout = 1000;
	var requestTimeout = 2000;
	
	var loadingQueue;

	var lastImage = [];

	function totalMillisecond(date) {
		console.log("ms"+date.getUTCMilliseconds());
		console.log("sc"+date.getUTCSeconds());
		console.log("mn"+date.getUTCMinutes());
		console.log("hr"+date.getUTCHours());
		return date.getUTCMilliseconds() + 1000*(date.getUTCSeconds() + 60*(date.getUTCMinutes() + 60*date.getUTCHours()));
	}

	function setId(id) {
		peerId = id;
		peerDepth = Math.floor(Math.LOG2E * Math.log(peerId + 1));
		elm_idview.innerHTML = peerId;
		document.title = DocumentTitle + " [ID = " + peerId + ", Depth = "+ peerDepth +"]";
	}

	function setLoader(text) {
		elm_phase.innerHTML = text;
		elm_loader.style.display = "";
	}

	function clearLoader() {
		elm_phase.innerHTML = "";
		elm_loader.style.display = "none";
	}

	function setInfo(pt, ch, text) {
		if (!__INFO__) return;

		if (!Array.isArray(ch)) ch = [ch];
		for (var k in ch) {
			var dom = document.getElementById(pt).children[0].children[ch[k]];
			if (ch[k] == 0) dom.children[2].innerHTML = text;
			else dom.children[1].innerHTML = text;
		}
	}

	function updateSettings(data) {
		audioSampleRate = data.audioSettingData.sampleRate;
		audioIsStereo = data.audioSettingData.isStereo ? 2 : 1;

		divisionNum = data.captureSettingData.divisionNum;
		aspectRatio = data.captureSettingData.aspectRatio;
		canvasSize = {width:data.captureSettingData.width, height:data.captureSettingData.height};
		framePerSecond = data.captureSettingData.framePerSecond;

		loadingQueue = Array.apply(null, Array(divisionNum * divisionNum)).map(function () { return 0 });

		/*
		while (elm_block.firstChild)
			elm_block.removeChild(elm_block.firstChild);

		for (var row = 0; row < divisionNum; row++) {
			var rowElem = document.createElement("div");
			elm_block.appendChild(rowElem);

			for (var cell = 0; cell < divisionNum; cell++) {
				var cellElem = document.createElement('div');
				var base = document.createElement('img');
				var cover = document.createElement('img');
				//cover.className = "cover";
				cellElem.appendChild(base);
				cellElem.appendChild(cover);

				rowElem.appendChild(cellElem);
			}
		}*/
	}

	function resizeCanvas(redraw = true) {
		var windowAspectRatio = window.innerWidth / window.innerHeight;
		if (windowAspectRatio < aspectRatio) {
			elm_frame.style.width = "100%";
			elm_frame.style.height = (elm_frame.clientWidth / aspectRatio) + "px";
		}
		else {
			elm_frame.style.height = "100%";
			elm_frame.style.width = (elm_frame.clientHeight * aspectRatio) + "px";
		}

		var divWidth = elm_frame.clientWidth / divisionNum;
		var divHeight = divWidth / aspectRatio;

		if (!redraw) return;

		elm_canvas.width = elm_frame.clientWidth;
		elm_canvas.height = elm_frame.clientHeight;

		var dw = elm_canvas.width / divisionNum;
		var dh = elm_canvas.height / divisionNum;
		for (var k in lastImage) {
			var segX = k % divisionNum;
			var segY = Math.floor(k / divisionNum);
			canvas_ctx.drawImage(lastImage[k], dw * segX, dh * segY, dw, dh);
		}
		/*
		for (var idxY in elm_block.children) {
			var cY = elm_block.children[idxY].children;
			for (var idxX in cY) {
				var cX = cY[idxX].children;
				for (var idxI in cX) {
					cX[idxI].width = divWidth;
					cX[idxI].height = divHeight;
				}
			}
		}*/
	}

	function decodeBuffer(data) {
		receivedCount++;
		var arr = new Uint8Array(data, 0, MaxHeaderLength);
		var type = arr[0];

		var elapsedMs = Date.now() - initializedLocalDate;
		var receivedTotalMs = connectedServerDate + elapsedMs;
		var serverTotalMs = 0;
		var parentTotalMs = 0;
		for (var i = 0; i < 4; i++)
			serverTotalMs += (arr[i+1] << (8*i));
		for (var i = 0; i < 4; i++)
			parentTotalMs += (arr[i+5] << (8*i));
		for (var i = 0; i < 4; i++)
			arr[i+5] = (receivedTotalMs >> (8*i)) & 0xff;

		var di = receivedCount % AverageLength;
		serverDelay[di] = receivedTotalMs - serverTotalMs;
		parentDelay[di] = receivedTotalMs - parentTotalMs;

		serverDelayAvg = average(serverDelay.slice(0, clamp(receivedCount, 0, AverageLength)));
		parentDelayAvg = average(parentDelay.slice(0, clamp(receivedCount, 0, AverageLength)));

		setInfo("_sv", 4, serverDelayAvg);

		//setInfo("_sv", 4, ""+Date.now());
		if (!directReceive) setInfo("_pt", 5, parentDelayAvg);
		var segIdx = null;

		switch (type) {
			case DataType.FrameBuffer:
			segIdx = arr[9];
			setFrame(segIdx, data);

			break;

			case DataType.AudioBuffer:
			if (elm_audioCheckBox.checked)
				playAudio(data);

			break;
		}

		return [arr.buffer, parentDelayAvg];
	}

	function setFrame(segIdx, frameBuffer) {
		if (loadingQueue[segIdx] >= 1) return;

		var dataArr = new Uint8Array(frameBuffer, FrameHeaderLength);
		var blob = new Blob([dataArr], {type:'image/jpeg'});
		var url = urlCreator.createObjectURL(blob);

		var segX = segIdx % divisionNum;
		var segY = Math.floor(segIdx / divisionNum);

		var dw = elm_canvas.width / divisionNum;
		var dh = elm_canvas.height / divisionNum;
		var img = new Image();
		img.src = url;
		img.onload = function() {
			canvas_ctx.drawImage(img, dw * segX, dh * segY, dw, dh);
			urlCreator.revokeObjectURL(url);
			lastImage[segIdx] = img;
			loadingQueue[segIdx]--;
	  	}

		/*
		var elem = elm_block.children[segY].children[segX];

		elem.children[1].onload = function() {
			var newimg = elem.children[1];
			var oldimg = elem.children[0];
			newimg.style.zIndex = 10;
			oldimg.style.zIndex = 0;
			elem.insertBefore(newimg, oldimg);
			if (oldimg != null)
				urlCreator.revokeObjectURL(oldimg.src);
			loadingQueue[segIdx]--;
		}*/

		//elem.children[1].src = url;

		loadingQueue[segIdx]++;

		//latestSegmentData[segIdx] = frameBuffer.slice(0);
	}

	function playAudio(audioBuffer) {
		var audioSrc = audioCtx.createBufferSource();
		var audioBuf = audioCtx.createBuffer(audioIsStereo, audioArr.length, audioSampleRate);

		var audioArr = new Float32Array(audioBuffer, AudioHeaderLength);
		audioBuf.getChannelData(0).set(audioArr);

		audioSrc.buffer = audioBuf;
		audioSrc.connect(audioCtx.destination);
		audioSrc.start(audioTime);

		if (audioCtx.currentTime < audioTime)
			audioTime += audioBuf.duration;
		else
			audioTime = audioCtx.currentTime + audioBuf.duration;
	}

	function sendToServer(socket, type, id, targetId, data) {
		var data = {
			"type" : type,
			"id": id,
			"targetId": targetId,
			"data": data,
		};
		var json = JSON.stringify(data);
		socket.Send(json);
		debugLog("[send] type:"+data.type+",id:"+data.id+",target:"+data.targetId);
	}

	function onCheckPacket(data) {
		setDelayTimer();

		if (__DEBUG__ && !dCast) return;

		for (var k in peer_offer)
			peer_offer[k].send(data);
	}

	function setDelayTimer() {
		if (peerId != 0 && (peerId - 1) % BrunchCount != 0) return;

		clearDelayTimer();
		delayTimer = setTimeout(function(){ delayTimerFunc(); }, 1000 / framePerSecond + delayTimeout);
	}

	function clearDelayTimer() {
		if (delayTimer != null) clearTimeout(delayTimer);
		if (requestTimer != null) clearTimeout(requestTimer);
		delayTimer = null;
		requestTimer = null;
	}

	function delayTimerFunc() {
		if (!isCasting) return;

		for (var k in peer_offer)
			peer_offer[k].send(CheckPacketIdentifier);

		if (requestTimer == null)
			requestTimer = setTimeout(function(){ requestTimerFunc(); }, 1000 / framePerSecond + delayTimeout);
		delayTimer = setTimeout(function(){ delayTimerFunc(); }, 1000 / framePerSecond);
	}

	function requestTimerFunc() {
		if (directReceive) {
			console.log("send to server: req : -1");
			//sendToServer(socket, MessageType.RequestReconnect, peerId, -1, {});
			return;
		}

		debugLog("request reconnection : "+ peer_answer.offerId, "red");
		sendToServer(socket, MessageType.RequestReconnect, peerId, peer_answer.offerId, {});

		requestTimer = setTimeout(function(){ requestTimerFunc(); }, 1000 / framePerSecond + requestTimeout);
	}

	resizeCanvas(false);
	window.onresize = resizeCanvas;

	// WebRTC ------------------------------------------------------------
	var webRTCPeerConnection = (window.RTCPeerConnection || window.mozRTCPeerConnection || window.webkitRTCPeerConnection || window.msRTCPeerConnection);
	var webRTCSessionDescription = (window.RTCSessionDescription || window.mozRTCSessionDescription ||　window.webkitRTCSessionDescription || window.msRTCSessionDescription);
	var webRTCIceCandidate = (window.RTCIceCandidate || window.mozRTCIceCandidate ||　window.webkitRTCIceCandidate || window.msRTCIceCandidate);

	var pc_config = null;//{"iceServers":[]};

	var peer_child = [null, null];
	var peer_offer = [];
	var peer_answer = null;

	var dataChannel_offer = [];
	var dataChannel_answer = [];

	var peerId = null;
	var peerDepth = 0;
	var directReceive = false;

	var delayTimer = null;
	var requestTimer = null;
	var periodicallyInterval = null;
	var reconnectRequested = false;

	var settings = null;

	// Offer Side -----------------------------
	function webRTCPeerConnectionOffer(config, childIdx, id, answerId) {
		var inst = this;
		inst.childIdx = childIdx;
		inst.id = id;
		inst.answerId = answerId;
		inst.connection = new webRTCPeerConnection(config);
		inst.dataChannel = inst.connection.createDataChannel('dataChannel' + inst.answerId);

		inst.dataChannel.onopen = function(evt) { inst._onDataChannelOpen(evt); };
		inst.connection.onicecandidate = function(evt) { inst._onIceCandidate(evt); };
		inst.connection.oniceconnectionstatechange = function(evt) {
			setInfo("_ch"+inst.childIdx, 1, inst.dataChannel.readyState);
			setInfo("_ch"+inst.childIdx, 3, inst.connection.iceConnectionState); 
		};

		inst.connection.createOffer().then(function(sdp) {
			return inst.connection.setLocalDescription(sdp);
		}).then(function() {
			inst._onCreateOffer();
		}).catch(function(reason) {
			console.log(reason);debugLog(reason);
		});
	}

	webRTCPeerConnectionOffer.prototype = {
		send : function(data) {
			if(this.dataChannel.readyState != 'open') return;
			if (__DEBUG__ && !dCast) return;

			this.dataChannel.send(data);
		},
		sendChanks: function(chanks) {
			if(this.dataChannel.readyState != 'open') return;
			if (__DEBUG__ && !dCast) return;

			for (var k in chanks)
				this.dataChannel.send(chanks[k]);

			this.dataChannel.send("\0");
		},
		setRemoteDescription: function(sdp) {
			var inst = this;
			inst.connection.setRemoteDescription(
				new webRTCSessionDescription(sdp),
				function() { 
					setInfo("_ch"+inst.childIdx, 1, inst.dataChannel.readyState);
					setInfo("_ch"+inst.childIdx, 2, "completed");
					debugLog("setRemoteDescription succeeded.", "blue"); 
				},
				function() { debugLog("setRemoteDescription failed.", "red"); }
			);
		},
		addIceCandidate: function(iceCandidate) {
			this.connection.addIceCandidate(new webRTCIceCandidate(iceCandidate));
		},
		close: function() {
			this.connection.close();
		},

		_onCreateOffer: function() {
			sendToServer(socket, MessageType.SDPOffer, this.id, this.answerId, this.connection.localDescription);
			setInfo("_ch"+this.childIdx, 2, "offerring");
		},
		_onIceCandidate: function(evt) {
			debugLog("offer_icecandidate");
			if (evt.candidate) {
				sendToServer(socket, MessageType.ICECandidateOffer, this.id, this.answerId, evt.candidate);
				//console.dir(evt.candidate)
				//debugLog("send ICECandidateOffer to id: " + message.targetId);
			}
			else {
				//debugLog("icecandidate phase: " + evt.eventPhase);
			}
		},
		_onDataChannelOpen: function(evt) {
			this.dataChannel.binaryType = "arraybuffer";
			//debugLog("connection to offer(child) is opened. id : " + message.targetId);

			if (latestSegmentData.length != 0) {
				debugLog("send buffer to new user", "yellow");
				for (var k in latestSegmentData) {
					//console.dir(latestSegmentData[k]);
					//console.log(latestSegmentData);
					var chank = latestSegmentData[k].segmentation(ChankSize);
					this.sendChanks(chank);
				}
			}

			setInfo("_ch"+this.childIdx, 1, "opened");

			if (__DEBUG__) {
				dLabelOffer[this.answerId] = dLabelID.cloneNode(true);
				dLabelOffer[this.answerId].innerHTML = "child ID : " + this.answerId;
				dBody.appendChild(dLabelOffer[this.answerId]);
				debugLog("childID: "+this.answerId);
			}
		},
	}

	// Answer Side -----------------------------
	function webRTCPeerConnectionAnswer(pc_config, id) {
		var inst = this;
		inst.id = id;
		inst.dataChannel = null;
		inst.connection = new webRTCPeerConnection(pc_config);
		inst.connectionOffer = [];

		inst.connection.oniceconnectionstatechange = function(evt) {
			if (inst.dataChannel != null) 
				setInfo("_pt", 1, inst.dataChannel.readyState);
			setInfo("_pt", 3, inst.connection.iceConnectionState);
		};
	}

	webRTCPeerConnectionAnswer.prototype = {
		setRemoteDescription: function(remoteSdp) {
			var inst = this;
			inst.connection.setRemoteDescription(new webRTCSessionDescription(remoteSdp)).then(function() {
				inst._onSetRemoteDescription();
			})
			.catch(function(reason) {
				debugLog(reason, "red");
			});
		},
		addIceCandidate: function(iceCandidate) {
			this.connection.addIceCandidate(new webRTCIceCandidate(iceCandidate));
		},
		close: function() {
			this.connection.close();
		},

		_onSetRemoteDescription: function() {
			var inst = this;
			this.connection.onicecandidate = function (evt) {
				inst._onIceCandidate(evt);
			};

			this.connection.createAnswer().then(function(sdp) {
				return inst.connection.setLocalDescription(sdp);
			})
			.then(function() {
				inst._onCreateAnswer();
				if (inst.dataChannel != null)
					setInfo("_pt", 1, inst.dataChannel.readyState);
				setInfo("_pt", 2, "completed"); 
			})
			.catch(function(reason) {
				console.log(reason);debugLog(reason);
			});
		},
		_onCreateAnswer: function() {
			var inst = this;
			debugLog("setLocalDescription succeeded.");

			this.connection.ondatachannel = function(evt) {
				inst._onDataChannel(evt);
			};
			sendToServer(socket, MessageType.SDPAnswer, this.id, this.offerId, this.connection.localDescription);
		},
		_onIceCandidate: function(evt) {
			//debugLog("onicecandidate");

			if (evt.candidate) {
				sendToServer(socket, MessageType.ICECandidateAnswer, this.id, this.offerId, evt.candidate);
				//debugLog("send ICECandidateAnswer to id: " + message.id);
			}
			else {
				//debugLog("icecandidate phase: " + evt.eventPhase);
			}
		},
		_onDataChannel: function(evt) {
			var inst = this;
			this.dataChannel = evt.channel;
			this.dataChannel.binaryType = "arraybuffer";
			this.dataChannel.onmessage = function (evt) {
				inst._onDataChannelMessage(evt);
			};

			if (!isCasting)
				setLoader("Ready");
			else
				clearLoader();

			setInfo("_pt", 1, "opened");

			if (__DEBUG__) {
				dLabelAns[this.offerId] = dLabelID.cloneNode(true);
				dLabelAns[this.offerId].innerHTML = "parent ID : " + this.offerId;
				dBody.appendChild(dLabelAns[this.offerId]);
				debugLog("parentID: "+this.offerId);
			}
		},
		_onDataChannelMessage: function(evt) {
			//if (!isCasting) return;

			/*
			var isUpdateTimer = true;
			if (evt.data == "empty") {console.log("get empty");
				for (var key in this.connectionOffer)
					this.connectionOffer[key].sendChanks(evt.data);
			}
			else */

			if (evt.data == CheckPacketIdentifier) {
				onCheckPacket(evt.data);
			} else if (evt.data != "\0") {
				chanks.push(evt.data);
			}
			else {
				if (isCasting) {
					var data = concatenation(chanks);
					decodeBuffer(data);
					//isUpdateTimer = decodeBuffer(data)[2] == 0;

					for (var k in peer_offer)
						peer_offer[k].sendChanks(chanks);
				}

				chanks = [];
			}

			//setPeriodicallyTimer();
			//if (isUpdateTimer)  setDelayTimer(pc);
		},
	}

	// AudioBuffer -------------------------------------------------------
	var audioContext = (window.AudioContext || window.webkitAudioContext);
	var audioSamplerate = 4000;
	var audioIsStereo = false;
	var audioTime = 0;

	if (!AudioBufferSourceNode.prototype.start)
		AudioBufferSourceNode.prototype.start = AudioBufferSourceNode.prototype.node;


	// Support ------------------------------------------------------------
	if (!webRTCPeerConnection || !webRTCSessionDescription || !webRTCIceCandidate || !audioContext) {
		var language = (navigator.browserLanguage || navigator.language || navigator.userLanguage);
		var language2L = language ? language.substr(0,2) : "ja";

		var supportLabel = document.getElementById("_support_" + language2L);
		supportLabel.style.display = "block";

		return;
	}

	var audioCtx = new audioContext();


	// WebSocket ------------------------------------------------------------

	function closeAllConnection() {
		for (var k in peer_offer)
			peer_offer[k].close();

		if (peer_answer != null)
			peer_answer.close();

		peer_offer = [];
		peer_answer = null;
	}

	socket = new Alchemy({
		Server: location.hostname,
		Port: "8081",
		BinaryType: "arraybuffer",
		DebugMode: false,
	});

	socket.Connected = function() {
		setInfo("_sv", 1, "connected");
		//ntpInitialDelay = (Date.now() - connectTime) / 2;

		/*
		if (__INFO__)
		{
			var ntpRequest = new NtpRequest("http://calc.cis.ibaraki.ac.jp/time.php");
			ntpRequest.onResponse = function(res) {
				ntpInitialDelay = (ntpRequest.responseTime - ntpRequest.requestTime) / 2;
				var local = Date.now();
				var	net = new Date(res * 1000 - ntpInitialDelay);
				//var utc = net.getTime() + net.getTimezoneOffset()*60*1000;
				initializedLocalDate = local;
				initializedNetworkTime = totalMillisecond(net);
				console.log("initializedLocalDate:"+initializedLocalDate);
				console.log("initializedNetworkTime:"+initializedNetworkTime);
				setInfo("_sv", 5, ntpInitialDelay);
			};
			ntpRequest.begin();
		}*/
	}

	socket.Disconnected = function() {
		setInfo("_sv", 1, "disconnected");
		debugLog("disconnected", "blue");
	};

	socket.MessageReceived = function(event) {
		setInfo("_sv", 3, nowTime());
		if (typeof event.data == "string") {
			if (event.data == CheckPacketIdentifier) {
				onCheckPacket(event.data);
				return;
			}

			var message = JSON.parse(event.data);
			switch (message.type) {
				case MessageType.Connected:
				connectedServerDate = message.data;
				initializedLocalDate = new Date();
				setId(message.id);
				directReceive = (message.id == 0);

				if (directReceive) {
					if (!isCasting) setLoader("Ready");
					else clearLoader();
				}
				else {
					peer_answer = new webRTCPeerConnectionAnswer(pc_config, message.id);
				}

				setInfo("_sv", 0, message.id);

				debugLog("connected. my id: " + message.id, "blue");
				if (__DEBUG__) dLabelID.innerHTML = "ID : " + message.id;
				break;


				case MessageType.UpdateID:
				setId(message.id);
				peer_answer.id = message.id;
				directReceive = (message.id == 0);
				setInfo("_sv", 0, message.id);

				clearDelayTimer();
				debugLog("update id. my id: " + message.id, "blue");
				if (__DEBUG__) dLabelID.innerHTML = "ID : " + message.id;
				break;


				case MessageType.PeerConnection: // new peer connected
				var targetId = message.targetId;
				if (peer_offer[targetId] != null) break;

				var childIdx = peer_child[0] == null ? 0 : 1;
				peer_offer[targetId] = new webRTCPeerConnectionOffer(pc_config, childIdx, peerId, targetId);
				peer_child[childIdx] = peer_offer[targetId];

				if (peer_answer !== null)
					peer_answer.connectionOffer[targetId] = peer_offer[targetId];

				setInfo("_ch"+childIdx, 0, targetId);

				debugLog("new user connected. id: " + message.targetId, "blue");
				//dButton.onclick = function(){console.dir(peer_offer[message.targetId]);}
				//setInterval(function(){console.dir(peer_offer[message.targetId]);},3000);
				//debugLog("webRTC peer count: " + peerConnectionCount);
				break;

				case MessageType.SDPOffer: // SDP Offer Received
				peer_answer.offerId = message.id;
				peer_answer.setRemoteDescription(message.data);
				setInfo("_pt", 0, message.id);
				debugLog("received SDPOffer from id: " + message.id, "blue");
				//dButton.onclick = function(){console.dir(peer_answer);}
				break;


				case MessageType.SDPAnswer: // SDP Answer Received
				peer_offer[message.id].setRemoteDescription(message.data);
				debugLog("received answer from id: " + message.id, "green");
				break;


				case MessageType.ICECandidateOffer: // ICECandidate Received from Offer
				peer_answer.addIceCandidate(message.data);
				debugLog("received ICECandidate from offer id: " + message.id, "blue");
				break;


				case MessageType.ICECandidateAnswer: // ICECandidate Received from Answer
				peer_offer[message.id].addIceCandidate(message.data);
				debugLog("received ICECandidate from answer id: " + message.id, "green");
				break;


				case MessageType.RemoveOffer: // Remove Offer
				var childIdx = peer_child[0] == peer_offer[message.id] ? 0 : 1;
				if (peer_offer[message.id]) peer_offer[message.id].close();
				peer_child[childIdx] = null;
				delete peer_offer[message.id];


				setInfo("_ch"+childIdx, [0,1,2,3], "-");

				debugLog("remove offer id: " + message.id, "yellow");
				break;


				case MessageType.RemoveAnswer: // Remove Answer
				if (peer_answer) peer_answer.close();
				peer_answer = new webRTCPeerConnectionAnswer(pc_config, peerId);


				setInfo("_pt", [0,1,2,3,4,5], "-");

				debugLog("remove answer id: " + message.id, "yellow");
				break;


				case MessageType.Settings:
				updateSettings(message.data);
				resizeCanvas();

				//settings = clone(message.data);
				debugLog("setting received", "blue");
				break;


				case MessageType.StartCasting:
				isCasting = true;
				receivedCount = 0;
				setDelayTimer();
				clearLoader();
				setInfo("_sv", 2, "yes");
				break;


				case MessageType.StopCasting:
				isCasting = false;
				setLoader("Ready");
				setInfo("_sv", 2, "no");

				clearDelayTimer();
				break;


				case MessageType.RequestReconnect:
				closeAllConnection();
				clearDelayTimer();

				debugLog("accept reconnection", "red");
				dCast = true;
				break;


				case MessageType.Disconnect:
				closeAllConnection();
				clearDelayTimer();

				socket.Stop();
				socket.Start();

				setLoader("Connecting...");
				debugLog("accept reconnection", "red");
				dCast = true;
				break;
			}

			debugLog("[recv] type:"+message.type+",id:"+message.id+",target:"+message.targetId);
		}
		else if (directReceive && isCasting) {
			var modified = decodeBuffer(event.data)[0];
			var chank = modified.segmentation(ChankSize);
			for (var k in peer_offer)
				peer_offer[k].sendChanks(chank);

			//setDelayTimer(null);
		}
	};

	socket.Error = function(evt) {
		document.location.reload(true);
	};

	setLoader("Connecting...");
	setInfo("_sv", [0,3,4,5], "-");
	setInfo("_sv", 1, "connecting");
	setInfo("_sv", 2, "no");

	connectTime = Date.now();

	socket.Start();

	//---------------------------------------------------------------------------

	if (!__INFO__)
		document.getElementById("_info").style.display = "none";
}