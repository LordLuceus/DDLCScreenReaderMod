# DDLCPlusAccess

A comprehensive accessibility mod for **Doki Doki Literature Club Plus** that provides screen reader support for blind and visually impaired players. This MelonLoader mod intercepts game text and outputs it directly to your screen reader using speech synthesis.

## Features

### Complete Game Coverage

- **Character Dialogue**: All character speech with localized speaker names
- **History Screen**: Navigate through dialogue history with Up/Down arrows
- **Menu Navigation**: Main menu, preferences, save/load screens
- **Poetry Minigame**: Word selection feedback and character reactions
- **File Browser**: Directory navigation with item counts and file operations
- **File Content Viewer**: Line-by-line navigation of text files with Up/Down arrows
- **Side Stories**: Accessible launcher navigation and story selection
- **Poems**: Formatted poem content including special poems with visual descriptions
- **Gallery**: Full descriptions for all CGs, wallpapers, poems, backgrounds, sketches, promotional art, and secret images
- **Mail App**: Email content and navigation
- **Jukebox**: Music player with track information and position announcements
- **System Messages**: Game notifications, boot sequences, and status updates

### Accessibility Features

- **Direct Speech Output**: Text spoken immediately via UnityAccessibilityLib
- **Speaker Identification**: Character names always included in dialogue
- **Duplicate Prevention**: Smart filtering to avoid repeated announcements
- **Real-time Processing**: Text appears immediately as displayed in game
- **Image Descriptions**: Comprehensive descriptions for all gallery content
- **No Configuration Required**: Works out of the box with optimal settings

### Keyboard Shortcuts

| Key         | Function                                                |
| ----------- | ------------------------------------------------------- |
| **R**       | Repeat last dialogue                                    |
| **C**       | Announce data collection percentage (Settings app only) |
| **P**       | Announce current position in playlist (Jukebox only)    |
| **Up/Down** | Navigate history entries or file content lines          |

## Installation

### Prerequisites

- **Doki Doki Literature Club Plus** (Steam version)
- **MelonLoader** installed in DDLC Plus directory
- Windows operating system

### Installation Steps

1. Download `DDLCPlusAccess.dll` and `UnityAccessibilityLib.dll` from the latest release
2. Copy `DDLCPlusAccess.dll` to your DDLC Plus `Mods` folder:

   ```
   [DDLC Plus Install Path]\Mods\DDLCPlusAccess.dll
   ```

3. Copy `UnityAccessibilityLib.dll` to your DDLC Plus root folder:

   ```
   [DDLC Plus Install Path]\UnityAccessibilityLib.dll
   ```

4. Launch DDLC Plus - MelonLoader will automatically load the mod

### Verification

- Check MelonLoader console for "DDLCPlusAccess initialized!" message
- Start a new game or load a save - text should be spoken by your screen reader
- Press **R** to repeat the last dialogue

## Usage

### Basic Operation

The mod works automatically once installed. All game text is spoken directly to your screen reader.

### History Navigation

When viewing dialogue history (accessible via the game's history button), use Up/Down arrow keys to navigate through previous dialogue entries.

### File Content Navigation

When viewing text files in the file browser, use Up/Down arrow keys to navigate line by line through the file content.

## Development

### Building the Mod

```bash
# Standard build
dotnet build

# Clean build
dotnet clean && dotnet build

# Release build
dotnet build --configuration Release
```

The build automatically copies the mod files to the game's Mods folder.

### Dependencies

- .NET Framework 4.7.2
- UnityAccessibilityLib (NuGet package)
- MelonLoader framework
- Harmony patching library
- Unity Engine APIs
- DDLC/RenpyParser assemblies

### Project Structure

- `Main.cs` - Main mod entry point and initialization
- `ClipboardUtils.cs` - Speech output wrapper for UnityAccessibilityLib
- `DescriptionManager.cs` - Centralized image description system
- `GameTextType.cs` - Text type definitions and mapping
- `TextHelper.cs` - Text cleaning and formatting utilities
- `SpeakerMapping.cs` - Character name resolution
- `Patches/` - Harmony patches for different game systems
- `ImageDescriptions/` - Gallery image description data

### Harmony Patches

| Patch File              | Purpose                               |
| ----------------------- | ------------------------------------- |
| `DialoguePatches.cs`    | Character dialogue and narrative text |
| `HistoryPatches.cs`     | History screen navigation             |
| `MenuPatches.cs`        | Menu navigation and UI interactions   |
| `LauncherPatches.cs`    | DDLC Plus launcher accessibility      |
| `FileBrowserPatches.cs` | File system navigation                |
| `FileContentPatches.cs` | File content viewing                  |
| `PoetryPatches.cs`      | Poetry minigame interactions          |
| `PoemPatches.cs`        | Poem reading and formatting           |
| `SideStoriesPatches.cs` | Side story navigation                 |
| `GalleryPatches.cs`     | Gallery navigation and descriptions   |
| `ImagePatches.cs`       | Background/CG announcements           |
| `JukeboxPatches.cs`     | Music player accessibility            |
| `MailPatches.cs`        | Email application                     |
| `SelectorPatches.cs`    | Generic selector UI                   |
| `TextPatches.cs`        | Fallback text capture                 |

## Compatibility

### Supported Versions

- **DDLC Plus**: All current versions
- **MelonLoader**: Compatible with standard MelonLoader installations
- **Screen Readers**: Works with NVDA, JAWS, Narrator, and other Windows screen readers via SAPI/UniversalSpeech

### Known Limitations

- Windows-only (uses Windows speech APIs)
- Requires MelonLoader framework

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues or questions:

- Check MelonLoader console for error messages
- Verify all DLL files are in the correct locations
- Ensure DDLC Plus and MelonLoader are properly installed
- Report issues at the project repository
