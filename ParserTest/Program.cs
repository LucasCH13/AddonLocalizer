using AddonLocalizer.Core.Services;

var parser = new LuaLocalizationParserService();
var addonPath = @"C:\World of Warcraft\_beta_\Interface\AddOns\TwintopInsanityBar";

try
{
    Console.WriteLine($"Parsing addon directory: {addonPath}");
    Console.WriteLine("Excluding: Localization subdirectory");
    Console.WriteLine(new string('=', 80));
    
    // Parse the main addon code (excluding Localization folder)
    var result = await parser.ParseDirectoryAsync(addonPath, new[] { "Localization" });
    
    // Parse the localization files to get DEFINED keys (left side of = only)
    var localizationPath = Path.Combine(addonPath, "Localization", "Localization.lua");
    var localizationPostPath = Path.Combine(addonPath, "Localization", "LocalizationPost.lua");
    
    var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    
    // Parse Localization.lua if it exists - only get defined keys
    if (File.Exists(localizationPath))
    {
        var definedKeys = await parser.ParseLocalizationDefinitionsAsync(localizationPath);
        existingKeys.UnionWith(definedKeys);
        Console.WriteLine($"Found {definedKeys.Count} defined keys in Localization.lua");
    }
    
    // Parse LocalizationPost.lua if it exists - only get defined keys
    if (File.Exists(localizationPostPath))
    {
        var definedKeys = await parser.ParseLocalizationDefinitionsAsync(localizationPostPath);
        existingKeys.UnionWith(definedKeys);
        Console.WriteLine($"Found {definedKeys.Count} defined keys in LocalizationPost.lua");
    }
    
    Console.WriteLine($"Total unique defined localization keys: {existingKeys.Count}");
    Console.WriteLine(new string('=', 80));
    
    // Filter out strings that already have localization
    var missingKeys = result.GlueStrings
        .Where(kvp => !existingKeys.Contains(kvp.Key))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    
    Console.WriteLine($"\nTotal unique glue strings found in code: {result.GlueStrings.Count}");
    Console.WriteLine($"Already localized: {result.GlueStrings.Count - missingKeys.Count}");
    Console.WriteLine($"Missing localization: {missingKeys.Count}");
    Console.WriteLine(new string('-', 80));
    
    if (missingKeys.Count == 0)
    {
        Console.WriteLine("\n? All glue strings are already localized!");
    }
    else
    {
        // Show non-concatenated strings that need localization
        var nonConcat = missingKeys.Values.Where(v => !v.HasConcatenation).ToList();
        if (nonConcat.Any())
        {
            Console.WriteLine($"\nNon-Concatenated Strings Needing Localization ({nonConcat.Count}):");
            Console.WriteLine("(Glue String ? Occurrence Count)");
            Console.WriteLine(new string('-', 80));
            
            foreach (var info in nonConcat.OrderBy(x => x.GlueString).Take(50))
            {
                Console.WriteLine($"  L[\"{info.GlueString}\"] ? {info.OccurrenceCount} occurrence(s)");
            }
            
            if (nonConcat.Count > 50)
            {
                Console.WriteLine($"  ... and {nonConcat.Count - 50} more");
            }
        }
        
        // Show concatenated strings with locations that need localization
        var concat = missingKeys.Values.Where(v => v.HasConcatenation).ToList();
        if (concat.Any())
        {
            Console.WriteLine($"\nConcatenated Strings Needing Localization ({concat.Count}):");
            Console.WriteLine("(Glue String ? Locations)");
            Console.WriteLine(new string('-', 80));
            
            foreach (var info in concat.OrderBy(x => x.GlueString).Take(20))
            {
                Console.WriteLine($"  L[\"{info.GlueString}\"] ({info.OccurrenceCount} occurrence(s)):");
                foreach (var location in info.Locations.Take(5))
                {
                    var fileName = Path.GetFileName(location.FilePath);
                    Console.WriteLine($"    - {fileName}:{location.LineNumber}");
                }
                if (info.Locations.Count > 5)
                {
                    Console.WriteLine($"    ... and {info.Locations.Count - 5} more locations");
                }
            }
            
            if (concat.Count > 20)
            {
                Console.WriteLine($"  ... and {concat.Count - 20} more concatenated strings");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nError: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
