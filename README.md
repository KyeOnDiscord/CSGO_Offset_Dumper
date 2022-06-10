# CSGO Offset Dumper

![](https://img.shields.io/github/languages/code-size/KyeOnDiscord/CSGO_Offset_Dumper)
[![GitHub issues](https://img.shields.io/github/issues/KyeOnDiscord/CSGO_Offset_Dumper)](https://github.com/KyeOnDiscord/CSGO_Offset_Dumper/issues)
[![GitHub stars](https://img.shields.io/github/stars/KyeOnDiscord/CSGO_Offset_Dumper)](https://github.com/KyeOnDiscord/CSGO_Offset_Dumper/stargazers)


An offset dumper for [Counter-Strike: Global Offensive](https://store.steampowered.com/app/730/CounterStrike_Global_Offensive/) written entirely in C#.




## Getting Started
___You can either build the project with the instructions below or download the prebuilt executable in [releases](https://github.com/KyeOnDiscord/CSGO_Offset_Dumper/releases)___

### Prerequisites
* [.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

### Building the Project
1. Clone the repo
   ```sh
   git clone https://github.com/KyeOnDiscord/CSGO_Offset_Dumper.git
   ```
2. Goto the folder directory
   ```sh
   cd CSGO_Offset_Dumper
   ```
4. Build the project
   ```sh
   dotnet build
   ```
   
   
## Usage

1. Put your config.json file into the same directory as `CSGO_Offset_Dumper.exe`. If you don't have a config.json file, you can get one from [hazedumper](https://github.com/frk1/hazedumper/blob/master/config.json).
2.  Launch csgo in steam using [`-insecure`](https://guidedhacking.com/threads/how-to-bypass-vac-valve-anti-cheat-info.8125/) so [VAC](https://en.wikipedia.org/wiki/Valve_Anti-Cheat) is not loaded.
3.  Open `CSGO_Offset_Dumper.exe` and your files will be dumped.

## Features

✔ Compatible with [hazedumper](https://github.com/frk1/hazedumper/blob/master/config.json) & [GH Offset Dumper](https://guidedhacking.com/resources/guided-hacking-offset-dumper-gh-offset-dumper.51/) config files

✔ Signature Offsets Dumper

✔ Netvar Offsets Dumper

✔ Dumps offsets to the following file formats:
* C++ (.h)
* C# (.cs)
* JSON (.json) (Also exports min.json which is the json in one line without line breaks)
* TOML (.toml)
* [Cheat Engine](https://www.cheatengine.org/)'s Cheat Tables (.ct)

### To Do:

- [ ] Export Cheat Tables with offsets pointing to the game's actual value
- [ ] Export Source Engine Classes

## Built With

* [C# .NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)


### Acknowledgments
* [GH Offset Dumper](https://github.com/guided-hacking/GH-Offset-Dumper)'s was massively inspired by this.
* [frk1](https://github.com/frk1)'s [hazedumper](https://github.com/frk1/hazedumper)'s config files.
* [Spectre.Console](https://github.com/spectreconsole/spectre.console) for an amazing and easy way to create good looking console apps.
