function onLoad()
{
	var __DEBUG__ = true;

	if (__DEBUG__)
	{
		var dLabelID = document.createElement("p");
		var dLabelOffer = [];
		var dLabelAns = [];
		var dBody = document.getElementById("_body");
		var dLabelLog = document.createElement("p");
		dBody.appendChild(dLabelLog);
		dBody.appendChild(dLabelID);
	}

	// Application ------------------------------------------------------------
	var audioCheckBox = document.getElementById("_audio");
	var block = document.getElementById("_block");
	
	var webRTCPeerConnection = (window.RTCPeerConnection || window.mozRTCPeerConnection || window.webkitRTCPeerConnection || window.msRTCPeerConnection);
	var webRTCSessionDescription = (window.RTCSessionDescription || window.mozRTCSessionDescription ||　window.webkitRTCSessionDescription || window.msRTCSessionDescription);
	var webRTCIceCandidate = (window.RTCIceCandidate || window.mozRTCIceCandidate ||　window.webkitRTCIceCandidate || window.msRTCIceCandidate);
	var audioContext = (window.AudioContext || window.webkitAudioContext);

	if (!webRTCPeerConnection　|| !webRTCSessionDescription || !webRTCIceCandidate || !audioContext)
	{
		var language = (navigator.browserLanguage || navigator.language || navigator.userLanguage);
		var language2L = language ? language.substr(0,2) : "ja";

		var supportLabel = document.getElementById("_support_" + language2L);
		supportLabel.style.display = "block";
		return;
	}

	// Def ------------------------------------------------------------
	var ChankSize = 1024 * 32;
	var FrameHeaderLength = 0x02;
	var AudioHeaderLength = 0x04;

	// server <-> client messages(WebSocket)
	var MessageType = 
	{
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
		"StartCapture"			: 0xa,
		"StopCapture"			: 0xb,
		"ReceiveTimeout"		: 0xc,
		"Disconnect"			: 0xd,
	};

	// server <-> client data type(WebRTC)
	var DataType = 
	{
		"FrameBuffer" : 0x0,
		"AudioBuffer" : 0x1,
	};

	// WebRTC ------------------------------------------------------------
	var pc_config = null;//{"iceServers":[]};

	var peer_offer = [];
	var peer_answer = [];

	var dataChannel_offer = [];
	var dataChannel_answer = [];

	var peerID = null;
	var peerConnectionCount = 0;
	var directReceive = false;

	var settings = null;

	// FrameBuffer -------------------------------------------------------
	var urlCreator = (window.URL || window.webkitURL);

	var frameBuffer = new Uint8Array();
	var intraImageData = [];

	var canvasLocking = false;
	var isCapturing = false;
	
	var divisionNum;
	var loadingQueue;

	var latestSegmentData = [];

	var RTCReceiveTimeout = 1000;

	// AudioBuffer -------------------------------------------------------
	var audioCtx = new audioContext();
	var audioSamplerate = 4000;
	var audioIsStereo = false;
	var audioTime = 0;

	if (!AudioBufferSourceNode.prototype.start)
		AudioBufferSourceNode.prototype.start = AudioBufferSourceNode.prototype.node;

	// utility ------------------------------------------------------------
	if (!ArrayBuffer.prototype.slice)
	{
		ArrayBuffer.prototype.slice = function (start, end) {
			var that = new Uint8Array(this);
			if (end == undefined) end = that.length;
			var result = new ArrayBuffer(end - start);
			var resultArray = new Uint8Array(result);
			for (var i = 0; i < resultArray.length; i++)
			   resultArray[i] = that[i + start];
			return result;
		}
	}

	// WebSocket ------------------------------------------------------------
	socket = new Alchemy(
	{ 
		Server: location.hostname,
		Port: "8081",
		BinaryType: "arraybuffer",
		DebugMode: false
	});

	socket.Disconnected = function()
	{
		debugLog("disconnected");
	};

	socket.MessageReceived = function(event)
	{
		if (typeof event.data == "string")
		{
			var message = JSON.parse(event.data);

			switch (message.type)
			{
				case MessageType.Connected:
				debugLog("connected. my id: " + message.id);
				peerID = message.id;
				directReceive = (message.id == 0);
				if (__DEBUG__) dLabelID.innerHTML = "ID : " + peerID;
				break;

				case MessageType.UpdateID:
				debugLog("update id. my id: " + message.id);
				peerID = message.id;
				directReceive = (message.id == 0);
				if (__DEBUG__) dLabelID.innerHTML = "ID : " + peerID;
				break;

				case MessageType.PeerConnection: // new peer connected
				debugLog("new user connected. id: " + message.targetId);
				peer_offer[message.targetId] = new webRTCPeerConnection(pc_config);
				dataChannel_offer[message.targetId] = peer_offer[message.targetId].createDataChannel('dataChannel' + message.targetId);
				dataChannel_offer[message.targetId].onopen = function(evt)
				{
					dataChannel_offer[message.targetId].binaryType = "arraybuffer";
					debugLog("connection to offer(child) is opened. id : " + message.targetId);

					if (latestSegmentData.length > 0)
					{
						debugLog("send buffer to new user");
						for (var i = 0; i < latestSegmentData.length; i++)
						{
							var chank = segmentation(latestSegmentData[i], ChankSize);
							sendData(dataChannel_offer[message.targetId], chank);
						}
					}

					if (__DEBUG__) 
					{
						dLabelOffer[message.targetId] = dLabelID.cloneNode(true);
						dLabelOffer[message.targetId].innerHTML = "child ID : " + message.targetId;
						dBody.appendChild(dLabelOffer[message.targetId]);
						debugLog("childID: "+message.targetId);
					}
				};

				peer_offer[message.targetId].onicecandidate = function (evt)
				{
					if (evt.candidate)
					{
						var data = 
						{
							"type" : MessageType.ICECandidateOffer, 
							"id": peerID, 
							"targetId": message.targetId, 
							"data": evt.candidate,
						};
						var json = JSON.stringify(data);
						socket.Send(json);

						//debugLog("send ICECandidateOffer to id: " + message.targetId);
					}
					else
					{
						//debugLog("icecandidate phase: " + evt.eventPhase);
					}
				};

				peer_offer[message.targetId].createOffer(
					function(sdp)
					{
						//debugLog("createOffer succeeded.");

						peer_offer[message.targetId].setLocalDescription(
							sdp,
							function()
							{
								var data = 
								{
									"type" : MessageType.SDPOffer, 
									"id": peerID, 
									"targetId": message.targetId, 
									"data": sdp,
								};
								var json = JSON.stringify(data);
								socket.Send(json);

								//debugLog("send SDPOffer to id: " + message.targetId);
							},
							function() { debugLog("setLocalDescription failed."); });
					},
					function() { debugLog("createOffer failed."); });

				peerConnectionCount++;
				//debugLog("webRTC peer count: " + peerConnectionCount);
				break;

				case MessageType.SDPOffer: // SDP Offer Received
				//debugLog("received SDPOffer from id: " + message.id);
				peer_answer[message.id] = new webRTCPeerConnection(pc_config);
				peer_answer[message.id].setRemoteDescription(
					new webRTCSessionDescription(message.data),
					function()
					{
						peer_answer[message.id].onicecandidate = function (evt)
						{
							//debugLog("onicecandidate");

							if (evt.candidate)
							{
								var data = 
								{
									"type" : MessageType.ICECandidateAnswer, 
									"id": peerID, 
									"targetId": message.id, 
									"data": evt.candidate,
								};
								var json = JSON.stringify(data);
								socket.Send(json);

								//debugLog("send ICECandidateAnswer to id: " + message.id);
							}
							else
							{
								//debugLog("icecandidate phase: " + evt.eventPhase);
							}
						};
						peer_answer[message.id].createAnswer(
							function(sdp)
							{
								//debugLog("createAnswer succeeded.");

								peer_answer[message.id].setLocalDescription(
									sdp, 
									function()
									{
										//debugLog("setLocalDescription succeeded.");

										peer_answer[message.id].ondatachannel = function(evt)
										{
											var chank = [];

											dataChannel_answer[message.id] = evt.channel;
											dataChannel_answer[message.id].binaryType = "arraybuffer";
											dataChannel_answer[message.id].onmessage = function (event)
											{
												if (event.data != "\0")
													chank.push(event.data);
												else
												{
													if (isCapturing)
													{
														var data = concatenation(chank);

														decodeBuffer(data);

														for (var key in dataChannel_offer)
															sendData(dataChannel_offer[key], chank);
													}

													chank = [];
												}
											};

											if (__DEBUG__) 
											{
												dLabelAns[message.id] = dLabelID.cloneNode(true);
												dLabelAns[message.id].innerHTML = "parent ID : " + message.id;
												dBody.appendChild(dLabelAns[message.id]);
												debugLog("parentID: "+message.id);
											}
										};

										var data = 
										{
											"type" : MessageType.SDPAnswer, 
											"id": peerID, 
											"targetId": message.id, 
											"data": sdp,
										};
										var json = JSON.stringify(data);
										socket.Send(json);

										//debugLog("send SDPAnswer to id: " + message.id);
									},
									function() { debugLog("setLocalDescription failed."); });
							},
							function() { debugLog("createAnswer failed."); });
					},
					function() { debugLog("setRemoteDescription failed."); });
				break;

				case MessageType.SDPAnswer: // SDP Answer Received
				//debugLog("received answer from id: " + message.id);
				peer_offer[message.id].setRemoteDescription(
					new webRTCSessionDescription(message.data),
					function() { debugLog("setRemoteDescription succeeded."); },
					function() { debugLog("setRemoteDescription failed."); });
				break;

				case MessageType.ICECandidateOffer: // ICECandidate Received from Offer
				//debugLog("received ICECandidate from offer id: " + message.id);
				peer_answer[message.id].addIceCandidate(new webRTCIceCandidate(message.data));
				break;

				case MessageType.ICECandidateAnswer: // ICECandidate Received from Answer
				//debugLog("received ICECandidate from answer id: " + message.id);
				peer_offer[message.id].addIceCandidate(new webRTCIceCandidate(message.data));
				break;

				case MessageType.RemoveOffer: // Remove Offer
				debugLog("remove offer id: " + message.id);
				peer_offer[message.id].close();
				peer_offer[message.id] = null;
				if (__DEBUG__) {dBody.removeChild(dLabelOffer[message.id])}
				break;

				case MessageType.RemoveAnswer: // Remove Answer
				debugLog("remove answer. id: " + message.id);
				peer_answer[message.id].close();
				peer_answer[message.id] = null;
				if (__DEBUG__) {dBody.removeChild(dLabelAns[message.id])}
				break;

				case MessageType.Settings:
				var dn = divisionNum;

				audioSampleRate = message.data.audioSettingData.sampleRate;
				audioIsStereo = message.data.audioSettingData.isStereo ? 2 : 1;

				divisionNum = message.data.captureSettingData.divisionNum;

				while (block.firstChild)
					block.removeChild(block.firstChild);

				for (var row = 0; row < divisionNum; row++)
				{
					var rowElem = document.createElement("div");
					block.appendChild(rowElem);
				
					for (var cell = 0; cell < divisionNum; cell++)
					{
						var cellElem = document.createElement('div');
						var base = document.createElement('img');
						var cover = document.createElement('img');
						cover.className = "cover";
						cellElem.appendChild(base);
						cellElem.appendChild(cover);
						rowElem.appendChild(cellElem);
					}
				}

				loadingQueue = Array.apply(null, Array(divisionNum * divisionNum)).map(function () { return 0 });

				settings = clone(message.data);
				break;

				case MessageType.StartCapture:
				isCapturing = true;
				break;

				case MessageType.StopCapture:
				isCapturing = false;
				break;

				case MessageType.ReceiveTimeout:
				break;

				case MessageType.Disconnect:
				socket.Stop();
				break;
			}
		}
		else if (isCapturing)
		{
			decodeBuffer(event.data);

			var chank = segmentation(event.data, ChankSize);

			for (var key in dataChannel_offer)
				sendData(dataChannel_offer[key], chank);
		}

		function decodeBuffer(data)
		{
			var arr = new Uint8Array(data);
			var type = arr[0];

			switch (type)
			{
				case DataType.FrameBuffer:
				var segIdx = arr[1];

				if (loadingQueue[segIdx] >= 1)
					break;

				var dataArr = new Uint8Array(data, FrameHeaderLength);
				var blob = new Blob([dataArr], {type:'image/jpeg'});
				var url = urlCreator.createObjectURL(blob);

				var segX = segIdx % divisionNum;
				var segY = Math.floor(segIdx / divisionNum);

				var elem = block.children[segY].children[segX];

				elem.children[1].onload = function()
				{
					elem.insertBefore(elem.children[1], elem.children[0]);

					elem.children[0].className = "";
					elem.children[1].className = "cover";
					
					if (elem.children[1] != null)
						urlCreator.revokeObjectURL(elem.children[1].src);

					loadingQueue[segIdx]--;
				}

				elem.children[1].src = url;
				loadingQueue[segIdx]++;

				latestSegmentData[segIdx] = data.slice(0);
				break;

				case DataType.AudioBuffer:
				if (!audioCheckBox.checked) 
					break;

				var audioArr = new Float32Array(data, AudioHeaderLength);
				var audioBuf = audioCtx.createBuffer(audioIsStereo, audioArr.length, audioSampleRate);
				var audioSrc = audioCtx.createBufferSource();

				var audioBufArr = audioBuf.getChannelData(0);
				audioBufArr.set(audioArr);

				audioSrc.buffer = audioBuf;
				audioSrc.connect(audioCtx.destination);

				if (audioCtx.currentTime < audioTime)
				{
					audioSrc.start(audioTime);
					audioTime += audioBuf.duration;
				}
				else
				{
					audioSrc.start(audioTime);
					audioTime = audioCtx.currentTime + audioBuf.duration;
				}

				break;
			}
		}

		function sendData(offer, chank)
		{
			if(offer.readyState != 'open') 
				return;

			for (var i = 0; i < chank.length; i++)
				offer.send(chank[i]);

			offer.send("\0");
		}

		function segmentation(arrayBuffer, segmentSize)
		{
			var segments= [];

			for(var i = 0; i * segmentSize < arrayBuffer.byteLength; i++)
				segments.push(arrayBuffer.slice(i * segmentSize, (i + 1) * segmentSize));

			return segments;
		}

		function concatenation(segments)
		{
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

		function clone(src)
		{
			var dst = {}
			
			for(var k in src)
				dst[k] = src[k];

			return dst;
		}
	};

	socket.Start();

	function debugLog(log)
	{
		if (__DEBUG__)
		{
			dLabelLog.innerHTML += "<br>"+log;
			//console.log(log);
		}
	}
}




