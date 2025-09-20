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

**ClipboardUtils.cs** - Windows clipboard integration system

- Uses Unity's built-in APIs to output text to clipboard for screen readers
- Implements rate limiting to prevent duplicate outputs
- Formats text according to type (dialogue, menu, narrator, etc.)

**ModConfig.cs** - JSON-based configuration system

- Manages all user-configurable settings
- Auto-creates default config file in MelonLoader UserData directory
- Handles save/load operations with error handling

### Harmony Patching Architecture

The mod uses Harmony to patch game methods and intercept text display:

**DialoguePatches.cs** - Captures character dialogue and narrative text

- Patches `RenpyDialogScreenUI` for system messages
- Patches `DialogueLine` constructor and text setter for character dialogue
- Uses `SpeakerMapping` to convert character codes to readable names

**MenuPatches.cs** - Intercepts menu and UI interactions

- Patches choice menu display (`RenpyChoiceMenuUI`)
- Patches various menu screens (main, preferences, history, save/load)
- Handles button focus events for navigation announcements

**LauncherPatches.cs** - Handles DDLC Plus launcher accessibility

- Patches launcher button selection and side story navigation
- Converts internal story codes to user-friendly names
- Manages confirmation dialog accessibility

### Text Processing System

**TextProcessor.cs** - Cleans and processes game text

- Removes Ren'Py formatting tags (`{w}`, `[color]`, etc.)
- Handles text escaping and whitespace normalization
- Determines text type (narrative vs. dialogue)

**SpeakerMapping.cs** - Character name resolution

- Maps character codes (s, n, y, m, mc, dc) to full names
- Filters out unwanted text (developer commentary, missing localizations)
- Provides centralized character name management

### Text Output Types

The mod categorizes all text into types defined in `ClipboardUtils.cs`:

- **Dialogue**: Character speech with optional speaker names
- **MenuChoice**: Game choice options
- **Menu**: Menu navigation and screen names
- **SystemMessage**: Game system notifications
- **Narrator**: Descriptive/narrative text without speakers

## Dependencies

- **.NET Framework 4.7.2** target
- **MelonLoader** - Mod loading framework
- **Harmony** - Runtime method patching
- **Unity Engine** - Game engine APIs
- **DDLC/RenpyParser** - Game-specific assemblies
- **TextMeshPro** - UI text component access
- **Newtonsoft.Json** - Configuration serialization
- Decompiled DDLC assemblies are included in the project's parent directory for reference.

## Development Guidelines

### Making Changes

- Always test with actual DDLC Plus installation
- Verify Harmony patches don't conflict with game updates
- Test configuration loading/saving edge cases
- Monitor MelonLoader console for errors during development

### Adding New Text Sources

1. Identify the game method that displays the text
2. Create appropriate Harmony patch in relevant Patches file
3. Use `ClipboardUtils.OutputGameText()` with correct TextType
4. Add configuration option in `ModConfig.cs` if needed
5. Test with different config combinations

### Debugging

- Enable verbose logging in config for detailed patch execution logs
- Use MelonLoader console output to track patch application
- Monitor clipboard output for duplicate or missing text
- Check Windows Event Viewer for native clipboard API errors

## Accessibility Focus

This mod's primary purpose is improving game accessibility for blind and visually impaired players. All changes should:

- Preserve or enhance screen reader compatibility
- Maintain clear, descriptive text output
- Avoid overwhelming users with excessive or duplicate announcements
- Respect user configuration preferences for different text types
