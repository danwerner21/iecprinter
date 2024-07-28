using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows;

namespace IECPrintServer
{
    public class PrinterEmulator : IDisposable
    {
        CbmPrinterFont prt = new CbmPrinterFont();
        int verticalpos = 0;
        int horizontalpos = 0;
        Bitmap Paper = new Bitmap(6400, 7040);
        PdfDocument document = new PdfDocument();
        int LPI = 1;
        bool Enhanced = false;
        bool Reversed = false;
        bool BitImage = false;
        bool EscMode = false;
        int Font = 0;
        bool DotAddressStateOne = false;
        bool AddressStateOne = false;
        bool DotAddressStateTwo = false;
        bool AddressStateTwo = false;

        public PrinterEmulator()
        {

        }

        public void print(char c)
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

        public BitmapImage ToBitmapImage()
        {
            using (var memory = new MemoryStream())
            {
                Paper.Save(memory, ImageFormat.Png);
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

        public void setLpi(int lpi)
        {
            if (lpi == 6)
            {
                LPI = 4;
            }
            else
            {
                LPI = 1;
            }
        }


        public void doFF()
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

        public void doSave(string FileName)
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
            document.Save(FileName);
            document = new PdfDocument();
        }



        public void Dispose()
        {            
        }
    }
}
