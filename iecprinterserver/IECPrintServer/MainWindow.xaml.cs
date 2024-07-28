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
        static CbmPrinterFont prt = new CbmPrinterFont();

        static int verticalpos = 0;
        static int horizontalpos = 0;
        static Bitmap Paper = new Bitmap(6400, 7040);
        PdfDocument document = new PdfDocument();
        static int LPI = 1;
        static bool Enhanced = false;
        static bool Reversed = false;
        static bool BitImage = false;
        static bool EscMode = false;
        static int Font = 0;
        static bool DotAddressStateOne = false;
        static bool AddressStateOne = false;
        static bool DotAddressStateTwo = false;
        static bool AddressStateTwo = false;




        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            server.Start();  // this will start the server
            InitializeComponent();
            OutputImage.Source = Utility.ToBitmapImage(Paper);

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
                            print((char)Buffer[c]);
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            OutputImage.Source = Utility.ToBitmapImage(Paper);
                        }));
                    }
                }
                catch { }

            }
            
        }
        private void print(char c)
        {


            if (EscMode)
            {
                if (c != (char)16) EscMode = false;
            }

            if (AddressStateTwo)
            {
                int p = (int)c - (int)'0';
                AddressStateTwo = false;
                horizontalpos = horizontalpos + p * 8;
                c = (char)0;
            }
            if (DotAddressStateTwo)
            {
                DotAddressStateTwo = false;
                horizontalpos = horizontalpos + (int)c;
                c = (char)0;
            }
            if (AddressStateOne)
            {
                int p = (int)c - (int)'0';
                AddressStateOne = false;
                AddressStateTwo = true;
                horizontalpos = (int)p * 80;
                c = (char)0;
            }
            if (DotAddressStateOne)
            {
                DotAddressStateOne = false;
                DotAddressStateTwo = true;
                horizontalpos = (int)c * 256;
                c = (char)0;
            }


            if (BitImage)
            {
                switch (c)
                {
                    case (char)0:
                        break;
                    case (char)10:
                        verticalpos = verticalpos + 7;
                        break;
                    case (char)13:
                        verticalpos = verticalpos + 7;
                        horizontalpos = 0;
                        break;
                    case (char)15:
                        BitImage = false;
                        break;
                    case (char)27:
                        EscMode = true;
                        break;
                    default:
                        ToGraphicPaper((char)((int)c - 127));
                        break;
                }
            }
            else
            {

                switch (c)
                {
                    case (char)8:
                        BitImage = true;
                        break;
                    case (char)10:
                        verticalpos = verticalpos + 8 + LPI;
                        break;
                    case (char)13:
                        verticalpos = verticalpos + 8 + LPI;
                        horizontalpos = 0;
                        break;
                    case (char)14:
                        Enhanced = true;
                        break;
                    case (char)15:
                        Enhanced = false;
                        break;
                    case (char)17:
                        Font = 1;
                        break;
                    case (char)16:
                        if (EscMode)
                        {
                            DotAddressStateOne = true;
                            EscMode = false;
                        }
                        else
                        {
                            AddressStateOne = true;
                        }
                        break;
                    case (char)18:
                        Reversed = true;
                        break;
                    case (char)27:
                        EscMode = true;
                        break;
                    case (char)145:
                        Font = 0;
                        break;
                    case (char)146:
                        Reversed = false;
                        break;
                    default:
                        ToPaper(c);
                        break;
                }
            }
        
        }

        private void ToPaper(char c)
        {
            int scale = 1;

            if (verticalpos + 8 + LPI >= 704)
            {
                verticalpos = 0;
                horizontalpos = 0;

                var P = document.AddPage();

                XGraphics gfx = XGraphics.FromPdfPage(P);

                MemoryStream S = new MemoryStream();
                Paper.Save(S, ImageFormat.Png);
                XImage i = XImage.FromStream(S);
                gfx.DrawImage(i, 0, 0, gfx.PdfPage.Width, gfx.PdfPage.Height);
                Paper = new Bitmap(6400, 8800);
            }

            if (horizontalpos >= 800)
            {
                verticalpos = verticalpos + 8 + LPI;
                horizontalpos = 0;
            }



            try
            {
                for (byte x = 0; x < 8; x++)
                {
                    var ch = prt.GetCharPattern(c, x, Font);

                    using (var graphics = Graphics.FromImage(Paper))
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            var m = Math.Pow(2, y);

                            if (((ch & Convert.ToInt32(Math.Pow(2, y))) > 0) ^ Reversed)
                            {
                                graphics.FillEllipse(System.Drawing.Brushes.Black, new RectangleF(horizontalpos * scale * 10, ((8 - y) + verticalpos) * scale * 10, 10 * scale, 10 * scale));
                                if (Enhanced) graphics.FillEllipse(System.Drawing.Brushes.Black, new RectangleF((horizontalpos + 1) * scale * 10, ((8 - y) + verticalpos) * scale * 10, 10 * scale, 10 * scale));
                            }
                        }
                    }
                    horizontalpos++;
                    if (Enhanced) horizontalpos++;
                }
            }
            catch { }

        }

        private void ToGraphicPaper(char c)
        {
            int scale = 1;

            if (verticalpos + 7 >= 704)
            {
                verticalpos = 0;
                horizontalpos = 0;

                var P = document.AddPage();

                XGraphics gfx = XGraphics.FromPdfPage(P);

                MemoryStream S = new MemoryStream();
                Paper.Save(S, ImageFormat.Png);
                XImage i = XImage.FromStream(S);
                gfx.DrawImage(i, 0, 0, gfx.PdfPage.Width, gfx.PdfPage.Height);
                Paper = new Bitmap(6400, 8800);
            }

            if (horizontalpos >= 800)
            {
                verticalpos = verticalpos + 7;
                horizontalpos = 0;
            }


            try
            {
                using (var graphics = Graphics.FromImage(Paper))
                {
                    for (int y = 0; y < 7; y++)
                    {
                        var m = Math.Pow(2, y);

                        if (((c & Convert.ToInt32(Math.Pow(2, y))) > 0))
                        {
                            graphics.FillEllipse(System.Drawing.Brushes.Black, new RectangleF(horizontalpos * scale * 10, ((y) + verticalpos) * scale * 10, 10 * scale, 10 * scale));
                        }
                    }
                }
                horizontalpos++;
            }
            catch { }

        }






        private void Button_Click_Quality(object sender, RoutedEventArgs e)
        {
                            OutputImage.Source = Utility.ToBitmapImage(Paper);
        }


        private void Button_Click_LPI(object sender, RoutedEventArgs e)
        {


            if (LPI == 1)
            {
                btnLPI.Content = "6 LPI";
                LPI = 4;
            }
            else
            {
                btnLPI.Content = "8 LPI";
                LPI = 1;
            }
        }


        private void Button_Click_FF(object sender, RoutedEventArgs e)
        {
            verticalpos = 0;
            horizontalpos = 0;

            var P = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(P);
            MemoryStream S = new MemoryStream();
            Paper.Save(S, ImageFormat.Png);
            XImage i = XImage.FromStream(S);
            gfx.DrawImage(i, 0, 0, gfx.PdfPage.Width, gfx.PdfPage.Height);
            //  document.Pages.Add(P);
            Paper = new Bitmap(6400, 8800);
        }

        private void Button_Click_SAVE(object sender, RoutedEventArgs e)
        {
            verticalpos = 0;
            horizontalpos = 0;

            var P = document.AddPage();

            XGraphics gfx = XGraphics.FromPdfPage(P);
            MemoryStream S = new MemoryStream();
            Paper.Save(S, ImageFormat.Png);
            XImage i = XImage.FromStream(S);
            gfx.DrawImage(i, 0, 0, gfx.PdfPage.Width, gfx.PdfPage.Height);
            Paper = new Bitmap(6400, 8800);

            var fileDialog = new SaveFileDialog();


            if (fileDialog.ShowDialog() == true)
            {
                document.Save(fileDialog.FileName);
                document = new PdfDocument();
            }

        }




    }

    public static class Utility
    {


        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }


    }
}
