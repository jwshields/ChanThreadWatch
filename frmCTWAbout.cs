using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace JDP {
    public partial class frmCTWAbout : Form {

        public frmCTWAbout() {
            InitializeComponent();
            this.lblCTWVersion.Text = String.Format("Version: {0}", General.Version);
            this.lblBuildDate.Text = String.Format("Build Date: {0}", General.ReleaseDate);

        }
        private void btnCloseAbout_Click(object sender, EventArgs e) {
            this.Close();
        }

        public void FormKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                this.Close();
            }
        }

        private void FormLinkClick(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(((Control.ControlAccessibleObject)((Control)sender).AccessibilityObject).Name);
        }
    }
}
