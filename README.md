# ScreenShare
LANで利用できる画面共有ツールです。  
ブラウザで共有画面を閲覧できます。

##手順
1. 共有元となるPCで「bin/ScreenShare.exe」を実行します。
2. サーバ設立ボタンでサーバを起動します。
3. 閲覧側はブラウザで共有元PCのIPアドレスにポート8080番で接続します("http://○○○.○○○.○○○.○○○:8008")。
4. キャプチャ開始ボタンを押すと、画面が共有されます。

##実行環境
###共有元PC
* Windows OS 7以降
* .Net Framework 4.5 以降

###閲覧側PC
*WebRTCに対応したブラウザ(Google Chrome, Firefox, Operaなど)

