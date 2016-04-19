@echo off
echo ファイアウォールにWebサーバ規則を追加します

netsh advfirewall firewall add rule name="HttpListener" dir=in action=allow
netsh advfirewall firewall set rule name="HttpListener" new program="system" profile=private protocol=tcp localport=8080

echo 完了しました

timeout 1