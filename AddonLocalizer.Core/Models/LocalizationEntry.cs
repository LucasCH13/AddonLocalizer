namespace AddonLocalizer.Core.Models
{
    public class LocalizationEntry
    {
        public string GlueString { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public bool HasConcatenation { get; set; }
        public string FullLineText { get; set; } = string.Empty;
    }

    public class ParseResult
    {
        public HashSet<string> AllGlueStrings { get; set; } = new();
        public List<LocalizationEntry> ConcatenatedEntries { get; set; } = new();
        public List<LocalizationEntry> AllEntries { get; set; } = new();
    }
}
