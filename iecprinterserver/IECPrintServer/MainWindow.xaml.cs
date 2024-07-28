using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO.Ports;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Microsoft.Win32;
using System.Net;      //required
using System.Net.Sockets;    //required
using System.Threading;

namespace IECPrintServer
{
    public partial class MainWindow : Window
    {
        TcpListener server = new TcpListener(IPAddress.Any, 6502);
        static bool _continue = true;
        PrinterEmulator printerEmulator;


        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            server.Start(); 
            InitializeComponent();
            printerEmulator = new PrinterEmulator();
            OutputImage.Source = printerEmulator.ToBitmapImage();

            Thread myThread = new Thread(new ThreadStart(printlistener));
            myThread.Start();
        }

        void printlistener()
        { 
            while (true)   //we wait for a connection
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

                NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                while (client.Connected)  //while the client is connected, we look for incoming messages
                {
                    Read(ns);
                }
            }


            
        }


        public async void Read(NetworkStream ns)
        {
            int toRead = 0;
            byte[] Buffer = new byte[81];

            while (_continue)
            {
                try
                {
                    if (ns.DataAvailable)
                    {
                        toRead = ns.Read(Buffer, 0, 80);

                        for (var c = 0; c < toRead; c++)
                        {
                            printerEmulator.print((char)Buffer[c]);
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            OutputImage.Source = printerEmulator.ToBitmapImage();
                        }));
                    }
                }
                catch { }

            }
            
        }




        private void Button_Click_Quality(object sender, RoutedEventArgs e)
        {                           
        }


        private void Button_Click_LPI(object sender, RoutedEventArgs e)
        {


            if (btnLPI.Content.ToString() != "6 LPI")
            {
                btnLPI.Content = "6 LPI";
                printerEmulator.setLpi(6);               
            }
            else
            {
                btnLPI.Content = "8 LPI";
                printerEmulator.setLpi(8);                
            }
        }


        private void Button_Click_FF(object sender, RoutedEventArgs e)
        {
            printerEmulator.doFF();
        }

        private void Button_Click_SAVE(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog();


            if (fileDialog.ShowDialog() == true)
            {
                printerEmulator.doSave(fileDialog.FileName);
            }

        }




    }

  
}
