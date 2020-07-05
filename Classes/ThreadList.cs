using System;
using System.IO;
using System.Xml;

namespace JDP
{
    public static class ThreadList {
        private static readonly XmlTextWriter xtw;
        private static readonly object _threadListSync = new object();
        private static readonly string _threadListPath = Path.Combine(Settings.GetSettingsDirectory(), Settings.ThreadsFileName);

        static ThreadList() {
            try {
                //writer.Formatting = Formatting.Indented;
                //_tmpThreadsDoc.Save(writer);
                //writer.Flush();
                //writer.Close();
                xtw = new XmlTextWriter(_threadListPath, null);
            }
            catch { }
        }

        public static void Log(string logMessage) {
            lock (_threadListSync) {
                //try {
                //    xtw.WriteLine("[{0} - {1}]", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
                //    xtw.WriteLine(logMessage);
                //    xtw.WriteLine("-------------------------------");
                //    xtw.Flush();
                //}
                //catch { }
            }
        }
    }
}
