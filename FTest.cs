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
    public partial class FTest : Form
    {
        private CVotoBaseDati DBDati;
        
        public FTest(CVotoBaseDati ADBDati)
        {
            InitializeComponent();
            DBDati = ADBDati;

            btnTestAssemblea.Visible = false;

#if DEBUG
            btnTestAssemblea.Visible = true;
#endif

        }

        private void btnTestVideate_Click(object sender, EventArgs e)
        {
            // testa le videate ciclando su di esse

        }
    }
}
