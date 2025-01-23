using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Linq;

namespace EasyScriptLauncher.Utils
{
    internal class Logger
    {
        const string LOG_FILE = "EasyScriptLauncher_Log.txt";
        const int LIMIT = 10;
        readonly string SEPARATOR = Environment.NewLine + Environment.NewLine;

        public Logger()
        {
            if (File.Exists(LOG_FILE))
            {
                var text = File.ReadAllText(LOG_FILE).Split(SEPARATOR).ToArray();
                if (text.Length > LIMIT)
                {
                    int amount = text.Length - LIMIT;
                    text = text.Skip(amount).ToArray();
                    File.WriteAllText(LOG_FILE, string.Join(SEPARATOR, text));
                }
                File.AppendAllText(LOG_FILE, Environment.NewLine);
            }
        }

        public void Log(string content)
        {
            string time = DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            string log = $"{time}: {content}";
            File.AppendAllText(LOG_FILE, log);
        }
    }
}
