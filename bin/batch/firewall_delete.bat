@echo off

netsh advfirewall firewall delete rule name="HttpListener" program="system"
netsh http delete urlacl url="http://+:8080/"

timeout 1