# DDLC Screen Reader Mod - Installation Instructions

## Prerequisites

This mod requires MelonLoader to function. 

### Installing MelonLoader

1. Download the MelonLoader Installer from: https://github.com/LavaGang/MelonLoader/releases/latest/
2. Run `MelonLoader.Installer.exe`
3. Click **SELECT** and navigate to your Doki Doki Literature Club Plus game folder
4. Select `Doki Doki Literature Club Plus.exe`
5. Click **INSTALL**
6. Launch the game once to let MelonLoader generate its folder structure, then close it

## Installing the Mod

1. Extract the mod's ZIP archive
2. Copy `DDLCScreenReaderMod.dll` from the archive
3. Navigate to your DDLC Plus installation folder
4. Open the `Mods` folder (created by MelonLoader)
5. Paste `DDLCScreenReaderMod.dll` into this folder

The mod will be active the next time you launch the game.

## Clipboard Reading Setup

The mod outputs text to your system clipboard. Enable automatic clipboard reading using the ClipReader or Autoclip add-ons for NVDA.

## Hotkeys

| Key | Function |
|-----|----------|
| **R** | Repeats the last dialogue line (main game and side stories) |
| **P** | Announces currently playing track (music player) |
| **C** | Reports data collection percentage (settings app) |

## Known Issues

### Settings App Navigation

In the desktop settings app, the Resolution and Language options may fail to announce when focused. The workaround: use left/right arrow keys to change values - these changes will be announced correctly.

This issue also affects the Language setting in the DDLC settings menu. The current selection won't announce on focus, but changing the value with arrow keys will read properly.
