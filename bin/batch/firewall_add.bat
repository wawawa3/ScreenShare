@echo off

netsh advfirewall firewall add rule name="HttpListener" dir=in action=allow
netsh advfirewall firewall set rule name="HttpListener" new program="system" profile=private protocol=tcp localport=8080
netsh http add urlacl url="http://+:8080/" user=everyone

timeout 1