namespace Green
{
    public sealed class SourceInfo
    {
        public SourceInfo(
            SourceType type,
            string fileName)
        {
            Type = type;
            FileName = fileName;
        }

        public SourceType Type { get; }
        public string FileName { get; }
    }
}
