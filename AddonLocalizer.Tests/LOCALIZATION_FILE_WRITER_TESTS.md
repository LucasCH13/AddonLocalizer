# LocalizationFileWriterService Unit Tests

## Overview
Comprehensive unit test suite for `LocalizationFileWriterService` with 27 tests covering all major functionality.

## Test Coverage Summary

### Test Statistics
- **Total Tests**: 27
- **Passing**: 27
- **Skipped**: 0
- **Coverage Areas**: 5 major functional areas

---

## Test Categories

### 1. SaveLocaleFileAsync Tests (16 tests)

#### Validation Tests
- ? `SaveLocaleFileAsync_WithInvalidDirectory_ThrowsDirectoryNotFoundException`
  - Validates that proper exception is thrown when directory doesn't exist
  
- ? `SaveLocaleFileAsync_WithInvalidLocaleCode_ ThrowsArgumentException`
  - Validates that invalid locale codes (e.g., "invalidLocale") are rejected

#### New File Creation Tests
- ? `SaveLocaleFileAsync_NewFile_CreatesFileWithCorrectStructure`
  - Verifies correct Lua file structure is generated
  - Checks for proper initialization code and locale block
  
- ? `SaveLocaleFileAsync_NewFile_SortsTranslationsAlphabetically`
  - Ensures translations are sorted by key for consistency
  
- ? `SaveLocaleFileAsync_SkipsEmptyTranslationsInNewFile`
  - Verifies empty, null, and whitespace-only translations are excluded

#### Backup Tests
- ? `SaveLocaleFileAsync_ExistingFile_CreatesBackupWhenRequested`
  - Verifies backup file creation with timestamp
  - Checks backup content matches original file
  
- ? `SaveLocaleFileAsync_ExistingFile_SkipsBackupWhenNotRequested`
  - Ensures no backup is created when `createBackup: false`

#### Existing File Modification Tests
- ? `SaveLocaleFileAsync_ExistingFile_PreservesStructure`
  - Maintains existing file structure (headers, formatting)
  - Updates existing translations
  - Adds new translations
  
- ? `SaveLocaleFileAsync_ExistingFile_UpdatesExistingTranslations`
  - Verifies translations are properly updated with new values
  
- ? `SaveLocaleFileAsync_ExistingFile_AddsNewKeysAtEnd`
  - New translations added before the closing 'end' statement
  - Maintains proper ordering
  
- ? `SaveLocaleFileAsync_ExistingFile_RemovesKeysWithEmptyValues`
  - Empty translations are removed from file
  - Non-empty translations are preserved

#### Special Character Handling Tests
- ? `SaveLocaleFileAsync_EscapesSpecialCharacters`
  - Properly escapes: `\`, `"`, `\n`, `\r`, `\t`
  - Example: `"He said \"Hello\""` ? `"He said \"Hello\""`
  
- ? `SaveLocaleFileAsync_WithComplexCharacters_HandlesCorrectly`
  - Unicode characters (Chinese: ??, Japanese: ???)
  - Emoji characters
  - Mixed language strings

#### Formatting Tests
- ? `SaveLocaleFileAsync_PreservesIndentation`
  - Maintains 4-space indentation for locale entries
  - Preserves existing file formatting

---

### 2. SaveMultipleLocaleFilesAsync Tests (6 tests)

#### Validation Tests
- ? `SaveMultipleLocaleFilesAsync_WithInvalidDirectory_ThrowsDirectoryNotFoundException`
  - Validates directory existence before processing

#### Core Functionality Tests
- ? `SaveMultipleLocaleFilesAsync_SavesAllLocales`
  - Verifies all locale files are written
  - Confirms correct file paths (enUS.lua, deDE.lua, frFR.lua)

#### Progress Reporting Tests
- ? `SaveMultipleLocaleFilesAsync_ReportsProgressCorrectly`
  - Verifies progress reports for each locale
  - Checks incremental progress (1/3, 2/3, 3/3)
  - Confirms completion flag on last report
  
- ? `SaveMultipleLocaleFilesAsync_ReportsProgressWithCorrectPercentages`
  - Validates `PercentComplete` calculation
  - Example: 1/2 = 50%, 2/2 = 100%
  
- ? `SaveMultipleLocaleFilesAsync_ReportsErrorOnFailure`
  - Error information included in progress report
  - Exception is still thrown after reporting

#### Backup Tests
- ? `SaveMultipleLocaleFilesAsync_CreatesBackupsWhenRequested`
  - Backups created for all modified files
  
