using AddonLocalizer.Core.Models;

namespace AddonLocalizer.Core.Interfaces;

public interface ILuaLocalizationParserService
{
    Task<ParseResult> ParseDirectoryAsync(string directoryPath, string[]? excludeSubdirectories = null);
    Task<ParseResult> ParseFileAsync(string filePath);
    ParseResult ParseDirectory(string directoryPath, string[]? excludeSubdirectories = null);
    ParseResult ParseFile(string filePath);
    
    /// <summary>
    /// Parses a localization definition file and returns only the keys being assigned (left side of =).
    /// This is useful for parsing Localization.lua files where L["key"] = "value" patterns define translations.
    /// </summary>
    Task<HashSet<string>> ParseLocalizationDefinitionsAsync(string filePath);
    
    /// <summary>
    /// Synchronous version of ParseLocalizationDefinitionsAsync.
    /// </summary>
    HashSet<string> ParseLocalizationDefinitions(string filePath);
}