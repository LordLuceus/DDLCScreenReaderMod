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
- Hardcoded repeat dialogue hotkey: R key

**ClipboardUtils.cs** - Windows clipboard integration system

- Uses Unity's built-in APIs to output text to clipboard for screen readers
- Implements rate limiting to prevent duplicate outputs
- Formats text according to type (dialogue, menu, narrator, etc.)
- All text types always enabled with no character limits
- Speaker names always included in dialogue

### Harmony Patching Architecture

The mod uses Harmony to patch game methods and intercept text display across 8 patch files:

**DialoguePatches.cs** - Captures character dialogue and narrative text

- Patches `RenpyDialogScreenUI` for system messages
- Patches `DialogueLine` constructor and text setter for character dialogue
- Uses `SpeakerMapping` to convert character codes to readable names
- Implements duplicate dialogue filtering with time-based windows

**MenuPatches.cs** - Intercepts menu and UI interactions

- Patches choice menu display (`RenpyChoiceMenuUI`)
- Patches various menu screens (main, preferences, history, save/load)
- Handles button focus events for navigation announcements

**LauncherPatches.cs** - Handles DDLC Plus launcher accessibility

- Patches launcher button selection and side story navigation
- Converts internal story codes to user-friendly names
- Manages confirmation dialog accessibility
- Monitors BIOS and boot sequence text announcements

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
2. Create appropriate Harmony patch in relevant Patches file
3. Use `ClipboardUtils.OutputGameText()` with correct TextType
4. Test across different game scenarios

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