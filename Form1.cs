using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.NetworkInformation;
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
            GetEthernetList();
        }


        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
            else
            {
                this.ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            EthernetName = ConfigurationManager.AppSettings["EthernetName"];
            GetEthernetList();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            if (!CheckIPAddress())
            {
                ButtonChanger(false, "IP addresses are not valid!", MessageBoxIcon.Error);
                return;
            }

            var reslt = DnsChanger(true);
            if (reslt)
            {
                ButtonChanger(reslt, "DNS changed successfuly.", MessageBoxIcon.Information);
            }
            else
            {
                ButtonChanger(reslt, "Can't change DNS setting.", MessageBoxIcon.Exclamation);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            var result = DnsChanger(false);
            if (result)
            {
                ButtonChanger(false, "DNS reseted to default values", MessageBoxIcon.Information);
            }
            else
            {
                ButtonChanger(result, "reset", MessageBoxIcon.Exclamation);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lblEthernetName.Text = $"We're checking [{EthernetName}] setting...";
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "برنامه بررسی لاک کردن ویندوز در حال اجرا است.";
            notifyIcon1.BalloonTipTitle = "Lock Screen Checker";
            notifyIcon1.ShowBalloonTip(2000);
            //txtDns1.Mask = "###.###.###.###";
            //txtDns2.Mask = "###.###.###.###";
            txtDns1.ValidatingType = typeof(System.Net.IPAddress);
            txtDns2.ValidatingType = typeof(System.Net.IPAddress);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Show();
            notifyIcon1.Visible = false;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                DnsChanger(false);
                MessageBox.Show("DNS config reset to DHCP.", "LockScreenChecker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Set/Reset dns setting
        /// </summary>
        /// <param name="v"></param>
        /// <exception cref="NotImplementedException"></exception>
        private bool DnsChanger(bool data)
        {
            bool result = false;

            try
            {
                if (data)
                {
                    ProcessExecutter($"/c netsh interface ip set dns \"{EthernetName}\" static {txtDns1.Text}");
                    ProcessExecutter($"/c netsh interface ip add dns \"{EthernetName}\" {txtDns2.Text} index=2");
                    ProcessExecutter("ipconfig/flushdns");
                }
                else
                {
                    ProcessExecutter($"/c netsh interface ip set dns \"{EthernetName}\" dhcp");
                }

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }

        /// <summary>
        /// Execute a command line
        /// </summary>
        /// <param name="command"></param>
        private void ProcessExecutter(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            try
            {
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = command;
                process.StartInfo = startInfo;
                process.Start();
            }
            catch
            {
                throw;
            }
            finally
            {
                process.Dispose();
            }
        }

        /// <summary>
        /// Get ethernet list
        /// </summary>
        /// <returns></returns>
        private void GetEthernetList()
        {
            lstEthernets.Items.Clear();
            var result = new List<string>();

            try
            {
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var name = item.Name;
                    string ips = string.Empty;

                    for (int i = 0; i < item.GetIPProperties().DnsAddresses.Count; i++)
                    {
                        ips += item.GetIPProperties().DnsAddresses[i].ToString() + "|";
                    }

                    if (string.IsNullOrEmpty(ips))
                    {
                        result.Add($"{name}");
                    }
                    else
                    {
                        result.Add($"{name}->{ips}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add($"Error: {ex.Message}");
            }

            lstEthernets.Items.AddRange(result.ToArray());
        }

        /// <summary>
        /// change button activity
        /// </summary>
        /// <param name="reslt"></param>
        private void ButtonChanger(bool reslt, string message, MessageBoxIcon icon)
        {
            if (reslt)
            {
                btnSet.Enabled = false;
                btnReset.Enabled = true;
                txtDns1.Enabled = false;
                txtDns2.Enabled = false;
                MessageBox.Show(message, "Set DNS", MessageBoxButtons.OK, icon);
            }
            else
            {
                btnSet.Enabled = true;
                btnReset.Enabled = false;
                txtDns1.Enabled = true;
                txtDns2.Enabled = true;
                MessageBox.Show(message, "Set DNS", MessageBoxButtons.OK, icon);
            }
        }

        /// <summary>
        /// check IP addresses
        /// </summary>
        /// <returns></returns>
        private bool CheckIPAddress()
        {
            bool result = false;
            IPAddress address;
            if (IPAddress.TryParse(txtDns1.Text, out address) && IPAddress.TryParse(txtDns2.Text, out address))
            {
                result = true;
            }
            return result;
        }

    }
}
