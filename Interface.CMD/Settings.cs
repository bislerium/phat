using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phat.Interface.CMD
{
    internal static class Settings
    {
        internal const string AppTitle = "Phat";
        internal const string AppDescription = "A console-based P2P Chat Application right to your terminal.";
        internal const int DefaultPort = 51123;
        internal static bool beepOnIncomingMessage = false;
    }
}
