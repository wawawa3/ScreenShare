@echo off
echo ファイアウォールからWebサーバ規則を削除します

netsh advfirewall firewall delete rule name="HttpListener" program="system"

echo 完了しました

timeout 1