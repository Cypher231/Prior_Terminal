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
    public partial class StageSelect : Form
    {
        public StageSelect()
        {
            InitializeComponent();
        }

     

        private void StageSelect_Load(object sender, EventArgs e)
        {
            this.Top = Globals.top+20;
            this.Left = Globals.left+20;
            this.BringToFront();
        }

        public void init(List<string> a)
        {
            foreach (string x in a)
            {
                listBox1.Items.Add(x);
            }

            listBox1.SelectedIndex = 0;
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            Globals.selectedStageIndex = listBox1.SelectedIndex;
            Close();
        }


    }
}
