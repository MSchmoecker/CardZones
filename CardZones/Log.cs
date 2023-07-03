namespace CardZones {
    /// <summary>
    /// Helper class for properly logging from static contexts.
    /// </summary>
    internal static class Log {
        internal static ModLogger _logSource;

        internal static void Init(ModLogger logSource) {
            _logSource = logSource;
        }

        internal static void LogDebug(string data) => _logSource.Log(data);
        internal static void LogWarning(string data) => _logSource.LogWarning(data);
        internal static void LogError(string data) => _logSource.LogError(data);
        internal static void LogException(string data) => _logSource.LogException(data);
        public static void LogCodeInstruction(string data) => _logSource.Log(data);
    }
}
