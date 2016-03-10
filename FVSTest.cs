using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VotoTouch
{
    public partial class FVSTest : Form
    {
        private System.Windows.Forms.Timer timCloseForm;
        private System.Windows.Forms.Timer timCloseFormLabel;
        private int timsec = 5;
        
        public FVSTest(string ABadge)
        {
            InitializeComponent();

            timCloseForm = new System.Windows.Forms.Timer();
            timCloseForm.Enabled = false;
            timCloseForm.Interval = 5000;
            timCloseForm.Tick += timCloseForm_Tick;
            timCloseFormLabel = new System.Windows.Forms.Timer();
            timCloseFormLabel.Enabled = false;
            timCloseFormLabel.Interval = 1000;
            timCloseFormLabel.Tick += timCloseFormLabel_Tick;

            lbTest.Text = ABadge;

            timsec = 5;

            timCloseForm.Enabled = true;
            timCloseFormLabel.Enabled = true;
        }

        private void timCloseForm_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timCloseFormLabel_Tick(object sender, EventArgs e)
        {
            timsec--;
            label1.Text = "La finestra si chiuderà tra " + timsec.ToString() + " secondi";
        }

    }
}