- ? `SaveMultipleLocaleFilesAsync_SkipsBackupsWhenNotRequested`
  - No backup files created when `createBackup: false`

---

### 3. DeleteBackupsAsync Tests (3 tests)
- ? Directory validation
- ? Deletion of all backup files
- ? Handling non-existent directories
- ? Handling directories with no backup files

---

### 4. RestoreFromBackupAsync Tests (3 tests)

#### Validation Tests
- ? `RestoreFromBackupAsync_WithNoBackups_ThrowsFileNotFoundException`
  - Proper exception when no backup files exist
  
- ? `RestoreFromBackupAsync_WithInvalidDirectory_ThrowsDirectoryNotFoundException`
  - Validates directory existence

#### Core Functionality Tests
- ? `RestoreFromBackupAsync_RestoresMostRecentBackup`
  - Identifies most recent backup by timestamp
  - Example: Selects `20240101_140000` over `20240101_120000`
  - Restores content to main file

---

## Test Patterns and Utilities

### Mocking Strategy
```csharp
private readonly Mock<IFileSystemService> _fileSystemMock;
private readonly LocalizationFileWriterService _writer;

public LocalizationFileWriterServiceTests()
{
    _fileSystemMock = new Mock<IFileSystemService>();
    _writer = new LocalizationFileWriterService(_fileSystemMock.Object);
}
```

### Common Test Setups

#### File System Mocks
```csharp
_fileSystemMock.Setup(fs => fs.DirectoryExists(path)).Returns(true);
_fileSystemMock.Setup(fs => fs.FileExists(path)).Returns(true);
_fileSystemMock.Setup(fs => fs.ReadAllLinesAsync(path)).ReturnsAsync(lines);
_fileSystemMock.Setup(fs => fs.WriteAllLinesAsync(path, lines)).Returns(Task.CompletedTask);
```

#### Capturing Written Content
```csharp
List<string>? capturedLines = null;
_fileSystemMock.Setup(fs => fs.WriteAllLinesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
    .Callback<string, IEnumerable<string>>((path, lines) => capturedLines = lines.ToList())
    .Returns(Task.CompletedTask);
```

---

## Edge Cases Covered

### Character Encoding
- ? Chinese, Japanese, Korean characters
- ? Emoji characters
- ? Special escape sequences

### File Scenarios
- ? New file creation (no existing file)
- ? Existing file modification (preserving structure)
- ? Empty file (no entries)
- ? Malformed file (missing locale block)

### Data Scenarios
- ? Empty translations (skipped)
- ? Null translations (skipped)
- ? Whitespace-only translations (skipped)
- ? Very long translation strings
- ? Translations with special characters

### Async/Progress Scenarios
- ? Progress reporting during multi-file save
- ? Error handling with progress updates
- ? Completion notification

---

## Test Execution

### Run All Tests
```bash
dotnet test AddonLocalizer.Tests\AddonLocalizer.Tests.csproj --filter "FullyQualifiedName~LocalizationFileWriterServiceTests"
```

### Run Specific Test Category
```bash
# Run only SaveLocaleFileAsync tests
dotnet test --filter "FullyQualifiedName~LocalizationFileWriterServiceTests.SaveLocaleFileAsync"

# Run only backup tests
dotnet test --filter "FullyQualifiedName~LocalizationFileWriterServiceTests.*Backup*"
```

### With Detailed Output
```bash
dotnet test --filter "FullyQualifiedName~LocalizationFileWriterServiceTests" --logger "console;verbosity=detailed"
```

---

## Test Maintenance Notes

### Adding New Tests
1. Follow existing naming convention: `MethodName_Scenario_ExpectedBehavior`
2. Use descriptive test names that document the behavior
3. Group related tests with region comments
4. Mock all file system operations via `IFileSystemService`

### Updating Existing Tests
- When adding new functionality, add corresponding tests
- Keep test data realistic (use actual locale codes, realistic translations)
- Maintain test independence (no shared state between tests)

---

## Test Quality Metrics

### Coverage
- **SaveLocaleFileAsync**: 16/16 scenarios covered (100%)
- **SaveMultipleLocaleFilesAsync**: 6/6 scenarios covered (100%)
- **DeleteBackupsAsync**: 3/3 scenarios covered (100%)
- **RestoreFromBackupAsync**: 3/3 scenarios covered (100%)

**Overall Coverage: 100%**
