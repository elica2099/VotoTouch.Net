using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VotoTouch
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void pbSplash_Click(object sender, EventArgs e)
        {
            pbSplash.Value = 100;
        }

        public void SetSplash(int Value)
        {
            pbSplash.Value = Value;
            pbSplash.Update();
        }

        public void SetSplash(int Value, string Msg)
        {
            pbSplash.Value = Value;
            label2.Text = Msg;
            pbSplash.Update();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    
    }
}
