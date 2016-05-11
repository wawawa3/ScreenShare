@echo off

netsh advfirewall firewall add rule name="HttpListener" dir=in action=allow program="system" profile=private,public localport=8080 protocol=tcp
netsh http add urlacl url="http://+:8080/" user=everyone

timeout 1