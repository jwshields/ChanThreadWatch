﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using JDP.Properties;

namespace JDP {
    public partial class frmChanThreadWatch : Form {
        private Dictionary<long, DownloadProgressInfo> _downloadProgresses = new();
        private frmDownloads _downloadForm;
        private frmCTWAbout _frmCTWAbout;
        private object _startupPromptSync = new();
        private bool _isExiting;
        private bool _saveThreadList;
        private int _itemAreaY;
        private int[] _columnWidths;
        private object _cboCheckEveryLastValue;
        private bool _isLoadingThreadsFromFile;
        private bool _isResizing;
        private static bool _unsafeShutdown;
        private bool _isMinimized;
        private static Dictionary<string, int> _categories = new();
        private static Dictionary<string, ThreadWatcher> _watchers = new();
        private static HashSet<string> _blacklist = new();

        // In the file `General.cs` - the variables `Version` and `ReleaseDate` must be updated on each build/release.

        public frmChanThreadWatch() {
            InitializeComponent();
            Icon = Resources.ChanThreadWatchIcon;
            niTrayIcon.Icon = Resources.ChanThreadWatchIcon;
            Settings.Load();
            if (Settings.IsRunning == true) {
                _unsafeShutdown = true;
            }
            else {
                _unsafeShutdown = false;
            }
            Settings.IsRunning = true;
            string logPath = Path.Combine(Settings.GetSettingsDirectory(), Settings.LogFileName);
            if (!File.Exists(logPath)) {
                try { File.Create(logPath); }
                catch (IOException ex) {
                    Logger.Log(ex.ToString());
                }
            }
            int initialWidth = ClientSize.Width;
            GUI.SetFontAndScaling(this);
            _isResizing = true;
            float scaleFactorX = (float)ClientSize.Width / initialWidth;
            Size? tempWindowSize = Settings.WindowSize;
            if (tempWindowSize == null) {
                ClientSize = MinimumSize;
            }
            else {
                if (tempWindowSize.Value.Width <= MinimumSize.Width && tempWindowSize.Value.Height <= MinimumSize.Height) {
                    Size = MinimumSize;
                }
                else {
                    Size = tempWindowSize.Value;
                }
            }
            Point? tempWindowLocation = Settings.WindowLocation;
            if (tempWindowLocation == null) {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            else {
                this.StartPosition = FormStartPosition.Manual;
                this.Left = tempWindowLocation.Value.X;
                this.Top = tempWindowLocation.Value.Y;
            }
            if (Settings.WindowState == FormWindowState.Maximized) {
                this.StartPosition = FormStartPosition.Manual;
                this.WindowState = FormWindowState.Maximized;
            }
            _isResizing = false;

            _columnWidths = new int[lvThreads.Columns.Count];
            for (int iColumn = 0; iColumn < lvThreads.Columns.Count; iColumn++) {
                ColumnHeader column = lvThreads.Columns[iColumn];
                if (iColumn < Settings.ColumnWidths.Length) {
                    column.Width = Settings.ColumnWidths[iColumn] > 0 ? Settings.ColumnWidths[iColumn] : 0;
                }
                else {
                    column.Width = Convert.ToInt32(column.Width * scaleFactorX);
                }
                _columnWidths[iColumn] = column.Width != 0 ? column.Width : Settings.DefaultColumnWidths[iColumn];
                if (iColumn < Settings.ColumnIndices.Length && Settings.ColumnIndices[iColumn] > 0 && Settings.ColumnIndices[iColumn] < lvThreads.Columns.Count) {
                    column.DisplayIndex = Settings.ColumnIndices[iColumn];
                }
            }
            GUI.EnableDoubleBuffering(lvThreads);

            BindCheckEveryList();
            BuildCheckEverySubMenu();
            BuildColumnHeaderMenu();

            if ((Settings.DownloadFolder == null) || !Directory.Exists(Settings.AbsoluteDownloadDirectory)) {
                Settings.DownloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Watched Threads");
                Settings.DownloadFolderIsRelative = false;
            }
            if (Settings.CheckEvery == 1) {
                Settings.CheckEvery = 0;
            }

            chkPageAuth.Checked = Settings.UsePageAuth ?? false;
            txtPageAuth.Text = Settings.PageAuth ?? String.Empty;
            chkImageAuth.Checked = Settings.UseImageAuth ?? false;
            txtImageAuth.Text = Settings.ImageAuth ?? String.Empty;
            chkOneTime.Checked = Settings.OneTimeDownload ?? false;
            chkAutoFollow.Checked = Settings.AutoFollow ?? false;
            if (Settings.CheckEvery != null) {
                foreach (ListItemInt32 item in cboCheckEvery.Items) {
                    if (item.Value != Settings.CheckEvery) continue;
                    cboCheckEvery.SelectedValue = Settings.CheckEvery;
                    break;
                }
                if ((int)cboCheckEvery.SelectedValue != Settings.CheckEvery) txtCheckEvery.Text = Settings.CheckEvery.ToString();
            }
            else {
                cboCheckEvery.SelectedValue = 3;
            }
            OnThreadDoubleClick = Settings.OnThreadDoubleClick ?? ThreadDoubleClickAction.OpenFolder;

            if ((Settings.CheckForUpdates == true) && (Settings.LastUpdateCheck ?? DateTime.MinValue) < DateTime.Now.Date) {
                CheckForUpdates();
            }
            niTrayIcon.Visible = Settings.MinimizeToTray ?? false;
        }

        public Dictionary<long, DownloadProgressInfo> DownloadProgresses {
            get { return _downloadProgresses; }
        }

        private ThreadDoubleClickAction OnThreadDoubleClick {
            get {
                if (rbEdit.Checked)
                    return ThreadDoubleClickAction.Edit;
                else if (rbOpenURL.Checked)
                    return ThreadDoubleClickAction.OpenURL;
                else
                    return ThreadDoubleClickAction.OpenFolder;
            }
            set {
                if (value == ThreadDoubleClickAction.Edit)
                    rbEdit.Checked = true;
                else if (value == ThreadDoubleClickAction.OpenURL)
                    rbOpenURL.Checked = true;
                else
                    rbOpenFolder.Checked = true;
            }
        }

        private void frmChanThreadWatch_Shown(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            btnAdd.Enabled = false;
            btnAddFromClipboard.Enabled = false;
            btnRemoveCompleted.Enabled = false;
            btnDownloads.Enabled = false;
            btnSettings.Enabled = false;
            btnAbout.Enabled = false;
            btnHelp.Enabled = false;
            lvThreads.Enabled = false;
            Application.DoEvents();

            lvThreads.Items.Add(new ListViewItem());
            _itemAreaY = lvThreads.GetItemRect(0).Y;
            lvThreads.Items.RemoveAt(0);

            Thread thread = new(() => {
                LoadThreadList();
                LoadBlacklist();

                Invoke(() => {
                    btnAdd.Enabled = true;
                    btnAddFromClipboard.Enabled = true;
                    btnRemoveCompleted.Enabled = true;
                    btnDownloads.Enabled = true;
                    btnSettings.Enabled = true;
                    btnAbout.Enabled = true;
                    btnHelp.Enabled = true;
                    lvThreads.Enabled = true;

                    lvThreads.ListViewItemSorter = new ListViewItemSorter(Settings.SortColumn ?? (int)ColumnIndex.AddedOn) { Ascending = Settings.SortAscending ?? true };
                    lvThreads.Sort();
                    FocusLastThread();
                });
            });
            lvThreads.BeginUpdate();
            thread.Start();
            lvThreads.EndUpdate();
            while (thread.IsAlive) {
                Thread.Sleep(5);
                Application.DoEvents();
            }
            this.Cursor = Cursors.Default;
            UpdateWindowTitle(GetMonitoringInfo());
            lblFilterThreadsTxt.Text = $"Filter Threads: All ({_watchers.Count})";
        }

        private void frmChanThreadWatch_OnFormClosing(object sender, FormClosingEventArgs e) {
            if (MessageBox.Show("Are you sure you want to exit?", "Confirm exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                e.Cancel = true;
            }
        }

        private void frmChanThreadWatch_FormClosed(object sender, FormClosedEventArgs e) {
            if (IsDisposed) return;

            tmrUpdateWaitStatus.Stop();
            Settings.UsePageAuth = chkPageAuth.Checked;
            Settings.PageAuth = txtPageAuth.Text;
            Settings.UseImageAuth = chkImageAuth.Checked;
            Settings.ImageAuth = txtImageAuth.Text;
            Settings.OneTimeDownload = chkOneTime.Checked;
            Settings.AutoFollow = chkAutoFollow.Checked;
            Settings.CheckEvery = pnlCheckEvery.Enabled ? (cboCheckEvery.Enabled ? (int)cboCheckEvery.SelectedValue : Int32.Parse(txtCheckEvery.Text)) : 0;
            Settings.OnThreadDoubleClick = OnThreadDoubleClick;
            Settings.WindowSize = RestoreBounds.Size;
            Settings.WindowLocation = new Point(RestoreBounds.Left, RestoreBounds.Top);
            Settings.WindowState = WindowState;
            Settings.IsRunning = false;

            int[] columnWidths = new int[lvThreads.Columns.Count];
            int[] columnIndices = new int[lvThreads.Columns.Count];
            for (int i = 0; i < lvThreads.Columns.Count; i++) {
                columnWidths[i] = lvThreads.Columns[i].Width;
                columnIndices[i] = lvThreads.Columns[i].DisplayIndex;
            }
            Settings.ColumnWidths = columnWidths;
            Settings.ColumnIndices = columnIndices;

            ListViewItemSorter sorter = (ListViewItemSorter)lvThreads.ListViewItemSorter;
            if (sorter != null) {
                Settings.SortColumn = sorter.Column;
                Settings.SortAscending = sorter.Ascending;
            }

            Settings.Save();

            foreach (ThreadWatcher watcher in ThreadWatchers) {
                watcher.Stop(StopReason.Exiting);
            }

            // Save before waiting in addition to after in case the wait hangs or is interrupted
            SaveThreadList();
            _isExiting = true;

            foreach (ThreadWatcher watcher in ThreadWatchers) {
                while (!watcher.WaitUntilStopped(10) || !watcher.WaitReparse(10)) {
                    Application.DoEvents();
                }
            }

            SaveThreadList();
        }

        private void frmChanThreadWatch_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("UniformResourceLocatorW") ||
                e.Data.GetDataPresent("UniformResourceLocator")) {
                if ((e.AllowedEffect & DragDropEffects.Copy) != 0) {
                    e.Effect = DragDropEffects.Copy;
                }
                else if ((e.AllowedEffect & DragDropEffects.Link) != 0) {
                    e.Effect = DragDropEffects.Link;
                }
            }
        }

