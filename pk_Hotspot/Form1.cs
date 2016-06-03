using MRJoiner.utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pk_Hotspot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

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
            }
        }

        private void exitClick(object sender, EventArgs e)
        {
            if (button1.Text == "Stop")
            {

                cmd.runCommand("netsh wlan stop hostednetwork");
                MessageBox.Show("Service Stopped!");
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

                cmd.runCommand("netsh wlan set hostednetwork mode=allow ssid=\"" + hostname + "\" key=\"" + pass + "\"");
                cmd.runCommand("netsh wlan start hostednetwork");
                MessageBox.Show("Service Started!");
                button1.Text = "Stop";
            }
            else if (button1.Text == "Stop")
            {

                cmd.runCommand("netsh wlan stop hostednetwork");
                MessageBox.Show("Service Stopped!!");
                button1.Text = "Start";
            }
        }catch(Exception exx) { MessageBox.Show("Something went wrong!"); Close(); }
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
                MessageBox.Show("Done!");
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
    }
}
