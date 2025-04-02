using System;
using System.IO;
using System.Text;

namespace Automation.Utils.Helpers
{
    public class ScriptEditor
    {
        private readonly string _filePath;
        private string _tempFile = Path.GetTempFileName();

        public ScriptEditor(string filePath)
        {
            _filePath = filePath;
        }

        public void EditDelayTime(int newDelayInSeconds)
        {
            try
            {
                Encoding encoding = GetEncoding(_filePath);

                using (var sr = new StreamReader(_filePath, encoding))
                using (var sw = new StreamWriter(_tempFile, false, encoding))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim().Contains("#Delay before loop", StringComparison.OrdinalIgnoreCase))
                        {
                            line = $"Start-Sleep -Seconds {newDelayInSeconds} #Delay before loop";
                        }
                        sw.WriteLine(line);
                    }
                }

                File.Delete(_filePath);
                File.Move(_tempFile, _filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
        private Encoding GetEncoding(string filePath)
        {
            using (var reader = new StreamReader(filePath, true))
            {
                reader.Peek(); // you need to do an operation to force the StreamReader to detect the encoding
                return reader.CurrentEncoding;
            }
        }
    }
}
