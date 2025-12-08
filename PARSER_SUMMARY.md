# Lua Localization Parser - Summary

## Overview
The `LuaLocalizationParser` service has been successfully created to parse World of Warcraft addon Lua files and extract localization glue strings in the format `L["GlueStringName"]`.

## Project Structure

### New Projects Created
1. **AddonLocalizer.Core** - Shared class library (.NET 10) containing business logic
2. **AddonLocalizer.Tests** - xUnit test project with 24 passing tests

### Files Created
- `AddonLocalizer.Core\Services\LuaLocalizationParser.cs` - Main parser service
- `AddonLocalizer.Core\Services\LocalizationEntry.cs` - Model classes
- `AddonLocalizer.Tests\Services\LuaLocalizationParserTests.cs` - Comprehensive unit tests

## Features

### Basic Parsing (Simple API)
The parser provides simple methods that return just the glue string names:

```csharp
var parser = new LuaLocalizationParser();

// Async methods
HashSet<string> strings = await parser.ParseFileAsync(filePath);
HashSet<string> allStrings = await parser.ParseDirectoryAsync(directoryPath);

// Sync methods
HashSet<string> strings = parser.ParseFile(filePath);
HashSet<string> allStrings = parser.ParseDirectory(directoryPath);
```

### Detailed Parsing (Enhanced API)
The parser also provides detailed methods that track concatenation, file locations, and line numbers:

```csharp
var parser = new LuaLocalizationParser();

// Async detailed parsing
ParseResult result = await parser.ParseFileDetailedAsync(filePath);
ParseResult dirResult = await parser.ParseDirectoryDetailedAsync(directoryPath);

// Sync detailed parsing
ParseResult result = parser.ParseFileDetailed(filePath);
ParseResult dirResult = parser.ParseDirectoryDetailed(directoryPath);
```

### ParseResult Class
```csharp
public class ParseResult
{
    // All unique glue strings found
    public HashSet<string> AllGlueStrings { get; set; }
    
    // Only entries that have string concatenation (..)
    public List<LocalizationEntry> ConcatenatedEntries { get; set; }
    
    // All entries with full details
    public List<LocalizationEntry> AllEntries { get; set; }
}
```

### LocalizationEntry Class
```csharp
public class LocalizationEntry
{
    public string GlueString { get; set; }        // e.g., "WelcomeMessage"
    public string FilePath { get; set; }           // Full file path
    public int LineNumber { get; set; }            // 1-based line number
    public bool HasConcatenation { get; set; }     // True if line contains ".."
    public string FullLineText { get; set; }       // Complete line text (trimmed)
}
```

## Concatenation Detection
The parser identifies lines with string concatenation using the `..` operator:

**Example Lua Code:**
```lua
local msg1 = L["Part1"] .. L["Part2"]  -- HasConcatenation = true
local msg2 = L["SimpleMessage"]        -- HasConcatenation = false
```

**Result:**
- `AllGlueStrings`: {"Part1", "Part2", "SimpleMessage"}
- `ConcatenatedEntries`: 2 entries (Part1 and Part2, both from line 1)
- `AllEntries`: 3 entries total

## Capabilities

? Extracts all `L["key"]` patterns from Lua files
? Recursively scans directories for .lua files
? Identifies entries with string concatenation
? Tracks file path and line number for each entry
? Handles special characters in glue strings (underscores, dashes, spaces, etc.)
? Returns unique glue strings (no duplicates)
? Provides both synchronous and asynchronous APIs
? Both simple and detailed parsing options

## Test Coverage

24 comprehensive unit tests covering:
- Single and multiple localization strings
- Duplicate handling
- Complex Lua code patterns
- Special characters in glue strings
- Directory and subdirectory scanning
- File type filtering (.lua only)
- Error handling (missing files/directories)
- Concatenation detection
- Line number tracking
- File path tracking
- Full line text capture

## Usage Example

```csharp
var parser = new LuaLocalizationParser();

// Get detailed results with concatenation tracking
var result = await parser.ParseDirectoryDetailedAsync(@"C:\WoW\Interface\AddOns\MyAddon");

Console.WriteLine($"Total unique strings: {result.AllGlueStrings.Count}");
Console.WriteLine($"Concatenated entries: {result.ConcatenatedEntries.Count}");

// List all concatenated entries
foreach (var entry in result.ConcatenatedEntries)
{
    Console.WriteLine($"  {entry.GlueString} at {Path.GetFileName(entry.FilePath)}:{entry.LineNumber}");
    Console.WriteLine($"    {entry.FullLineText}");
}
```

## Next Steps
This foundation is ready for building upon to:
- Track which strings have translations in different languages
- Compare human translations vs Google Translate
- Generate reports of missing translations
- Export/import localization data
