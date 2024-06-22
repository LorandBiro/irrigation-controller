#!/bin/bash

rm -rf src/IrrigationController/bin/Release/net8.0/publish
dotnet publish src/IrrigationController/IrrigationController.csproj

sudo systemctl stop irrigation-controller
sudo rm -rf /usr/local/irrigation-controller
sudo cp -r src/IrrigationController/bin/Release/net8.0/publish/. /usr/local/irrigation-controller
sudo systemctl start irrigation-controller

journalctl -u irrigation-controller -f
