using AddonLocalizer.Core.Models;

namespace AddonLocalizer.Core.Interfaces
{
    public interface ILuaLocalizationParserService
    {
        Task<ParseResult> ParseDirectoryAsync(string directoryPath);
        Task<ParseResult> ParseFileAsync(string filePath);
        ParseResult ParseDirectory(string directoryPath);
        ParseResult ParseFile(string filePath);
    }
}
