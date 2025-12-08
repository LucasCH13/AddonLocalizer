# Dependency Injection Setup for AddonLocalizer.Core

## Overview

The `AddonLocalizer.Core` project now includes full dependency injection support through the `ServiceCollectionExtensions` class.

## Usage

### In a .NET MAUI Application

Add the following to your `MauiProgram.cs`:

```csharp
using AddonLocalizer.Core;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // ... other MAUI configuration ...
        
        // Register AddonLocalizer.Core services
        builder.Services.AddAddonLocalizerCore();
        
        return builder.Build();
    }
}
```

### In a Console Application or Other .NET Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using AddonLocalizer.Core;

var services = new ServiceCollection();
services.AddAddonLocalizerCore();

var serviceProvider = services.BuildServiceProvider();
var parser = serviceProvider.GetRequiredService<LuaLocalizationParser>();
```

## Registered Services

The `AddAddonLocalizerCore()` extension method registers the following services:

| Service | Implementation | Lifetime | Description |
|---------|---------------|----------|-------------|
| `IFileSystemService` | `FileSystemService` | **Singleton** | Provides file system operations (read files, check existence, etc.) |
| `LuaLocalizationParser` | `LuaLocalizationParser` | **Transient** | Parses Lua files to extract localization strings |

### Service Lifetimes Explained

- **Singleton** (`IFileSystemService`): A single instance is created and shared across the entire application. This is appropriate since file system operations are stateless.

- **Transient** (`LuaLocalizationParser`): A new instance is created each time it's requested. This allows for concurrent parsing operations without shared state.

## Consuming Services

### Constructor Injection (Recommended)

```csharp
public class MyViewModel
{
    private readonly LuaLocalizationParser _parser;
    
    public MyViewModel(LuaLocalizationParser parser)
    {
        _parser = parser;
    }
    
    public async Task ScanAddonAsync(string addonPath)
    {
        var result = await _parser.ParseDirectoryDetailedAsync(addonPath);
        // Process results...
    }
}
```

### Direct Service Provider Resolution

```csharp
var parser = serviceProvider.GetRequiredService<LuaLocalizationParser>();
var result = await parser.ParseDirectoryDetailedAsync(@"C:\WoW\AddOns\MyAddon");
```

## Manual Instantiation (Without DI)

If you prefer not to use dependency injection, you can still create instances manually:

```csharp
// Uses default FileSystemService
var parser = new LuaLocalizationParser();

// Or with custom IFileSystemService (e.g., for testing)
var mockFileSystem = new Mock<IFileSystemService>();
var parser = new LuaLocalizationParser(mockFileSystem.Object);
```

## Testing with Dependency Injection

The DI setup makes testing easier by allowing you to inject mocked services:

```csharp
[Fact]
public void TestWithMockedFileSystem()
{
    var services = new ServiceCollection();
    
    // Replace the file system service with a mock
    var mockFileSystem = new Mock<IFileSystemService>();
    services.AddSingleton<IFileSystemService>(mockFileSystem.Object);
    
    // Register the parser (it will receive the mocked file system)
    services.AddTransient<LuaLocalizationParser>();
    
    var serviceProvider = services.BuildServiceProvider();
    var parser = serviceProvider.GetRequiredService<LuaLocalizationParser>();
    
    // Parser now uses the mocked file system
}
```

## Architecture Benefits

1. **Testability**: Easy to mock dependencies for unit testing
2. **Flexibility**: Swap implementations without changing consuming code
3. **Maintainability**: Clear separation of concerns and dependencies
4. **Best Practices**: Follows .NET dependency injection patterns
5. **Framework Integration**: Works seamlessly with MAUI, ASP.NET Core, etc.

## Package Requirements

The following NuGet package is required:

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
```

This is automatically included when you reference `AddonLocalizer.Core`.

## Files

- **AddonLocalizer.Core\ServiceCollectionExtensions.cs** - DI registration extension methods
- **AddonLocalizer.Core\Interfaces\IFileSystemService.cs** - File system abstraction interface
- **AddonLocalizer.Core\Services\FileSystemService.cs** - File system implementation
- **AddonLocalizer.Core\Services\LuaLocalizationParser.cs** - Lua parser service
