using MRJoiner.utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!isElevated)
            {
                button1.Enabled = false;
                MessageBox.Show("Please restart as Administrator");
            }
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
                MessageBox.Show("Done!");
                button1.Text = "Stop";
            }
            else if (button1.Text == "Stop")
            {

                cmd.runCommand("netsh wlan stop hostednetwork");
                MessageBox.Show("Done!");
                button1.Text = "Start";
            }
        }catch(Exception exx) { MessageBox.Show("Something went wrong!"); }
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
    }
}
