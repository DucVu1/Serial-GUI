using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using ZedGraph;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml;
using System.Collections;
using System.Threading;

namespace Senddata
{
    public partial class ComPortSerial : Form
    {
        string dataOUT;
        string sendWith;
        string dataIN;
        PointPairList myList = new PointPairList();
        public ComPortSerial()
        {
            InitializeComponent();
            initGraph();
            CheckForIllegalCrossThreadCalls = false;
        }
        private void initGraph()
        {
            GraphPane myPane = zg1.GraphPane;
            myPane.Title.Text = "Data";
            myPane.XAxis.Title.Text = "Time";
            myPane.Y2Axis.Title.Text = "Speed";

            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            myPane.Chart.Fill = new Fill(Color.White, Color.Beige, 45.0f);
            LineItem myCurve = myPane.AddCurve("", myList, Color.Blue, SymbolType.Circle);

            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Blue);
            myCurve.Symbol.Size = 5;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cBoxCOMPORT.Items.AddRange(ports);

            btnOpen.Enabled = true;
            btnClose.Enabled = false;
            chBoxDTREnable2.Checked = false;
            serialPort1.DtrEnable = false;
            chBoxRTSEnable.Checked = false;
            serialPort1.RtsEnable = false;
            btnSendData.Enabled = false;
            chBoxWriteLine.Checked = false;
            chBoxWrite.Checked = true;
            chBoxAddtoOldData.Checked = true;
            chBoxAlwaysUpdate.Checked = false;

            sendWith = "Write";

        }
        int i = 0;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cBoxCOMPORT.Text;
                serialPort1.BaudRate = Convert.ToInt32(cBoxBaudRate.Text);
                serialPort1.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);
                serialPort1.Open();
                progressBar1.Value = 100;
                btnOpen.Enabled = false;
                btnClose.Enabled = true;
                lblStatus.Text = "ON";
                timer1.Start();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatus.Text = "OFF";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                progressBar1.Value = 0;
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatus.Text = "OFF";
            }
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                dataOUT = tBoxDataOut.Text +"|"+ tBoxDataOut2.Text;
                if (sendWith == "WriteLine")
                {
                    serialPort1.WriteLine(dataOUT);
                }
                else if (sendWith == "Write")
                {
                    serialPort1.Write(dataOUT);
                }
            }

        }
        private void chBoxDTREnable2_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxDTREnable2.Checked)
            {
                serialPort1.DtrEnable = true;
            }
            else
            {
                serialPort1.DtrEnable = false;
            }
        }

        private void chBoxRTSEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxRTSEnable.Checked)
            {
                serialPort1.RtsEnable = true;
            }
            else
            {
                serialPort1.RtsEnable = false;
            }
        }
        private void btnClearData_Click(object sender, EventArgs e)
        {
            if (tBoxDataOut.Text != "")
            {
                tBoxDataOut.Text = "";
            }
        }
        private void chBoxUsingButton_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxUsingButton.Checked)
            {
                btnSendData.Enabled = true;
            }
            else
            {
                btnSendData.Enabled = false;
            }
        }

        private void tBoxDataOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (chBoxUsingEnter.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (serialPort1.IsOpen)
                    {
                        dataOUT = tBoxDataOut.Text;
                        if (sendWith == "WriteLine")
                        {
                            serialPort1.WriteLine(dataOUT);
                        }
                        else if (sendWith == "Write")
                        {
                            serialPort1.Write(dataOUT);
                        }
                    }
                }
            }
        }

        private void chBoxWriteLine_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxWriteLine.Checked)
            {
                sendWith = "WriteLine";
                chBoxWrite.Checked = false;
                chBoxWriteLine.Checked = true;
            }
        } 

        private void chBoxWrite_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxWrite.Checked)
            {
                sendWith = "Write";
                chBoxWrite.Checked = true;
                chBoxWriteLine.Checked = false;
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            dataIN = serialPort1.ReadByte().ToString();
            this.Invoke(new EventHandler(ShowData));
            this.Invoke(new EventHandler(UpdateGraphWithData));

        }
        private void UpdateGraphWithData(object sender, EventArgs e)
        {
            if (double.TryParse(dataIN, out double plotdata))
            {
                // Parse the received data and add it to the graph
                double time = i; // Use the number of data points as the time
                myList.Add(time, plotdata);
                UpdateGraph();

                if (chBoxAddtoOldData.Checked)
                {
                    // Append the received data to the textbox
                    tBoxDataIn.AppendText(dataIN + Environment.NewLine);
                }
                else if (chBoxAlwaysUpdate.Checked)
                {
                    // Display the most recent received data in the textbox
                    tBoxDataIn.Text = dataIN;
                }
            }
        }
        private void ShowData(object sender, EventArgs e)
        {
            if (chBoxAlwaysUpdate.Checked)
            {
                tBoxDataIn.Text = dataIN;
            }
            else if (chBoxAddtoOldData.Checked)
            {
                tBoxDataIn.Text += dataIN + "\n";
            }
        }
        private void btnClearDataIn_Click(object sender, EventArgs e)
        {
            if (tBoxDataIn.Text != "")
            {
                tBoxDataIn.Text = "";
            }
        }

        private void chBoxAlwaysUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxAlwaysUpdate.Checked)
            {
                chBoxAlwaysUpdate.Checked = true;
                chBoxAddtoOldData.Checked = false;
            }
            else
            {
                chBoxAddtoOldData.Checked = true;
            }
        }

        private void chBoxAddtoOldData_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxAddtoOldData.Checked)
            {
                chBoxAddtoOldData.Checked = true;
                chBoxAlwaysUpdate.Checked = false;
            }
            else
            {
                chBoxAlwaysUpdate.Checked = true;
            }
        }
        private void UpdateGraph()
        {
            zg1.AxisChange();
            zg1.Invalidate();
            zg1.Update();
            zg1.Refresh();
        }
        private void ClearPlotData()
        {
            // Clear the PointPairList
            myList.Clear();

            // Update the graph to reflect the changes
            UpdateGraph();
        }

        private void btnClearPlot_Click(object sender, EventArgs e)
        {
            ClearPlotData();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            i++;
        }
  
    }
}
