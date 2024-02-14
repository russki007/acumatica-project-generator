@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy ByPass -Command "& """%~dp0\csMake\csMake.ps1""" %*"
exit /b %ErrorLevel%