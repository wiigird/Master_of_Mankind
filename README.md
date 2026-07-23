# Master of Mankind

An unofficial `Slay the Spire 2` character mod that lets you play as the Emperor of Mankind.

## Features

- Foresight: reveal enemy actions 2, 3, and 4 turns ahead.
- Retain: prepare cards for a stronger future turn.
- Decrees: prepare commands, then execute them without spending Energy.
- Character cards, powers, relics, events, and localization in Korean, English, and Simplified Chinese.

## Requirements

- Slay the Spire 2 public branch, version `0.107.1` or later.
- BaseLib `3.3.6` or later.
- .NET 9 SDK and Godot 4.5.1 Mono to build from source.

## Build

1. Close Slay the Spire 2.
2. Copy `Directory.Build.props.example` to `Directory.Build.props`, then set the local Godot 4.5.1 Mono executable path.
3. Open this folder in VS Code.
4. Run `BuildMod.bat`.
4. The build deploys `Master_of_Mankind.dll`, `Master_of_Mankind.pck`, and `Master_of_Mankind.json` to the game's `mods/Master_of_Mankind` folder.

### Slim Build

Run `BuildSlimMod.bat` (or `python BuildSlimMod.py`) to create the recommended smaller PCK. It keeps runtime scenes, card art, localization, animations, and UI assets, while excluding editor caches, temporary working files, unused character-select source frames, and unused background variants.

Set `STS2_MOD_DIR` before running it when the game uses a different mod directory.

## Release Contents

A mod release should contain only these runtime files in a `Master_of_Mankind` folder:

- `Master_of_Mankind.dll`
- `Master_of_Mankind.pck`
- `Master_of_Mankind.json`

## Project Status

Current version: `v0.1.2`

## Steam Workshop

The published mod is available on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3766953732).

This is an unofficial fan project. Slay the Spire and Slay the Spire 2 are properties of Mega Crit. Warhammer 40,000 names, settings, and trademarks are properties of Games Workshop. This project is not affiliated with or endorsed by either company.

Before making a public repository or release, verify that every included visual asset may be redistributed. Do not publish original game files or third-party artwork without permission.
