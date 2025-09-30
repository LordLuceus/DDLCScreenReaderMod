# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Build Commands

- **Build the project**: `dotnet build` or `MSBuild DDLCScreenReaderMod.csproj`
- **Clean build**: `dotnet clean && dotnet build`
- **Release build**: `dotnet build --configuration Release`

### Testing Installation

- Copy `DDLCScreenReaderMod.dll` from `bin/Debug/net472/` or `bin/Release/net472/` to the DDLC Plus `Mods` folder (automatically done by post-build event)
- Launch DDLC Plus with MelonLoader to test

## Project Architecture

### Core Components

**ScreenReaderMod.cs** - Main mod entry point that inherits from MelonMod

- Initializes all systems on mod startup
- Handles scene loading events
- Manages application lifecycle
- Implements hotkeys:
  - **R key**: Repeat last dialogue
  - **C key**: Announce data collection percentage (in Settings app)
  - **P key**: Announce jukebox position (in Jukebox app)

**ClipboardUtils.cs** - Windows clipboard integration system

- Uses Unity's built-in APIs to output text to clipboard for screen readers
- Implements rate limiting to prevent duplicate outputs (0.5 second window)
- Formats text according to type (dialogue, menu, narrator, etc.)
- All text types always enabled with no character limits
- Speaker names always included in dialogue
- Maintains queue of messages processed by CoroutineManager

**CoroutineManager.cs** - Clipboard queue processor

- Unity MonoBehaviour that processes clipboard message queue
- Runs as singleton coroutine that persists across scenes
- Processes messages at 0.025 second intervals
- Prevents clipboard API blocking by queuing messages

**DescriptionManager.cs** - Centralized image description system

- Manages descriptions for all gallery image types
- Registers description dictionaries from ImageDescriptions/ folder
- Provides lookup methods for image descriptions by name or section
- Supports sections: Poems, Wallpapers, Secrets, Backgrounds, Promos, CGs, Sketches

### Harmony Patching Architecture

The mod uses Harmony to patch game methods and intercept text display across multiple patch files:

**DialoguePatches.cs** - Captures character dialogue and narrative text

- Patches `RenpyDialogScreenUI` for system messages
- Patches `DialogueLine` constructor and text setter for character dialogue
- Uses `SpeakerMapping` to convert character codes to readable names
- Implements duplicate dialogue filtering with time-based windows

**MenuPatches.cs** - Intercepts menu and UI interactions

- Patches choice menu display (`RenpyChoiceMenuUI`)
- Patches various menu screens (main, preferences, history, save/load)
- Handles button focus events for navigation announcements
- Announces slider values and toggle states in settings menus
- Uses `PreferenceTypeNames` to format setting values for screen readers

**LauncherPatches.cs** - Handles DDLC Plus launcher accessibility

- Patches launcher button selection and side story navigation
- Converts internal story codes to user-friendly names
- Manages confirmation dialog accessibility
- Monitors BIOS, boot sequence, and reset sequence text announcements

**FileBrowserPatches.cs** - File browser accessibility

- Patches file/folder selection announcements
- Provides directory change notifications with item counts
- Handles context menu operations (open, delete)

**FileContentPatches.cs** - File content accessibility

- Automatically opens viewed files in Notepad for screen reader access
- Extracts and processes TextAsset content
- Handles localization and line break conversion

**PoetryPatches.cs** - Poetry minigame accessibility

- Announces word selection and hover events
- Provides game progress feedback
- Shows character reactions to selected words

**PoemPatches.cs** - Poem reading accessibility

- Captures and formats poem content with proper line breaks
- Handles title and author information
- Preserves poem formatting while cleaning Ren'Py tags

**SideStoriesPatches.cs** - Side stories navigation

- Maps internal story codes to user-friendly titles
- Differentiates between story parts (Part 1, Part 2)
- Provides readable names for character exclusive stories

**GalleryPatches.cs** - Gallery navigation and image viewing

- Announces gallery section names and image selection
- Provides image descriptions from DescriptionManager
- Handles locked/unlocked status announcements
- Announces navigation within gallery sections

**ImagePatches.cs** - Background and CG image announcements

- Monitors scene background changes
- Announces current wallpaper in virtual desktop
- Provides context for visual changes during gameplay

**JukeboxPatches.cs** - Music player accessibility

