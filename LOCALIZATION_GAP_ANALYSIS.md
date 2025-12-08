# Localization Gap Analysis Feature

## Overview

The parser now identifies which glue strings in your addon code are missing localization entries. This helps you quickly find what needs to be translated.

## How It Works

### Step 1: Parse Addon Code
Scans all `.lua` files in the addon directory (excluding the `Localization` folder):
```csharp
var result = await parser.ParseDirectoryAsync(addonPath, new[] { "Localization" });
```

### Step 2: Parse Existing Localizations
Reads the localization definition files to get existing entries:
- `Localization\Localization.lua` - World of Warcraft provided strings
- `Localization\LocalizationPost.lua` - Manually created strings

```csharp
var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

// Parse both localization files
var locResult = await parser.ParseFileAsync(localizationPath);
var locPostResult = await parser.ParseFileAsync(localizationPostPath);

// Collect all existing keys
existingKeys.UnionWith(locResult.GlueStrings.Keys);
existingKeys.UnionWith(locPostResult.GlueStrings.Keys);
```

### Step 3: Find the Gap
Filters out strings that already have localization:
```csharp
var missingKeys = result.GlueStrings
    .Where(kvp => !existingKeys.Contains(kvp.Key))
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
```

## Output Format

### Summary Statistics
```
Total unique glue strings found in code: 1773
Already localized: 120
Missing localization: 1653
```

### Missing Strings (Non-Concatenated)
Shows strings that need localization with their usage count:
```
L["AbilityThresholdLinesHeader"] ? 19 occurrence(s)
L["AddNewBarTextArea"] ? 1 occurrence(s)
L["AudioChannel"] ? 1 occurrence(s)
...
```

### Missing Strings (Concatenated)
Shows dynamically-keyed strings with their locations:
```
L["SomeKey" .. var] (3 occurrence(s)):
  - Options.lua:123
  - Init.lua:456
  - Helper.lua:789
```

## Real-World Example: TwintopInsanityBar

### Results
- **Total glue strings in code:** 1,773
- **Already localized:** 120 (6.8%)
  - 110 from `Localization.lua` (WoW provided strings)
  - 19 from `LocalizationPost.lua` (manually created)
  - 9 duplicates between the two files
- **Missing localization:** 1,653 (93.2%)

### Analysis
The addon has good coverage of core WoW strings but needs:
1. Translation of addon-specific UI elements
2. Translation of options/settings strings
3. Translation of variable/token descriptions

### Common Missing Patterns
- `L["BarText*"]` - Bar text configuration strings
- `L["BarTextVariable*"]` - Variable/token descriptions
- `L["AudioChannel*"]` - Audio settings
- `L["*Header"]` - Section headers
- `L["*Description"]` - Tooltip descriptions

## Usage

### Command Line
```bash
dotnet run --project ParserTest\ParserTest.csproj
```

### Programmatic
```csharp
var parser = new LuaLocalizationParserService();
var addonPath = @"C:\Path\To\Addon";

// Parse code (excluding localization folder)
var codeResult = await parser.ParseDirectoryAsync(addonPath, new[] { "Localization" });

// Parse existing localizations
var locPath = Path.Combine(addonPath, "Localization", "Localization.lua");
var locResult = await parser.ParseFileAsync(locPath);

// Find missing keys
var existingKeys = new HashSet<string>(locResult.GlueStrings.Keys);
var missingKeys = codeResult.GlueStrings
    .Where(kvp => !existingKeys.Contains(kvp.Key))
    .Select(kvp => kvp.Key);
```

## Benefits

### ? Prioritization
See which strings are used most frequently:
- `L["AudioOptionsHeader"]` ? 40 occurrences (high priority)
- `L["AddNewBarTextArea"]` ? 1 occurrence (lower priority)

### ? Coverage Tracking
Monitor localization progress:
- Current: 120/1773 (6.8%)
- Target: 1773/1773 (100%)

### ? Quality Assurance
Ensure no strings are missed:
- All code strings are checked
- No manual file comparison needed
- Case-insensitive matching (handles typos)

### ? Translation Planning
Export missing keys for translators:
```csharp
// Get all missing keys
var missingList = missingKeys
    .OrderBy(kvp => kvp.Key)
    .Select(kvp => $"L[\"{kvp.Key}\"] = \"{kvp.Key}\"");

// Write to file for translators
File.WriteAllLines("missing_translations.lua", missingList);
```

## Next Steps

### Generate Template File
Create a template with all missing strings:
```lua
-- Missing Translations
-- Total: 1653 strings

L["AbilityThresholdLinesHeader"] = "Ability Threshold Lines"
L["AddNewBarTextArea"] = "Add New Bar Text Area"
L["ArcaneCharge1"] = "Arcane Charge 1"
-- ... etc
```

### Export for Google Translate
Generate a simple list for batch translation:
```
AbilityThresholdLinesHeader
AddNewBarTextArea
ArcaneCharge1
...
```

### Track Progress
Run regularly to see coverage improve:
```
Week 1: 120/1773 (6.8%)
Week 2: 450/1773 (25.4%)
Week 3: 892/1773 (50.3%)
Week 4: 1773/1773 (100%) ?
```

## Summary

The localization gap analysis feature helps you:
1. **Identify** which strings need translation
2. **Prioritize** based on usage frequency
3. **Track** progress toward 100% coverage
4. **Generate** template files for translators

This turns a manual, error-prone process into an automated, reliable workflow.
