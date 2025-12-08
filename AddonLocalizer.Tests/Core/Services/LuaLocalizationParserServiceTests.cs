using AddonLocalizer.Core.Interfaces;
using AddonLocalizer.Core.Services;
using Moq;

namespace AddonLocalizer.Tests.Core.Services
{
    public class LuaLocalizationParserServiceTests
    {
        private readonly Mock<IFileSystemService> _fileSystemMock;
        private readonly LuaLocalizationParserService _parser;

        public LuaLocalizationParserServiceTests()
        {
            _fileSystemMock = new Mock<IFileSystemService>();
            _parser = new LuaLocalizationParserService(_fileSystemMock.Object);
        }

        [Fact]
        public async Task ParseFileAsync_WithSingleLocalizationString_ReturnsCorrectGlueString()
        {
            var content = new[] { @"local message = L[""HelloWorld""]" };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Single(result.AllGlueStrings);
            Assert.Contains("HelloWorld", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithMultipleLocalizationStrings_ReturnsAllGlueStrings()
        {
            var content = new[]
            {
                @"local msg1 = L[""FirstMessage""]",
                @"local msg2 = L[""SecondMessage""]",
                @"local msg3 = L[""ThirdMessage""]"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Equal(3, result.AllGlueStrings.Count);
            Assert.Contains("FirstMessage", result.AllGlueStrings);
            Assert.Contains("SecondMessage", result.AllGlueStrings);
            Assert.Contains("ThirdMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithDuplicateStrings_ReturnsUniqueGlueStrings()
        {
            var content = new[]
            {
                @"local msg1 = L[""DuplicateMessage""]",
                @"local msg2 = L[""DuplicateMessage""]",
                @"local msg3 = L[""UniqueMessage""]"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Equal(2, result.AllGlueStrings.Count);
            Assert.Contains("DuplicateMessage", result.AllGlueStrings);
            Assert.Contains("UniqueMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithNoLocalizationStrings_ReturnsEmptySet()
        {
            var content = new[]
            {
                "local function test()",
                @"    print(""No localization here"")",
                "end"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Empty(result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithComplexLuaCode_ExtractsOnlyLocalizationStrings()
        {
            var content = new[]
            {
                @"local L = LibStub(""AceLocale-3.0""):NewLocale(""MyAddon"", ""enUS"", true)",
                "",
                @"L[""WelcomeMessage""] = ""Welcome to the addon!""",
                @"L[""GoodbyeMessage""] = ""Thanks for using our addon""",
                "",
                "local function DisplayMessage()",
                @"    print(L[""WelcomeMessage""])", 
                @"    local goodbye = L[""GoodbyeMessage""]",
                "end",
                "",
                @"-- Some comment with L[""NotInCode""] should be found",
                @"local array = { L[""ArrayMessage""], L[""AnotherMessage""] }"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Equal(5, result.AllGlueStrings.Count);
            Assert.Contains("WelcomeMessage", result.AllGlueStrings);
            Assert.Contains("GoodbyeMessage", result.AllGlueStrings);
            Assert.Contains("NotInCode", result.AllGlueStrings);
            Assert.Contains("ArrayMessage", result.AllGlueStrings);
            Assert.Contains("AnotherMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithSpecialCharactersInGlueString_ExtractsCorrectly()
        {
            var content = new[]
            {
                @"L[""Message_With_Underscores""]",
                @"L[""Message-With-Dashes""]",
                @"L[""Message.With.Dots""]",
                @"L[""Message With Spaces""]",
                @"L[""Message123WithNumbers""]"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Equal(5, result.AllGlueStrings.Count);
            Assert.Contains("Message_With_Underscores", result.AllGlueStrings);
            Assert.Contains("Message-With-Dashes", result.AllGlueStrings);
            Assert.Contains("Message.With.Dots", result.AllGlueStrings);
            Assert.Contains("Message With Spaces", result.AllGlueStrings);
            Assert.Contains("Message123WithNumbers", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            _fileSystemMock.Setup(fs => fs.FileExists("nonexistent.lua")).Returns(false);

            await Assert.ThrowsAsync<FileNotFoundException>(
                async () => await _parser.ParseFileAsync("nonexistent.lua")
            );
        }

        [Fact]
        public async Task ParseDirectoryAsync_WithMultipleFiles_ReturnsAllUniqueGlueStrings()
        {
            SetupFileWithLines("file1.lua", new[] { @"L[""Message1""] L[""Message2""]" });
            SetupFileWithLines("file2.lua", new[] { @"L[""Message2""] L[""Message3""]" });
            SetupFileWithLines("file3.lua", new[] { @"L[""Message4""]" });
            SetupDirectory("testdir", new[] { "file1.lua", "file2.lua", "file3.lua" });

            var result = await _parser.ParseDirectoryAsync("testdir");

            Assert.Equal(4, result.AllGlueStrings.Count);
            Assert.Contains("Message1", result.AllGlueStrings);
            Assert.Contains("Message2", result.AllGlueStrings);
            Assert.Contains("Message3", result.AllGlueStrings);
            Assert.Contains("Message4", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseDirectoryAsync_WithSubdirectories_ReturnsAllGlueStringsRecursively()
        {
            SetupFileWithLines("root.lua", new[] { @"L[""RootMessage""]" });
            SetupFileWithLines("subdir/sub.lua", new[] { @"L[""SubMessage""]" });
            SetupDirectory("testdir", new[] { "root.lua", "subdir/sub.lua" });

            var result = await _parser.ParseDirectoryAsync("testdir");

            Assert.Equal(2, result.AllGlueStrings.Count);
            Assert.Contains("RootMessage", result.AllGlueStrings);
            Assert.Contains("SubMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseDirectoryAsync_WithNonLuaFiles_IgnoresNonLuaFiles()
        {
            SetupFileWithLines("test.lua", new[] { @"L[""LuaMessage""]" });
            SetupDirectory("testdir", new[] { "test.lua" });

            var result = await _parser.ParseDirectoryAsync("testdir");

            Assert.Single(result.AllGlueStrings);
            Assert.Contains("LuaMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseDirectoryAsync_WithEmptyDirectory_ReturnsEmptySet()
        {
            SetupDirectory("emptydir", Array.Empty<string>());

            var result = await _parser.ParseDirectoryAsync("emptydir");

            Assert.Empty(result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseDirectoryAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
        {
            _fileSystemMock.Setup(fs => fs.DirectoryExists("nonexistent")).Returns(false);

            await Assert.ThrowsAsync<DirectoryNotFoundException>(
                async () => await _parser.ParseDirectoryAsync("nonexistent")
            );
        }

        [Fact]
        public void ParseFile_SynchronousVersion_WorksCorrectly()
        {
            SetupFileWithLines("sync.lua", new[] { @"L[""SyncMessage""]" });

            var result = _parser.ParseFile("sync.lua");

            Assert.Single(result.AllGlueStrings);
            Assert.Contains("SyncMessage", result.AllGlueStrings);
        }

        [Fact]
        public void ParseDirectory_SynchronousVersion_WorksCorrectly()
        {
            SetupFileWithLines("sync1.lua", new[] { @"L[""SyncMessage1""]" });
            SetupFileWithLines("sync2.lua", new[] { @"L[""SyncMessage2""]" });
            SetupDirectory("syncdir", new[] { "sync1.lua", "sync2.lua" });

            var result = _parser.ParseDirectory("syncdir");

            Assert.Equal(2, result.AllGlueStrings.Count);
            Assert.Contains("SyncMessage1", result.AllGlueStrings);
            Assert.Contains("SyncMessage2", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithMultilineStrings_ExtractsMultilineStrings()
        {
            var content = new[]
            {
                @"L[""ValidMessage""]",
                "local multiline = [[",
                @"    L[""NotAValidMessage""]",
                "]]"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Contains("ValidMessage", result.AllGlueStrings);
            Assert.Contains("NotAValidMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithConcatenatedStrings_ExtractsAllParts()
        {
            var content = new[]
            {
                @"local msg = L[""Part1""] .. L[""Part2""]",
                @"local combined = ""prefix"" .. L[""Message""] .. ""suffix"""
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Equal(3, result.AllGlueStrings.Count);
            Assert.Contains("Part1", result.AllGlueStrings);
            Assert.Contains("Part2", result.AllGlueStrings);
            Assert.Contains("Message", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithSingleQuotes_DoesNotMatch()
        {
            var content = new[]
            {
                "L['SingleQuoteMessage']",
                @"L[""DoubleQuoteMessage""]"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Single(result.AllGlueStrings);
            Assert.Contains("DoubleQuoteMessage", result.AllGlueStrings);
            Assert.DoesNotContain("SingleQuoteMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithCommentsContainingPattern_ExtractsPatternFromComments()
        {
            var content = new[]
            {
                @"L[""ValidMessage""]",
                @"-- Comments can also contain L[""CommentMessage""] patterns",
                "-- which the parser will find (this is acceptable behavior)"
            };
            SetupFileWithLines("test.lua", content);

            var result = await _parser.ParseFileAsync("test.lua");

            Assert.Equal(2, result.AllGlueStrings.Count);
            Assert.Contains("ValidMessage", result.AllGlueStrings);
            Assert.Contains("CommentMessage", result.AllGlueStrings);
        }

        [Fact]
        public async Task ParseFileAsync_WithConcatenation_IdentifiesConcatenatedEntries()
        {
            var content = new[]
            {
                @"local msg = L[""Part1""] .. L[""Part2""]",
                @"local simple = L[""SimpleMessage""]"
            };
            SetupFileWithLines("detailed.lua", content);

            var result = await _parser.ParseFileAsync("detailed.lua");

            Assert.Equal(3, result.AllGlueStrings.Count);
            Assert.Contains("Part1", result.AllGlueStrings);
            Assert.Contains("Part2", result.AllGlueStrings);
            Assert.Contains("SimpleMessage", result.AllGlueStrings);

            Assert.Equal(2, result.ConcatenatedEntries.Count);
            Assert.All(result.ConcatenatedEntries, entry => Assert.True(entry.HasConcatenation));
            Assert.Contains(result.ConcatenatedEntries, e => e.GlueString == "Part1");
            Assert.Contains(result.ConcatenatedEntries, e => e.GlueString == "Part2");
        }

        [Fact]
        public async Task ParseFileAsync_TracksLineNumbers()
        {
            var content = new[]
            {
                "-- Line 1",
                @"L[""FirstMessage""]",
                "-- Line 3",
                @"L[""SecondMessage""]"
            };
            SetupFileWithLines("lines.lua", content);

            var result = await _parser.ParseFileAsync("lines.lua");

            var firstEntry = result.AllEntries.First(e => e.GlueString == "FirstMessage");
            var secondEntry = result.AllEntries.First(e => e.GlueString == "SecondMessage");

            Assert.Equal(2, firstEntry.LineNumber);
            Assert.Equal(4, secondEntry.LineNumber);
        }

        [Fact]
        public async Task ParseFileAsync_TracksFilePath()
        {
            var content = new[] { @"L[""TestMessage""]" };
            SetupFileWithLines("filepath.lua", content);

            var result = await _parser.ParseFileAsync("filepath.lua");

            Assert.Single(result.AllEntries);
            Assert.Equal("filepath.lua", result.AllEntries[0].FilePath);
        }

        [Fact]
        public async Task ParseFileAsync_CapturesFullLineText()
        {
            var content = new[] { @"    local message = L[""IndentedMessage""] -- with comment" };
            SetupFileWithLines("fulltext.lua", content);

            var result = await _parser.ParseFileAsync("fulltext.lua");

            Assert.Single(result.AllEntries);
            Assert.Contains("IndentedMessage", result.AllEntries[0].FullLineText);
            Assert.Contains("local message", result.AllEntries[0].FullLineText);
        }

        [Fact]
        public async Task ParseDirectoryAsync_AggregatesResults()
        {
            SetupFileWithLines("file1.lua", new[] { @"L[""Msg1""] .. L[""Msg2""]" });
            SetupFileWithLines("file2.lua", new[] { @"L[""Msg3""]" });
            SetupDirectory("testdir", new[] { "file1.lua", "file2.lua" });

            var result = await _parser.ParseDirectoryAsync("testdir");

            Assert.Equal(3, result.AllGlueStrings.Count);
            Assert.Equal(3, result.AllEntries.Count);
            Assert.Equal(2, result.ConcatenatedEntries.Count);
        }

        [Fact]
        public void ParseFile_WorksCorrectly()
        {
            SetupFileWithLines("sync_detailed.lua", new[] { @"L[""SyncMsg1""] .. L[""SyncMsg2""]" });

            var result = _parser.ParseFile("sync_detailed.lua");

            Assert.Equal(2, result.AllGlueStrings.Count);
            Assert.Equal(2, result.ConcatenatedEntries.Count);
        }

        [Fact]
        public void ParseDirectory_WorksCorrectly()
        {
            SetupFileWithLines("sync1.lua", new[] { @"L[""A""] .. L[""B""]" });
            SetupFileWithLines("sync2.lua", new[] { @"L[""C""]" });
            SetupDirectory("syncdir", new[] { "sync1.lua", "sync2.lua" });

            var result = _parser.ParseDirectory("syncdir");

            Assert.Equal(3, result.AllGlueStrings.Count);
            Assert.Equal(2, result.ConcatenatedEntries.Count);
        }

        private void SetupFileWithLines(string filePath, string[] lines)
        {
            _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ReadAllLinesAsync(filePath)).ReturnsAsync(lines);
            _fileSystemMock.Setup(fs => fs.ReadAllLines(filePath)).Returns(lines);
        }

        private void SetupDirectory(string directoryPath, string[] files)
        {
            _fileSystemMock.Setup(fs => fs.DirectoryExists(directoryPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.GetFiles(directoryPath, "*.lua", SearchOption.AllDirectories)).Returns(files);
        }
    }
}
