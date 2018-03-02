/**
 * @fileoverview Client side for ScreenShare
 * @preserve © 2015 Daiki Ito
 */

/**
 * スクリプト本体
 */
function onLoad() {
	/**
	 * アプリケーション情報を表示する
	 * @const {boolean}
	 */
	const __INFO__ = true;

	/**
	 * アプリケーションデバッグを有効にする
	 * @const {boolean}
	 */
	const __DEBUG__ = true;

	// 状態要素を準備する
	if (__INFO__) {
		var setInfo = (pt, ch, text) => {
			if (!Array.isArray(ch)) ch = [ch];
			for (let k in ch) {
				let dom = document.getElementById(pt).children[0].children[ch[k]];
				if (ch[k] == 0) dom.children[2].innerHTML = text;
				else dom.children[1].innerHTML = text;
			}
		}
	}
	else {
		var setInfo = (pt, ch, text) => {};
	}

	// デバッグ用要素を準備する
	if (__DEBUG__) {
		var d_LabelID = document.createElement(`p`);
		var d_LabelOffer = [];
		var d_LabelAns = [];
		var d_Body = document.getElementById(`_body`);
		var d_DivLog = document.createElement(`div`);
		var d_TableLog = document.createElement(`table`);
		var d_LabelChkboxCast = document.createElement(`p`);
		var d_LabelChkboxReceive = document.createElement(`p`);
		var d_LabelChkboxSegment = document.createElement(`p`);
		var d_LabelChkboxRectangle = document.createElement(`p`);
		var d_CheckboxCast = document.createElement(`input`);
		var d_CheckboxReceive = document.createElement(`input`);
		var d_CheckboxSegment = document.createElement(`input`);
		var d_CheckboxRectangle = document.createElement(`input`);

		d_DivLog.style = `overflow-y:scroll;`;
		d_TableLog.border = 1;
		//d_TableLog.innerHTML = `<th><td>Tag</td><td>Log</td></th>`

		d_LabelChkboxCast.innerHTML = `Casting`;
		d_LabelChkboxReceive.innerHTML = `Receiving`;
		d_LabelChkboxSegment.innerHTML = `DrawCastedSegmentsOnly`;
		d_LabelChkboxRectangle.innerHTML = `DrawCastedSegmentsRectangle`;
		d_CheckboxCast.type = d_CheckboxReceive.type = d_CheckboxSegment.type = d_CheckboxRectangle.type = `checkbox`;
		d_CheckboxCast.checked = d_CheckboxReceive.checked = true;

		d_Body.appendChild(d_LabelID);
		d_Body.appendChild(d_DivLog);

		d_DivLog.appendChild(d_TableLog);

		d_LabelChkboxCast.appendChild(d_CheckboxCast);
		d_LabelChkboxReceive.appendChild(d_CheckboxReceive);
		d_LabelChkboxSegment.appendChild(d_CheckboxSegment);
		d_LabelChkboxRectangle.appendChild(d_CheckboxRectangle);

		d_Body.appendChild(d_LabelChkboxCast);
		d_Body.appendChild(d_LabelChkboxReceive);
		d_Body.appendChild(d_LabelChkboxSegment);
		d_Body.appendChild(d_LabelChkboxRectangle);

		var d_log = (tag, log, col = `black`) => {
			// var row = d_TableLog.insertRow(-1);
			// row.insertCell(-1).innerHTML = `<font color=${col}>${tag}</font>`;
			// row.insertCell(-1).innerHTML = log;
			//d_TableLog.innerHTML += `<font color=${col}><tr><td>${tag}</td><td>${log}</td></tr>`;
			// cconsole.trace();
			console.log(`%c:[${tag}]${Array(10-tag.length + 1).join(' ')}${log}`, `color : ${col}`);
		}
	}
	else {
		var d_log = (tag, log, col = `black`) => {}
	}

	// Utility ------------------------------------------------------------

	/**
	 * ArrayBufferの分割
	 * @param {number} segmentSize - 分割バイト数
	 * @return {Array} 分割された配列
	 */
	if (!ArrayBuffer.prototype.segmentation) {
		ArrayBuffer.prototype.segmentation = function(segmentSize) {
			if (this.byteLength <= segmentSize)
				return [this];

			let segments = [];

			for(let i = 0; i * segmentSize < this.byteLength; i++)
				segments.push(this.slice(i * segmentSize, (i + 1) * segmentSize));

			return segments;
		}
	}

	/**
	 * Uint32Arrayの数値の平均値
	 * @return {number} 平均値
	 */
	if (!Int32Array.prototype.average) {
		Int32Array.prototype.average = function() {
			if (this.length <= 1)
				return this.length == 0 ? 0 : this[0];

			let sum = this.reduce((pre, cur, index, array) => pre + cur);

			return sum / this.length;
		}
	}

	/**
	 * Canavsをblobに落とし込む({@link https://developer.mozilla.org/ja/docs/Web/API/HTMLCanvasElement/toBlob ポリフィル})
	 * @param {function(Blob)} callback - コールバック関数
	 * @param {string} type - 画像のmime type
	 * @param {number} quality - 画像の品質
	 */
	if (!HTMLCanvasElement.prototype.toBlob) {
		Object.defineProperty(HTMLCanvasElement.prototype, `toBlob`, {
			value: function (callback, type, quality) {
				let binStr = atob( this.toDataURL(type, quality).split(`,`)[1] ),
				len = binStr.length,
				arr = new Uint8Array(len);

				for (let i=0; i<len; i++ ) {
					arr[i] = binStr.charCodeAt(i);
				}

				callback( new Blob( [arr], {type: type || `image/png`} ) );
  			}
	 	});
	}

	/**
	 * 音声再生関数を同一にする
	 */
	if (!AudioBufferSourceNode.prototype.start)
		AudioBufferSourceNode.prototype.start = AudioBufferSourceNode.prototype.node;

	/**
	 * ArrayBufferの結合
	 * @param {ArrayBuffer[]} segments - 連結したいArrayBuffer配列
	 * @return {ArrayBuffer} 結合したArrayBuffer
	 */
	function concatenation(segments) {
		let sumLength = 0;

		for(let i = 0; i < segments.length; i++)
			sumLength += segments[i].byteLength;

		let whole = new Uint8Array(sumLength);
		let pos = 0;

		for(let i = 0; i < segments.length; i++)
		{
			whole.set(new Uint8Array(segments[i]), pos);
			pos += segments[i].byteLength;
		}

		return whole.buffer;
	}

	/**
	 * 数値を範囲内に固定する
	 * @param {number} num - 固定したい数値
	 * @param {number} min - 最低値
	 * @param {number} max - 最高値
	 * @return {number} 範囲内に固定されたnum
	 */
	function clamp(num, min, max) {
		return (num > max ? max : (num < min ? min : num));
	}

	/**
	 * 大きい数値を返す
	 * @param {number} a - 数値1
	 * @param {number} b - 数値2
	 * @param {number} 大きいほうの数値
	 */
	function max(a, b) {
		return a > b ? a : b;
	}

	/**
	 * 現在時刻を`hh:mm:ss`で返す
	 * @return {string} 現在時刻
	 */
	function nowTime() {
		let now = new Date();
		return `${now.getHours()}:${now.getMinutes()}:${now.getSeconds()}`;
	}

	function nowTimeM(date) {
		return `${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}:${date.getMilliseconds()}`;
	}

	// Constants ------------------------------------------------------------

	/**
	 * サーバが管理しているクライアント木構造の枝数(=子ノード数)
	 * @const {number}
	 */
	const BranchCount = 2;

	/**
	 * サーバから送信されるデータ内の`TimeChunk`のバイト長
	 * @const {number}
	 */
	const Header_TimeChunkLength = 0x08;

	/**
	 * サーバから送信されるデータ内の`BodyChunk`のバイト長
	 * @const {number}
	 */
	const Header_BodyChunkLength = 0x05;

	/**
	 * サーバから送信されるデータのヘッダー長
	 * @const {number}
	 */
	const HeaderLength = Header_TimeChunkLength + Header_BodyChunkLength;

	/**
	 * 親ノードから送信されるCCPのタイムアウト期間
	 * @const {number}
	 */
	const CCPDelayTimeout = 1000;

	/**
	 * 遅延平均タイムアウト間隔
	 * @const {number}
	 */
	const DelayAverageTimeout = 2000;

	/**
	 * ディレイの時間平均窓数
	 * @const {string}
	 */
	const DelayAverageWindowCount = 5;

	/**
	 * サーバ、親ノードから送信される通信継続確認用メッセージ
	 * @const {string}
	 */
	const CheckContinuesMessage = `cp`;

	/**
	 * サーバから送信されるメッセージ型
	 * @enum {string}
	 */
	const MessageType = {
		Connected			: 0x0,
		UpdateID			: 0x1,
		PeerConnection		: 0x2,
		SDPOffer			: 0x3,
		SDPAnswer			: 0x4,
		ICECandidateOffer	: 0x5,
		ICECandidateAnswer	: 0x6,
		RemoveOffer			: 0x7,
		RemoveAnswer		: 0x8,
		Settings			: 0x9,
		StartCasting		: 0xa,
		StopCasting			: 0xb,
		Report				: 0xc,
		Reset				: 0xd,
		Disconnect			: 0xe,
	};

	/**
	 * サーバから送信されるデータ型
	 * @enum {string}
	 */
	const DataType = {
		FrameBuffer : 0x0,
		AudioBuffer : 0x1,
	};

	/**
	 * 画像と音声のデータチャネル
	 * @const {string}
	 */
	const WebRTCDataChannel_Frame = `dataChannel-frame`;

	/**
	 * 通信持続確認用のデータチャネル
	 * @const {string}
	 */
	const WebRTCDataChannel_CCP = `dataChannel-ccp`;

	/**
	 * キャンバス画像送受信用のデータチャネル
	 * @const {string}
	 */
	const WebRTCDataChannel_CanvasImage = `dataChannel-canvasImage`;

	/**
	 * WebPの圧縮品質
	 * @const {number}
	 */
	const WebPQuality = 0.4;

	/**
	 * タイトル
	 * @const {string}
	 */
	const DocumentTitle = `ScreenShare`;

	// Application ------------------------------------------------------------

	/**
	 * クロスブラウザクラス
	 * @type {object}
	 */
	let urlCreator = (window.URL || window.webkitURL);
	let webRTCPeerConnection = (window.RTCPeerConnection || window.mozRTCPeerConnection || window.webkitRTCPeerConnection || window.msRTCPeerConnection);
	let webRTCSessionDescription = (window.RTCSessionDescription || window.mozRTCSessionDescription ||　window.webkitRTCSessionDescription || window.msRTCSessionDescription);
	let webRTCIceCandidate = (window.RTCIceCandidate || window.mozRTCIceCandidate || window.webkitRTCIceCandidate || window.msRTCIceCandidate);
	let audioContext = (window.AudioContext || window.webkitAudioContext);

	// WebRTCやAudioContextのサポートを確認する
	if (!webRTCPeerConnection || !webRTCSessionDescription || !webRTCIceCandidate || !audioContext) {
		let language = (navigator.browserLanguage || navigator.language || navigator.userLanguage);
		let language2L = language ? language.substr(0,2) : `ja`;

		let supportLabel = document.getElementById(`_support_${language2L}`);
		supportLabel.style.display = `block`;

		return;
	}

	/**
	 * DOM
	 * @type {object}
	 */
	let elm_frame = document.getElementById(`_frame`);			// キャンバスをラップするフレーム
	let elm_canvas = document.getElementById(`_canvas`);		// キャンバス
	let elm_audioCheckBox = document.getElementById(`_audio`);	// 音声スイッチ
	let elm_idview = document.getElementById(`_idview`);		// ID表示枠
	let elm_loader = document.getElementById(`_loader`);		// ローディングアイコン
	let elm_phase = document.getElementById(`_phase`);			// ローディングのテキスト
	let elm_info = document.getElementById(`_info`);			// コネクションステータス

	/**
	 * キャンバスコンテキスト
	 * @type {object}
	 */
	let canvas_ctx = elm_canvas.getContext(`2d`);

	/**
	 * 配信中かどうか
	 * @type {boolean}
	 */
	let isCasting = false;

	/**
	 * キャンバスのアスペクト比率
	 * @type {number}
	 */
	let aspectRatio = 1.5;

	/**
	 * キャンバスサイズ
	 * @type {object}
	 */
	let canvasSize = {};

	/**
	 * 配信フレームレート
	 * @type {number}
	 */
	let framePerSecond = 10;

	/**
	 * フレームデータを受信した回数
	 * @type {number}
	 */
	let receivedCount = 0;

	/**
	 * サーバに接続開始した時のローカル時刻
	 * @type {number}
	 */
	let connectTime;

	/**
	 * サーバに接続した時のローカル時刻
	 * @type {number}
	 */
	let connectedLocalDate;

	/**
	 * サーバに接続した時のサーバ時刻
	 * @type {number}
	 */
	let connectedServerDate = null;

	/**
	 * 時間平均を計るためにサーバとwebアプリとの遅延を格納する配列
	 * @type {Int32Array[]}
	 */
	let serverDelay = new Int32Array(DelayAverageWindowCount);

	/**
	 * 時間平均を計るために親ノードとwebアプリとの遅延を格納する配列
	 * @type {Int32Array[]}
	 */
	let parentDelay = new Int32Array(DelayAverageWindowCount);

	/**
	 * CCPのタイムアウトを計るためのタイマー
	 * @type {object}
	 */
	let delayTimer = null;

	/**
	 * 連続タイムアウトを計るためのタイマー
	 * @type {object}
	 */
	let reportTimer = null;

	/**
	 * キャンバスに描画されたかどうか
	 * @type {boolean}
	 */
	let reported = false;
	/**
	 * キャンバスに描画されたかどうか
	 * @type {boolean}
	 */
	let canvasDrawn = false;

	/**
	 * WebRTCの設定用オブジェクト
	 * @type {object}
	 */
	let pcConfigures = null;//{`iceServers`:[]};

	/**
	 * 子ノードのWebRTCピア
	 * IDを添え字にする連想配列
	 * @type {Array}
	 */
	let peerChildren = [];

	/**
	 * 親ノードのWebRTCピア
	 * @type {Array}
	 */
	let peerParent = null;

	/**
	 * 自ノードID
	 * @type {number}
	 */
	let peerId;

	/**
	 * 自ノードの二分木上の深さ
	 * @type {number}
	 */
	let peerDepth;

	/**
	 * サーバから直にフレームデータを受け取るかどうか
	 * @type {boolean}
	 */
	let directReceive = false;

	/**
	 * 再接続要求を出したかどうか
	 * @type {boolean}
	 */
	let reconnectRequested = false;

	/**
	 * 音声再生コンテキスト
	 * @type {AudioContext}
	 */
	//let audioCtx = new audioContext();

	/**
	 * 音声再生サンプルレート
	 * @type {number}
	 */
	let audioSamplerate = 4000;

	/**
	 * 音声サンプルがステレオかどうか
	 * @type {boolean}
	 */
	let audioIsStereo = false;

	/**
	 * 音声再生時間
	 * @type {number}
	 */
	let audioPlaybackTime = 0;


/*
	function totalMillisecond(date) {
		return date.getUTCMilliseconds() + 1000*(date.getUTCSeconds() + 60*(date.getUTCMinutes() + 60*date.getUTCHours()));
	}
*/

	/**
	 * 自ノードのIDを設定する
	 * @param {number} id - ノードID
	 */
	function setId(id) {
		peerId = id;
		peerDepth = Math.floor(Math.LOG2E * Math.log(peerId + 1));
		elm_idview.innerHTML = peerId;
		document.title = `${DocumentTitle}[ID = ${peerId}, Depth = ${peerDepth}]`;
	}

	/**
	 * 読み込み中テキストを表示する
	 * @param {string} text - 読み込み中テキスト
	 */
	function setLoader(text) {
		elm_phase.innerHTML = text;
		elm_loader.style.display = ``;
	}

	/**
	 * 読み込み中テキストを非表示にする
	 */
	function clearLoader() {
		elm_phase.innerHTML = ``;
		elm_loader.style.display = `none`;
	}

	/**
	 * 配信設定を更新する
	 * @param {object} data - 配信設定
	 */
	function updateSettings(data) {
		audioSampleRate = data.audioSettingData.sampleRate;
		audioIsStereo = data.audioSettingData.isStereo ? 2 : 1;

		aspectRatio = data.captureSettingData.aspectRatio;
		[elm_canvas.width, elm_canvas.height] = [data.captureSettingData.width, data.captureSettingData.height];
		framePerSecond = data.captureSettingData.framePerSecond;
	}

	/**
	 * キャンバスをウィンドウのサイズに合わせる
	 */
	function resizeCanvas() {
		let windowAspectRatio = window.innerWidth / window.innerHeight;
		if (windowAspectRatio < aspectRatio) {
			elm_frame.style.width = `100%`;
			elm_frame.style.height = `${elm_frame.clientWidth / aspectRatio}px`;
		}
		else {
			elm_frame.style.height = `100%`;
			elm_frame.style.width = `${elm_frame.clientHeight * aspectRatio}px`;
		}
	}

	/**
	 * 受信したフレームデータを処理する
	 * @param {ArrayBuffer} data - 受信したフレームデータ
	 * @param {function} timeModifiedCallback - TimeChunkを処理した後のコールバック関数
	 * @return {ArrayBuffer} 処理したフレームデータ
	 */
	function decodeData(data, timeModifiedCallback = null) {
		receivedCount++;

		decodeHeader_TimeChunk(data);

		if (timeModifiedCallback !== null)
			timeModifiedCallback();

		let bodyInfo = decodeHeader_BodyChunk(data);

		switch (bodyInfo.type) {
			case DataType.FrameBuffer:
			decodeBody_CapturedImage(data);
			break;

			case DataType.AudioBuffer:
			if (elm_audioCheckBox.checked)
				decodeBody_CapturedAudio(data);

			break;
		}

		return data;
	}

	/**
	 * 受信したフレームデータのヘッダー(TimeChunk)を処理する
	 * @param {ArrayBuffer} data - 受信したフレームデータ
	 */
	function decodeHeader_TimeChunk(data) {
		let view = new DataView(data, 0, Header_TimeChunkLength);
		let elapsedMs = Date.now() - connectedLocalDate;
		let receivedTotalMs = connectedServerDate + elapsedMs;
		let serverTotalMs = view.getInt32(0, true);
		let parentTotalMs = view.getInt32(4, true);

		view.setInt32(4, receivedTotalMs, true);

		let di = receivedCount % DelayAverageWindowCount;
		serverDelay[di] = receivedTotalMs - serverTotalMs;
		parentDelay[di] = receivedTotalMs - parentTotalMs;
		//console.log("serverTotalMs:"+serverTotalMs+"\nparentTotalMs:"+parentTotalMs+"\nreceivedTotalMs:"+receivedTotalMs+"\nserverDelay[di]:"+serverDelay[di]+"\nparentDelay[di]:"+parentDelay[di]);

		let count = clamp(receivedCount, 0, DelayAverageWindowCount);
		serverDelayAvg = new Int32Array(serverDelay.buffer, 0, count).average();
		parentDelayAvg = new Int32Array(parentDelay.buffer, 0, count).average();

		if (parentDelayAvg >= DelayAverageTimeout) {
			if (!reported && reportTimer === null)
				reportTimer = setTimeout(() => { reportTimerFunc(); }, 1000 / framePerSecond + CCPDelayTimeout);
		}

		setInfo(`_sv`, 4, serverDelayAvg);
		if (!directReceive)
			setInfo(`_pt`, 5, parentDelayAvg);
	}

	/**
	 * 受信したフレームデータのヘッダー(BodyChunk)を処理する
	 * @param {ArrayBuffer} data - 受信したフレームデータ
	 * @return {object} フレームデータの型とバイト長
	 */
	function decodeHeader_BodyChunk(data) {
		let view = new DataView(data, Header_TimeChunkLength, Header_BodyChunkLength);
		let type = view.getInt8(0, true);
		let length = view.getInt32(1, true);

		return { type: type, length: length, };
	}

	/**
	 * 受信したフレームデータのボディー(画像データ)を処理する
	 * @param {ArrayBuffer} data - 受信したフレームデータ
	 */
	function decodeBody_CapturedImage(data) {
		if (__DEBUG__ && d_CheckboxSegment.checked) {
			canvas_ctx.fillStyle = `black`;
			canvas_ctx.fillRect(0, 0, elm_canvas.width, elm_canvas.height);
		}

		let view = new DataView(data, HeaderLength);

		let capturedSegmentCnt = view.getInt16(0, true);

		let viewPoint = 2;
		let array = new Uint8Array(data, HeaderLength + viewPoint);

		for (let i = 0; i < capturedSegmentCnt; i++) {
			view = new DataView(data, HeaderLength + viewPoint);
			let capturedSegmentRect = {
				x: view.getInt16(0, true),
				y: view.getInt16(2, true),
				width: view.getInt16(4, true),
				height: view.getInt16(6, true),
			};
			let capturedSegmentLength = view.getInt32(8, true);
			let capturedSegmentBuffer = array.subarray(12, 12 + capturedSegmentLength);

			setCapturedImageSegment(capturedSegmentBuffer, capturedSegmentRect);

			viewPoint += 12 + capturedSegmentLength;
			array = array.subarray(12 + capturedSegmentLength);
		}
	}

	/**
	 * 受信したフレームデータのボディー(音声データ)を処理する
	 * @param {ArrayBuffer} data - 受信したフレームデータ
	 */
	function decodeBody_CapturedAudio(data) {
		let audioArr = new Float32Array(data, HeaderLength);
		let audioSrc = audioCtx.createBufferSource();
		let audioBuf = audioCtx.createBuffer(audioIsStereo, audioArr.length, audioSampleRate);

		audioBuf.getChannelData(0).set(audioArr);

		audioSrc.buffer = audioBuf;
		audioSrc.connect(audioCtx.destination);
		audioSrc.start(audioPlaybackTime);

		if (audioCtx.currentTime < audioPlaybackTime)
			audioPlaybackTime += audioBuf.duration;
		else
			audioPlaybackTime = audioCtx.currentTime + audioBuf.duration;
	}

	/**
	 * 画像データをキャンバスに描画する
	 * @param {ArrayBuffer} capturedSegmentBuffer - 画像データ
	 * @param {object} capturedSegmentRect - 描画先矩形
	 */
	function setCapturedImageSegment(capturedSegmentBuffer, capturedSegmentRect) {
		let blob = new Blob([capturedSegmentBuffer], {type:`image/webp`});
		let url = urlCreator.createObjectURL(blob);
		let img = new Image();
		img.onload = () => {
			canvas_ctx.drawImage(img, capturedSegmentRect.x, capturedSegmentRect.y, capturedSegmentRect.width, capturedSegmentRect.height);
			urlCreator.revokeObjectURL(url);

			if (!canvasDrawn) {
				canvasDrawn = true;
			}

			if (__DEBUG__ && d_CheckboxRectangle.checked) {
				canvas_ctx.strokeStyle = `blue`;
				canvas_ctx.strokeRect(capturedSegmentRect.x, capturedSegmentRect.y, capturedSegmentRect.width, capturedSegmentRect.height);
			}
		}

		if (!capturedSegmentRect)
			capturedSegmentRect = { x:0, y:0, width:canvasSize.width, height:canvasSize.height, };

		img.src = url;
	}

	/**
	 * キャンバス画像を取得する
	 * @param {function} callback - 取得した画像データを引数に持つコールバック
	 */
	function getCanvasImage(callback) {
		elm_canvas.toBlob((blob) => { callback(blob); }, `image/webp`, WebPQuality);
	}

	/**
	 * サーバにメッセージを送信する
	 * @param {MessageType} type - メッセージの型
	 * @param {number} id - 自ノードID
	 * @param {number} targetId - 送信先ノードID
	 * @param {object} data - 送信データ
	 */
	function sendToServer(type, id, targetId, data) {
		let sendData = {
			type		: type,
			id			: id,
			targetId	: targetId,
			data		: data,
		};
		let json = JSON.stringify(sendData);
		socket.Send(json);

		//d_log(`send`, `type : ${ data.type }, from ID : ${ data.id }, to ID : ${ data.targetId);
	}

	/**
	 * CCPを受信した際の処理
	 * @param {string} ccpMessage - CCPメッセージ
	 */
	function onCheckContinuesMessage(ccpMessage) {
		setCCPTimer();

		if (__DEBUG__ && !d_CheckboxCast.checked) return;

		for (let k in peerChildren){
			peerChildren[k].sendCCP(ccpMessage);
		}
	}

	/**
	 * CCPTimerをセットする
	 * この関数がCCPDelayTimeoutを超えても発火しなかった際にCCPTimerFuncが呼ばれる
	 */
	function setCCPTimer() {
		clearDelayTimer();
		delayTimer = setTimeout(() => { CCPTimerFunc(); }, 1000 / framePerSecond + CCPDelayTimeout);
	}

	/**
	 * CCPTimer, reportTimerを消去する
	 */
	function clearDelayTimer() {
		if (delayTimer !== null) clearTimeout(delayTimer);
		if (reportTimer !== null) clearTimeout(reportTimer);
		delayTimer = null;
		reportTimer = null;
	}

	/**
	 * CCPのタイムアウトが発火した際の処理
	 * 子にCCPを送信してreportTimerをセットする
	 */
	function CCPTimerFunc() {
		for (let k in peerChildren)
			peerChildren[k].sendCCP(CheckContinuesMessage);

		delayTimer = setTimeout(() => { CCPTimerFunc(); }, 1000 / framePerSecond);
		if (peerId >= 1 && (peerId - 1) % BranchCount != 0) return;

		if (!reported && reportTimer === null)
			reportTimer = setTimeout(() => { reportTimerFunc(); }, 1000 / framePerSecond + CCPDelayTimeout);
	}

	/**
	 * reportTimerが発火した際の処理
	 * Reportメッセージを送信する
	 */
	function reportTimerFunc() {
		if (directReceive) {
			d_log(`report`, `requested reconnection to SERVER`, `red`);
			//sendToServer(MessageType.Report, peerId, -1, {});
			setLoader(`Error`);
			return;
		}

		d_log(`report`, `request reconnection to ${peerParent.offerId}`, `red`);
		sendToServer(MessageType.Report, peerId, peerParent.offerId, {});

		reported = true;
	}

	// ウィンドウとキャンバスのサイズを同期する
	resizeCanvas(false);
	window.onresize = resizeCanvas;

	// Offer Side -----------------------------
	class WebRTCPeerConnectionOffer {
		static get DATAGRAM_MAXLENGTH() { return 64 * 1024; }

		constructor(config, id, answerId) {
			if (!WebRTCPeerConnectionOffer.count)
				WebRTCPeerConnectionOffer.count = 0;

			this.id = id;
			this.childIdx = WebRTCPeerConnectionOffer.count++;
			this.answerId = answerId;
			this.connection = new webRTCPeerConnection(config);

			this.connection.onicecandidate = (evt) => this._onIceCandidate(evt);
			this.connection.oniceconnectionstatechange = (evt) => {
				setInfo(`_ch${this.childIdx}`, 1, this.dc_frame.readyState);
				setInfo(`_ch${this.childIdx}`, 3, this.connection.iceConnectionState);
			};

			this.dc_frame = this.connection.createDataChannel(`${this.answerId}:${WebRTCDataChannel_Frame}`);
			this.dc_ccp = this.connection.createDataChannel(`${this.answerId}:${WebRTCDataChannel_CCP}`);
			this.dc_canvasImage = this.connection.createDataChannel(`${this.answerId}:${WebRTCDataChannel_CanvasImage}`);
			this.dc_frame.onopen = (evt) => this._onDataChannelOpened_frame(evt);
			this.dc_ccp.onopen = (evt) => this._onDataChannelOpened_ccp(evt);
			this.dc_canvasImage.onopen = (evt) => this._onDataChannelOpened_canvasImage(evt);

			this.connection.createOffer().then((sdp) => {
				d_log(`webRTC`, `[offer => ${this.answerId}] created Offer`);
				return this.connection.setLocalDescription(sdp);
			})
			.then(() => this._onCreateOffer())
			.catch((reason) => {
				d_log(`webRTC`, `[offer => ${this.answerId}] createOffer failed. : ${reason}`, `red`);
			});

			d_log(`webRTC`, `[offer => ${this.answerId}] started peer connection to ${this.answerId}`);
		}

		send(data) {
			if(this.dc_frame.readyState != `open`) return;
			if (__DEBUG__ && !d_CheckboxCast.checked) return;

			let len = !data.length ? data.byteLength : data.length;
			if (len > WebRTCPeerConnectionOffer.DATAGRAM_MAXLENGTH) {
				let chunk = data.segmentation(WebRTCPeerConnectionOffer.DATAGRAM_MAXLENGTH);

				this.dc_frame.send(`Chunk`);
				for (let k in chunk)
					this.dc_frame.send(chunk[k]);
				this.dc_frame.send(`\0`);
			}
			else {
				this.dc_frame.send(data);
			}
		}

		sendCCP() {
			this.dc_ccp.send(CheckContinuesMessage);
		}

		sendCanvasImage(data) {
			this.dc_canvasImage.send(data);
		}

		setRemoteDescription(sdp) {
			this.connection.setRemoteDescription(
				new webRTCSessionDescription(sdp),
				() => {
					setInfo(`_ch${this.childIdx}`, 1, this.dc_frame.readyState);
					setInfo(`_ch${this.childIdx}`, 2, `completed`);
					d_log(`webRTC`, `[offer => ${this.answerId}] setRemoteDescription succeeded.`);
				},
				() => {
					d_log(`webRTC`, `[offer => ${this.answerId}] setRemoteDescription failed.`, `red`);
				}
			);
		}

		addIceCandidate(iceCandidate) {
			this.connection.addIceCandidate(new webRTCIceCandidate(iceCandidate));
		}

		close() {
			this.connection.close();
			WebRTCPeerConnectionOffer.count--;
		}

		_onCreateOffer() {
			sendToServer(MessageType.SDPOffer, this.id, this.answerId, this.connection.localDescription);
			setInfo(`_ch${this.childIdx}`, 2, `offerring`);

			d_log(`webRTC`, `[offer => ${this.answerId}] send sdp to ${this.answerId}`);
		}

		_onIceCandidate(evt) {
			if (evt.candidate) {
				sendToServer(MessageType.ICECandidateOffer, this.id, this.answerId, evt.candidate);

				d_log(`webRTC`, `[offer => ${this.answerId}] get ICE candidate`);
				d_log(`webRTC`, `[offer => ${this.answerId}] send ICE candidate to ID : ${this.answerId}`);
			}
			else {
				d_log(`webRTC`, `[offer => ${this.answerId}] ICE candidate phase ${evt.eventPhase}`, `orange`);
			}
		}

		_onDataChannelOpened_frame(evt) {
			d_log(`webRTC`, `[offer => ${this.answerId}] data channel for 'frame' opened`);
			this.dc_frame.binaryType = `arraybuffer`;

			this.dc_frame.onclose = (evt) => {
				d_log(`webRTC`, `[offer => ${this.answerId}] datachannel for 'frame' closed`);
			}

			setInfo(`_ch${this.childIdx}`, 1, `opened`);

			if (__DEBUG__) {
				d_LabelOffer[this.answerId] = d_LabelID.cloneNode(true);
				d_LabelOffer[this.answerId].innerHTML = `child ID : ${this.answerId}`;
				d_Body.appendChild(d_LabelOffer[this.answerId]);
			}
		}

		_onDataChannelOpened_ccp(evt) {
			d_log(`webRTC`, `[offer => ${this.answerId}] data channel for 'check continue message' opened`);

			this.dc_ccp.onclose = (evt) => {
				d_log(`webRTC`, `[offer => ${this.answerId}] datachannel for 'check continue message' closed`);
			}
		}

		_onDataChannelOpened_canvasImage(evt) {
			d_log(`webRTC`, `[offer => ${this.answerId}] data channel for 'canvas image' opened`);
			this.dc_canvasImage.binaryType = `arraybuffer`;

			this.dc_canvasImage.onclose = (evt) => {
				d_log(`webRTC`, `[offer => ${this.answerId}] datachannel for 'canvas image' closed`);
			}

			//this.task_canvasImage.pass();
			this._onTaskPassed_canvasImage();
		}

		_onTaskPassed_canvasImage() {
			getCanvasImage((blob) => {
				let fr = new FileReader();
				fr.onload = (frEvt) => {
					this.sendCanvasImage(fr.result);
					this.dc_canvasImage.close();
					d_log(`webRTC`, `[offer => ${this.answerId}] sent canvas image`);
				};
				fr.onerror = (frEvt) => {
					d_log(`webRTC`, `[offer => ${this.answerId}] failed to send canvas image`, `orange`);
				};
				fr.readAsArrayBuffer(blob);
			});
		}
	}

	// Answer Side -----------------------------
	class WebRTCPeerConnectionAnswer {
		constructor(pcConfigures, id) {
			this.id = id;
			this.dc_frame = null;
			this.dc_ccp = null;
			this.dc_canvasImage = null;
			this.collecting = false;
			this.chunk = [];
			this.connection = new webRTCPeerConnection(pcConfigures);
			this.connectionOffer = [];

			this.connection.oniceconnectionstatechange = (evt) => {
				if (this.dc_frame != null) 
					setInfo(`_pt`, 1, this.dc_frame.readyState);
				setInfo(`_pt`, 3, this.connection.iceConnectionState);
			};
		}

		setRemoteDescription(remoteSdp) {
			this.connection.setRemoteDescription(new webRTCSessionDescription(remoteSdp)).then(() =>{
				this._onSetRemoteDescription();
			})
			.catch((reason) => {
				d_log(`webRTC`, `[answer <= ${this.offerId}] createOffer failed. : ${reason}`, `red`);
			});
		}

		addIceCandidate(iceCandidate) {
			this.connection.addIceCandidate(new webRTCIceCandidate(iceCandidate));
		}

		close() {
			this.connection.close();
		}

		_onSetRemoteDescription() {
			this.connection.onicecandidate = (evt) => {
				this._onIceCandidate(evt);
			};

			this.connection.createAnswer().then((sdp) => {
				return this.connection.setLocalDescription(sdp);
			})
			.then(() => {
				this._onCreateAnswer();

				if (this.dc_frame != null)
					setInfo(`_pt`, 1, this.dc_frame.readyState);
				setInfo(`_pt`, 2, `completed`); 
			})
			.catch((reason) => {
				console.log(reason);
				d_log(`webRTC`, `[answer <= ${this.offerId}] createAnswer failed. : ${reason}`, `red`);
			});
		}

		_onCreateAnswer() {
			d_log(`webRTC`, `[answer <= ${this.offerId}] setLocalDescription succeeded.`);

			this.connection.ondatachannel = (evt) => {
				if (evt.channel.label.indexOf(WebRTCDataChannel_Frame) !== -1)
					this._onDataChannelOpened_frame(evt);
				else if (evt.channel.label.indexOf(WebRTCDataChannel_CCP) !== -1)
					this._onDataChannelOpened_ccp(evt);
				else if (evt.channel.label.indexOf(WebRTCDataChannel_CanvasImage) !== -1)
					this._onDataChannelOpened_canvasImage(evt);
				else
					d_log(`webRTC`, `[answer <= ${this.offerId}] unknown datachannel opened`, `red`);
			};
			sendToServer(MessageType.SDPAnswer, this.id, this.offerId, this.connection.localDescription);
		}

		_onIceCandidate(evt) {
			if (evt.candidate) {
				sendToServer(MessageType.ICECandidateAnswer, this.id, this.offerId, evt.candidate);

				d_log(`webRTC`, `[answer <= ${this.offerId}] get ICE candidate`);
				d_log(`webRTC`, `[answer <= ${this.offerId}] send ICE candidate to ID : ${this.offerId}`);
			}
			else {
				d_log(`webRTC`, `[answer <= ${this.offerId}] ICE candidate phase ${evt.eventPhase}`, `orange`);
			}
		}

		_onDataChannelOpened_frame(evt) {
			d_log(`webRTC`, `[answer <= ${this.offerId}] data channel for 'frame' opened`);

			this.dc_frame = evt.channel;
			this.dc_frame.onmessage = (evt) => {
				this._onDataChannelMessage_frame(evt);
			};

			this.dc_frame.onclose = (evt) => {
				d_log(`webRTC`, `[answer <= ${this.offerId}] data channel for 'frame' closed`);
			}

			if (!isCasting)
				setLoader(`Ready`);
			else
				clearLoader();

			setInfo(`_pt`, 1, `opened`);

			if (__DEBUG__) {
				d_LabelAns[this.offerId] = d_LabelID.cloneNode(true);
				d_LabelAns[this.offerId].innerHTML = `parent ID : ${this.offerId}`;
				d_Body.appendChild(d_LabelAns[this.offerId]);
			}
		}

		_onDataChannelOpened_ccp(evt) {
			d_log(`webRTC`, `[answer <= ${this.offerId}] data channel for 'check continues message' opened`);

			this.dc_ccp = evt.channel;
			this.dc_ccp.onmessage = (evt) => {
				onCheckContinuesMessage(evt.data);
			};

			this.dc_ccp.onclose = (evt) => {
				d_log(`webRTC`, `[answer <= ${this.offerId}] data channel for 'check continues message' closed`);
			};

			setCCPTimer();
		}

		_onDataChannelOpened_canvasImage(evt) {
			d_log(`webRTC`, `[answer <= ${this.offerId}] data channel for 'canvas image' opened`);

			this.dc_canvasImage = evt.channel;
			this.dc_canvasImage.onmessage = (evt) => {
				setCapturedImageSegment(evt.data);
				d_log(`webRTC`, `[answer <= ${this.offerId}] received & set canvas image`);
			};

			this.dc_canvasImage.onclose = (evt) => {
				d_log(`webRTC`, `[answer <= ${this.offerId}] data channel for 'canvas image' closed`);
			}
		}

		_onDataChannelMessage_frame(evt) {
			if (__DEBUG__ && !d_CheckboxReceive.checked) return;

			if (this.collecting) {
				if (evt.data == `\0`) {
					let data = concatenation(this.chunk);

					this._decode(data);

					this.chunk = [];
					this.collecting = false;
				} else {
					this.chunk.push(evt.data);
				}
			}
			else if (evt.data == `Chunk`) {
				this.collecting = true;
			}
			else {
				this._decode(evt.data);
			}
		}

		_decode(data) {
			decodeData(data, () => {
				for (let k in peerChildren)
					peerChildren[k].send(data);
			});
		}
	}



	// WebSocket ------------------------------------------------------------

	function closeAllConnection() {
		for (let k in peerChildren)
			peerChildren[k].close();

		if (peerParent != null)
			peerParent.close();

		peerChildren = [];
		peerParent = null;

		setInfo(`_sv`, [0,3,4,5], `-`);
		setInfo(`_sv`, 1, `connecting`);
		setInfo(`_sv`, 2, `no`);
	}

	socket = new Alchemy({
		Server 		: location.hostname,
		Port 		: `8081`,
		BinaryType 	: `arraybuffer`,
		DebugMode 	: false,
	});

	socket.Connected = () => {
		setInfo(`_sv`, 1, `connected`);
	}

	socket.Disconnected = () => {
		setInfo(`_sv`, 1, `disconnected`);
		d_log(`WebSocket`, `connection disconnected`, `blue`);
	};

	socket.MessageReceived = (event) => {
		if (__DEBUG__ && !d_CheckboxReceive.checked) return;

		setInfo(`_sv`, 3, nowTime());
		if (typeof event.data == `string`) {
			if (event.data == CheckContinuesMessage) {
				onCheckContinuesMessage(event.data);
				return;
			}

			let message = JSON.parse(event.data);
			switch (message.type) {
				case MessageType.Connected:
				connectedServerDate = message.data;
				connectedLocalDate = new Date();
				setId(message.id);
				directReceive = (message.id == 0);

				if (directReceive) {
					if (!isCasting) setLoader(`Ready`);
					else clearLoader();
				}
				else {
					peerParent = new WebRTCPeerConnectionAnswer(pcConfigures, message.id);
				}

				setInfo(`_sv`, 0, message.id);

				d_log(`WebSocket`, `connection established. ID => ${message.id}`, `blue`);
				d_log(`WebSocket`, `server date: ${connectedServerDate}`);
				if (__DEBUG__) d_LabelID.innerHTML = `ID : ${message.id}`;
				break;


				case MessageType.UpdateID:
				setId(message.id);
				peerParent.id = message.id;
				directReceive = (message.id === 0);
				setInfo(`_sv`, 0, message.id);

				clearDelayTimer();
				d_log(`WebSocket`, `update ID => ${message.id}`, `blue`);
				if (__DEBUG__) d_LabelID.innerHTML = `ID : ${message.id}`
				break;


				case MessageType.PeerConnection: // new peer connected
				let targetId = message.targetId;

				if (Object.keys(peerChildren).length >= BranchCount) {
					d_log(`WebSocket`, `rejected peer: too many peers. peer count = ${peerChildren.length}`, `red`);
					break;
				}

				if (peerChildren[targetId] != null) {
					d_log(`WebSocket`, `rejected peer: same peer id as existing peer. peer count = ${peerChildren.length}`, `red`);
					break;
				}

				peerChildren[targetId] = new WebRTCPeerConnectionOffer(pcConfigures, peerId, targetId);

				if (peerParent != null)
					peerParent.connectionOffer[targetId] = peerChildren[targetId];

				setInfo(`_ch${peerChildren[targetId].childIdx}`, 0, targetId);

				d_log(`WebSocket`, `peer connected as child. child ID : ${message.targetId}`, `blue`);
				break;


				case MessageType.SDPOffer: // SDP Offer Received
				peerParent.offerId = message.id;
				peerParent.setRemoteDescription(message.data);
				setInfo(`_pt`, 0, message.id);
				d_log(`WebSocket`, `received SDPOffer from ID : ${message.id}`, `blue`);
				//d_CheckboxCast.onclick = function(){console.dir(peerParent);}
				break;


				case MessageType.SDPAnswer: // SDP Answer Received
				peerChildren[message.id].setRemoteDescription(message.data);
				d_log(`WebSocket`, `received answer from ID : ${message.id}`, `blue`);
				break;


				case MessageType.ICECandidateOffer: // ICECandidate Received from Offer
				peerParent.addIceCandidate(message.data);
				d_log(`WebSocket`, `received ICECandidate from offer ID : ${message.id}`, `blue`);
				break;


				case MessageType.ICECandidateAnswer: // ICECandidate Received from Answer
				peerChildren[message.id].addIceCandidate(message.data);
				d_log(`WebSocket`, `received ICECandidate from answer ID : ${message.id}`, `blue`);
				break;


				case MessageType.RemoveOffer: // Remove Offer
				setInfo(`_ch${peerChildren[message.id].childIdx}`, [0,1,2,3], `-`);

				if (peerChildren[message.id]) peerChildren[message.id].close();
				delete peerChildren[message.id];

				d_log(`WebSocket`, `remove offer ID : ${message.id}`, `orange`);
				break;


				case MessageType.RemoveAnswer: // Remove Answer
				if (peerParent) peerParent.close();
				peerParent = new WebRTCPeerConnectionAnswer(pcConfigures, peerId);


				setInfo(`_pt`, [0,1,2,3,4,5], `-`);

				d_log(`WebSocket`, `remove answer ID : ${message.id}`, `orange`);
				break;


				case MessageType.Settings:
				updateSettings(message.data);
				resizeCanvas();

				d_log(`WebSocket`, `settings received`, `blue`);
				break;


				case MessageType.StartCasting:
				isCasting = true;
				receivedCount = 0;

				setCCPTimer();
				clearLoader();

				setInfo(`_sv`, 2, `yes`);

				d_log(`WebSocket`, `casting started`, `blue`);
				break;


				case MessageType.StopCasting:
				isCasting = false;
				clearDelayTimer();

				setLoader(`Ready`);
				setInfo(`_sv`, 2, `no`);

				d_log(`WebSocket`, `casting stopped`, `blue`);
				break;

				case MessageType.Reset:
				case MessageType.Disconnect:
				closeAllConnection();
				clearDelayTimer();

				socket.Stop();
				setTimeout(() => { socket.Start(); }, 1000);

				if (message.type === MessageType.Reset) {
					d_log(`WebSocket`, `accept reset from ID :${message.id}`, `red`);
					setLoader(`Connecting...`);
				}
				else {
					d_log(`WebSocket`, `disconnected`, `red`);
					setLoader(`Disconnected`);
				}
				break;

				default:
				d_log(`WebSocket`, `An unexpected message received. : ${message.type}`, `red`);
				break;
			}

			d_log(`recv`,`type:${message.type},id:${message.id},target:${message.targetId}`);
		}
		else {
			let modified = decodeData(event.data);
			if (!directReceive || !isCasting) return;

			for (let k in peerChildren)
				peerChildren[k].send(modified);

			//setCCPTimer(null);
		}
	};

	socket.Error = (evt) => {
		document.location.reload(true);
	};

	setLoader(`Connecting...`);

	connectTime = Date.now();

	socket.Start();

	//---------------------------------------------------------------------------

	if (!__INFO__)
		document.getElementById(`_info`).style.display = `none`;
}