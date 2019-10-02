using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization ;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using COMMS;

using System.Collections;

using System.IO;

using GLOBALS;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;


/*
 * 1.9  15/2/18
 *      change netwrok path name from prior-ds to ps-fs01
 *      
 * 1.8  12/7/16
 *      E10253
 *      further to 1.7, provide notification to user if database cannot be opened due to network failure
 *      
 * 1.7  29/7/16
 *      E10253
 *      Read Stage database at startup and get user to select correct stage type when connected.
 *      Check actual stage travel during init and compare against database
 *      Use openxml and SpreadSheetLight for access to excel files.
 *      avoid x-thread direct control access. Use xthread class.
 *      
 * 1.6  16/5/16
 *      Add menu entry to disable limit checking during soak
 */

namespace StageSoakTest
{
    public partial class Form1 : Form
    {

        private BackgroundWorker work = new BackgroundWorker();

        private frmSpeeds Perf = new frmSpeeds();

        enum SoakState
        {
            SSnotconnected,
            SSconnected,
            SSBackLeft,
            SSFrontRight,
            SSRun,
            SSfinished,
            SSnostage,
            SSstalled,
            SSKill
        };

        // controller type
        enum controllerType
        {
           None,
           ProScan,
           OptiScan
        };

        class SoakData
        {
           // soak state data
           public SoakState state;
           public int nSoakCycles;
           public int cycle;
           public int position;

           public controllerType controller;

           // stage specific data
           public int xtravel;
           public int ytravel;
           public int XtravelSpec;
           public int YtravelSpec;

           public int centreX;
           public int centreY;
           public bool resetController;
           public bool dataBaseRead;

           /* lists of stage characteristics */
           public List<string> stageEntries;
           public List<string> stageXTravel;
           public List<string> stageYTravel;
           public List<string> stageSAC;
           public List<string> stageCurrents;

           public bool flasher;

           public SoakData()
           {
                SoakState state = SoakState.SSnotconnected;

                // # of cycles to soak for
                nSoakCycles = 0;

                // current soak cycle
                cycle = 0;
                position = 0;

                controllerType controller = controllerType.None;

                // stage specific data
                xtravel = 0;
                ytravel = 0;
                XtravelSpec = 0;
                YtravelSpec = 0;

                centreX = 0;
                centreY = 0;
                resetController = false;
                dataBaseRead = true;

                // arraylist of 
                stageEntries = new List<string>();
                stageXTravel = new List<string>();
                stageYTravel = new List<string>();
                stageSAC = new List<string>();
                stageCurrents = new List<string>();

                flasher = false;
           }
        };

        private SoakData SD = new SoakData();





        public Form1()
        {
            InitializeComponent();
        }


        void connectionLost()
        {
            
		    SD.state = SoakState.SSnotconnected;

            xthread.ICA<ListBox>(lstComms, lst => lst.Items.Clear());
            xthread.ICA<Label>(lblState, lbl => lbl.Text = "NO CONTROLLER");
            xthread.ICA<Label>(lblState, lbl => lbl.BackColor = System.Drawing.Color.Yellow);
            Globals.interval = 4000;
         }


        private bool openConnection()
        {
            if (Comms.txrx("COMP 0") != "")
            {
                Comms.txrx("RES S 1");
                return true;
            }
            else
                return false;	
        }

        private bool checkConnection()
        {

            if (Comms.txrx("$") == "")
            {
	            connectionLost();
                return false;
            }
            else
	            return true;	
	    }

