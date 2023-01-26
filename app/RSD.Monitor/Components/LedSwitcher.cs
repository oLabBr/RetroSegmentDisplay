using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace RSD.Monitor.Components
{
    public partial class LedSwitcher : UserControl
    {
        public LedSwitcher()
        {
            InitializeComponent();
        }

        private bool myValue;

        public bool Value
        {
            get { return myValue; }
            set 
            { 
                myValue = value;
                this.pictureBox1.Image = 
                    (myValue ? Properties.Resources.LED_ON : Properties.Resources.LED_OFF);
            }
        }

        private void LedSwitcher_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void LedSwitcher_DoubleClick(object sender, EventArgs e)
        {
            this.Value = !this.myValue;
        }
	
    }
}
