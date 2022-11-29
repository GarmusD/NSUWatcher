# NSUWatcher

NSUWatcher initially was created as a reboot service for the NSU2 controller if controller firmware halts. Software works on ARM Cortex CPU of UDOO board (Linux).\
Little by little NSUWatcher started to log sensor data (temperatures, ...) for statistics into MySql database and become a "data and command center" of NSU2 controller. Also it is a connection enpoint for Windows and Android applications.

# HOW IT WORKS
Communicates with NSU2 firmware over serial line. NSUWatcher connects to NSU, sends commands, receives responses, logs sensor data. TcpServer acts as server for connections from Windows and Android applications.

## Features
### Firmware "KeepAlive"
Reboots NSU2 firmware if stops receiving pings
### Upload firmware
Allows upload NSU2 firmware remotely.
### TCP Server
Allows Windows and Android applications to connect and control NSU2.
### Data logger
Logs sensor data into MySql database.

# CHANGES
### Initial release
Basic functionality

### 0.4.2
NSUWatcherNet, log sensors data to MySql database.

### 0.5.1
Upgrading project to .netFramework 4.8. .netCore cannot be used because the software runs on linux 14.04 and some functionality requires newer system libraries.
Hosting environment is used if running as service.
