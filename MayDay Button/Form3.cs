using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using Microsoft.Win32;
using System.Management;
using System.Security.Principal;
using System.Media;

namespace MayDayButton
{
    using ps = Properties.Settings;
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChangeStartup(false);
            LoadSettings();
        }

        private void ChangeStartup(bool Remove = false)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", false);
                Object o = key.GetValue("MayDayButton");
                if (o == null)
                {
                    key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
                    key.SetValue("MayDayButton", Application.ExecutablePath);
                }
            }
            catch { GetAdmin(); }
        }

        private void GetAdmin(bool ShouldLoop = false)
        {
            Process p = new Process();
            try
            {
                var exeName = @"C:\MayDayButton\MayDayButton.exe";
                p.StartInfo.FileName = exeName;
                p.StartInfo.Verb = "runas";
                p.Start();
            }
            catch {
                if (ShouldLoop)
                    GetAdmin();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendData("UPDATE", "192.168.1.151");
            SendData("UPDATE", "192.168.1.203");
            SendData("UPDATE", "192.168.1.158");
            SendData("UPDATE", "192.168.1.89");
            SendData("UPDATE", "192.168.1.138");
            SendData("UPDATE", "192.168.1.161");
        }

        private void SendData(string Input, string IP)
        {
            try
            {
                TcpClient tcpclnt = new TcpClient();
                Console.WriteLine("Connecting.....");
                tcpclnt.Connect(IP, 8001);

                Console.WriteLine("Connected");
                Console.WriteLine("Enter the string to be transmitted : {0}", Input);

                Stream stm = tcpclnt.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(Input);
                Console.WriteLine("Transmitting.....");

                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[100];
                int k = stm.Read(bb, 0, 100);

                for (int i = 0; i < k; i++)
                    Console.WriteLine(Convert.ToChar(bb[i]));

                tcpclnt.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        //This is gross and I hate it
        string vFalse = @"\\192.168.1.210\Server\MaydayButton\TechVacation[FALSE].txt";
        string vTrue = @"\\192.168.1.210\Server\MaydayButton\TechVacation[TRUE].txt";
        private void button3_Click(object sender, EventArgs e)
        {
                if (File.Exists(vFalse))
                    File.Move(vFalse, vTrue);
                else if (File.Exists(vTrue))
                    File.Move(vTrue, vFalse);
                else
                    using (StreamWriter sw = File.CreateText(vFalse))
                        sw.WriteLine("");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GetAdmin();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            RestoreConnection(p);
            Thread.Sleep(1000);
            CloseConnection(p);
        }

        private void RestoreConnection(Process p)
        {
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = @"\\192.168.1.210\Server\MaydayButton\";
            p.Start();
            //p.WaitForInputIdle(100);
            //p.Kill();
            //p.Dispose();
        }

        private void CloseConnection(Process p)
        {
            p.Kill();
            p.Dispose();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Process.Start("cmd.exe");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            cmd("shutdown /r", false, true);
        }

        public void cmd(string Arguments, bool isHidden = false, bool asAdmin = false)
        {
            ProcessStartInfo ProcessInfo;
            Process Process;
            try
            {
                ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + Arguments);
                ProcessInfo.UseShellExecute = true;
                ProcessInfo.CreateNoWindow = isHidden;
                if (asAdmin)
                    ProcessInfo.Verb = "runas";
                Process = Process.Start(ProcessInfo);
                Process.WaitForExit();
                Process.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Processing ExecuteCommand : " + e.Message);
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LoadSettings();   
        }

        private void LoadSettings()
        {
            groupBox1.Text = "Settings - Max X:" + Screen.PrimaryScreen.WorkingArea.Width; ;
            xPos.Maximum = Screen.PrimaryScreen.WorkingArea.Width;

            xPos.Value = ps.Default.X;
            yPos.Value = ps.Default.Y_Norm;
            yAdj.Value = ps.Default.Y_Adjustment;

            sUpdate.Checked = ps.Default.ShouldUpdate;
            highDPI.Checked = ps.Default.HighDPI;
            aAdmin.Checked = ps.Default.AdminStart;

            cTimes.Value = ps.Default.Tried2Contact;
            if(File.Exists(Log))
                logView.Text = File.ReadAllText(Log);
        }

        const string Log = @"C:\MayDayButton\Log.txt";

        private void button10_Click(object sender, EventArgs e)
        {
            ps.Default.X = (Int32)xPos.Value;
            ps.Default.Y_Adjustment = (Int32)yAdj.Value;
            ps.Default.Y_Norm = (Int32)yPos.Value;

            ps.Default.ShouldUpdate = sUpdate.Checked;
            ps.Default.HighDPI = highDPI.Checked;
            ps.Default.AdminStart = aAdmin.Checked;

            ps.Default.Tried2Contact = (Int32)cTimes.Value;

            ps.Default.Save();
        }

        string SettingsFile = @"\\192.168.1.210\Server\MaydayButton\Settings.Config";
        private void button11_Click(object sender, EventArgs e)
        {
            File.Delete(SettingsFile);
            using (StreamWriter sw = File.CreateText(SettingsFile))
            {
                sw.WriteLine(ps.Default.X);
                sw.WriteLine(ps.Default.Y_Adjustment);
                sw.WriteLine(ps.Default.Y_Norm);

                sw.WriteLine(ps.Default.ShouldUpdate);
                sw.WriteLine(ps.Default.HighDPI);
                sw.WriteLine(ps.Default.AdminStart);

                //sw.WriteLine(ps.Default.Tried2Contact);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (File.Exists(SettingsFile))
            {
                string[] Settings = File.ReadAllLines(SettingsFile);
                ps.Default.X = Int32.Parse(Settings[0]);
                ps.Default.Y_Adjustment = Int32.Parse(Settings[1]);
                ps.Default.Y_Norm = Int32.Parse(Settings[2]);

                ps.Default.ShouldUpdate = Boolean.Parse(Settings[3]);
                ps.Default.HighDPI = Boolean.Parse(Settings[4]);
                ps.Default.AdminStart = Boolean.Parse(Settings[5]);

                ps.Default.Save();
                LoadSettings();
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            SendData("IMPORT", "192.168.1.151");
            SendData("IMPORT", "192.168.1.203");
            SendData("IMPORT", "192.168.1.158");
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            SendData("CHECKS", GetLocalIPAddress());
        }

        private void button15_Click(object sender, EventArgs e)
        {
            File.Delete(Log);
            logView.Text = "";
        }

        private void button17_Click(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.DeleteValue("MayDayButton");
        }

        private void button16_Click(object sender, EventArgs e)
        {

        }
    }
}
