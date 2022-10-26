namespace phat.Interface.CMD
{
    internal static class Settings
    {
        internal const String AppVersion = "v0.8";
        internal const string AppTitle = "Phat";
        internal const string AppDescription = "A console-based P2P chat application right to your terminal.";
        internal const string AppRepoURL = "https://github.com/gcbishal/phat";
        internal const int DefaultPort = 51123;
        internal const string IPAddressRegex = "^(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        internal static bool beepOnIncomingMessage = false;
    }
}