        private void frmChanThreadWatch_DragDrop(object sender, DragEventArgs e) {
            if (_isExiting) return;
            string url = null;
            if (e.Data.GetDataPresent("UniformResourceLocatorW")) {
                byte[] data = ((MemoryStream)e.Data.GetData("UniformResourceLocatorW")).ToArray();
                url = Encoding.Unicode.GetString(data, 0, General.StrLenW(data) * 2);
            }
            else if (e.Data.GetDataPresent("UniformResourceLocator")) {
                byte[] data = ((MemoryStream)e.Data.GetData("UniformResourceLocator")).ToArray();
                url = Encoding.Default.GetString(data, 0, General.StrLen(data));
            }
            url = General.CleanPageURL(url);
            if (url != null) {
                AddThread(url);
                FocusThread(url);
                _saveThreadList = true;
            }
        }

        private void frmChanThreadWatch_Resize(object sender, EventArgs e) {
            if (_isResizing) return;
            if (WindowState == FormWindowState.Minimized) {
                _isMinimized = true;
                tmrUpdateWaitStatus.Stop();
                if (Settings.MinimizeToTray == true) {
                    Hide();
                }
                return;
            }
            else {
                if (_isMinimized) {
                    _isMinimized = false;
                    UpdateWaitingWatcherStatuses();
                    tmrUpdateWaitStatus.Start();
                }
                Settings.WindowLocation = new Point(RestoreBounds.X, RestoreBounds.Y);
                Settings.WindowSize = RestoreBounds.Size;
                Settings.WindowState = WindowState;
                Settings.Save();
            }
            return;
        }

        private void frmChanThreadWatch_ResizeEnd(object sender, EventArgs e) {
            _isResizing = false;
            Settings.WindowSize = RestoreBounds.Size;
            Settings.WindowLocation = new Point(RestoreBounds.X, RestoreBounds.Y);
            Settings.WindowState = WindowState;
            Settings.Save();
        }

        private void frmChanThreadWatch_ResizeBegin(object sender, EventArgs e) {
            _isResizing = true;
        }

        private void txtPageURL_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                btnAdd_Click(txtPageURL, null);
                e.SuppressKeyPress = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            if (txtPageURL.Text.Trim().Length == 0) return;
            string pageURL = General.CleanPageURL(txtPageURL.Text);
            if (pageURL == null) {
                MessageBox.Show(this, "The specified URL is invalid.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!AddThread(pageURL)) {
                MessageBox.Show(this, "The same thread is already being watched, downloaded or has been blacklisted.", "Cannot Add Thread", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPageURL.Clear();
                FocusThread(pageURL);
                return;
            }
            FocusThread(pageURL);
            txtPageURL.Clear();
            txtPageURL.Focus();
            txtBoxThreadFilter_TextChanged();
            _saveThreadList = true;
        }

        private void btnAddFromClipboard_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            string text;
            try {
                text = Clipboard.GetText();
            }
            catch {
                return;
            }
            string[] urls = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (urls.Length > 0) {
                lvThreads.SelectedItems.Clear();
                lvThreads.Select();
            }
            for (int iURL = 0; iURL < urls.Length; iURL++) {
                string url = General.CleanPageURL(urls[iURL]);
                if (url == null) continue;
                AddThread(url);
                if (urls.Length == 1) {
                    FocusThread(url);
                }
                else {
                    SiteHelper siteHelper = SiteHelpers.GetInstance((new Uri(url)).Host);
                    siteHelper.SetURL(url);
                    if (_watchers.TryGetValue(siteHelper.GetPageID(), out ThreadWatcher watcher)) {
                        (((WatcherExtraData)watcher.Tag).ListViewItem).Selected = true;
                    }
                }
            }
            _saveThreadList = true;
            txtBoxThreadFilter_TextChanged();
            UpdateWindowTitle(GetMonitoringInfo());
        }

        private void btnRemoveCompleted_Click(object sender, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            if (Settings.MoveToCompletedFolder != true) {
                RemoveThreads(true, false);
            }
            else {
                if (!Directory.Exists(Settings.AbsoluteCompletedDirectory)) {
                    Settings.CompletedFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Completed Threads");
                    Settings.CompletedFolderIsRelative = false;
                }
                RemoveThreads(true, false,
                        (watcher) => {
                            string destDir = Path.Combine(Settings.AbsoluteCompletedDirectory,
                                General.GetRelativeDirectoryPath(watcher.ThreadDownloadDirectory, watcher.MainDownloadDirectory));
                            if (Directory.Exists(watcher.ThreadDownloadDirectory)) {
                                if (Directory.Exists(destDir)) {
                                    Directory.Delete(destDir);
                                }
                                if (watcher.Category.Length != 0) {
                                    Directory.CreateDirectory(General.RemoveLastDirectory(destDir));
                                }
                                Directory.Move(watcher.ThreadDownloadDirectory, destDir);
                            }
                            string categoryPath = General.RemoveLastDirectory(watcher.ThreadDownloadDirectory);
                            if (categoryPath != watcher.MainDownloadDirectory && Directory.GetFiles(categoryPath).Length == 0 && Directory.GetDirectories(categoryPath).Length == 0) {
                                Directory.Delete(categoryPath);
                            }
                        });
            }
            txtBoxThreadFilter_TextChanged();
            UpdateWindowTitle(GetMonitoringInfo());
            this.Cursor = Cursors.Default;
        }

        private void miStop_Click(object sender, EventArgs e) {
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                watcher.Stop(StopReason.UserRequest);
            }
            _saveThreadList = true;
        }

