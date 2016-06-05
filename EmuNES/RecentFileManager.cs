using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace EmuNES
{
    public class RecentFileManager
    {
        /// <exception cref="ArgumentException">If anything is null or nameOfProgram contains a forward slash or is empty.</exception>
        public RecentFileManager(ToolStripMenuItem parentMenuItem,
            Action<object, EventArgs> recentFileClickedHandler,
            Action<object, EventArgs> clearRecentFilesClickedHandler = null)
        {
            if (parentMenuItem == null)
                throw new ArgumentNullException("parentMenuItem");

            this.ParentMenuItem = parentMenuItem;
            this.RecentFileClicked = recentFileClickedHandler;
            this.ClearRecentFilesClicked = clearRecentFilesClickedHandler;

            this.RefreshRecentFilesMenu();
        }

        #region Public members

        public void AddRecentFile(string recentFile)
        { 
            string recentFiles = ConfigurationManager.AppSettings["RecentFiles"];
            if (recentFiles == null)
                recentFiles = "";

            recentFiles = recentFile.Trim() + "|" + recentFiles;

            UpdateRecentFiles(recentFiles);

            this.RefreshRecentFilesMenu();
        }

        public void RemoveRecentFile(string recentFile)
        {
            string recentFiles = ConfigurationManager.AppSettings["RecentFiles"];

            recentFiles = String.Join("|",
                recentFiles.Split(new char[] { '|'}).Where(
                    (x) => x.ToLower() != recentFile.ToLower()));

            UpdateRecentFiles(recentFiles);

            this.RefreshRecentFilesMenu();
        }

        #endregion

        #region private methods

        private void UpdateRecentFiles(string recentFiles)
        {
            try
            {
                var configurationFile
                    = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configurationFile.AppSettings.Settings;

                if (settings["RecentFiles"] == null)
                    settings.Add("RecentFiles", recentFiles);
                else
                    settings["RecentFiles"].Value = recentFiles;

                configurationFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configurationFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Unabled to update list of most recently used files",
                    "Recently Used Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshRecentFilesMenu()
        {
            string recentFiles = ConfigurationManager.AppSettings["RecentFiles"];
            if (recentFiles == null)
            {
                this.ParentMenuItem.Enabled = false;
                return;
            }

            ToolStripItem recentFileMenuItem = null;

            this.ParentMenuItem.DropDownItems.Clear();
            string[] valueNames = recentFiles.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string valueName in valueNames)
            {
                recentFileMenuItem = this.ParentMenuItem.DropDownItems.Add(valueName);
                if (RecentFileClicked != null)
                    recentFileMenuItem.Click += new EventHandler(this.RecentFileClicked);
            }

            if (this.ParentMenuItem.DropDownItems.Count == 0)
            {
                this.ParentMenuItem.Enabled = false;
                return;
            }

            this.ParentMenuItem.DropDownItems.Add("-");
            recentFileMenuItem = this.ParentMenuItem.DropDownItems.Add("Clear list");
            recentFileMenuItem.Click += new EventHandler(this.OnClearRecentFilesClicked);
            this.ParentMenuItem.Enabled = true;
        }

        private void OnClearRecentFilesClicked(object obj, EventArgs evt)
        {
            UpdateRecentFiles("");
            if (ClearRecentFilesClicked != null)
                this.ClearRecentFilesClicked(obj, evt);
        }

        #endregion

        #region Private members
        private ToolStripMenuItem ParentMenuItem;
        private Action<object, EventArgs> RecentFileClicked;
        private Action<object, EventArgs> ClearRecentFilesClicked;
        #endregion
    }
}

