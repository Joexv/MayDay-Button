using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Security.Principal;
using System.Threading;
using SHDocVw;
using System.Linq;

namespace MayDayButton
{
    using ps = Properties.Settings;
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        const string Password = "password";
        public int Label_Height { get; set; }
        MD_Core MD = new MD_Core();

        private void button1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(MD.listPrinters());
            //MessageBox.Show(MD.listPrintQueues());
            string zebraError = MD.zebraStatuses();
            if (zebraError != "")
            {
                MessageBox.Show("Errors found for your Zebra Printer(Label Printer). Please correct the following error before trying again:\n---------------\n" + zebraError);
            }
            else
            {
                MD.testPrinter();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AppendLog("Restarting");
            Application.Restart();
        }

        //TODO add proper checks for making sure high DPI devices dont get hidden on accident
        private void Form2_Load(object sender, EventArgs e)
        {
            Form1 frm1 = new Form1();
            Decimal x = (decimal)ps.Default.X;
            if (x < 0) x = 0;
            int MaxX = Screen.PrimaryScreen.WorkingArea.Width;
            if (x >= MaxX) x = MaxX - frm1.Width;
            numericUpDown1.Value = x;
            numericUpDown1.Maximum = MaxX - frm1.Width;
            numericUpDown1.Minimum = 0;
            checkBox1.Checked = ps.Default.HighDPI;

            if (!ps.Default.HighDPI)
            {
                numericUpDown2.Value = ps.Default.Y_Norm;

                int MinY = (frm1.Height * -1) + Label_Height;
                Console.WriteLine(MinY + 25);
                Console.WriteLine(frm1.Height * -1);
                numericUpDown2.Maximum = MinY + 25;
                numericUpDown2.Minimum = (frm1.Height * -1);
            }
            else
            {
                numericUpDown2.Maximum = 999;
                numericUpDown2.Minimum = -999;
                numericUpDown2.Value = ps.Default.Y_Adjustment;
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            AppendLog("Someone tried to close me via password!");
            string promptValue = ShowDialog("Warning!", "In order to close the MayDay Button you must confirm with a password.\nIf you do not know the password you should not be closing the MayDay Button!");
            if (promptValue == Password)
                Application.Exit();
        }

        const string Log = @"C:\MayDayButton\Log.txt";
        private void AppendLog(string Text)
        {
            string formatted = String.Format("[{0}]::{1}", DateTime.Now.ToString("MM/dd h:mm tt"), Text);
            Directory.CreateDirectory(@"C:\MayDayButton\");
            if (!File.Exists(Log))
                using (StreamWriter sw = File.CreateText(Log))
                    sw.WriteLine(formatted);
            else
                using (StreamWriter sw = File.AppendText(Log))
                    sw.WriteLine(formatted);
        }

        public static string ShowDialog(string caption, string text)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                AutoScaleMode = AutoScaleMode.Dpi,
                AutoSize = true
            };
            Label textLabel = new Label() { Left = 15, Top = 20, Text = text };
            textLabel.AutoSize = true;
            TextBox textBox = new TextBox() { Left = 15, Top = 60, Width = 375 };
            textBox.UseSystemPasswordChar = true;
            Button confirmation = new Button() { Text = "Ok", Left = 315, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ps.Default.ShouldUpdate = true;
            ps.Default.Save();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ps.Default.HighDPI = checkBox1.Checked;
            ps.Default.Save();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            GetAdmin();
        }

        private void GetAdmin()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
            startInfo.Verb = "runas";
            Process.Start(startInfo);
            Application.Exit();
        }

        public static bool IsAdministrator =>
             new WindowsPrincipal(WindowsIdentity.GetCurrent())
               .IsInRole(WindowsBuiltInRole.Administrator);

        string vFalse = @"\\192.168.1.210\Server\MaydayButton\TechVacation[FALSE].txt";
        string vTrue = @"\\192.168.1.210\Server\MaydayButton\TechVacation[TRUE].txt";

        private void button6_Click(object sender, EventArgs e)
        {
            if (File.Exists(vFalse))
                File.Move(vFalse, vTrue);
            else if (File.Exists(vTrue))
                File.Move(vTrue, vFalse);
            else
                using (StreamWriter sw = File.CreateText(vFalse))
                    sw.WriteLine("");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form3 frm = new Form3();
            string promptValue = ShowDialog("Warning!", "The Admin Panel has a lot of things that can potentially break the register, only use it if you know what you're doing!\nEnter the password to gain complete access or simply press OK to view licensing information.");
            if (promptValue != Password)
            {
                //Disable Buttons in the admin panel if password is incorrect
                //foreach (var button in frm.Controls.OfType<Button>().Where(a => a.Text != "License Information"))
                foreach (var button in frm.Controls.OfType<Button>())
                {
                    button.Enabled = false;
                    button.Visible = false;
                }

                foreach (var gp in frm.Controls.OfType<GroupBox>())
                {
                    gp.Enabled = false;
                    gp.Visible = false;
                }
            }

            frm.Show();
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ps.Default.X = (Int32)numericUpDown1.Value;
            ps.Default.Save();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (ps.Default.HighDPI)
            {
                ps.Default.Y_Adjustment = (Int32)numericUpDown2.Value;
                ps.Default.Save();
            }
            else
            {
                ps.Default.Y_Norm = (Int32)numericUpDown2.Value;
                ps.Default.Save();
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
