
using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace COMMS
{
    public static class Comms
    {
        static SerialPort myPort = new SerialPort() ;

        static System.Object lockThis = new Object ();

        public static void clearRx()
        {
            try
            {
                myPort.DiscardInBuffer();
            }
            catch (Exception )
            {
            }
        }

        public static void setTimeout(int time)
        {
            try
            {
                myPort.ReadTimeout = time;
            }
            catch (Exception)
            {
            }
        }
        public static bool close()
        {
            try
            {
                if (myPort.IsOpen == true)
                {
                    tx("BAUD,9600");
                    rx();
                    myPort.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return true;
        }

        public static void open(int port)
        {
            myPort.PortName = "COM" + port.ToString();
            myPort.BaudRate = 9600;
            myPort.NewLine = "\r";
            myPort.ReadTimeout = 2000;
            myPort.Open();
        }

        public static void tx(string cmd)
        {
            myPort.WriteLine(cmd);
        }

        public static string rx()
        {
            string response = "";

            //try
            //{
                response = myPort.ReadLine();
            //}
            //catch (Exception ex)
            //{
            //     response = "";
            //}
            return response;
        }

        public static string txrx(string cmd)
        {
            lock (lockThis)
            {
                tx(cmd);
                return rx();
            }
        }
    }
}
