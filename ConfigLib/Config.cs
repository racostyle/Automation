namespace ConfigLib
{
    public class Config
    {
        public string? ScriptsFolder { get; set; }
        public bool SearchForScriptsRecursively { get; set; }
        public bool RunInSameWindow { get; set; }
        public bool HideWindow { get; set; }
        public bool TestBehaviour { get; set; }
        public bool LoadProfile { get; set; }
        public int DelayInMils { get; set; }
    }
}