        public void hide()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(hide));
            }
            else
            {
                this.Visible = false;
            }
        }

        public void show()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(show));
            }
            else
            {
                this.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String[] arguments = Environment.GetCommandLineArgs();
            int height, width;
            int port = 1;

            SD.position = 1;

            try
            {
                port = Convert.ToInt32(arguments[1]);
                SD.position = Convert.ToInt32(arguments[2]);

                if ((SD.position < 1) || (SD.position > 8))
                {
                    MessageBox.Show("Error : position must be in range 1..8");
                    this.Close();
                    return;
                }
            }
            catch
            {
            }

            if (arguments.Length > 3)
            {
                MessageBox.Show("Usage : StageSoakTest.exe <com port n> < screen pos 1..8>");
                this.Close();
                return;
            }
           

            try
            {
                Comms.open(port);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
                return;
            }

            this.Text = this.Text + " v" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString() + " on COM" + port.ToString();

            width = Screen.PrimaryScreen.Bounds.Width;
            height = Screen.PrimaryScreen.Bounds.Height;

            this.Width = width / 4;
            this.Height = height / 2;

            if (SD.position < 5)
            {
                this.Top = 0;
                this.Left = (SD.position - 1) * this.Width;
            }
            else
            {
                this.Top = this.Height;
                this.Left = (SD.position - 5) * this.Width;
            }

            Globals.top = this.Top;
            Globals.left = this.Left;


            work.WorkerReportsProgress = true;
            work.WorkerSupportsCancellation = true;
            work.DoWork += new DoWorkEventHandler(work_DoWork);
            work.RunWorkerCompleted += new RunWorkerCompletedEventHandler(work_RunWorkerCompleted);

            connectionLost();

            SD.nSoakCycles = 1000 ;
            lblState.Text = "Initialising ... wait";
       
            work.RunWorkerAsync();
          
        }

        private void updateSAC()
        {
            Comms.txrx("SMS " + Globals.Speed.ToString());
            Comms.txrx("SAS " + Globals.Acc.ToString());

            if (SD.controller == controllerType.ProScan)
            {
                Comms.txrx("SCS " + Globals.Curve.ToString());
            }
        }

        private void work_DoWork(object sender, DoWorkEventArgs e)
        {
            string response;
            string[] tokens;
	        int expectedLimits ;
		    int limits;

            Thread.Sleep(20000);

            loadDataBase(SD.position);

  
            do
            {
                try
                {
                    Thread.Sleep(Globals.interval);

                    /* check for user changes to SAC via frmspeed */
                    if (Globals.SACchanged == true)
                    {
                        updateSAC();
                        Globals.SACchanged = false;
                    }

                    switch (SD.state)
                    {
                        case SoakState.SSnotconnected:
                        {
                            if (openConnection() == true)
                            {
                                if (SD.stageEntries.Count > 1)
                                {
                                    StageSelect a = new StageSelect();
                                    a.init(SD.stageEntries);

                                    hide();
                                    if (a.ShowDialog() != DialogResult.OK)
                                        Globals.selectedStageIndex = 0;
                                    show();
                                }
                                else
                                {
                                    if (SD.dataBaseRead == false)
                                    {
                                        xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("** NO DATABASE **"));
                                    }
                                }

                                SD.state = SoakState.SSconnected;
                                SD.cycle = 0;
                                xthread.ICA<Label>(lblState, lbl => lbl.Text = "CONNECTED");
                                xthread.ICA<Label>(lblState, lbl => lbl.BackColor = System.Drawing.Color.Cyan);
                                Globals.interval = 750;

                            }

                            break;
                        }

                        case SoakState.SSconnected:
                        {
                            Comms.tx("?");

                            do
                            {
                                response = Comms.rx();

                                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add(response));

                                if (response.Contains("PROSCAN"))
                                    SD.controller = controllerType.ProScan;
                                else
                                    if (response.Contains("OPTISCAN"))
                                        SD.controller = controllerType.OptiScan;


                                if (response.Contains("STAGE"))
                                    if (response.Contains("NONE"))
                                        SD.state = SoakState.SSnostage;

                            }
                            while (response.Equals("END") != true);

                            xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add(""));


                            if (SD.state == SoakState.SSconnected)
                            {
                                double x, y;
                                string currents;

                                x = Convert.ToDouble(SD.stageXTravel[Globals.selectedStageIndex]);
                                y = Convert.ToDouble(SD.stageYTravel[Globals.selectedStageIndex]);

                                /* use double the expected travel in microns so we go passed limit switch */
                                SD.xtravel = Convert.ToInt32(x * 2000);
                                SD.ytravel = Convert.ToInt32(y * 2000);

                                /* use specified travel in microns so we can check initial actual travel */
                                SD.XtravelSpec = Convert.ToInt32(x * 1000);
                                SD.YtravelSpec = Convert.ToInt32(y * 1000);

                                currents = SD.stageCurrents[Globals.selectedStageIndex].ToString();
                                Comms.txrx("CURRENT,1," + currents);
                                Comms.txrx("CURRENT,2," + currents);
                                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("STAGE CURRENT = " + currents));

                                tokens = SD.stageSAC[Globals.selectedStageIndex].Split(',');
                                Perf.setSAC(Convert.ToInt32(tokens[0]), Convert.ToInt32(tokens[1]), Convert.ToInt32(tokens[2]));    
                                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("STAGE SAC = " + SD.stageSAC[Globals.selectedStageIndex].ToString()));


                                Comms.txrx("GR," + (-SD.xtravel).ToString() + "," + (-SD.ytravel).ToString());
                         
                                xthread.ICA<Label>(lblState, lbl => lbl.Text = "INIT");
                                SD.state = SoakState.SSBackLeft;
                            }

                            break;
                        }

                        case SoakState.SSBackLeft:
                        {
                            response = Comms.txrx("$");

                            if (response.Equals("0"))
                            {
                                response = Comms.txrx("LMT");

                                if (response.Equals("0A"))
                                {
                                    response = Comms.txrx("Z");
                                    response = Comms.txrx("GR," + SD.xtravel.ToString() + "," + SD.ytravel.ToString());
                                    SD.state = SoakState.SSFrontRight;
                                }
                                else
                                {
                                    if (limitCheckOffToolStripMenuItem.Checked == false)
                                    {
                                        xthread.ICA<Label>(lblState, lbl => lbl.Text = "STALLED or BL LIMITS");
                                        xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("Expected 0A, got " + response));
                                        SD.state = SoakState.SSstalled;
                                    }
                                }
                            }

                            break;
                        }

                        case SoakState.SSFrontRight:
                        {
                            response = Comms.txrx("$");

                            if (response.Equals("0"))
                            {
                                response = Comms.txrx("LMT");

                                if (response.Equals("05"))
                                {
                                    int x, y;

                                    SD.state = SoakState.SSRun;

                                    response = Comms.txrx("PS");

                                    tokens = response.Split(',');

                                    x = Convert.ToInt32(tokens[0]) ;
                                    y = Convert.ToInt32(tokens[1]) ;

                                    if (SD.dataBaseRead == true)
                                    {
                                        xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("Expected Travel is " + SD.XtravelSpec + " x " + SD.YtravelSpec));

                                        if ((x < SD.XtravelSpec) || (y < SD.YtravelSpec))
                                        {
                                            xthread.ICA<Label>(lblState, lbl => lbl.Text = "TRAVEL WRONG");
                                            SD.state = SoakState.SSstalled;
                                        }
                                    }

                                    xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("Measured Travel is " + x + " x " + y));

                                    SD.centreX = x / 2;
                                    SD.centreY = y / 2;
                                }
                                else
                                {
                                    if (limitCheckOffToolStripMenuItem.Checked == false)
                                    {
                                        xthread.ICA<Label>(lblState, lbl => lbl.Text = "STALLED or FR LIMITS");
                                        xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("Expected 05, got " + response));
                                        SD.state = SoakState.SSstalled;
                                    }
                                }
                            }

                            break;
                        }


                        case SoakState.SSRun:
                        {

                            response = Comms.txrx("$");

                            if (response.Equals("0"))
                            {
                                /* ASSERT: not moving
                                 */

                                if (SD.cycle != 0)
                                {
                                    // check limits

                                    xthread.ICA<Label>(lblState, lbl => lbl.Text = "CYCLE " + (SD.cycle / 2).ToString());

                                    response = Comms.txrx("LMT");

                                    if (response.Equals("")) goto aborted;

                                    if ((SD.cycle & 1) == 1)
                                        expectedLimits = 10;
                                    else
                                        expectedLimits = 5;



                                    limits = int.Parse(response, NumberStyles.HexNumber);
                                    limits &= 0x0F;

                                    if (limitCheckOffToolStripMenuItem.Checked == false)
                                    {
                                        if (limits != expectedLimits)
                                        {
                                            xthread.ICA<Label>(lblState, lbl => lbl.Text = "STALLED/LIMITS");

                                            SD.state = SoakState.SSstalled;


                                            xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add(""));

                                            if (((limits ^ expectedLimits) & 1) == 1)
                                                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("CHECK +VE X LIMIT"));

                                            if (((limits ^ expectedLimits) & 2) == 2)
                                                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("CHECK -VE X LIMIT"));

                                            if (((limits ^ expectedLimits) & 4) == 4)
                                                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("CHECK +VE Y LIMIT"));

                                            if (((limits ^ expectedLimits) & 8) == 8)
                                                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("CHECK -VE Y LIMIT"));


                                            xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("** completed " + SD.cycle + " cycles"));
                                            goto aborted;
                                        }
                                    }
                                }


                                if (SD.cycle > SD.nSoakCycles)
                                {
                                    SD.state = SoakState.SSfinished;
                                    response = Comms.txrx("ps");

                                    response = Comms.txrx("G " + SD.centreX.ToString() + " " + SD.centreY.ToString());
                                }
                                else
                                {
                                    if ((SD.cycle & 1) == 1)
                                    {
                                        response = Comms.txrx("GR," + SD.xtravel.ToString() + "," + SD.ytravel.ToString());
                                    }
                                    else
                                    {
                                        response = Comms.txrx("GR," + (-SD.xtravel).ToString() + "," + (-SD.ytravel).ToString());
                                    }

                                    SD.cycle = SD.cycle + 1;
                                }
                            }

                            break;
                        }

                        case SoakState.SSfinished:
                        {
                            xthread.ICA<Label>(lblState, lbl => lbl.BackColor = System.Drawing.Color.Lime);
                            xthread.ICA<Label>(lblState, lbl => lbl.Text = "FINISHED");

                            if (checkConnection() == false)
                                SD.state = SoakState.SSnotconnected;

                            break;
                        }

                        case SoakState.SSnostage:
                        {
                            xthread.ICA<Label>(lblState, lbl => lbl.BackColor = System.Drawing.Color.Red);
                            xthread.ICA<Label>(lblState, lbl => lbl.Text = "NO STAGE");

                            if (checkConnection() == false)
                                SD.state = SoakState.SSnotconnected;

                            break;
                        }

                        case SoakState.SSstalled:
                        {
                            if (SD.flasher == true)
                                xthread.ICA<Label>(lblState, lbl => lbl.BackColor = System.Drawing.Color.White);
                            else
                                xthread.ICA<Label>(lblState, lbl => lbl.BackColor = System.Drawing.Color.Red);

                            SD.flasher = !SD.flasher;
                            
                            checkConnection();
                            break;
                        }

                        case SoakState.SSKill:
                        {
                            break;
                        }

                    }

                aborted:
                    if (SD.resetController == true)
                    {
                        Comms.tx("RESET");
                        SD.resetController = false;
                        connectionLost();
                    }
                }

                
                catch (TimeoutException)
                {
                    connectionLost();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected exception occurred : " + ex.Message);
                    return;
                }

            } while (true);
        }


        private void work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close(); 
        }

        private void mnuCycles_Click(object sender, EventArgs e)
        {
            string cycles = SD.nSoakCycles.ToString();

            if (InputBox.Show("Soak Cycles:", "Enter no of cycles", ref cycles) == DialogResult.OK)
            {
                int test;

                if (int.TryParse(cycles, out test) == true)
                    SD.nSoakCycles = test * 2;
            }
        }

        private void mnuPerformance_Click(object sender, EventArgs e)
        {
            Perf.Show();
        }

        private void mnuReset_Click(object sender, EventArgs e)
        {
            SD.resetController = true;
        }

        private void loadDataBase(int id)
        {

            //string actualDB = @"\\prior-ds\StageResults\StageDataBase.xlsx";
            string actualDB = @"\\ps-fs01\shared\stage\StageResults\StageDataBase.xlsx";
           
            SD.stageEntries.Clear();
            SD.stageXTravel.Clear();
            SD.stageYTravel.Clear();

            if (File.Exists(actualDB))
            {
                try
                {
                    FileStream fs;

                    //string localDB = Path.GetTempPath();

                    //localDB = localDB + @"\DB" + id.ToString() + ".xlsx";

                    //File.Copy(actualDB, localDB, true);

                    using (SLDocument DB = new SLDocument(fs = new FileStream(actualDB, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        string part = "";
                        int row = 3;

                        do
                        {
                            part = DB.GetCellValueAsString(row, 1);

                            if (part.Equals("") == false)
                            {
                                SD.stageEntries.Add(part);
                                SD.stageXTravel.Add(DB.GetCellValueAsString(row, 26));
                                SD.stageYTravel.Add(DB.GetCellValueAsString(row, 27));
                                SD.stageSAC.Add(DB.GetCellValueAsString(row, 38));
                                SD.stageCurrents.Add(DB.GetCellValueAsString(row, 39));

                                row++;
                            }
                        }
                        while (part.Equals("") == false);
                    }
                }
                catch (Exception ex)
                {
                    xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("** ERROR reading stagedataBase.xlsx"));
                    xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("** " + ex.Message));
                }
            }
            else
            {
                xthread.ICA<ListBox>(lstComms, lst => lst.Items.Add("** NO STAGE DATABASE FOUND **"));
            }

            if (SD.stageEntries.Count == 0)
            {
                SD.stageEntries.Add("BFG STAGE");
                SD.stageXTravel.Add("500");
                SD.stageYTravel.Add("500");
                SD.stageCurrents.Add("1000,500,500");
                SD.stageSAC.Add("100,100,100");
                Globals.selectedStageIndex = 0;
                SD.dataBaseRead = false;
            }

            Globals.SACchanged = false;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
