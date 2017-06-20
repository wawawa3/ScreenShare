# ScreenShare
LANで利用できる画面・音声の配信ツールです。  
WebRTCに対応したブラウザで配信画面の閲覧と、配信音声を聞くことができます。


## 手順
1. 共有元となるPCで「bin/ScreenShare.exe」を実行します。
2. サーバ設立ボタンでサーバを起動します。
3. 閲覧側はブラウザで共有元PCのIPアドレスにポート8080番で接続します("http://○○○.○○○.○○○.○○○:8008")。
4. キャプチャ開始ボタンを押すと、画面が配信されます。


## 実行環境
#### 共有元PC
* Windows OS 7以降
* .Net Framework 4.5 以降

#### 閲覧側PC
* WebRTCに対応したブラウザ(Google Chrome, Firefox, Operaなど)

