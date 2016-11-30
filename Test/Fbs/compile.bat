@echo off
cd /d "%~dp0"
FbsGen.exe --fbs-root . --package TestMessage
pause
