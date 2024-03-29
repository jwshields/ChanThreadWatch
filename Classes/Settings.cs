﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace JDP {
    public static class Settings {
        private static Dictionary<string, string> _settings;

        public static string ApplicationName {
            get { return "Chan Thread Watch"; }
        }

        public static bool? UseCustomUserAgent {
            get { return GetBool("UseCustomUserAgent"); }
            set { SetBool("UseCustomUserAgent", value); }
        }

        public static string CustomUserAgent {
            get { return Get("CustomUserAgent"); }
            set { Set("CustomUserAgent", value); }
        }

        public static bool? UsePageAuth {
            get { return GetBool("UsePageAuth"); }
            set { SetBool("UsePageAuth", value); }
        }

        public static bool? SlowLoadThreads {
            get { return GetBool("SlowLoadThreads"); }
            set { SetBool("SlowLoadThreads", value); }
        }

        public static string PageAuth {
            get { return Get("PageAuth"); }
            set { Set("PageAuth", value); }
        }

        public static bool? UseImageAuth {
            get { return GetBool("UseImageAuth"); }
            set { SetBool("UseImageAuth", value); }
        }

        public static string ImageAuth {
            get { return Get("ImageAuth"); }
            set { Set("ImageAuth", value); }
        }

        public static bool? OneTimeDownload {
            get { return GetBool("OneTimeDownload"); }
            set { SetBool("OneTimeDownload", value); }
        }

        public static bool? AutoFollow {
            get { return GetBool("AutoFollow"); }
            set { SetBool("AutoFollow", value); }
        }

        public static int? CheckEvery {
            get { return GetInt("CheckEvery"); }
            set { SetInt("CheckEvery", value); }
        }

        public static bool? DownloadFolderIsRelative {
            get { return GetBool("DownloadFolderIsRelative"); }
            set { SetBool("DownloadFolderIsRelative", value); }
        }

        public static string DownloadFolder {
            get { return Get("DownloadFolder"); }
            set { Set("DownloadFolder", value); }
        }

        public static bool? CompletedFolderIsRelative {
            get { return GetBool("CompletedFolderIsRelative"); }
            set { SetBool("CompletedFolderIsRelative", value); }
        }

        public static string CompletedFolder {
            get { return Get("CompletedFolder"); }
            set { Set("CompletedFolder", value); }
        }
        
        public static bool? MoveToCompletedFolder {
            get { return GetBool("MoveToCompletedFolder"); }
            set { SetBool("MoveToCompletedFolder", value); }
        }

        public static bool? RenameDownloadFolderWithDescription {
            get { return GetBool("RenameDownloadFolderWithDescription"); }
            set { SetBool("RenameDownloadFolderWithDescription", value); }
        }

        public static bool? RenameDownloadFolderWithCategory {
            get { return GetBool("RenameDownloadFolderWithCategory"); }
            set { SetBool("RenameDownloadFolderWithCategory", value); }
        }

        public static bool? RenameDownloadFolderWithParentThreadDescription {
            get { return GetBool("RenameDownloadFolderWithParentThreadDescription"); }
            set { SetBool("RenameDownloadFolderWithParentThreadDescription", value); }
        }

        public static string ParentThreadDescriptionFormat {
            get { return Get("ParentThreadDescriptionFormat"); }
            set { Set("ParentThreadDescriptionFormat", value); }
        }

        public static bool? ChildThreadsAreNewFormat {
            get { return GetBool("ChildThreadsAreNewFormat"); }
            set { SetBool("ChildThreadsAreNewFormat", value); }
        }

        public static bool? SortImagesByPoster {
            get { return GetBool("SortImagesByPoster"); }
            set { SetBool("SortImagesByPoster", value); }
        }

        public static bool? RecursiveAutoFollow {
            get { return GetBool("RecursiveAutoFollow"); }
            set { SetBool("RecursiveAutoFollow", value); }
        }

        public static bool? InterBoardAutoFollow {
            get { return GetBool("InterBoardAutoFollow"); }
            set { SetBool("InterBoardAutoFollow", value); }
        }

        public static int? SaveThumbnails {
            get { return GetInt("SaveThumbnails"); }
            set { SetInt("SaveThumbnails", (int)value); }
        }

        public static bool? UseOriginalFileNames {
            get { return GetBool("UseOriginalFileNames"); }
            set { SetBool("UseOriginalFileNames", value); }
        }

        public static bool? VerifyImageHashes {
            get { return GetBool("VerifyImageHashes"); }
            set { SetBool("VerifyImageHashes", value); }
        }

        public static bool? UseSlug {
            get { return GetBool("UseSlug"); }
            set { SetBool("UseSlug", value); }
        }

        public static bool? SaveURLs {
            get { return GetBool("SaveURLs"); }
            set { SetBool("SaveURLs", value); }
        }

        public static SlugType SlugType {
            get {
                string value = Get("SlugType") ?? String.Empty;
                if (String.IsNullOrEmpty(value)) return SlugType.Last;
                SlugType valueSlug;
                try {
                    valueSlug = (SlugType)Enum.Parse(typeof (SlugType), value);
                }
                catch (ArgumentException) {
                    valueSlug = SlugType.Last;
                }
                return valueSlug;
            }
            set { Set("SlugType", value.ToString()); }
        }

        public static bool? CheckForUpdates {
            get { return GetBool("CheckForUpdates"); }
            set { SetBool("CheckForUpdates", value); }
        }

        public static DateTime? LastUpdateCheck {
            get { return GetDate("LastUpdateCheck"); }
            set { SetDate("LastUpdateCheck", value); }
        }

        public static string LatestUpdateVersion {
            get { return Get("LatestUpdateVersion"); }
            set { Set("LatestUpdateVersion", value); }
        }

        public static bool? BlacklistWildcards {
            get { return GetBool("BlacklistWildcards"); }
            set { SetBool("BlacklistWildcards", value); }
        }

        public static bool? MinimizeToTray {
            get { return GetBool("MinimizeToTray"); }
            set {  SetBool("MinimizeToTray", value); }
        }

        public static bool? BackupThreadList {
            get { return GetBool("BackupThreadList"); }
            set { SetBool("BackupThreadList", value); }
        }

        public static int? BackupEvery {
            get { return GetInt("BackupEvery"); }
            set { SetInt("BackupEvery", value); }
        }

        public static bool? BackupCheckSize {
            get { return GetBool("BackupCheckSize"); }
            set { SetBool("BackupCheckSize", value); }
        }

        public static int? ThreadStatusThreshold {
            get { return GetInt("ThreadStatusThreshold"); }
            set { SetInt("ThreadStatusThreshold", value); }
        }

        public static bool? ThreadStatusSimple {
            get { return GetBool("ThreadStatusSimple"); }
            set { SetBool("ThreadStatusSimple", value); }
        }

        public static long? MaximumBytesPerSecond {
            get { return GetLong("MaximumBytesPerSecond"); }
            set { SetLong("MaximumBytesPerSecond", value); }
        }
        
        public static string WindowTitle {
            get { return Get("WindowTitle"); }
            set { Set("WindowTitle", value); }
        }

        public static bool? UseExeDirectoryForSettings { get; set; }

        public static string ExeDirectory {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public static string AppDataDirectory {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName); }
        }

        public static string SettingsFileName {
            get { return "settings.xml"; }
        }

        public static string ThreadsFileName {
            get { return "threads.xml"; }
        }

        public static string LogFileName {
            get { return "log.txt"; }
        }

        public static string BlacklistFileName {
            get { return "blacklist.txt"; }
        }

        public static string DebugFolderName {
            get { return "Debug"; }
        }

        public static ThreadDoubleClickAction? OnThreadDoubleClick {
            get {
                int x = GetInt("OnThreadDoubleClick") ?? -1;
                return Enum.IsDefined(typeof (ThreadDoubleClickAction), x) ?
                    (ThreadDoubleClickAction?)x : null;
            }
            set { SetInt("OnThreadDoubleClick", value.HasValue ? (int?)value.Value : null); }
        }

        public static string GetSettingsDirectory() {
            if (UseExeDirectoryForSettings == null) {
                #if DEBUG
                    UseExeDirectoryForSettings = File.Exists(Path.Combine(Path.Combine(ExeDirectory, DebugFolderName), SettingsFileName)) || File.Exists(Path.Combine(Path.Combine(ExeDirectory, DebugFolderName), "settings.txt"));
                #else
                    UseExeDirectoryForSettings = File.Exists(Path.Combine(ExeDirectory, SettingsFileName)) || File.Exists(Path.Combine(ExeDirectory, "settings.txt"));
                #endif
            }
            return GetSettingsDirectory(UseExeDirectoryForSettings.Value);
        }

        public static string GetSettingsDirectory(bool useExeDirForSettings) {
            if (useExeDirForSettings) {
                #if DEBUG
                    return Path.Combine(ExeDirectory, DebugFolderName);
                #else
                    return ExeDirectory;
                #endif
            }
            else {
                #if DEBUG
                    string dir = Path.Combine(AppDataDirectory, DebugFolderName);
                #else
                    string dir = AppDataDirectory;
                #endif
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        public static string AbsoluteDownloadDirectory {
            get {
                #if DEBUG
                    string dir = Path.Combine(DownloadFolder, DebugFolderName);
                #else
                    string dir = DownloadFolder;
                #endif
                if (!String.IsNullOrEmpty(dir) && (DownloadFolderIsRelative == true)) {
                    dir = General.GetAbsoluteDirectoryPath(dir, ExeDirectory);
                }
                return dir;
            }
        }

        public static string AbsoluteCompletedDirectory {
            get {
                #if DEBUG
                    string dir = Path.Combine(CompletedFolder, DebugFolderName);
                #else
                    string dir = CompletedFolder;
                #endif
                if (!String.IsNullOrEmpty(dir) && (CompletedFolderIsRelative == true)) {
                    dir = General.GetAbsoluteDirectoryPath(dir, ExeDirectory);
                }
                return dir;
            }
        }

        public static Size? WindowSize {
            get {
                int[] size = GetIntArray("WindowSize");
                if (size.Length != 2 || size[0] < 1 || size[1] < 1) return null;
                return new Size(size[0], size[1]);
            }
            set { Set("WindowSize", value.HasValue ? value.Value.Width + "," + value.Value.Height : null); }
        }

        public static Point? WindowLocation {
            get {
                int[] windowLoc = GetIntArray("WindowLocation");
                if (windowLoc.Length != 2) return null;
                return new Point(windowLoc[0], windowLoc[1]);
            }
            set { Set("WindowLocation", !value.Value.IsEmpty ? value.Value.X + "," + value.Value.Y : null); }
        }

        public static FormWindowState WindowState {
            get {
                int? tempState = GetInt("WindowState");
                switch (tempState) {
                    case 0:
                        return FormWindowState.Normal;
                    case 2:
                        return FormWindowState.Maximized;
                    default:
                        return FormWindowState.Normal;
                }
            }
            set { Set("WindowState", $"{(int)value}"); }
        }

        public static bool? IsRunning {
            get { return GetBool("IsRunning"); }
            set { SetBool("IsRunning", value); }
        }

        public static int[] ColumnWidths {
            get { return GetIntArray("ColumnWidths"); }
            set { SetIntArray("ColumnWidths", value); }
        }

        public static int[] DefaultColumnWidths {
            get { return new[] { 110, 150, 115, 115, 110, 75 }; }
        }

        public static int[] ColumnIndices {
            get { return GetIntArray("ColumnIndices"); }
            set { SetIntArray("ColumnIndices", value); }
        }
        
        public static int? SortColumn {
            get { return GetInt("SortColumn"); }
            set { SetInt("SortColumn", value); }
        }

        public static bool? SortAscending {
            get { return GetBool("SortAscending"); }
            set { SetBool("SortAscending", value); }
        }

        private static string Get(string name) {
            lock (_settings) {
                return _settings.TryGetValue(name, out string value) ? value : null;
            }
        }

        private static bool? GetBool(string name) {
            string value = Get(name);
            if (value == null) return null;
            return value != "0";
        }

        private static int? GetInt(string name) {
            string value = Get(name);
            if (value == null) return null;
            return Int32.TryParse(value, out int x) ? x : (int?)null;
        }

        private static long? GetLong(string name) {
            string value = Get(name);
            if (value == null) return null;
            return Int64.TryParse(value, out long x) ? x : (long?)null;
        }

        private static DateTime? GetDate(string name) {
            string value = Get(name);
            if (value == null) return null;
            return DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime x) ? x : (DateTime?)null;
        }

        private static int[] GetIntArray(string name) {
            string value = Get(name);
            if (value == null) return new int[0];
            string[] array = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int[] values = new int[array.Length];
            for (int i = 0; i < array.Length; i++) {
                Int32.TryParse(array[i], out values[i]);
            }
            return values;
        }

        private static void Set(string name, string value) {
            lock (_settings) {
                if (value == null) {
                    _settings.Remove(name);
                }
                else {
                    _settings[name] = value;
                }
            }
        }

        private static void SetBool(string name, bool? value) {
            Set(name, value.HasValue ? (value.Value ? "1" : "0") : null);
        }

        private static void SetInt(string name, int? value) {
            Set(name, value.HasValue ? value.Value.ToString() : null);
        }

        private static void SetLong(string name, long? value) {
            Set(name, value.HasValue ? value.Value.ToString() : null);
        }

        private static void SetDate(string name, DateTime? value) {
            Set(name, value.HasValue ? value.Value.ToString("yyyyMMdd") : null);
        }

        private static void SetIntArray(string name, int[] value) {
            Set(name, value.Length > 0 ? String.Join(",", Array.ConvertAll(value, Convert.ToString)) : null);
        }

        public static void Load() {
            string path = Path.Combine(GetSettingsDirectory(), SettingsFileName);
            bool needsConversion;
            _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(path)) {
                string pathTxt = Path.Combine(GetSettingsDirectory(UseExeDirectoryForSettings ?? false), "settings.txt");
                if (File.Exists(pathTxt)) {
                    needsConversion = true;
                }
                else return;
                if (needsConversion) {
                    ConvertTxttoXML();
                }
            }

            try {
                XmlReaderSettings xmlReaderSettings = new() {XmlResolver = null, IgnoreWhitespace = false};
                XmlReader xmlSettingsReader = XmlReader.Create(path, xmlReaderSettings);
                XmlDocument settingsDoc = new() {XmlResolver = null};
                settingsDoc.Load(xmlSettingsReader);
                xmlSettingsReader.Close();
                foreach (XmlNode childNode in settingsDoc.SelectSingleNode("Settings")) {
                    string name = childNode.Name;
                    string val = childNode.InnerText;
                    if (!_settings.ContainsKey(name)) {
                        _settings.Add(name, val);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        public static void Save() {
            string path = Path.Combine(GetSettingsDirectory(), SettingsFileName);
            XmlDocument tempsettingsDoc = new() {XmlResolver = null};
            XmlElement settingsElement = tempsettingsDoc.CreateElement(string.Empty, "Settings", string.Empty);
            foreach (KeyValuePair<string, string> kvp in _settings) {
                XmlElement xmlElement = tempsettingsDoc.CreateElement(kvp.Key);
                XmlText xmlElementValue = tempsettingsDoc.CreateTextNode(kvp.Value);
                xmlElement.AppendChild(xmlElementValue);
                settingsElement.AppendChild(xmlElement);
            }
            tempsettingsDoc.AppendChild(settingsElement);
            try {
                XmlWriterSettings _tmpSettingsDocSettings = new() {Indent = true};
                XmlWriter writer = XmlWriter.Create(path, _tmpSettingsDocSettings);
                tempsettingsDoc.Save(writer);
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private static void ConvertTxttoXML() {
            string oldPath = Path.Combine(GetSettingsDirectory(), "settings.txt");
            if (File.Exists(oldPath)) {
                Dictionary<string, string> _tempSettings = new(StringComparer.OrdinalIgnoreCase);
                using (StreamReader sr = File.OpenText(oldPath)) {
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        int pos = line.IndexOf('=');
                        if (pos != -1) {
                            string name = line.Substring(0, pos);
                            string val = line.Substring(pos + 1);
                            if (!_tempSettings.ContainsKey(name)) {
                                _tempSettings.Add(name, val);
                            }
                        }
                    }
                }
                XmlDocument tempsettingsDoc = new() {XmlResolver = null};
                XmlElement settingsElement = tempsettingsDoc.CreateElement(string.Empty, "Settings", string.Empty);
                foreach (KeyValuePair<string, string> kvp in _tempSettings) {
                    XmlElement xmlElement = tempsettingsDoc.CreateElement(kvp.Key);
                    XmlText xmlElementValue = tempsettingsDoc.CreateTextNode(kvp.Value);
                    xmlElement.AppendChild(xmlElementValue);
                    settingsElement.AppendChild(xmlElement);
                }
                tempsettingsDoc.AppendChild(settingsElement);
                string path = Path.Combine(GetSettingsDirectory(), SettingsFileName);
                try {
                    using (XmlTextWriter writer = new(path, null)) {
                        writer.Formatting = Formatting.Indented;
                        tempsettingsDoc.Save(writer);
                        writer.Flush();
                        writer.Close();
                        File.Delete(oldPath);
                    }
                }
                catch (Exception ex) {
                    Logger.Log(ex.ToString());
                }
            }
        }
    }
}
