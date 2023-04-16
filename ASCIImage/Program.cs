using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;

namespace ASCIImage
{
    internal class Program
    {
        static void sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        static void change_color(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        static void reset_color()
        {
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static Bitmap AdjustContrast(Bitmap Image, float Value)
        {
            Value = (100.0f + Value) / 100.0f;
            Value *= Value;
            Bitmap NewBitmap = (Bitmap)Image.Clone();
            BitmapData data = NewBitmap.LockBits(
                new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height),
                ImageLockMode.ReadWrite,
                NewBitmap.PixelFormat);
            int Height = NewBitmap.Height;
            int Width = NewBitmap.Width;

            unsafe
            {
                for (int y = 0; y < Height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    int columnOffset = 0;
                    for (int x = 0; x < Width; ++x)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte R = row[columnOffset + 2];

                        float Red = R / 255.0f;
                        float Green = G / 255.0f;
                        float Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * Value) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * Value) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * Value) + 0.5f) * 255.0f;

                        int iR = (int)Red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)Green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)Blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;

                        columnOffset += 4;
                    }
                }
            }

            NewBitmap.UnlockBits(data);

            return NewBitmap;
        }

        static void progress_bar(int y, int imgH, float imgH100)
        {
            double percentage = ((100 - ((imgH - y) / imgH100)));

            if(percentage < 0)
                percentage = 0;

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write("Progress -> [ ");
            if (percentage > 98)
                change_color(ConsoleColor.Red);
            else if (percentage > 90)
                change_color(ConsoleColor.Yellow);
            else
                change_color(ConsoleColor.Cyan);

            Console.Write(String.Format("{0:0.00}", percentage));
            reset_color();
            change_color(ConsoleColor.Gray);
            Console.Write("%");
            reset_color();
            Console.Write(" ] \n");
        }

        static int Map(float x, float in_min, float in_max, int out_min, int out_max)
        {
            return Convert.ToInt32(Math.Round((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min));
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        [STAThread]
        static void Main(string[] args)
        {
            float contrast = 0.0f, scale = 0.0f;
            string pixelsShort = "@%#*+=-:. ";
            string pixels = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ";
            bool HDmode = false, reverseColors = false, changeContrast = false, resizeImg = false;
            Console.WriteLine("Press \"h\" to activate HD mode(may be slower but uses more chars to represent brightness)\nPress \"c\" to activate custom brightness char mode(may be slower)\nor any other key to leave everything off");
            char key = Console.ReadKey().KeyChar;
            if (key == 'h') {  HDmode = true; }
            if (key == 'c') {
                Console.WriteLine("\nType in your brightness gradient string: ");
                pixels = pixelsShort = Console.ReadLine();
                Console.WriteLine("\"" + pixels + "\" Brightness Gradient string submitted.");
            }
            Console.WriteLine("Press \"v\" to change the Scale\nor any other key to leave it off");
            if (Console.ReadKey().KeyChar == 'v')
            {
                resizeImg = true;
                Console.WriteLine("\nType in Scale value: ");
                scale = float.Parse(Console.ReadLine());
            }
            Console.WriteLine("Press \"r\" to Reverse Colors\nor any other key to leave it off");
            if (Console.ReadKey().KeyChar == 'r') { reverseColors = true; }
            Console.WriteLine("Press \"x\" to set the Contrast\nor any other key to leave the default contrast");
            if (Console.ReadKey().KeyChar == 'x') {
                changeContrast = true;
                Console.WriteLine("\nType in contrast value(idk-idk): ");
                contrast = float.Parse(Console.ReadLine());
            }
            string path = null;

            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse Image Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "png",
                Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            change_color(ConsoleColor.Cyan);
            Console.Write("\n[?]");
            reset_color();
            change_color(ConsoleColor.Green);
            Console.Write("->");
            reset_color();
            Console.Write("Selecting image...\n");

            openFileDialog1.ShowDialog();
            path = openFileDialog1.FileName;

            if (path == null)
                return;

            //Console.Clear();

            string asciiOut = "";
            if (!reverseColors)
            {
                pixels = Reverse(pixels);
                pixelsShort = Reverse(pixelsShort);
            }
            Bitmap image = new Bitmap(path);
            if (changeContrast)
            {
                image = AdjustContrast(image, contrast);
            }
            if (resizeImg)
            {
                image = new Bitmap(image, new Size((int)Math.Floor(image.Width*scale), (int)Math.Floor(image.Height*scale)));
            }
            float imgh100 = image.Height / 100;
            change_color(ConsoleColor.Cyan);
            Console.Write("\n[?]");
            reset_color();
            change_color(ConsoleColor.Green);
            Console.Write("->");
            reset_color();
            Console.Write("Path: " + path + " Size: { X: " + image.Width + ", Y: " + image.Height + " } Total Pixels: " + (image.Width*image.Height) + ".\n\n");
            //Color pixelColor;
            Console.CursorVisible = false;
            if (HDmode)
            {
                for (int h = 0; h < image.Height; h++)
                {
                    for (int w = 0; w < image.Width; w++)
                    {
                        asciiOut += pixels[Map(image.GetPixel(w, h).GetBrightness(), 0.0000f, 1.0000f, 0, pixels.Length - 1)];
                        progress_bar(h, image.Height, imgh100);
                    }
                    asciiOut += "\n";
                }
            }
            else
            {
            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    asciiOut += pixels[Map(image.GetPixel(w, h).GetBrightness(), 0.0000f, 1.0000f, 0, pixelsShort.Length - 1)];
                    progress_bar(h, image.Height, imgh100);
                }
                asciiOut += "\n";
            }
            }
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write("Progress -> [ ");
            change_color(ConsoleColor.Green);
            Console.Write("DONE");
            reset_color();
            change_color(ConsoleColor.Gray);
            reset_color();
            Console.Write(" ]");
            Clipboard.SetText(asciiOut);
            change_color(ConsoleColor.Cyan);
            Console.Write("\n[?]");
            reset_color();
            change_color(ConsoleColor.Green);
            Console.Write("->");
            reset_color();
            Console.Write("Text copied to clipboard.\n");
            //Console.WriteLine(asciiOut);
            for (int t = 0; t < 3; t++)
            {
                Console.WriteLine("Waiting for " + t + " second/s.");
                sleep(1000);
            }
        }
    }
}
