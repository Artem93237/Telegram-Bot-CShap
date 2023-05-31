using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelegramBot
{
    public partial class EditGoodNumber : Form
    {
        public string returnNumber;
        public EditGoodNumber(string orgt)
        {
            InitializeComponent();
            textBox1.Text = orgt;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            returnNumber = textBox1.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
