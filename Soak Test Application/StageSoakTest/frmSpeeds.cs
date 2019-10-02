using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GLOBALS;

namespace StageSoakTest
{
    public partial class frmSpeeds : Form
    {
   
        public frmSpeeds()
        {
            InitializeComponent();
            init();
        }


        public void init()
        {
		    int i;
		
		    for( i = 1 ; i  < 1001; i++)
			    lstSpeed.Items.Add(i);
    
            for (i = 1; i < 1001; i++)
                lstAcc.Items.Add(i);
    		
		    for( i = 1 ; i  < 1001; i++)
			    lstCurve.Items.Add(i);

            lstSpeed.SelectedIndex = 99;
            lstAcc.SelectedIndex = 99;
            lstCurve.SelectedIndex = 99;

        }

        public void setSAC(int sms, int sas, int scs)
        {
            try
            {
                lstSpeed.SelectedIndex = sms - 1;
                lstAcc.SelectedIndex = sas - 1;
                lstCurve.SelectedIndex = scs - 1;
            }
            catch (Exception )
            {
            }
        }

        private void frmSpeeds_FormClosing(object sender, FormClosingEventArgs e)
        {

            this.Visible = false;
            e.Cancel = true;
        }

        private void frmSpeeds_Load(object sender, EventArgs e)
        {
            this.Top = Globals.top + 20;
            this.Left = Globals.left + 20;
        }

        private void lstSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.Speed = lstSpeed.SelectedIndex+1;
            Globals.SACchanged = true;
        }

        private void lstAcc_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.Acc = lstAcc.SelectedIndex+1; 
            Globals.SACchanged = true;
        }

        private void lstCurve_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.Curve = lstCurve.SelectedIndex+1; 
            Globals.SACchanged = true;
        }

      
    }
}
