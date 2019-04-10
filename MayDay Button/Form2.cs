using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            Application.Restart();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = (decimal)Properties.Settings.Default.X;
        }
    }
}
