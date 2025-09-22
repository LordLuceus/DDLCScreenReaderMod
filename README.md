# DDLC Screen Reader Mod

A comprehensive accessibility mod for **Doki Doki Literature Club Plus** that provides screen reader support for blind and visually impaired players. This MelonLoader mod intercepts game text and outputs it to the Windows clipboard for screen reader access.

## Features

### Complete Game Coverage
- **Character Dialogue**: All character speech with speaker names
- **Menu Navigation**: Main menu, preferences, save/load screens
- **Poetry Minigame**: Word selection feedback and character reactions
- **File Browser**: Directory navigation with item counts and file operations
- **Side Stories**: Accessible launcher navigation and story selection
- **Poems**: Formatted poem content with proper line breaks
- **System Messages**: Game notifications and status updates

### Accessibility Features
- **Automatic Text Output**: All game text automatically sent to clipboard
- **Speaker Identification**: Character names always included in dialogue
- **Duplicate Prevention**: Smart filtering to avoid repeated announcements
- **Real-time Processing**: Text appears immediately as displayed in game
- **No Configuration Required**: Works out of the box with optimal settings

### Supported Text Types
- Dialogue (character speech)
- Menu choices and navigation
- Narrator text and descriptions
- Poetry game interactions
- File browser operations
- System messages and notifications

## Installation

### Prerequisites
- **Doki Doki Literature Club Plus** (Steam version)
- **MelonLoader** installed in DDLC Plus directory
- Windows operating system (for clipboard integration)

### Installation Steps
1. Download `DDLCScreenReaderMod.dll` from the latest release
2. Copy the DLL file to your DDLC Plus `Mods` folder:
   ```
   [DDLC Plus Install Path]\Mods\DDLCScreenReaderMod.dll
   ```
3. Launch DDLC Plus - MelonLoader will automatically load the mod

### Verification
- Check MelonLoader console for "DDLC Screen Reader Mod initialized!" message
- Start a new game or load a save - text should begin appearing in clipboard
- Use screen reader clipboard reading functionality to access game text

## Usage

### Basic Operation
The mod works automatically once installed. All game text is sent to the Windows clipboard and can be accessed using your screen reader's clipboard reading commands.

### Keyboard Shortcuts
- **R Key**: Repeat last dialogue (hardcoded hotkey)
- **Screen Reader Clipboard Commands**: Use your screen reader's standard clipboard reading functionality

### Text Format
- **Dialogue**: `[Character Name]: Dialogue text`
- **Menu Items**: `Menu option text`
- **Narrator**: `Descriptive text without speaker`
- **File Browser**: `Folder name (X items)` or `Filename`

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

### Dependencies
- .NET Framework 4.7.2
- MelonLoader framework
- Harmony patching library
- Unity Engine APIs
- DDLC/RenpyParser assemblies

### Project Structure
- `ScreenReaderMod.cs` - Main mod entry point and initialization
- `ClipboardUtils.cs` - Windows clipboard integration system
- `*Patches.cs` - Harmony patches for different game systems
- `TextProcessor.cs` - Text cleaning and formatting
- `SpeakerMapping.cs` - Character name resolution

## Technical Details

### How It Works
The mod uses **Harmony** to patch game methods that display text, intercepting UI updates before they reach the screen. Text is processed to remove Ren'Py formatting tags and sent to the Windows clipboard for screen reader access.

### Harmony Patches
- **DialoguePatches**: Character dialogue and narrative text
- **MenuPatches**: Menu navigation and UI interactions
- **LauncherPatches**: DDLC Plus launcher accessibility
- **FileBrowserPatches**: File system navigation
- **PoetryPatches**: Poetry minigame interactions
- **PoemPatches**: Poem reading and formatting
- **SideStoriesPatches**: Side story navigation
- **FileContentPatches**: File content accessibility

### Performance
- Minimal performance impact on game
- Efficient text processing with compiled regex patterns
- Smart duplicate filtering prevents excessive clipboard updates
- Background processing doesn't interfere with gameplay

## Compatibility

### Supported Versions
- **DDLC Plus**: All current versions
- **MelonLoader**: Compatible with standard MelonLoader installations
- **Screen Readers**: Works with NVDA, JAWS, and other Windows screen readers

### Known Limitations
- Windows-only (uses Windows clipboard APIs)
- Requires MelonLoader framework
- Some visual novel elements may need manual navigation

## Contributing

This mod is designed specifically for accessibility and follows these principles:
- All features permanently enabled for optimal accessibility
- Comprehensive text coverage across all game systems
- Clear, descriptive output suitable for screen readers
- Minimal user configuration to reduce complexity

## License

This project is an accessibility modification for educational and accessibility purposes. Please respect Team Salvato's intellectual property rights for the original game content.

## Support

For issues or questions:
- Check MelonLoader console for error messages
- Verify mod installation in correct Mods folder
- Ensure DDLC Plus and MelonLoader are properly installed
- Test with a fresh game save if problems persist

---

*This mod significantly improves the accessibility of Doki Doki Literature Club Plus for blind and visually impaired players by providing comprehensive screen reader support through Windows clipboard integration.*