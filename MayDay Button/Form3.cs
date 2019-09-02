using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace MayDayButton
{
    using ps = Properties.Settings;
    public partial class Form3 : Form
    {
        MD_Core MD = new MD_Core();
        Form1 frm1 = new Form1();
        public static string ServerLocation = Form1.ServerLocation;

        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MD.SetStartup();
            LoadSettings();
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

        public void SendData(string Input, string IP)
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


        private void button3_Click(object sender, EventArgs e)
        {
            MD.setVayCay();
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
            MD.GetAdmin();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MD.RestoreConnection();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Process.Start("cmd.exe");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            MD.cmd("shutdown /r /t 0");
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LoadSettings();   
        }

        string Log = @"C:\MayDayButton\Log.txt";
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

        private void button10_Click(object sender, EventArgs e)
        {
            ps.Default.X = (int)xPos.Value;
            ps.Default.Y_Norm = (int)yPos.Value;
            ps.Default.Y_Adjustment = (int)yAdj.Value;

            ps.Default.ShouldUpdate = sUpdate.Checked;
            ps.Default.HighDPI = highDPI.Checked;
            ps.Default.AdminStart = aAdmin.Checked;

            ps.Default.Tried2Contact = (int)cTimes.Value;

            ps.Default.Save();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            MD.exportSettings();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            MD.importSettings(ServerLocation + "\\Settings.Config");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            SendData("IMPORT", "192.168.1.151");
            SendData("IMPORT", "192.168.1.203");
            SendData("IMPORT", "192.168.1.158");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            MD.Checks();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            File.Delete(Log);
            logView.Text = "";
        }

        private void button17_Click(object sender, EventArgs e)
        {
            MD.DeleteStartup();
            LoadSettings();
        }

        private void button16_Click(object sender, EventArgs e)
        {

        }

        private void button16_Click_1(object sender, EventArgs e)
        {
           logView.Text = MD.DisplayUpNetworkConnectionsInfo();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            MD.RestoreConnection();
           if(File.Exists(ServerLocation + "LicenseTerms.txt"))
            {
                logView.Text = File.ReadAllText(ServerLocation + "LicenseTerms.txt");
            }
            else
            {
                MessageBox.Show("License terms missing! Please verify the server location is correct!");
            }
        }

        private void yAdj_ValueChanged(object sender, EventArgs e)
        {
            ps.Default.Y_Adjustment = (Int32)yAdj.Value;
            ps.Default.Save();
        }

        private void yPos_ValueChanged(object sender, EventArgs e)
        {
            ps.Default.Y_Norm = (Int32)yPos.Value;
            ps.Default.Save();
        }

        private void highDPI_CheckedChanged(object sender, EventArgs e)
        {
            ps.Default.HighDPI = highDPI.Checked;
            ps.Default.Save();
        }

        private void aAdmin_CheckedChanged(object sender, EventArgs e)
        {
            ps.Default.AdminStart = aAdmin.Checked;
            ps.Default.Save();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            MD.exportSettings(ServerLocation + "\\Settings.Config");
        }

        private void button19_Click(object sender, EventArgs e)
        {
            MD.importSettings();
        }
    }
}
