using LockScreenChecker.Properties;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace LockScreenChecker
{
    public partial class frmMain : Form
    {
        public string EthernetName { get; set; }

        public frmMain()
        {
            InitializeComponent();
            EthernetName = ConfigurationManager.AppSettings["EthernetName"];
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "برنامه بررسی لاک کردن ویندوز در حال اجرا است.";
            notifyIcon1.BalloonTipTitle = "Lock Screen Checker";
            notifyIcon1.ShowBalloonTip(2000);
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/c netsh interface ip set dns \"{EthernetName}\" dhcp";
                process.StartInfo = startInfo;
                process.Start();
                MessageBox.Show("DNS config reset to DHCP.", "LockScreenChecker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Show();
            notifyIcon1.Visible = false;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            EthernetName = ConfigurationManager.AppSettings["EthernetName"];
        }
    }
}
