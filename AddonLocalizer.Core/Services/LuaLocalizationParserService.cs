using System.Text.RegularExpressions;
using AddonLocalizer.Core.Interfaces;
using AddonLocalizer.Core.Models;

namespace AddonLocalizer.Core.Services
{
    public class LuaLocalizationParserService(IFileSystemService fileSystem) : ILuaLocalizationParserService
    {
        private static readonly Regex LocalizationPattern = new(@"L\[""([^""]+)""\]", RegexOptions.Compiled);
        private static readonly Regex ConcatenationPattern = new(@"\.\.", RegexOptions.Compiled);

        public LuaLocalizationParserService() : this(new FileSystemService())
        {
        }

        public async Task<ParseResult> ParseDirectoryAsync(string directoryPath)
        {
            if (!fileSystem.DirectoryExists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            var result = new ParseResult();
            var luaFiles = fileSystem.GetFiles(directoryPath, "*.lua", SearchOption.AllDirectories);

            foreach (var filePath in luaFiles)
            {
                var fileResult = await ParseFileAsync(filePath);
                
                foreach (var glueString in fileResult.AllGlueStrings)
                {
                    result.AllGlueStrings.Add(glueString);
                }
                
                result.AllEntries.AddRange(fileResult.AllEntries);
                result.ConcatenatedEntries.AddRange(fileResult.ConcatenatedEntries);
            }

            return result;
        }

        public async Task<ParseResult> ParseFileAsync(string filePath)
        {
            if (!fileSystem.FileExists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var result = new ParseResult();
            var lines = await fileSystem.ReadAllLinesAsync(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var lineNumber = i + 1;
                var matches = LocalizationPattern.Matches(line);

                if (matches.Count > 0)
                {
                    bool hasConcatenation = ConcatenationPattern.IsMatch(line);

                    foreach (Match match in matches)
                    {
                        if (match.Success && match.Groups.Count > 1)
                        {
                            var glueString = match.Groups[1].Value;
                            result.AllGlueStrings.Add(glueString);

                            var entry = new LocalizationEntry
                            {
                                GlueString = glueString,
                                FilePath = filePath,
                                LineNumber = lineNumber,
                                HasConcatenation = hasConcatenation,
                                FullLineText = line.Trim()
                            };

                            result.AllEntries.Add(entry);

                            if (hasConcatenation)
                            {
                                result.ConcatenatedEntries.Add(entry);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public ParseResult ParseDirectory(string directoryPath)
        {
            if (!fileSystem.DirectoryExists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            var result = new ParseResult();
            var luaFiles = fileSystem.GetFiles(directoryPath, "*.lua", SearchOption.AllDirectories);

            foreach (var filePath in luaFiles)
            {
                var fileResult = ParseFile(filePath);
                
                foreach (var glueString in fileResult.AllGlueStrings)
                {
                    result.AllGlueStrings.Add(glueString);
                }
                
                result.AllEntries.AddRange(fileResult.AllEntries);
                result.ConcatenatedEntries.AddRange(fileResult.ConcatenatedEntries);
            }

            return result;
        }

        public ParseResult ParseFile(string filePath)
        {
            if (!fileSystem.FileExists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var result = new ParseResult();
            var lines = fileSystem.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var lineNumber = i + 1;
                var matches = LocalizationPattern.Matches(line);

                if (matches.Count > 0)
                {
                    bool hasConcatenation = ConcatenationPattern.IsMatch(line);

                    foreach (Match match in matches)
                    {
                        if (match.Success && match.Groups.Count > 1)
                        {
                            var glueString = match.Groups[1].Value;
                            result.AllGlueStrings.Add(glueString);

                            var entry = new LocalizationEntry
                            {
                                GlueString = glueString,
                                FilePath = filePath,
                                LineNumber = lineNumber,
                                HasConcatenation = hasConcatenation,
                                FullLineText = line.Trim()
                            };

                            result.AllEntries.Add(entry);

                            if (hasConcatenation)
                            {
                                result.ConcatenatedEntries.Add(entry);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
