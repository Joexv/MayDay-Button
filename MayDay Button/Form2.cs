using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MayDayButton
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

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
            numericUpDown1.Value = (decimal)Properties.Settings.Default.X;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AppendLog("Someone tried to close me via password!");
            string promptValue = ShowDialog("Warning!", "In order to close the MayDay Button you must confirm with a password.\nIf you do not know the password you should not be closing the MayDay Button!");
            if (promptValue == "password")
                Application.Exit();
        }

        const string Log = @"C:\MayDayButton\Log.txt";
        private void AppendLog(string Text)
        {
            string formatted = String.Format("[{0}]::{1}", DateTime.Now.ToString("MM/dd h:mm tt"), Text);
            Directory.CreateDirectory(@"C:\MayDayButton\");
            if (!File.Exists(Log))
                using (StreamWriter sw = File.CreateText(Log))
                {
                    sw.WriteLine(formatted);
                }
            else
                using (StreamWriter sw = File.AppendText(Log))
                {
                    sw.WriteLine(formatted);
                }
        }

        public static string ShowDialog(string caption, string text)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            textLabel.AutoSize = true;
            TextBox textBox = new TextBox() { Left = 50, Top = 60, Width = 400 };
            textBox.UseSystemPasswordChar = true;
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 80, DialogResult = DialogResult.OK };
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
            Application.Restart();
        }
    }
}
