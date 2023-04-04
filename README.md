# Description

A simple command-line tool that backups all ABB IRC5 controllers available on the network.

# Usage
```sh
cmd> .\ABB_IRC5_Local_Network_Backup.exe --help
    ABB_IRC5_Local_Network_Backup 1.0.0.0
    Copyright (c)  2023
      -v, --verbose    Set output to verbose messages.
      -o               Defines local folder for backups. Example: -o "C:\Users\User\Documents\Robostudio Backups\Auto\"
      -a               Backup all available controllers (including virtual ones).
      -l               List all available controllers (including virtual ones).
      --help           Display this help screen.
      --version        Display version information.
```

## Examples
List all controllers without creating backups:
```sh
.\ABB_IRC5_Local_Network_Backup.exe -l
```
List and backup all available controllers to a local folder:
```sh
.\ABB_IRC5_Local_Network_Backup.exe -l  -o "C:\Users\User\Documents\RobotStudio Backups\Auto Backups"
```

## Build requirements

- .NET Framework 4.8.1
- ABB PC [SDK 2022.3](https://developercenter.robotstudio.com/) 
- Visual Studio 2019 Community

## Latest builds

- [ABB IRC5 Local Network Backup V0.1](https://github.com/romdnop/irc5_backup_tool/releases/tag/v0.1)
