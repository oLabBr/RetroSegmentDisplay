using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using RSD.Monitor.Controller;
using RSD.Monitor.Sensors;
using System.Diagnostics;

namespace RSD.Monitor
{
    public partial class Main : Form
    {
        private ISensorReader reader;

        private ControllerCommunicator comm
        {
            get
            {
                return Program.Comm;
            }
        }

        public Main()
        {
            InitializeComponent();
            this.btnInit.Enabled = true;
            this.btnStop.Enabled = false;

            this.reader = new EverestSensorReader(); // MAKE CONFIGURABLE
        }

        private void LoadSettings()
        {
            this.cbPort.SelectedItem = Properties.Settings.Default.DefaultPort;
            this.chkAutoStart.Checked = Properties.Settings.Default.AutoStart;
            this.chkStartsMinimized.Checked = Properties.Settings.Default.StartsMinimized;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.DefaultPort = (string)this.cbPort.SelectedItem;
            Properties.Settings.Default.AutoStart = this.chkAutoStart.Checked;
            Properties.Settings.Default.StartsMinimized = this.chkStartsMinimized.Checked;
            Properties.Settings.Default.Save();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.cbPort.Items.Clear();
            this.cbPort.Items.AddRange(SerialPort.GetPortNames());

            this.LoadSettings();

            if (this.chkAutoStart.Checked)
            {
                this.btnInit_Click(sender, e);
            }

            if (this.chkStartsMinimized.Checked)
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            Program.Comm = new ControllerCommunicator(this.cbPort.SelectedItem as String);
            comm.Initializes();

            this.btnInit.Enabled = false;
            this.btnStop.Enabled = true;
            this.tmUpdate.Enabled = true;
            this.cbPort.Enabled = false;

            this.SaveSettings();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // = Clear
            comm.Clear();

            // =
            comm.Stops();
            this.btnInit.Enabled = true;
            this.btnStop.Enabled = false;
            this.tmUpdate.Enabled = false;
            this.cbPort.Enabled = true;
        }

        private void tmUpdate_Tick(object sender, EventArgs e)
        {
            // Updates the Sensor Data
            try
            {
                reader.Update();
            }
            catch (Exception exp)
            {
                Trace.TraceError("Sensor update error: " + exp.ToString());
            }

            // Sends all Data to the Display Controller
            if (this.comm.IsOpen())
            {
                this.comm.SetValue(1, reader.GetSensorValue(SensorId.MB_Temp));
                this.comm.SetValue(2, reader.GetSensorValue(SensorId.HD1_Temp));
                this.comm.SetValue(3, reader.GetSensorValue(SensorId.CPU_Voltage));

                this.comm.SetLed(0, this.ledSwitcher1.Value);
                this.comm.SetLed(1, this.ledSwitcher2.Value);
            }

            // Update the Status Label
            this.UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (this.comm.IsOpen())
            {
                this.lbConnectionStatus.Text = "OPEN "
                    + (this.cbPort.SelectedItem as string)
                    + " " + reader.GetSensorValue(SensorId.MB_Temp)
                    + " " + reader.GetSensorValue(SensorId.HD1_Temp)
                    + " " + reader.GetSensorValue(SensorId.CPU_Voltage);
            }
        }

        private void menuOpenHide_Click(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void menuQuit_Click(object sender, EventArgs e)
        {
            if (this.comm.IsOpen())
            {
                this.btnStop_Click(null, null);
            }

            this.notifyIcon1.Visible = false;

            Application.Exit();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveSettings();
            MessageBox.Show("Settings stored!");
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}