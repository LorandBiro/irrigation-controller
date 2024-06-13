@echo off

rd /s /q IrrigationController\bin\Release\net8.0\linux-arm64\publish 2>nul
dotnet publish --runtime linux-arm64

scp -r IrrigationController\bin\Release\net8.0\linux-arm64\publish\* lbiro@irrigation.local:/usr/local/irrigation-controller
ssh lbiro@irrigation.local sudo systemctl restart irrigation-controller
