using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Security.Principal;
using System.Threading;
using SHDocVw;

namespace MayDayButton
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        const string Password = "password";

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.X = (Int32)numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AppendLog("Restarting");
            Application.Restart();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Decimal x = (decimal)Properties.Settings.Default.X;
            if (x < 0) x = 0;
            int MaxX = Screen.PrimaryScreen.WorkingArea.Width;
            if (x >= MaxX) x = MaxX - this.Width;
            numericUpDown1.Value = x;
            numericUpDown1.Maximum = MaxX - this.Width;
            numericUpDown1.Minimum = 0;
            checkBox1.Checked = Properties.Settings.Default.HighDPI;
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
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            textLabel.AutoSize = true;
            TextBox textBox = new TextBox() { Left = 50, Top = 60, Width = 400 };
            textBox.UseSystemPasswordChar = true;
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShouldUpdate = true;
            Properties.Settings.Default.Save();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.HighDPI = checkBox1.Checked;
            Properties.Settings.Default.Save();
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
            //AppendLog("Someone tried to open the admin panel");
            string promptValue = ShowDialog("Warning!", "The Admin Panel has a lot of things that can potentially break the register, only use it if you know what you're doing!");
            if (promptValue == Password)
            {
                Form3 frm = new Form3();
                frm.Show();
                this.Close();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
