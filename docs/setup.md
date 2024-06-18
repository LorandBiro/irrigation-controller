# Raspberry Pi setup

Use Raspberry Pi Imager to burn Raspberry Pi OS Lite (64-bit).
- Configure wifi
- Enable ssh
- Set username: lbiro
- Set the hostname: irrigation
- Configure localization

## Node Exporter

Create the systemd service: /usr/local/lib/systemd/system/node_exporter.service
```
[Unit]
Description=Node Exporter
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
User=node_exporter
Group=node_exporter
Restart=always
RestartSec=1s
ExecStart=/usr/local/bin/node_exporter

[Install]
WantedBy=multi-user.target
```

Install Node Exporter. Find the latest `arm64` release here: https://github.com/prometheus/node_exporter/releases
```
VERSION=1.7.0
wget https://github.com/prometheus/node_exporter/releases/download/v$VERSION/node_exporter-$VERSION.linux-arm64.tar.gz
tar -xvf node_exporter-$VERSION.linux-arm64.tar.gz
sudo cp node_exporter-$VERSION.linux-arm64/node_exporter /usr/local/bin

sudo useradd -r node_exporter

sudo systemctl enable node_exporter
sudo systemctl start node_exporter
```

## Irrigation Controller

Create the systemd service: /usr/local/lib/systemd/system/irrigation-controller.service
```
[Unit]
Description=Irrigation Controller
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
User=irrigation-controller
Group=irrigation-controller
Restart=always
RestartSec=1s
ExecStart=dotnet /usr/local/irrigation-controller/IrrigationController.dll
WorkingDirectory=/usr/local/irrigation-controller
CapabilityBoundingSet=CAP_NET_BIND_SERVICE
AmbientCapabilities=CAP_NET_BIND_SERVICE
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
```

Install .NET and prepare the user:
```
wget https://dot.net/v1/dotnet-install.sh
sudo ./dotnet-install.sh --install-dir /usr/local/dotnet --runtime aspnetcore
sudo ln -s /usr/local/dotnet/dotnet /usr/local/bin

sudo useradd -r irrigation-controller
sudo adduser irrigation-controller gpio

sudo mkdir /var/lib/irrigation-controller
sudo chown irrigation-controller:irrigation-controller /var/lib/irrigation-controller

sudo systemctl enable irrigation-controller
```

Use `deploy.bat` in `src` to build and deploy the app.
