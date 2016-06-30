using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SharpNes
{
    public class RecentFileManager
    {
        /// <exception cref="ArgumentException">If anything is null or nameOfProgram contains a forward slash or is empty.</exception>
        public RecentFileManager(ToolStripMenuItem parentMenuItem,
            Action<object, EventArgs> recentFileClickedHandler,
            Action<object, EventArgs> clearRecentFilesClickedHandler = null,
            Image fileIcon = null)
        {
            if (parentMenuItem == null)
                throw new ArgumentNullException("parentMenuItem");

            this.ParentMenuItem = parentMenuItem;
            this.RecentFileClicked = recentFileClickedHandler;
            this.ClearRecentFilesClicked = clearRecentFilesClickedHandler;
            this.FileIcon = fileIcon;

            this.RefreshRecentFilesMenu();
        }

        #region Public Properties

        public Image FileIcon { get; set; }

        #endregion

        #region Public members

        public void AddRecentFile(string recentFile)
        {
            // remove if already there to add as first
            RemoveRecentFile(recentFile);

            string recentFiles = EmulatorConfiguration.Instance["RecentFiles"];
            if (recentFiles == null)
                recentFiles = "";

            recentFiles = recentFile.Trim() + "|" + recentFiles;

            EmulatorConfiguration.Instance["RecentFiles"] = recentFiles;

            this.RefreshRecentFilesMenu();
        }

        public void RemoveRecentFile(string recentFile)
        {
            string recentFiles = EmulatorConfiguration.Instance["RecentFiles"];
            if (recentFiles == null)
                recentFiles = "";

            recentFiles = String.Join("|",
                recentFiles.Split(new char[] { '|'}).Where(
                    (x) => x.ToLower() != recentFile.ToLower()));

            EmulatorConfiguration.Instance["RecentFiles"] = recentFiles;

            this.RefreshRecentFilesMenu();
        }

        #endregion

        #region private methods

        private void RefreshRecentFilesMenu()
        {
            string recentFiles = EmulatorConfiguration.Instance["RecentFiles"];
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
                if (FileIcon != null)
                    recentFileMenuItem.Image = FileIcon;

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

        private void OnClearRecentFilesClicked(object sender, EventArgs eventArgs)
        {
            EmulatorConfiguration.Instance["RecentFiles"] = "";
            RefreshRecentFilesMenu();
            if (ClearRecentFilesClicked != null)
                this.ClearRecentFilesClicked(sender, eventArgs);
        }

        #endregion

        #region Private members
        private ToolStripMenuItem ParentMenuItem;
        private Action<object, EventArgs> RecentFileClicked;
        private Action<object, EventArgs> ClearRecentFilesClicked;
        #endregion
    }
}