        private void miStart_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (!watcher.IsRunning && !watcher.IsReparsing) {
                    watcher.Start();
                }
            }
            _saveThreadList = true;
        }

        private void miEdit_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            var selectedThreadWatchers = new List<ThreadWatcher>(SelectedThreadWatchers);
            if (selectedThreadWatchers.Count == 0) return;

            using (frmThreadEdit editForm = new(selectedThreadWatchers, _categories)) {
                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.IsDirty) {
                    foreach (ThreadWatcher watcher in selectedThreadWatchers) {
                        if (editForm.Description.IsDirty) {
                            watcher.Description = editForm.Description.Value;
                        }
                        if (editForm.Category.IsDirty) {
                            UpdateCategories(watcher.Category, true);
                            UpdateCategories(editForm.Category.Value);
                            watcher.Category = editForm.Category.Value;
                        }
                        if (editForm.CheckIntervalSeconds.IsDirty) {
                            watcher.CheckIntervalSeconds = editForm.CheckIntervalSeconds.Value;
                        }
                        if (!watcher.IsRunning) {
                            if (editForm.PageAuth.IsDirty) {
                                watcher.PageAuth = editForm.PageAuth.Value;
                            }
                            if (editForm.ImageAuth.IsDirty) {
                                watcher.ImageAuth = editForm.ImageAuth.Value;
                            }
                            if (editForm.OneTimeDownload.IsDirty) {
                                watcher.OneTimeDownload = editForm.OneTimeDownload.Value;
                            }
                            if (editForm.AutoFollow.IsDirty) {
                                watcher.AutoFollow = editForm.AutoFollow.Value;
                            }
                        }
                        DisplayData(watcher);
                    }
                    _saveThreadList = true;
                }
            }
        }

        private void miOpenFolder_Click(object sender, EventArgs e) {
            int selectedCount = lvThreads.SelectedItems.Count;
            if (selectedCount > 5 && MessageBox.Show(this, "Do you want to open the folders of all " + selectedCount + " selected items?",
                "Open Folders", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                return;
            }
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                string dir = watcher.ThreadDownloadDirectory;
                ThreadWatcher tmpWatcher = watcher;
                ThreadPool.QueueUserWorkItem((s) => {
                    try {
                        if (!Directory.Exists(dir)) {
                            tmpWatcher.Stop(StopReason.Other);
                            BeginInvoke(() => {
                                MessageBox.Show(this, "The folder " + dir + " does not exists. The watcher has been stopped to let you fix this, in case of an unwanted deletion or rename. If the thread file cannot be found for the next check, it won't include possible deleted posts.",
                                    "Folder Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            });
                        }
                        else {
                            Process.Start(dir);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Log(ex.ToString());
                    }
                });
            }
        }

        private void miOpenURL_Click(object sender, EventArgs e) {
            int selectedCount = lvThreads.SelectedItems.Count;
            if (selectedCount > 5 && MessageBox.Show(this, "Do you want to open the URLs of all " + selectedCount + " selected items?",
                "Open URLs", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                return;
            }
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                string url = watcher.PageURL;
                ThreadPool.QueueUserWorkItem((s) => {
                    try {
                        Process.Start(url);
                    }
                    catch (Exception ex) {
                        Logger.Log(ex.ToString());
                    }
                });
            }
        }

        private void miCopyURL_Click(object sender, EventArgs e) {
            StringBuilder sb = new();
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (sb.Length != 0) sb.Append(Environment.NewLine);
                sb.Append(watcher.PageURL);
            }
            try {
                Clipboard.Clear();
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex) {
                MessageBox.Show(this, "Unable to copy to clipboard: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void miRemove_Click(object sender, EventArgs e) {
            RemoveThreads(false, true);
        }

        private void miRemoveAndDeleteFolder_Click(object sender, EventArgs e) {
            if (MessageBox.Show(this, "Are you sure you want to delete the " + lvThreads.SelectedItems.Count + " selected threads and all associated files from disk?",
                "Delete From Disk", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                return;
            }
            RemoveThreads(false, true,
                (watcher) => {
                    if (Directory.Exists(watcher.ThreadDownloadDirectory)) Directory.Delete(watcher.ThreadDownloadDirectory, true);
                    string categoryPath = General.RemoveLastDirectory(watcher.ThreadDownloadDirectory);
                    if (categoryPath != watcher.MainDownloadDirectory && Directory.GetFiles(categoryPath).Length == 0 && Directory.GetDirectories(categoryPath).Length == 0) {
                        Directory.Delete(categoryPath);
                    }
                });
        }

        private void miBlacklist_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            List<string> lines = new();
            foreach (string rule in _blacklist) {
                lines.Add(rule);
            }
            HashSet<string> blacklist = new();
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (!_blacklist.Contains(watcher.PageID) && blacklist.Add(watcher.PageID)) {
                    lines.Add(watcher.PageID);
                }
            }
            try {
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.BlacklistFileName);
                File.WriteAllLines(path, lines.ToArray());
                foreach (string pageID in blacklist) {
                    _blacklist.Add(pageID);
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private void miCheckNow_Click(object sender, EventArgs e) {
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                watcher.MillisecondsUntilNextCheck = 0;
            }
        }

        private void miCheckEvery_Click(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                int checkIntervalSeconds = Convert.ToInt32(menuItem.Tag) * 60;
                foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                    watcher.CheckIntervalSeconds = checkIntervalSeconds;
                }
                UpdateWaitingWatcherStatuses();
            }
            _saveThreadList = true;
        }

        private void miReparse_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (!watcher.IsRunning && !watcher.IsReparsing && Settings.SaveThumbnails != 0) {
                    watcher.BeginReparse();
                }
            }
            _saveThreadList = true;
        }

        private void btnDownloads_Click(object sender, EventArgs e) {
            if (_downloadForm != null && !_downloadForm.IsDisposed) {
                _downloadForm.Activate();
            }
            else {
                _downloadForm = new frmDownloads(this);
                GUI.CenterChildForm(this, _downloadForm);
                _downloadForm.Show(this);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            using (frmSettings settingsForm = new()) {
                GUI.CenterChildForm(this, settingsForm);
                settingsForm.ShowDialog(this);
            }
            niTrayIcon.Visible = Settings.MinimizeToTray ?? false;
            tmrBackupThreadList.Interval = (Settings.BackupEvery ?? 1) * 60 * 1000;
            UpdateWindowTitle(GetMonitoringInfo());
        }

        private void btnAbout_Click(object sender, EventArgs e) {
            if (_frmCTWAbout != null && !_frmCTWAbout.IsDisposed) {
                GUI.CenterChildForm(this, _frmCTWAbout);
                _frmCTWAbout.ShowDialog(this);
            }
            else {
                _frmCTWAbout = new();
                GUI.CenterChildForm(this, _frmCTWAbout);
                _frmCTWAbout.ShowDialog(this);

            }
        }

        private void btnHelp_Click(object sender, EventArgs e) {
            Process.Start(General.WikiURL);
        }

        private void lvThreads_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Delete) {
                RemoveThreads(false, true);
            }
            else if (e.Control && e.KeyCode == Keys.A) {
                foreach (ListViewItem item in lvThreads.Items) {
                    item.Selected = true;
                }
            }
            else if (e.Control && e.KeyCode == Keys.I) {
                foreach (ListViewItem item in lvThreads.Items) {
                    item.Selected = !item.Selected;
                }
            }
        }

        private void lvThreads_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                int selectedCount = lvThreads.SelectedItems.Count;
                if (selectedCount != 0) {
                    bool anyRunning = false;
                    bool anyStopped = false;
                    bool anyNotReparsing = false;
                    foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                        bool isRunning = watcher.IsRunning;
                        anyRunning |= isRunning;
                        anyStopped |= !isRunning;
                        anyNotReparsing |= !watcher.IsReparsing;
                    }
                    miStop.Visible = anyRunning;
                    miStart.Visible = anyStopped && anyNotReparsing;
                    miCheckNow.Visible = anyRunning;
                    miCheckEvery.Visible = anyRunning;
                    miRemove.Visible = anyStopped && anyNotReparsing;
                    miRemoveAndDeleteFolder.Visible = anyStopped && anyNotReparsing;
                    miReparse.Visible = anyStopped && anyNotReparsing;
                    cmThreads.Show(lvThreads, e.Location);
                }
            }
        }

        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (OnThreadDoubleClick == ThreadDoubleClickAction.Edit) {
                miEdit_Click(null, null);
            }
            else if (OnThreadDoubleClick == ThreadDoubleClickAction.OpenFolder) {
                miOpenFolder_Click(null, null);
            }
            else {
                miOpenURL_Click(null, null);
            }
        }

        private void lvThreads_ColumnClick(object sender, ColumnClickEventArgs e) {
            ListViewItemSorter sorter = (ListViewItemSorter)lvThreads.ListViewItemSorter;
            if (sorter == null) {
                sorter = new ListViewItemSorter(e.Column);
                lvThreads.ListViewItemSorter = sorter;
            }
            else if (e.Column != sorter.Column) {
                sorter.Column = e.Column;
                sorter.Ascending = true;
            }
            else {
                sorter.Ascending = !sorter.Ascending;
            }
            lvThreads.Sort();
        }

        private void chkOneTime_CheckedChanged(object sender, EventArgs e) {
            pnlCheckEvery.Enabled = !chkOneTime.Checked;
        }

        private void chkPageAuth_CheckedChanged(object sender, EventArgs e) {
            txtPageAuth.Enabled = chkPageAuth.Checked;
        }

        private void chkImageAuth_CheckedChanged(object sender, EventArgs e) {
            txtImageAuth.Enabled = chkImageAuth.Checked;
        }

        private void txtCheckEvery_TextChanged(object sender, EventArgs e) {
            if (Int32.TryParse(txtCheckEvery.Text, out _)) {
                cboCheckEvery.SelectedIndex = -1;
                cboCheckEvery.Enabled = false;
            }
            else {
                if (cboCheckEvery.SelectedIndex == -1) cboCheckEvery.SelectedValue = _cboCheckEveryLastValue;
                cboCheckEvery.Enabled = true;
            }
        }

        private void txtCheckEvery_Leave(object sender, EventArgs e) {
            if (Int32.TryParse(txtCheckEvery.Text, out int checkEvery)) {
                int idx_cboCheckEvery = cboCheckEvery.FindStringExact(txtCheckEvery.Text);
                if (idx_cboCheckEvery != -1) {
                    cboCheckEvery.SelectedIndex = idx_cboCheckEvery;
                    cboCheckEvery.Enabled = true;
                    txtCheckEvery.Text = "";
                }
                else {
                    if (checkEvery == 1) {
                        idx_cboCheckEvery = cboCheckEvery.FindStringExact("1 or <");
                        cboCheckEvery.SelectedIndex = idx_cboCheckEvery;
                        cboCheckEvery.Enabled = true;
                        txtCheckEvery.Text = "";
                    }
                    else {
                        cboCheckEvery.SelectedIndex = -1;
                        cboCheckEvery.Enabled = false;
                    }
                }
            }
            else {
                if (cboCheckEvery.SelectedIndex == -1) cboCheckEvery.SelectedValue = _cboCheckEveryLastValue;
                cboCheckEvery.Enabled = true;
            }
        }
        private void txtBoxThreadFilter_TextChanged() {
            string filterThreadsValue = txtBoxThreadFilter.Text;
            int threadCount = _watchers.Count;
            lvThreads.BeginUpdate();
            lvThreads.Items.Clear();
            string lblFilterTextOut;
            if (String.IsNullOrEmpty(filterThreadsValue)) {
                lblFilterTextOut = $"All ({threadCount})";
                foreach (KeyValuePair<String, ThreadWatcher> watcher in _watchers) {
                    WatcherExtraData watcherextra = (WatcherExtraData)watcher.Value.Tag;
                    lvThreads.Items.Add(watcherextra.ListViewItem);
                }
            }
            else {
                int shownThreadCount = 0;
                foreach (KeyValuePair<String, ThreadWatcher> watcher in _watchers) {
                    if (watcher.Value.Description.Contains(filterThreadsValue)) {
                        shownThreadCount++;
                        WatcherExtraData watcherextra = (WatcherExtraData)watcher.Value.Tag;
                        lvThreads.Items.Add(watcherextra.ListViewItem);
                    }
                }
                lblFilterTextOut = $"({shownThreadCount} of {threadCount})";
            }
            lblFilterThreadsTxt.Text = $"Filter Threads: {lblFilterTextOut}";
            lvThreads.EndUpdate();
            return;
        }

        private void txtBoxThreadFilter_TextChanged(object sender, EventArgs e) {
            txtBoxThreadFilter_TextChanged();
        }

        private void cboCheckEvery_SelectedIndexChanged(object sender, EventArgs e) {
            if (cboCheckEvery.SelectedIndex == -1) return;
            if (cboCheckEvery.Focused) txtCheckEvery.Clear();
            if (_cboCheckEveryLastValue == null && (int)cboCheckEvery.SelectedValue == 0 && (int)cboCheckEvery.SelectedValue != Settings.CheckEvery) {
                _cboCheckEveryLastValue = 3;
            }
            else {
                _cboCheckEveryLastValue = cboCheckEvery.SelectedValue;
            }
        }

        private void tmrSaveThreadList_Tick(object sender, EventArgs e) {
            if (_saveThreadList && !_isExiting) {
                SaveThreadList();
                _saveThreadList = false;
            }
        }

        private void tmrUpdateWaitStatus_Tick(object sender, EventArgs e) {
            UpdateWaitingWatcherStatuses();
        }

        private void tmrMaintenance_Tick(object sender, EventArgs e) {
            lock (_downloadProgresses) {
                if (_downloadProgresses.Count == 0) return;
                List<long> oldDownloadIDs = new();
                long ticksNow = TickCount.Now;
                foreach (DownloadProgressInfo info in _downloadProgresses.Values) {
                    if (info.EndTicks != null && ticksNow - info.EndTicks.Value > 5000) {
                        oldDownloadIDs.Add(info.DownloadID);
                    }
                }
                foreach (long downloadID in oldDownloadIDs) {
                    _downloadProgresses.Remove(downloadID);
                }
            }
        }

        private void tmrMonitor_Tick(object sender, EventArgs e) {
            MonitoringInfo monitoringInfo = GetMonitoringInfo();
            UpdateWindowTitle(monitoringInfo);
            miMonitorTotal.Text = string.Format("Watching {0} thread{1}", monitoringInfo.TotalThreads, monitoringInfo.TotalThreads != 1 ? "s" : String.Empty);
            miMonitorRunning.Text = string.Format("    {0} running", monitoringInfo.RunningThreads);
            miMonitorDead.Text = string.Format("    {0} dead", monitoringInfo.DeadThreads);
            miMonitorStopped.Text = string.Format("    {0} stopped", monitoringInfo.StoppedThreads);
        }

        private void tmrBackupThreadList_Tick(object sender, EventArgs e) {
            if (Settings.BackupThreadList == true) {
                General.BackupThreadList(Settings.BackupCheckSize ?? false);
            }
        }

        private void niTrayIcon_Click(object sender, EventArgs e) {
            // Nothing for now
        }

        private void niTrayIcon_DoubleClick(object sender, EventArgs e) {
            Show();
        }

        private void miExit_Click(object sender, EventArgs e) {
            Close();
        }

        private void ThreadWatcher_DownloadStatus(ThreadWatcher watcher, DownloadStatusEventArgs args) {
            WatcherExtraData extraData = (WatcherExtraData)watcher.Tag;
            bool isInitialPageDownload = false;
            bool isFirstImageUpdate = false;
            if (args.DownloadType == DownloadType.Page) {
                if (!extraData.HasDownloadedPage) {
                    extraData.HasDownloadedPage = true;
                    isInitialPageDownload = true;
                }
                extraData.PreviousDownloadWasPage = true;
            }
            if (args.DownloadType == DownloadType.Image && extraData.PreviousDownloadWasPage) {
                extraData.LastImageOn = DateTime.Now;
                extraData.PreviousDownloadWasPage = false;
                isFirstImageUpdate = true;
            }
            BeginInvoke(() => {
                SetDownloadStatus(watcher, args.DownloadType, args.CompleteCount, args.TotalCount);
                if (isInitialPageDownload) {
                    DisplayDescription(watcher);
                    _saveThreadList = true;
                }
                if (isFirstImageUpdate) {
                    DisplayLastImageOn(watcher);
                    _saveThreadList = true;
                }
                SetupWaitTimer();
            });
        }

        private void ThreadWatcher_WaitStatus(ThreadWatcher watcher, EventArgs args) {
            BeginInvoke(() => {
                SetWaitStatus(watcher);
                SetupWaitTimer();
            });
        }

        private void ThreadWatcher_StopStatus(ThreadWatcher watcher, StopStatusEventArgs args) {
            BeginInvoke(() => {
                SetStopStatus(watcher, args.StopReason);
                SetupWaitTimer();
                if (args.StopReason != StopReason.UserRequest && args.StopReason != StopReason.Exiting) {
                    _saveThreadList = true;
                }
            });
        }

        private void ThreadWatcher_ReparseStatus(ThreadWatcher watcher, ReparseStatusEventArgs args) {
            BeginInvoke(() => {
                SetReparseStatus(watcher, args.ReparseType, args.CompleteCount, args.TotalCount);
                SetupWaitTimer();
            });
        }

        private void ThreadWatcher_ThreadDownloadDirectoryRename(ThreadWatcher watcher, EventArgs args) {
            BeginInvoke(() => {
                _saveThreadList = true;
            });
        }

        private void ThreadWatcher_DownloadStart(ThreadWatcher watcher, DownloadStartEventArgs args) {
            DownloadProgressInfo info = new() {
                DownloadID = args.DownloadID,
                URL = args.URL,
                TryNumber = args.TryNumber,
                StartTicks = TickCount.Now,
                TotalSize = args.TotalSize
            };
            lock (_downloadProgresses) {
                _downloadProgresses[args.DownloadID] = info;
            }
        }

        private void ThreadWatcher_DownloadProgress(ThreadWatcher watcher, DownloadProgressEventArgs args) {
            lock (_downloadProgresses) {
                if (!_downloadProgresses.TryGetValue(args.DownloadID, out DownloadProgressInfo info)) return;
                info.DownloadedSize = args.DownloadedSize;
                _downloadProgresses[args.DownloadID] = info;
            }
        }

        private void ThreadWatcher_DownloadEnd(ThreadWatcher watcher, DownloadEndEventArgs args) {
            lock (_downloadProgresses) {
                if (!_downloadProgresses.TryGetValue(args.DownloadID, out DownloadProgressInfo info)) return;
                info.EndTicks = TickCount.Now;
                info.DownloadedSize = args.DownloadedSize;
                info.TotalSize = args.DownloadedSize;
                _downloadProgresses[args.DownloadID] = info;
            }
        }

        private void ThreadWatcher_AddThread(ThreadWatcher watcher, AddThreadEventArgs args) {
            BeginInvoke(() => {
                ThreadInfo thread = new() {
                    URL = args.PageURL,
                    PageAuth = watcher.PageAuth,
                    ImageAuth = watcher.ImageAuth,
                    CheckIntervalSeconds = watcher.CheckIntervalSeconds,
                    OneTimeDownload = watcher.OneTimeDownload,
                    SaveDir = null,
                    Description = String.Empty,
                    StopReason = null,
                    ExtraData = new WatcherExtraData {
                        AddedOn = DateTime.Now,
                        AddedFrom = watcher.PageID
                    },
                    Category = watcher.Category,
                    AutoFollow = Settings.RecursiveAutoFollow != false
                };
                SiteHelper siteHelper = SiteHelpers.GetInstance((new Uri(thread.URL)).Host);
                siteHelper.SetURL(thread.URL);
                if (_watchers.ContainsKey(siteHelper.GetPageID())) return;
                if (AddThread(thread)) {
                    _saveThreadList = true;
                }
            });
        }

        private bool AddThread(string pageURL) {
            ThreadInfo thread = new() {
                URL = pageURL,
                PageAuth = (chkPageAuth.Checked && (txtPageAuth.Text.IndexOf(':') != -1)) ? txtPageAuth.Text : String.Empty,
                ImageAuth = (chkImageAuth.Checked && (txtImageAuth.Text.IndexOf(':') != -1)) ? txtImageAuth.Text : String.Empty,
                CheckIntervalSeconds = pnlCheckEvery.Enabled ? (cboCheckEvery.Enabled ? (int)cboCheckEvery.SelectedValue * 60 : Int32.Parse(txtCheckEvery.Text) * 60) : 0,
                OneTimeDownload = chkOneTime.Checked,
                SaveDir = null,
                Description = String.Empty,
                StopReason = null,
                ExtraData = null,
                Category = cboCategory.Text,
                AutoFollow = chkAutoFollow.Checked
            };
            return AddThread(thread);
        }

        private bool AddThread(ThreadInfo thread) {
            ThreadWatcher watcher = null;
            ThreadWatcher parentThread = null;
            ListViewItem newListViewItem = null;
            SiteHelper siteHelper = SiteHelpers.GetInstance((new Uri(thread.URL)).Host);
            siteHelper.SetURL(thread.URL);
            string pageID = siteHelper.GetPageID();
            if (IsBlacklisted(pageID)) return false;

            if (_watchers.ContainsKey(pageID)) {
                watcher = _watchers[pageID];
                if (watcher.IsRunning) return false;
            }

            if (watcher == null) {
                watcher = new ThreadWatcher(thread.URL);
                watcher.ThreadDownloadDirectory = thread.SaveDir;
                watcher.Description = thread.Description;
                if (_isLoadingThreadsFromFile) watcher.DoNotRename = true;
                watcher.Category = thread.Category;
                watcher.DoNotRename = false;
                if (thread.ExtraData != null && !String.IsNullOrEmpty(thread.ExtraData.AddedFrom)) {
                    _watchers.TryGetValue(thread.ExtraData.AddedFrom, out parentThread);
                    watcher.ParentThread = parentThread;
                }
                watcher.DownloadStatus += ThreadWatcher_DownloadStatus;
                watcher.WaitStatus += ThreadWatcher_WaitStatus;
                watcher.StopStatus += ThreadWatcher_StopStatus;
                watcher.ReparseStatus += ThreadWatcher_ReparseStatus;
                watcher.ThreadDownloadDirectoryRename += ThreadWatcher_ThreadDownloadDirectoryRename;
                watcher.DownloadStart += ThreadWatcher_DownloadStart;
                watcher.DownloadProgress += ThreadWatcher_DownloadProgress;
                watcher.DownloadEnd += ThreadWatcher_DownloadEnd;
                watcher.AddThread += ThreadWatcher_AddThread;

                newListViewItem = new ListViewItem(String.Empty);
                for (int i = 1; i < lvThreads.Columns.Count; i++) {
                    newListViewItem.SubItems.Add(String.Empty);
                }
                newListViewItem.Tag = watcher;
                lvThreads.Items.Add(newListViewItem);
                lvThreads.Sort();
                UpdateCategories(watcher.Category);
            }

            watcher.PageAuth = thread.PageAuth;
            watcher.ImageAuth = thread.ImageAuth;
            watcher.CheckIntervalSeconds = thread.CheckIntervalSeconds;
            watcher.OneTimeDownload = thread.OneTimeDownload;
            watcher.AutoFollow = thread.AutoFollow;

            if (thread.ExtraData == null) {
                thread.ExtraData = watcher.Tag as WatcherExtraData ?? new WatcherExtraData { AddedOn = DateTime.Now };
            }
            if (newListViewItem != null) {
                thread.ExtraData.ListViewItem = newListViewItem;
            }
            watcher.Tag = thread.ExtraData;

            if (parentThread != null) parentThread.ChildThreads.Add(watcher.PageID, watcher);
            if (!_watchers.ContainsKey(watcher.PageID)) {
                _watchers.Add(watcher.PageID, watcher);
            }
            else {
                _watchers[watcher.PageID] = watcher;
            }
            DisplayData(watcher);

            if (thread.StopReason == null && !_isLoadingThreadsFromFile) {
                watcher.Start();
            }
            else if (thread.StopReason != null) {
                watcher.Stop(thread.StopReason.Value);
            }
            return true;
        }

        private void RemoveThreads(bool removeCompleted, bool removeSelected) {
            RemoveThreads(removeCompleted, removeSelected, null);
        }

        private void RemoveThreads(bool removeCompleted, bool removeSelected, Action<ThreadWatcher> preRemoveAction) {
            int i = 0;
            lvThreads.BeginUpdate();
            while (i < lvThreads.Items.Count) {
                ThreadWatcher watcher = (ThreadWatcher)lvThreads.Items[i].Tag;
                if ((removeCompleted || (removeSelected && lvThreads.Items[i].Selected)) && !watcher.IsRunning && !watcher.IsReparsing) {
                    if (preRemoveAction != null) {
                        try { preRemoveAction(watcher); }
                        catch (Exception ex) {
                            Logger.Log(ex.ToString());
                        }
                    }
                    UpdateCategories(watcher.Category, true);
                    lvThreads.Items.RemoveAt(i);
                    _watchers.Remove(watcher.PageID);
                }
                else {
                    i++;
                }
            }
            lvThreads.EndUpdate();
            UpdateWindowTitle(GetMonitoringInfo());
            _saveThreadList = true;
            txtBoxThreadFilter_TextChanged();
        }

        private void BindCheckEveryList() {
            cboCheckEvery.ValueMember = "Value";
            cboCheckEvery.DisplayMember = "Text";
            cboCheckEvery.DataSource = new[] {
                new ListItemInt32(0, "1 or <"),
                new ListItemInt32(2, "2"),
                new ListItemInt32(3, "3"),
                new ListItemInt32(5, "5"),
                new ListItemInt32(10, "10"),
                new ListItemInt32(60, "60")
            };
        }

        private void BuildCheckEverySubMenu() {
            for (int i = 0; i < cboCheckEvery.Items.Count; i++) {
                int minutes = ((ListItemInt32)cboCheckEvery.Items[i]).Value;
                MenuItem menuItem = new() {
                    Index = i,
                    Tag = minutes,
                    Text = minutes > 0 ? minutes + " Minutes" : "1 Minute or <"
                };
                menuItem.Click += miCheckEvery_Click;
                miCheckEvery.MenuItems.Add(menuItem);
            }
        }

        private void BuildColumnHeaderMenu() {
            ContextMenu contextMenu = new();
            contextMenu.Popup += (s, e) => {
                for (int i = 0; i < lvThreads.Columns.Count; i++) {
                    contextMenu.MenuItems[i].Checked = lvThreads.Columns[i].Width != 0;
                }
            };
            for (int i = 0; i < lvThreads.Columns.Count; i++) {
                MenuItem menuItem = new() {
                    Index = i,
                    Tag = i,
                    Text = lvThreads.Columns[i].Text
                };
                menuItem.Click += (s, e) => {
                    int iColumn = (int)((MenuItem)s).Tag;
                    ColumnHeader column = lvThreads.Columns[iColumn];
                    if (column.Width != 0) {
                        _columnWidths[iColumn] = column.Width;
                        column.Width = 0;
                    }
                    else {
                        column.Width = _columnWidths[iColumn];
                    }
                };
                contextMenu.MenuItems.Add(menuItem);
            }
            ContextMenuStrip contextMenuStrip = new();
            contextMenuStrip.Opening += (s, e) => {
                e.Cancel = true;
                Point pos = lvThreads.PointToClient(Control.MousePosition);
                if (pos.Y >= _itemAreaY) return;
                contextMenu.Show(lvThreads, pos);
            };
            lvThreads.ContextMenuStrip = contextMenuStrip;
        }

        private void SetupWaitTimer() {
            bool anyWaiting = false;
            foreach (ThreadWatcher watcher in ThreadWatchers) {
                if (watcher.IsWaiting) {
                    anyWaiting = true;
                    break;
                }
            }
            if (!tmrUpdateWaitStatus.Enabled && anyWaiting) {
                tmrUpdateWaitStatus.Start();
            }
            else if (tmrUpdateWaitStatus.Enabled && !anyWaiting) {
                tmrUpdateWaitStatus.Stop();
            }
        }

        private void UpdateWaitingWatcherStatuses() {
            foreach (ThreadWatcher watcher in ThreadWatchers) {
                if (watcher.IsWaiting) {
                    SetWaitStatus(watcher);
                }
            }
        }

        private void SetSubItemText(ThreadWatcher watcher, ColumnIndex columnIndex, string text) {
            ListViewItem item = ((WatcherExtraData)watcher.Tag).ListViewItem;
            var subItem = item.SubItems[(int)columnIndex];
            if (subItem.Text != text) {
                subItem.Text = text;
                if (!_isLoadingThreadsFromFile) lvThreads.Sort();
            }
        }

        private void DisplayDescription(ThreadWatcher watcher) {
            SetSubItemText(watcher, ColumnIndex.Description, watcher.Description);
        }

        private void DisplayStatus(ThreadWatcher watcher, string status) {
            SetSubItemText(watcher, ColumnIndex.Status, status);
        }

        private void DisplayAddedOn(ThreadWatcher watcher) {
            DateTime time = ((WatcherExtraData)watcher.Tag).AddedOn;
            SetSubItemText(watcher, ColumnIndex.AddedOn, time.ToString("yyyy/MM/dd HH:mm:ss"));
        }

        private void DisplayLastImageOn(ThreadWatcher watcher) {
            DateTime? time = ((WatcherExtraData)watcher.Tag).LastImageOn;
            SetSubItemText(watcher, ColumnIndex.LastImageOn, time != null ? time.Value.ToString("yyyy/MM/dd HH:mm:ss") : String.Empty);
        }

        private void DisplayAddedFrom(ThreadWatcher watcher) {
            _watchers.TryGetValue(((WatcherExtraData)watcher.Tag).AddedFrom ?? String.Empty, out ThreadWatcher fromWatcher);
            SetSubItemText(watcher, ColumnIndex.AddedFrom, fromWatcher != null ? fromWatcher.Description : String.Empty);
        }

        private void DisplayCategory(ThreadWatcher watcher) {
            SetSubItemText(watcher, ColumnIndex.Category, watcher.Category);
        }

        private void DisplayData(ThreadWatcher watcher) {
            DisplayDescription(watcher);
            DisplayAddedOn(watcher);
            DisplayLastImageOn(watcher);
            if (!_isLoadingThreadsFromFile) DisplayAddedFrom(watcher);
            DisplayCategory(watcher);
        }

        private void SetDownloadStatus(ThreadWatcher watcher, DownloadType downloadType, int completeCount, int totalCount) {
            string type;
            bool hideDetail = false;
            int percComplete = 100;
            switch (downloadType) {
                case DownloadType.Page:
                    type = totalCount == 1 ? "page" : "pages";
                    hideDetail = totalCount == 1;
                    break;
                case DownloadType.Image:
                    type = "images";
                    break;
                case DownloadType.Thumbnail:
                    type = "thumbnails";
                    break;
                default:
                    return;
            }
            if (totalCount > 0) {
                percComplete = ((completeCount * 100) / totalCount);
            }
            else { }
            string status = hideDetail ? "Downloading " + type :
                string.Format("Downloading {0}: {1}% ({2} of {3} completed)", type, percComplete, completeCount, totalCount);
            DisplayStatus(watcher, status);
        }

        private void SetWaitStatus(ThreadWatcher watcher) {
            var remainingSeconds = (watcher.MillisecondsUntilNextCheck + 999) / 1000;
            var threadStatusMatchesSettings = ((watcher.ThreadStatusSimple == Settings.ThreadStatusSimple) && (watcher.ThreadStatusThreshold == Settings.ThreadStatusThreshold));
            SetWaitStatusString(remainingSeconds, out string statusStringOut);
            if (!threadStatusMatchesSettings) {
                DisplayStatus(watcher, statusStringOut);
                watcher.ThreadStatusSimple = Settings.ThreadStatusSimple;
                watcher.ThreadStatusThreshold = Settings.ThreadStatusThreshold;
                return;
            }
            if (Settings.ThreadStatusSimple == true && (remainingSeconds / 60) >= Settings.ThreadStatusThreshold && remainingSeconds % 60 != (0 | 59)) {
                return;
            }
            DisplayStatus(watcher, statusStringOut);
        }

        private static void SetWaitStatusString(int remainingSeconds, out string templateStatusStringOut) {
            string outNum = remainingSeconds.ToString();
            string outTimeScale = "Second";
            templateStatusStringOut = @"Waiting {0} {1}{2}";
            if (Settings.ThreadStatusSimple == true) {
                var remainingMinutes = remainingSeconds / 60;
                if ((remainingMinutes <= Settings.ThreadStatusThreshold) && Settings.ThreadStatusThreshold == 0) {
                    outNum = "less than 1";
                    outTimeScale = "Minute";
                }
                else if (remainingMinutes >= Settings.ThreadStatusThreshold) {
                    outNum = (remainingSeconds / 60).ToString();
                    outTimeScale = "Minute";
                }
                else {
                    outNum = remainingSeconds.ToString();
                }
            }
            string outPlural = outNum.Equals("1") || outNum.Equals("less than 1") ? "" : "s";
            templateStatusStringOut = string.Format(templateStatusStringOut, outNum, outTimeScale, outPlural);
        }

        private void SetStopStatus(ThreadWatcher watcher, StopReason stopReason) {
            string status = "Stopped: ";
            switch (stopReason) {
                case StopReason.UserRequest:
                    status += "User requested";
                    break;
                case StopReason.Exiting:
                    status += "Exiting";
                    break;
                case StopReason.PageNotFound:
                    status += "Page not found";
                    break;
                case StopReason.DownloadComplete:
                    status += "Download complete";
                    break;
                case StopReason.IOError:
                    status += "Error writing to disk";
                    break;
                case StopReason.DirtyShutdown:
                    status += "Unsafe shutdown";
                    break;
                default:
                    status += "Unknown error";
                    break;
            }
            DisplayStatus(watcher, status);
        }

        private void SetReparseStatus(ThreadWatcher watcher, ReparseType reparseType, int completeCount, int totalCount) {
            string type;
            bool hideDetail = false;
            switch (reparseType) {
                case ReparseType.Page:
                    type = totalCount == 1 ? "page" : "pages";
                    hideDetail = totalCount == 1;
                    break;
                case ReparseType.Image:
                    type = "images";
                    break;
                default:
                    return;
            }
            string status = hideDetail ? "Reparsing " + type :
                string.Format("Reparsing {0}: {1} of {2} completed", type, completeCount, totalCount);
            DisplayStatus(watcher, status);
        }

        private void SaveThreadList() {
            if (_isLoadingThreadsFromFile) return;
            try {
                XmlDocument _tmpThreadsDoc = new() {XmlResolver = null};
                XmlElement rootElem = _tmpThreadsDoc.CreateElement(String.Empty, "WatchedThreads", String.Empty);
                _tmpThreadsDoc.AppendChild(rootElem);
                XmlElement fileVersionElement = _tmpThreadsDoc.CreateElement(String.Empty, "FileVersion", String.Empty);
                XmlText fileVersionAttribute = _tmpThreadsDoc.CreateTextNode("5");
                fileVersionElement.AppendChild(fileVersionAttribute);
                XmlElement threadsElement = _tmpThreadsDoc.CreateElement(String.Empty, "Threads", String.Empty);
                foreach (ThreadWatcher watcher in ThreadWatchers) {
                    XmlElement _tmpXmlThread = _tmpThreadsDoc.CreateElement(String.Empty, "Thread", String.Empty);
                    foreach (string[] saveProp in watcher.ThreadSaveProperties()) {
                        XmlElement savePropElem = _tmpThreadsDoc.CreateElement(saveProp[0]);
                        XmlText savePropElemAttr = _tmpThreadsDoc.CreateTextNode(saveProp[1]);
                        savePropElem.AppendChild(savePropElemAttr);
                        _tmpXmlThread.AppendChild(savePropElem);
                    }
                    threadsElement.AppendChild(_tmpXmlThread);
                }
                _tmpThreadsDoc.DocumentElement.AppendChild(fileVersionElement);
                _tmpThreadsDoc.DocumentElement.AppendChild(threadsElement);
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.ThreadsFileName);
                try {
                    XmlWriterSettings _tmpThreadsDocSettings = new() {Indent = true};
                    XmlWriter writer = XmlWriter.Create(path, _tmpThreadsDocSettings);
                    _tmpThreadsDoc.Save(writer);
                    writer.Flush();
                    writer.Close();
                }
                catch (Exception ex) {
                    Logger.Log(ex.ToString());
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private void LoadThreadList() {
            try {
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.ThreadsFileName);
                if (!File.Exists(path)) {
                    bool needsConversion = false;
                    string txtPath = Path.Combine(Settings.GetSettingsDirectory(), "threads.txt");
                    if (File.Exists(txtPath)) {
                        needsConversion = true;
                    }
                    else return;
                    if (needsConversion) {
                        bool conversionSuccess = ConvertThreadsTxttoXml();
                        if (!conversionSuccess) { return; }
                    }
                }
                _isLoadingThreadsFromFile = true;
                Invoke(() => {
                    UpdateCategories(String.Empty);
                });
                try {
                    XmlReaderSettings xmlThreadsReadersettings = new() {XmlResolver = null};
                    XmlReader xmlThreadsReader = XmlReader.Create(path, xmlThreadsReadersettings);
                    XmlDocument xmlThreadsDoc = new() {XmlResolver = null};
                    xmlThreadsDoc.Load(xmlThreadsReader);
                    xmlThreadsReader.Close();
                    int fileVersion = Int32.Parse(xmlThreadsDoc.SelectSingleNode("WatchedThreads").SelectSingleNode("FileVersion").InnerText);
                    foreach (XmlNode childNode in xmlThreadsDoc.SelectSingleNode("WatchedThreads").SelectSingleNode("Threads")) {
                        if (childNode.Name != "Thread") continue;
                        ThreadInfo thread = new() {ExtraData = new WatcherExtraData()};
                        XmlNode URLLine = childNode.SelectSingleNode("PageURL");
                        if (URLLine != null) {
                            thread.URL = URLLine.InnerText;
                        }
                        else {
                            thread.URL = childNode.SelectSingleNode("URL").InnerText;
                        }
                        thread.PageAuth = childNode.SelectSingleNode("PageAuth").InnerText;
                        thread.ImageAuth = childNode.SelectSingleNode("ImageAuth").InnerText;
                        thread.CheckIntervalSeconds = Int32.Parse(childNode.SelectSingleNode("CheckIntervalSeconds").InnerText);
                        thread.OneTimeDownload = childNode.SelectSingleNode("OneTimeDownload").InnerText == "1";
                        thread.SaveDir = childNode.SelectSingleNode("SaveDir").InnerText;
                        thread.SaveDir = thread.SaveDir.Length != 0 ? General.GetAbsoluteDirectoryPath(thread.SaveDir, Settings.AbsoluteDownloadDirectory) : null;
                        string stopReasonLine = childNode.SelectSingleNode("StopReason").InnerText;
                        if (stopReasonLine.Length != 0) {
                            thread.StopReason = (StopReason)Int32.Parse(stopReasonLine);
                        }
                        else if (stopReasonLine.Length == 0 && _unsafeShutdown) {
                            thread.StopReason = (StopReason)6;
                        }
                        else { };
                        thread.Description = childNode.SelectSingleNode("Description").InnerText;
                        thread.ExtraData.AddedOn = new DateTime(Int64.Parse(childNode.SelectSingleNode("AddedOn").InnerText), DateTimeKind.Utc).ToLocalTime();
                        string lastImageOn = childNode.SelectSingleNode("LastImageOn").InnerText;
                        if (lastImageOn.Length != 0) {
                            thread.ExtraData.LastImageOn = new DateTime(Int64.Parse(lastImageOn), DateTimeKind.Utc).ToLocalTime();
                        }
                        thread.ExtraData.AddedFrom = childNode.SelectSingleNode("AddedFrom").InnerText;
                        thread.Category = childNode.SelectSingleNode("Category").InnerText;
                        thread.AutoFollow = childNode.SelectSingleNode("AutoFollow").InnerText == "1";
                        Invoke(() => {
                            AddThread(thread);
                        });
                    }
                }
                catch (Exception ex) {
                    Logger.Log(ex.ToString());
                }
                List<StopReason> _stopReasons = new() { StopReason.PageNotFound, StopReason.UserRequest, StopReason.DirtyShutdown };
                foreach (ThreadWatcher threadWatcher in ThreadWatchers) {
                    _watchers.TryGetValue(((WatcherExtraData)threadWatcher.Tag).AddedFrom, out ThreadWatcher parentThread);
                    threadWatcher.ParentThread = parentThread;
                    if (parentThread != null && !parentThread.ChildThreads.ContainsKey(threadWatcher.PageID) && !parentThread.ChildThreads.ContainsKey(parentThread.PageID)) {
                        parentThread.ChildThreads.Add(threadWatcher.PageID, threadWatcher);
                    }
                    ThreadWatcher watcher = threadWatcher;
                    Invoke(() => {
                        DisplayAddedFrom(watcher);
                    });
                    if (Settings.ChildThreadsAreNewFormat == true && !_stopReasons.Contains(threadWatcher.StopReason)) {
                        threadWatcher.Start();
                    }
                }
                if (Settings.ChildThreadsAreNewFormat != true) {
                    foreach (ThreadWatcher threadWatcher in ThreadWatchers) {
                        if (threadWatcher.ChildThreads.Count == 0 || threadWatcher.ParentThread != null) continue;
                        foreach (ThreadWatcher descendantThread in threadWatcher.DescendantThreads.Values) {
                            descendantThread.DoNotRename = true;
                            string sourceDir = descendantThread.ThreadDownloadDirectory;
                            string destDir;
                            if (General.RemoveLastDirectory(sourceDir) == descendantThread.MainDownloadDirectory) {
                                destDir = Path.Combine(descendantThread.MainDownloadDirectory, General.RemoveLastDirectory(sourceDir));
                            }
                            else {
                                destDir = Path.Combine(General.RemoveLastDirectory(threadWatcher.ThreadDownloadDirectory),
                                    General.GetRelativeDirectoryPath(descendantThread.ThreadDownloadDirectory, threadWatcher.ThreadDownloadDirectory));
                            }
                            if (String.Equals(destDir, sourceDir, StringComparison.Ordinal) || !Directory.Exists(sourceDir)) continue;
                            try {
                                if (String.Equals(destDir, sourceDir, StringComparison.OrdinalIgnoreCase)) {
                                    Directory.Move(sourceDir, destDir + " Temp");
                                    sourceDir = destDir + " Temp";
                                }
                                if (!Directory.Exists(General.RemoveLastDirectory(destDir))) Directory.CreateDirectory(General.RemoveLastDirectory(destDir));
                                Directory.Move(sourceDir, destDir);
                                descendantThread.ThreadDownloadDirectory = destDir;
                            }
                            catch (Exception ex) {
                                Logger.Log(ex.ToString());
                            }
                            descendantThread.DoNotRename = false;
                        }
                    }
                    Settings.ChildThreadsAreNewFormat = true;
                    Settings.Save();

                    foreach (ThreadWatcher threadWatcher in ThreadWatchers) {
                        if (threadWatcher.StopReason != StopReason.PageNotFound && threadWatcher.StopReason != StopReason.UserRequest) threadWatcher.Start();
                    }
                }
                _isLoadingThreadsFromFile = false;
            }
            catch (Exception ex) {
                _isLoadingThreadsFromFile = false;
                Logger.Log(ex.ToString());
            }
        }

        private static bool ConvertThreadsTxttoXml() {
            string txtPath = Path.Combine(Settings.GetSettingsDirectory(), "threads.txt");
            string[] lines = File.ReadAllLines(txtPath);
            if (lines.Length < 1) return false;
            int fileVersion = Int32.Parse(lines[0]);
            int linesPerThread;
            switch (fileVersion) {
                case 1: linesPerThread = 6; break;
                case 2: linesPerThread = 7; break;
                case 3: linesPerThread = 10; break;
                case 4: linesPerThread = 13; break;
                default: return false;
            }
            if (lines.Length < (1 + linesPerThread)) return false;
            int i = 1;
            List<Dictionary<string, string>> _tmpthreads = new();
            while (i <= lines.Length - linesPerThread) {
                Dictionary<string, string> _tmpThreadDict = new();
                _tmpThreadDict.Add("PageURL", lines[i++]);
                _tmpThreadDict.Add("PageAuth", lines[i++]);
                _tmpThreadDict.Add("ImageAuth", lines[i++]);
                _tmpThreadDict.Add("CheckIntervalSeconds", lines[i++]);
                _tmpThreadDict.Add("OneTimeDownload", lines[i++]);
                _tmpThreadDict.Add("SaveDir", lines[i++]);
                if (fileVersion >= 2) {
                    string stopReasonLine = lines[i++];
                    if (stopReasonLine.Length != 0) {
                        _tmpThreadDict.Add("StopReason", stopReasonLine);
                    }
                    else {
                        _tmpThreadDict.Add("StopReason", String.Empty);
                    }
                }
                if (fileVersion >= 3) {
                    _tmpThreadDict.Add("Description", lines[i++]);
                    _tmpThreadDict.Add("AddedOn", new DateTime(Int64.Parse(lines[i++]), DateTimeKind.Utc).ToLocalTime().Ticks.ToString());
                    string lastImageOn = lines[i++];
                    if (lastImageOn.Length != 0) {
                        _tmpThreadDict.Add("LastImageOn", new DateTime(Int64.Parse(lastImageOn), DateTimeKind.Utc).ToLocalTime().Ticks.ToString());
                    }
                    else {
                        _tmpThreadDict.Add("LastImageOn", String.Empty);
                    }
                }
                else {
                    _tmpThreadDict.Add("Description", String.Empty);
                    _tmpThreadDict.Add("AddedOn", DateTime.Now.ToString());
                    _tmpThreadDict.Add("LastImageOn", String.Empty);
                }
                if (fileVersion >= 4) {
                    _tmpThreadDict.Add("AddedFrom", lines[i++]);
                    _tmpThreadDict.Add("Category", lines[i++]);
                    _tmpThreadDict.Add("AutoFollow", lines[i++]);
                }
                else {
                    _tmpThreadDict.Add("AddedFrom", String.Empty);
                    _tmpThreadDict.Add("Category", String.Empty);
                    _tmpThreadDict.Add("AutoFollow", String.Empty);
                }
                _tmpthreads.Add(_tmpThreadDict);
            }
            XmlDocument _tmpThreadsDoc = new() {XmlResolver = null};
            XmlElement rootElem = _tmpThreadsDoc.CreateElement(String.Empty, "WatchedThreads", String.Empty);
            _tmpThreadsDoc.AppendChild(rootElem);
            XmlElement fileVersionElement = _tmpThreadsDoc.CreateElement(String.Empty, "FileVersion", String.Empty);
            XmlText fileVersionAttribute = _tmpThreadsDoc.CreateTextNode("5");
            fileVersionElement.AppendChild(fileVersionAttribute);
            XmlElement threadsElement = _tmpThreadsDoc.CreateElement(String.Empty, "Threads", String.Empty);
            foreach (Dictionary<string, string> _threaditem in _tmpthreads) {
                XmlElement _tmpXmlThread = _tmpThreadsDoc.CreateElement(String.Empty, "Thread", String.Empty);
                foreach (KeyValuePair<string, string> kvp in _threaditem) {
                    XmlElement _tmpXmlThreadElement = _tmpThreadsDoc.CreateElement(String.Empty, kvp.Key, String.Empty);
                    XmlText _tmpXmlThreadAttr = _tmpThreadsDoc.CreateTextNode(kvp.Value);
                    _tmpXmlThreadElement.AppendChild(_tmpXmlThreadAttr);
                    _tmpXmlThread.AppendChild(_tmpXmlThreadElement);
                }
                threadsElement.AppendChild(_tmpXmlThread);
            }
            _tmpThreadsDoc.DocumentElement.AppendChild(fileVersionElement);
            _tmpThreadsDoc.DocumentElement.AppendChild(threadsElement);
            string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.ThreadsFileName);
            try {
                XmlWriterSettings _tmpThreadsDocSettings = new() {Indent = true};
                XmlWriter writer = XmlWriter.Create(path, _tmpThreadsDocSettings);
                _tmpThreadsDoc.Save(writer);
                writer.Flush();
                writer.Close();
                string threadsTxtBakPath = $"{txtPath}.bak";
                if (File.Exists(threadsTxtBakPath)) {
                    try {
                        File.Delete(threadsTxtBakPath);
                    }
                    catch (Exception ex) {
                        Logger.Log(ex.ToString());
                    }
                }
                else { }
                string txtBackupPath = $"{txtPath}.xmlConversion.bak";
                File.Move(txtPath, txtBackupPath);
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
            return true;
        }

        private void LoadBlacklist() {
            try {
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.BlacklistFileName);
                if (!File.Exists(path)) return;
                string[] lines = File.ReadAllLines(path);
                if (lines.Length < 1) return;
                for (int i = 0; i < lines.Length; i++) {
                    string rule = lines[i];
                    if (rule.Split('/').Length == 3) {
                        _blacklist.Add(rule);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private void CheckForUpdates() {
            Thread thread = new(CheckForUpdateThread) {IsBackground = true};
            thread.Start();
        }

        private void CheckForUpdateThread() {
            string html;
            try {
                html = General.DownloadPageToString(General.ProgramURL);
            }
            catch {
                return;
            }
            Settings.LastUpdateCheck = DateTime.Now.Date;
            var htmlParser = new HTMLParser(html);
            List<string> latestVersions = new();
            foreach (HTMLTag repoContentDivTagStart in Enumerable.Where(htmlParser.FindStartTags("div"), t => HTMLParser.ClassAttributeValueHas(t, "css-truncate-target"))) {
                HTMLTag repoContentDivTagEnd = htmlParser.FindCorrespondingEndTag(repoContentDivTagStart);
                string repoDivInnerHtml = htmlParser.GetInnerHTML(repoContentDivTagStart, repoContentDivTagEnd);
                if (repoDivInnerHtml.Contains("span")) {
                    HTMLTag innerSpanStart = null;
                    HTMLTag innerSpanEnd = null;
                    HTMLParser spanHtmlParser = new(repoDivInnerHtml);
                    foreach (HTMLTag tempTag in spanHtmlParser.Tags) {
                        if (tempTag.Name == "span" && tempTag.IsEnd == false) { innerSpanStart = tempTag; }
                        else if (tempTag.Name == "span" && tempTag.IsEnd == true) { innerSpanEnd = tempTag; }
                    }
                    string spanInnerText = spanHtmlParser.GetInnerHTML(innerSpanStart, innerSpanEnd);
                    latestVersions.Add(spanInnerText.Trim(HTMLParser.GetWhiteSpaceChars()).Replace("v", ""));
                }
            }
            Version newestVersion = new("0.0");
            foreach (string incomingVersions in latestVersions) {
                Version incomingVersion = new(incomingVersions);
                var res = newestVersion.CompareTo(incomingVersion);
                if (res < 0) { newestVersion = incomingVersion; }
            }
            int latest = General.ParseVersionNumber(newestVersion.ToString());
            Console.WriteLine(latest);
            if (latest == -1) return;
            int current = General.ParseVersionNumber(General.Version);
            Console.WriteLine(current);
            if (!String.IsNullOrEmpty(Settings.LatestUpdateVersion)) {
                current = Math.Max(current, General.ParseVersionNumber(Settings.LatestUpdateVersion));
            }
            if (latest > current) {
                lock (_startupPromptSync) {
                    if (IsDisposed) return;
                    Settings.LatestUpdateVersion = newestVersion.ToString();
                    Invoke(() => {
                        if (MessageBox.Show(this, "A newer version of Chan Thread Watch is available.  Would you like to open the Chan Thread Watch website?",
                            "Newer Version Found", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) {
                            Process.Start(General.ProgramURL);
                        }
                    });
                }
            }
        }

        private IAsyncResult BeginInvoke(MethodInvoker method) {
            return BeginInvoke((Delegate)method);
        }

        private object Invoke(MethodInvoker method) {
            return Invoke((Delegate)method);
        }

        private static IEnumerable<ThreadWatcher> ThreadWatchers {
            get { return _watchers.Values; }
        }

        private IEnumerable<ThreadWatcher> SelectedThreadWatchers {
            get {
                foreach (ListViewItem item in lvThreads.SelectedItems) {
                    yield return (ThreadWatcher)item.Tag;
                }
            }
        }

        private enum ColumnIndex {
            Description = 0,
            Status = 1,
            LastImageOn = 2,
            AddedOn = 3,
            AddedFrom = 4,
            Category = 5
        }

        private void UpdateCategories(string key, bool remove = false) {
            key = key ?? String.Empty;
            bool hasKey = _categories.TryGetValue(key, out int count);
            int newCount = Math.Max(0, remove ? count - 1 : count + 1);
            _categories[key] = newCount;

            if (newCount == 0) {
                if (!String.IsNullOrEmpty(key)) {
                    _categories.Remove(key);
                    cboCategory.Items.Remove(key);
                }
            }
            else if (!hasKey) {
                cboCategory.Items.Add(key);
            }
        }

        private void FocusThread(string pageURL) {
            SiteHelper siteHelper = SiteHelpers.GetInstance((new Uri(pageURL)).Host);
            siteHelper.SetURL(pageURL);
            if (_watchers.TryGetValue(siteHelper.GetPageID(), out ThreadWatcher watcher)) {
                FocusThread(watcher);
            }
        }

        private void FocusThread(ThreadWatcher watcher) {
            ListViewItem item = ((WatcherExtraData)watcher.Tag).ListViewItem;
            lvThreads.SelectedItems.Clear();
            lvThreads.Select();
            item.Selected = true;
            item.EnsureVisible();
        }

        private void FocusLastThread() {
            if (lvThreads.Items.Count > 0) {
                FocusThread((ThreadWatcher)lvThreads.Items[lvThreads.Items.Count - 1].Tag);
            }
        }

        private static bool IsBlacklisted(string pageID) {
            if (_blacklist.Contains(pageID)) return true;
            if (Settings.BlacklistWildcards != true) return false;
            string[] pageIDSplit = pageID.Split('/');
            if (pageIDSplit.Length != 3) return false;
            foreach (string rule in _blacklist) {
                string[] ruleSplit = rule.Split('/');
                if (ruleSplit.Length != 3) continue;
                if (ruleSplit[0] != "*" && ruleSplit[0] != pageIDSplit[0]) continue;
                if (ruleSplit[1] != "*" && ruleSplit[1] != pageIDSplit[1]) continue;
                if (ruleSplit[2] != "*" && ruleSplit[2] != pageIDSplit[2]) continue;
                return true;
            }
            return false;
        }

        private static MonitoringInfo GetMonitoringInfo() {
            int running = 0;
            int dead = 0;
            int stopped = 0;
            foreach (ThreadWatcher watcher in ThreadWatchers) {
                if (watcher.IsRunning || watcher.IsWaiting) {
                    running++;
                }
                else if (watcher.StopReason == StopReason.PageNotFound) {
                    dead++;
                }
                else {
                    stopped++;
                }
            }
            return new MonitoringInfo {
                TotalThreads = _watchers.Count,
                RunningThreads = running,
                DeadThreads = dead,
                StoppedThreads = stopped
            };
        }

        private void UpdateWindowTitle(MonitoringInfo monitoringInfo) {
            Text = (Settings.WindowTitle ?? String.Format("{{{0}}}", WindowTitleMacro.ApplicationName))
                .Replace(String.Format("{{{0}}}", WindowTitleMacro.ApplicationName), Settings.ApplicationName)
                .Replace(String.Format("{{{0}}}", WindowTitleMacro.TotalThreads), monitoringInfo.TotalThreads.ToString())
                .Replace(String.Format("{{{0}}}", WindowTitleMacro.RunningThreads), monitoringInfo.RunningThreads.ToString())
                .Replace(String.Format("{{{0}}}", WindowTitleMacro.DeadThreads), monitoringInfo.DeadThreads.ToString())
                .Replace(String.Format("{{{0}}}", WindowTitleMacro.StoppedThreads), monitoringInfo.StoppedThreads.ToString());
        }
    }
}
