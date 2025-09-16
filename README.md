# DDLC Screen Reader Accessibility Mod

A MelonLoader mod for Doki Doki Literature Club Plus that makes the game accessible to blind and visually impaired players by outputting game text to the Windows clipboard for screen reader consumption.

## Features

- **Dialogue Text Capture**: Captures character dialogue with speaker names
- **Menu Navigation**: Announces menu options and navigation elements
- **Choice Selection**: Reads available choices during decision points
- **System Messages**: Outputs system notifications and status messages
- **Narrator Text**: Captures narrative and descriptive text
- **Configurable Settings**: Customizable output options via JSON configuration

## Installation

1. Install MelonLoader for DDLC Plus
2. Copy `DDLCScreenReaderMod.dll` to your DDLC Plus `Mods` folder
3. Launch the game - the mod will create a configuration file automatically

## Configuration

The mod creates a configuration file at:
`%USERPROFILE%\AppData\LocalLow\VRChat\VRChat\UserData\DDLCScreenReaderMod\config.json`

### Configuration Options

```json
{
  "enableDialogue": true,
  "enableMenus": true,
  "enableChoices": true,
  "enableSystemMessages": true,
  "enableNarrator": true,
  "includeSpeakerNames": true,
  "clipboardUpdateDelayMs": 100,
  "enableLogging": true,
  "enableVerboseLogging": false,
  "filterDuplicateText": true,
  "maxTextLength": 1000
}
```

### Configuration Details

- `enableDialogue`: Enable/disable character dialogue output
- `enableMenus`: Enable/disable menu navigation announcements
- `enableChoices`: Enable/disable choice selection output
- `enableSystemMessages`: Enable/disable system message output
- `enableNarrator`: Enable/disable narrator text output
- `includeSpeakerNames`: Include character names with dialogue
- `clipboardUpdateDelayMs`: Minimum delay between clipboard updates (milliseconds)
- `enableLogging`: Enable basic logging to MelonLoader console
- `enableVerboseLogging`: Enable detailed debug logging
- `filterDuplicateText`: Prevent duplicate text from being output repeatedly
- `maxTextLength`: Maximum length of text sent to clipboard (longer text is truncated)

## How It Works

The mod uses Harmony patches to intercept text display methods in the game and automatically outputs the text to the Windows clipboard. Screen readers can then read this text aloud as it appears.

### Text Types

- **Dialogue**: `"Character Name: Dialog text"`
- **Menu Choices**: `"Choice: Option text"`
- **Menu Navigation**: `"Menu: Menu name"`
- **System Messages**: `"System: Message text"`
- **Narrator**: Descriptive text without prefixes

## Screen Reader Setup

1. Configure your screen reader to monitor clipboard changes
2. For NVDA users:
   - Install the "Clipboard Monitor" add-on
   - Or use NVDA+C to manually check clipboard content
3. For JAWS users:
   - Enable clipboard monitoring in settings
4. For other screen readers, refer to their documentation for clipboard monitoring

## Troubleshooting

### Text Not Appearing
- Check that the mod is properly loaded (look for initialization message in MelonLoader console)
- Verify configuration file settings
- Ensure screen reader is monitoring clipboard changes

### Too Much/Too Little Text
- Adjust configuration settings to enable/disable specific text types
- Modify `clipboardUpdateDelayMs` to change update frequency
- Use `filterDuplicateText` to reduce repetitive output

### Performance Issues
- Increase `clipboardUpdateDelayMs` value
- Disable verbose logging
- Reduce `maxTextLength` for shorter text

## Compatibility

- Requires MelonLoader 0.6.1 or later
- Compatible with DDLC Plus
- Tested with Windows screen readers (NVDA, JAWS, Windows Narrator)

## Support

This mod is designed to improve accessibility for blind and visually impaired players. For issues or suggestions, please check the configuration options first, then consult the troubleshooting section.

## Technical Details

The mod uses:
- **Harmony patches** to intercept game text methods
- **Windows clipboard API** for text output
- **JSON configuration** for customizable settings
- **Text processing** to clean formatting tags and markup
- **Rate limiting** to prevent spam and improve performance

## License

This accessibility mod is provided free of charge to improve game accessibility for disabled players.