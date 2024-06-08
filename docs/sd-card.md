# Creating the SD card for Raspberry Pi

- Use Raspberry Pi Imager to burn Raspberry Pi OS Lite (64-bit). Configure wireless connection, enable ssh, set the hostname, configure localization as needed.
- Install Node Exporter. Find the latest `arm64` release here: https://github.com/prometheus/node_exporter/releases
```
VERSION=1.7.0
wget https://github.com/prometheus/node_exporter/releases/download/v$VERSION/node_exporter-$VERSION.linux-arm64.tar.gz
tar -xvf node_exporter-$VERSION.linux-arm64.tar.gz

sudo cp node_exporter-$VERSION.linux-arm64/node_exporter /usr/local/bin
```
- Create the systemd unit file
```
sudo useradd -r node_exporter
sudo mkdir -p /usr/local/lib/systemd/system
sudo nano /usr/local/lib/systemd/system/node_exporter.service
```
```
[Unit]
Description=Node Exporter
After=network.target

[Service]
User=node_exporter
Group=node_exporter
Type=simple
Restart=on-failure
ExecStart=/usr/local/bin/node_exporter

[Install]
WantedBy=multi-user.target
```
- Enable and start the unit:
```
sudo systemctl enable node_exporter
sudo systemctl start node_exporter
```
- Install .NET
```
wget https://dot.net/v1/dotnet-install.sh
sudo ./dotnet-install.sh --install-dir /usr/local/dotnet --runtime aspnetcore
sudo ln -s /usr/local/dotnet/dotnet /usr/local/bin
```
