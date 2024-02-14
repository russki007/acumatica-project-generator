@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy ByPass -Command "& """%~dp0\acumatica-project-generator\csMake.ps1""" %*"
exit /b %ErrorLevel%