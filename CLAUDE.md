# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Build Commands

- **Build the project**: `dotnet build` or `MSBuild DDLCScreenReaderMod.csproj`
- **Clean build**: `dotnet clean && dotnet build`
- **Release build**: `dotnet build --configuration Release`

### Testing Installation

- Post-build automatically copies `DDLCScreenReaderMod.dll` and `MelonAccessibilityLib.dll` to the DDLC Plus `Mods` folder
- Launch DDLC Plus with MelonLoader to test

## Project Architecture

### Core Components

**ScreenReaderMod.cs** - Main mod entry point (MelonMod)

- Initializes MelonAccessibilityLib (`SpeechManager`, `TextCleaner`, `AccessibilityLog`)
- Registers Ren'Py/TMP-specific text cleaning patterns with `TextCleaner.AddRegexReplacement()`
- Handles scene loading events
- Implements hotkeys:
  - **R key**: Repeat last dialogue (`SpeechManager.RepeatLast()`)
  - **C key**: Announce data collection percentage (in Settings app)
  - **P key**: Announce jukebox position (in Jukebox app)

**ClipboardUtils.cs** - Thin wrapper around MelonAccessibilityLib's SpeechManager

- `OutputGameText(speaker, text, textType)` → `SpeechManager.Output()` or `SpeechManager.Announce()`
- `OutputPoemText(text)` → `SpeechManager.Announce()`
- `RepeatCurrentDialogue()` → `SpeechManager.RepeatLast()`
- Maintains API compatibility with all patch files

**GameTextType.cs** - Text type constants and mapping

- Maps legacy `TextType` enum to library's `TextType` constants
- Defines custom types: PoetryGame, FileBrowser, Poem, Settings, Mail, Jukebox
- Provides `GetTextTypeNames()` for logging and `ShouldStoreForRepeat()` predicate

**MelonLoggerAdapter.cs** - Bridges MelonLoader logging to MelonAccessibilityLib

**TextHelper.cs** - Supplementary text processing utilities

- `CleanText()` - Wraps `TextCleaner.Clean()`
- `IsNarrativeText()` - Determines if text is narrative (no speaker)

**DescriptionManager.cs** - Centralized image description system

- Manages descriptions for gallery images (CGs, Wallpapers, Poems, Backgrounds, etc.)
- Provides lookup methods by image name or section

### Harmony Patching Architecture

The mod uses Harmony to patch game methods. All patches output text via `ClipboardUtils.OutputGameText()`:

| Patch File | Purpose |
|------------|---------|
| `DialoguePatches.cs` | Character dialogue and narrative text |
| `MenuPatches.cs` | Menu navigation, choices, settings |
| `LauncherPatches.cs` | DDLC Plus launcher accessibility |
| `FileBrowserPatches.cs` | File browser navigation |
| `FileContentPatches.cs` | File content viewing |
| `PoetryPatches.cs` | Poetry minigame |
| `PoemPatches.cs` | Poem reading |
| `SideStoriesPatches.cs` | Side stories navigation |
| `GalleryPatches.cs` | Gallery navigation and descriptions |
| `ImagePatches.cs` | Background/CG announcements |
| `JukeboxPatches.cs` | Music player |
| `MailPatches.cs` | Email application |
| `SelectorPatches.cs` | Generic selector UI |
| `TextPatches.cs` | Fallback text capture |

### Text Output Types

Defined in `GameTextType.cs` (legacy enum in same file for patch compatibility):

- **Dialogue** (0): Character speech with speaker names
- **Narrator** (1): Descriptive text without speakers
- **Menu** (2): Menu navigation
- **MenuChoice** (3): Game choices
- **SystemMessage** (4): System notifications
- **PoetryGame** (101): Poetry minigame
- **FileBrowser** (102): File browser
- **Poem** (103): Poem content
- **Settings** (104): Settings values
- **Mail** (105): Email content
- **Jukebox** (106): Music player info

### Text Cleaning

Text cleaning is handled by MelonAccessibilityLib's `TextCleaner` with custom patterns registered in `ScreenReaderMod.RegisterTextCleanerPatterns()`:

- Ren'Py tags: `{w}`, `{nw}`, `{clear}`, etc. (pattern: `\{[^}]*\}`)
- TMP square bracket tags: `[color=...]`, `[size=...]`, `[b]`, `[i]`, etc.
- Special replacements: keyboard sprite → "Press Enter to Apply"

## Dependencies

- **.NET Framework 4.7.2** target
- **MelonAccessibilityLib** (NuGet) - Speech output, text cleaning, duplicate detection
- **MelonLoader** - Mod loading framework
- **Harmony** - Runtime method patching
- **Unity Engine** - Game engine APIs
- **DDLC/RenpyParser** - Game-specific assemblies
- Decompiled DDLC assemblies in `../Decomp` folder for reference

## Development Guidelines

### Adding New Text Sources

1. Identify the game method that displays the text
2. Create Harmony patch in relevant Patches file
3. Call `ClipboardUtils.OutputGameText(speaker, text, TextType.X)`
4. Library handles text cleaning and duplicate detection automatically

### Adding New Image Descriptions

1. Identify the image asset name in the game
2. Add description to appropriate file in `ImageDescriptions/` folder
3. Dictionary key = asset name, value = description text

### Debugging

- Use MelonLoader console to monitor patch application and speech output
- Library logs all speech output with text type names
- All logging enabled by default

## Accessibility Focus

This mod improves game accessibility for blind and visually impaired players. All changes should:

- Preserve screen reader compatibility
- Maintain clear, descriptive text output
- Avoid excessive or duplicate announcements
