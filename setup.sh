#!/bin/bash

# Install Node Exporter
VERSION=1.7.0 # https://github.com/prometheus/node_exporter/releases
wget https://github.com/prometheus/node_exporter/releases/download/v$VERSION/node_exporter-$VERSION.linux-arm64.tar.gz
tar -xvf node_exporter-$VERSION.linux-arm64.tar.gz
sudo cp node_exporter-$VERSION.linux-arm64/node_exporter /usr/local/bin

# Prepare Node Exporter
sudo useradd -r node_exporter

# Node Exporter Service
sudo cp ./setup/node_exporter.service /usr/local/lib/systemd/system/node_exporter.service
sudo systemctl enable node_exporter
sudo systemctl start node_exporter

# Install .NET
wget https://dot.net/v1/dotnet-install.sh
chmod +x ./dotnet-install.sh
sudo ./dotnet-install.sh --install-dir /usr/local/dotnet
sudo ln -s /usr/local/dotnet/dotnet /usr/local/bin

# Prepare Irrigation Controller
sudo useradd -r irrigation-controller
sudo adduser irrigation-controller gpio
sudo mkdir /var/lib/irrigation-controller
sudo chown irrigation-controller:irrigation-controller /var/lib/irrigation-controller

# Irrigation Controller Service
sudo cp ./setup/irrigation-controller.service /usr/local/lib/systemd/system/irrigation-controller.service
sudo systemctl enable irrigation-controller
