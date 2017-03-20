using Microsoft.Win32;
using MRJoiner.utility;
using NETCONLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pk_Hotspot
{
    public partial class Form1 : Form
    {

        NetworkInterface wifi;
        public Form1()
        {
            InitializeComponent();
            // when resumeing from Sleep or hibernate
            SystemEvents.PowerModeChanged += PowerModeChanged;
            MenuItem exit = new MenuItem();
            exit.Text = "Exit";
            exit.Click += new EventHandler(exitClick); ;
            exit.Index = 0;
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(exit);

            
            notifyIcon1.ContextMenu = menu;

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!isElevated)
            {
                button1.Enabled = false;
                MessageBox.Show("Please restart as Administrator");
                Close();
            }
            
            
        }

        private void changeStatus()
        {
            if (status.Text == "Started")
            {
                status.ForeColor = Color.Red;
                status.Text = "Stopped";
            }
            else
            {
                status.ForeColor = Color.Green;
                status.Text = "Started";
            }
        }

        private void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            // when resuming from hibernate or sleep mode
            if (e.Mode == PowerModes.Resume)
            {
                if (button1.Text == "Stop")
                {

                    cmd.runCommand("netsh wlan stop hostednetwork");
                    //MessageBox.Show("Service Stopped!!");
                    button1.Text = "Start";
                    string hostname = textBox1.Text;
                    string pass = textBox2.Text;

                    cmd.runCommand("netsh wlan set hostednetwork mode=allow ssid=\"" + hostname + "\" key=\"" + pass + "\"");
                    cmd.runCommand("netsh wlan start hostednetwork");
                    //MessageBox.Show("Service Started!");
                    button1.Text = "Stop";
                }
            }
        }

        private void exitClick(object sender, EventArgs e)
        {
            if (button1.Text == "Stop")
            {

                cmd.runCommand("netsh wlan stop hostednetwork");
                //MessageBox.Show("Service Stopped!");
                button1.Text = "Start";

            }
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/BurningHAM18");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool ok = doControls();
            try { 
            if (button1.Text == "Start" && ok)
            {
                string hostname = textBox1.Text;
                string pass = textBox2.Text;
                    saveconfigs();
                cmd.runCommand("netsh wlan set hostednetwork mode=allow ssid=\"" + hostname + "\" key=\"" + pass + "\"");
                cmd.runCommand("netsh wlan start hostednetwork");
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    //MessageBox.Show("Service Started!");
                    changeStatus();
                    
                button1.Text = "Stop";
            }
            else if (button1.Text == "Stop")
            {
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    cmd.runCommand("netsh wlan stop hostednetwork");
                    //MessageBox.Show("Service Stopped!!");
                    changeStatus();
                button1.Text = "Start";
            }
        }catch(Exception exx) { MessageBox.Show("Something went wrong!"); Close(); }
        }

        private void saveconfigs()
        {
            string[] configs = new string[2];
            configs[0] = textBox1.Text;
            configs[1] = textBox2.Text;
            File.WriteAllLines("configs", configs);
        }

        private bool doControls()
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("Fill all fields first!");
                return false;
            }
            if(textBox2.TextLength < 8)
            {
                MessageBox.Show("Password must be longer than 8 characters");
                return false;
            }
            return true;
        }

        private void Closevent(object sender, FormClosingEventArgs e)
        {
            if (button1.Text == "Stop")
            {

                cmd.runCommand("netsh wlan stop hostednetwork");
                button1.Text = "Start";
            }
        }

        private void iconDoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            { this.Show(); this.WindowState = FormWindowState.Normal; }

            this.Activate();
            notifyIcon1.Visible = false;
        }

        private void minimizedChanged(object sender, EventArgs e)
        {
            //now is the new minimize method
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filename = "readme_pk.txt";
            if (!File.Exists(filename)) File.WriteAllText(filename, Properties.Resources.help);
            System.Diagnostics.Process.Start("readme_pk.txt");
            System.Threading.Thread.Sleep(2000);
            if (File.Exists(filename)) File.Delete(filename);
        }

        #region AUTOSTART
        private void StartupAppInRegistery(bool enable)
        {
            if (enable)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", Application.ProductName,
                    string.Format("\"{0}\" " + "-startup", Application.ExecutablePath));

            }
            else
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", Application.ProductName, "");
            }
        }

        private void chkAutoStartWindows_CheckedChanged(object sender, EventArgs e)
        {
            var auto = chkAutoStartWindows.Checked;
            try
            {
                var proc = new Process();
                proc.EnableRaisingEvents = true;
                proc.Exited += (s, args) =>
                {
                    if (proc.ExitCode == 0)
                    {
                        StartupAppInRegistery(false);
                    }
                    else
                    {
                        StartupAppInRegistery(true);
                    }
                    proc.Dispose();
                };


                if (auto)
                {
                    proc.StartInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments =
                            string.Format("/create /f /sc onlogon /tn pk_hotspot /rl highest /DELAY 0001:00 /tr \"{0} -startup\"",
                                Application.ExecutablePath),
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    };
                }
                else
                {
                    proc.StartInfo = (new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = "/delete /f /tn pk_hotspot",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    });
                }
                proc.Start();

            }
            catch { }
        }
        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void OnLoadEvent(object sender, EventArgs e)
        {
            if (File.Exists("configs"))
            {
                string[] configs = File.ReadAllLines("configs");
                textBox1.Text = configs[0];
                textBox2.Text = configs[1];

            }
        }
    }

}
