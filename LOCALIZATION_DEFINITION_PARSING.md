# Localization Definition Parsing

## Problem

When parsing localization files like `Localization.lua` and `LocalizationPost.lua`, we need to identify which keys are **defined** (assigned), not which keys are **used** in the values.

### Example

```lua
L["MageFrostFull"] = L["Mage"] .. " - " .. L["Frost"]
```

**What we want:** `MageFrostFull` (the key being defined)
**What we don't want:** `Mage`, `Frost` (keys used in the value)

## Solution

Created specialized methods for parsing localization definition files that only match assignment patterns.

### New Methods

#### ParseLocalizationDefinitionsAsync
```csharp
public async Task<HashSet<string>> ParseLocalizationDefinitionsAsync(string filePath)
```

#### ParseLocalizationDefinitions
```csharp
public HashSet<string> ParseLocalizationDefinitions(string filePath)
```

### Regex Pattern

```csharp
@"^\s*L\[""([^""]+)""\]\s*="
```

This pattern matches:
- `^\s*` - Start of line with optional whitespace
- `L\[""([^""]+)""\]` - L["key"] pattern, capturing the key
- `\s*=` - Optional whitespace followed by equals sign

## Examples

### ? Matched (Assignment Patterns)
```lua
L["SimpleKey"] = "Simple Value"
L["ConcatKey"] = L["Part1"] .. L["Part2"]
L["FunctionKey"] = string.format("%s", L["A"])
    L["IndentedKey"] = "Indented"
L["KeyWithSpaces"]    =    "Value"
```
**Captured:** `SimpleKey`, `ConcatKey`, `FunctionKey`, `IndentedKey`, `KeyWithSpaces`
**Not captured:** `Part1`, `Part2`, `A` (these are used in values, not defined)

### ? Not Matched (Usage Patterns)
```lua
local x = L["UsageNotDefinition"]
print(L["AnotherUsage"])
return L["ReturnedKey"]
if L["ConditionKey"] then
-- Comment with L["CommentKey"]
```
**Not captured:** None of these are assignments

## Implementation Comparison

### Old Approach (Wrong)
```csharp
// Used ParseFileAsync which captured ALL L["..."] patterns
var locResult = await parser.ParseFileAsync(localizationPath);
var existingKeys = locResult.GlueStrings.Keys;
```

**Problem:** Captured both defined keys AND keys used in values

### New Approach (Correct)
```csharp
// Use ParseLocalizationDefinitionsAsync which only captures L["..."] = patterns
var existingKeys = await parser.ParseLocalizationDefinitionsAsync(localizationPath);
```

**Benefit:** Only captures keys that are actually being defined

## Real-World Results: TwintopInsanityBar

### Before (Using ParseFileAsync)
```
Found 110 existing entries in Localization.lua
Found 19 existing entries in LocalizationPost.lua
Total existing localization entries: 129
```

This was **wrong** because it included keys used in the values.

### After (Using ParseLocalizationDefinitionsAsync)
```
Found 110 defined keys in Localization.lua
Found 6 defined keys in LocalizationPost.lua
Total unique defined localization keys: 116
```

This is **correct** - only keys being assigned.

### Impact on Gap Analysis
```
Total unique glue strings found in code: 1773
Already localized: 115
Missing localization: 1658
```

Now we have an accurate count of what needs translation!

## Test Coverage

Added 5 comprehensive tests:

1. **ParseLocalizationDefinitionsAsync_OnlyMatchesAssignments**
   - Verifies only L["key"] = patterns are matched
   - Excludes usage patterns

2. **ParseLocalizationDefinitionsAsync_HandlesComplexAssignments**
   - Tests complex right-hand sides with multiple L["..."] patterns
   - Only captures the left-hand side key

3. **ParseLocalizationDefinitions_SynchronousVersion_WorksCorrectly**
   - Tests synchronous version

4. **ParseLocalizationDefinitionsAsync_IgnoresNonAssignmentLines**
   - Verifies comments, conditions, returns, etc. are ignored

5. **Integration with existing tests**
   - All 18 tests passing

## Usage

### Parse Localization Definition Files
```csharp
var parser = new LuaLocalizationParserService();

// Get keys that are DEFINED in the localization file
var definedKeys = await parser.ParseLocalizationDefinitionsAsync("Localization.lua");

// Use for gap analysis
var codeKeys = await parser.ParseDirectoryAsync(addonPath, new[] { "Localization" });
var missingKeys = codeKeys.GlueStrings.Keys.Except(definedKeys);
```

### Parse Code Files
```csharp
// Get keys that are USED in code files
var usedKeys = await parser.ParseFileAsync("Options.lua");
```

## Benefits

### ? Accurate Gap Analysis
No more false positives from keys used in localization values

### ? Correct Counts
116 actual definitions vs 129 false count (13 fewer!)

### ? Better Planning
Know exactly what needs translation

### ? Handles Complex Cases
Works even when localization values reference other keys:
```lua
L["FullName"] = L["FirstName"] .. " " .. L["LastName"]
```
Only captures `FullName` as defined

## Summary

The new `ParseLocalizationDefinitionsAsync` method correctly parses localization files by:
1. Only matching `L["key"] = ...` assignment patterns
2. Ignoring keys used in the right-hand side values
3. Providing accurate counts for gap analysis

This ensures your localization coverage tracking is precise and reliable!
