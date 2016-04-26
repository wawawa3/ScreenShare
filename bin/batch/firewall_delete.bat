@echo off

netsh advfirewall firewall delete rule name="HttpListener" program="system"

timeout 1