- Announces track selection and playback state
- Provides track information (title, artist, album)
- Handles play/pause state changes

**MailPatches.cs** - Email application accessibility

- Announces email selection and content
- Handles email list navigation
- Provides sender and subject information

**SelectorPatches.cs** - Generic selector UI accessibility

- Handles arrow-based selection UI elements
- Announces selected options in various game screens

**TextPatches.cs** - Generic text UI monitoring

- Catches text changes in various UI components
- Provides fallback text capture for unmapped UI elements

### Text Processing System

**TextProcessor.cs** - Cleans and processes game text

- Removes comprehensive set of Ren'Py formatting tags (`{w}`, `[color]`, etc.)
- Handles HTML tags and special character unescaping
- Determines text type (narrative vs. dialogue vs. system messages)
- Uses compiled regex patterns for performance

**SpeakerMapping.cs** - Character name resolution

- Maps character codes (s, n, y, m, mc, dc) to full names
- Dynamically retrieves player's chosen name for MC
- Filters out unwanted text (developer commentary, missing localizations)
- Provides centralized character name management

**PreferenceTypeNames.cs** - Settings UI name formatting

- Converts internal preference type enums to human-readable names
- Formats slider values with appropriate units (percentage, etc.)
- Formats toggle states as "On" or "Off"

**SpecialPoemDescriptions.cs** - Special poem metadata

- Contains descriptions for special poem variants
- Loaded during mod initialization

### Text Output Types

The mod categorizes all text into types defined in `ClipboardUtils.cs`:

- **Dialogue**: Character speech with speaker names
- **MenuChoice**: Game choice options
- **Menu**: Menu navigation and screen names
- **SystemMessage**: Game system notifications
- **Narrator**: Descriptive/narrative text without speakers
- **PoetryGame**: Poetry minigame word selections and feedback
- **FileBrowser**: File browser navigation and operations
- **Poem**: Complete poem content with formatting
- **Settings**: Settings menu options and values
- **Mail**: Email application content
- **Jukebox**: Music player information

### Image Description System

The mod includes comprehensive image descriptions stored in `ImageDescriptions/` folder:

- **CGDescriptions.cs**: Character CG artwork descriptions
- **WallpaperDescriptions.cs**: Desktop wallpaper descriptions
- **PoemDescriptions.cs**: Poem background image descriptions
- **BackgroundDescriptions.cs**: Scene background descriptions
- **PromoDescriptions.cs**: Promotional artwork descriptions
- **SecretDescriptions.cs**: Hidden/secret image descriptions
- **SketchDescriptions.cs**: Character sketch descriptions

All descriptions are managed centrally by `DescriptionManager.cs` for consistent access.

## Dependencies

- **.NET Framework 4.7.2** target
- **MelonLoader** - Mod loading framework
- **Harmony** - Runtime method patching
- **Unity Engine** - Game engine APIs
- **DDLC/RenpyParser** - Game-specific assemblies
- **TextMeshPro** - UI text component access
- Decompiled DDLC assemblies are included in the project's parent directory for reference

## Development Guidelines

### Making Changes

- Always test with actual DDLC Plus installation
- Verify Harmony patches don't conflict with game updates
- Monitor MelonLoader console for errors during development
- All features are permanently enabled - no configuration system

### Adding New Text Sources

1. Identify the game method that displays the text
2. Create appropriate Harmony patch in relevant Patches file (or add to existing patch file)
3. Use `ClipboardUtils.OutputGameText()` with correct TextType
4. Test across different game scenarios
5. Ensure duplicate filtering works correctly if needed

### Adding New Image Descriptions

1. Identify the image asset name in the game
2. Add description to appropriate file in `ImageDescriptions/` folder
3. Description dictionary uses asset name as key
4. Verify description appears when image is viewed in gallery
5. Test with actual screen reader to ensure description is helpful

### Debugging

- Use MelonLoader console output to track patch application
- Monitor clipboard output for duplicate or missing text
- Check Windows Event Viewer for native clipboard API errors
- All logging is enabled by default

## Accessibility Focus

This mod's primary purpose is improving game accessibility for blind and visually impaired players. All changes should:

- Preserve or enhance screen reader compatibility
- Maintain clear, descriptive text output
- Avoid overwhelming users with excessive or duplicate announcements
- Provide comprehensive coverage of all game text and interactions