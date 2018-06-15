using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Includes for the Lab
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Kinect;
using System.Windows.Forms;
using System.Drawing;

using Emgu.CV;

using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Windows.Interop;

namespace GTI780_TP1
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Size of the raw depth stream
        private const int RAWDEPTHWIDTH = 512;
        private const int RAWDEPTHHEIGHT = 424;

        // Size of the raw color stream
        private const int RAWCOLORWIDTH = 1920;
        private const int RAWCOLORHEIGHT = 1080;

        // Number of bytes per pixel for the format used in this project
        private int BYTESPERPIXELS = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;



        // Size of the target display screen
        private double screenWidth;
        private double screenHeight;

        // Bitmaps to display
        private WriteableBitmap colorBitmap = null;
        private WriteableBitmap depthBitmap = null;


        // The kinect sensor

        private KinectSensor kinectSensor = null;
        private CoordinateMapper coordinateMapper = null;
        private DepthSpacePoint[] colorMappedToDepthPoints = null;

        int rightImageFrameCount;


        // The kinect frame reader

        private ColorFrameReader colorFrameReader = null;

        private MultiSourceFrameReader mfsReader = null;


        public MainWindow()
        {
            InitializeComponent();

            // Sets the correct size to the display components
            InitializeComponentsSize();

            // Instanciate the WriteableBitmaps used to display the kinect frames
            this.colorBitmap = new WriteableBitmap(RAWCOLORWIDTH, RAWCOLORHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.depthBitmap = new WriteableBitmap(RAWDEPTHWIDTH, RAWDEPTHHEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);

            // Connect to the Kinect Sensor
            this.kinectSensor = KinectSensor.GetDefault();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            this.colorMappedToDepthPoints = new DepthSpacePoint[RAWCOLORWIDTH * RAWCOLORHEIGHT];

            // open the reader for the color frames
            //this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.mfsReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            rightImageFrameCount = 0;


            // wire handler for frame arrival
            //this.colorFrameReader.FrameArrived += this.Reader_FrameArrived;
            mfsReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;


            // Open the kinect sensor
            this.kinectSensor.Open();


            // Sets the context for the data binding
            this.DataContext = this;
        }

        /// <summary>
        /// This event will be called whenever the color Frame Reader receives a new frame
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">args of the event</param>
        void Reader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // Store the depth and the color frame
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;

            // Store the state of the frame lock
            bool isColorBitmapLocked = false;
            bool isDepthBitmapLocked = false;

            // Acquire a new frame
            colorFrame = e.FrameReference.AcquireFrame();

            // If the frame has expired or is invalid, return
            if (colorFrame == null) return;

            // Using a try/finally structure allows us to liberate/dispose of the elements even if there was an error
            try
            {


                // ===============================
                // ColorFrame code block
                // ===============================   

                FrameDescription colorDesc = colorFrame.FrameDescription;
                // Using an IDisposable buffer to work with the color frame. Will be disposed automatically at the end of the using block.
                using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                {
                    // Lock the colorBitmap while we write in it.
                    this.colorBitmap.Lock();
                    isColorBitmapLocked = true;

                    // Check for correct size
                    if (colorDesc.Width == this.colorBitmap.Width && colorDesc.Height == this.colorBitmap.Height)
                    {
                        //write the new color frame data to the display bitmap
                        colorFrame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, (uint)(colorDesc.Width * colorDesc.Height * BYTESPERPIXELS), ColorImageFormat.Bgra);

                        // Mark the entire buffer as dirty to refresh the display
                        this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorDesc.Width, colorDesc.Height));
                    }

                    // Unlock the colorBitmap
                    this.colorBitmap.Unlock();
                    isColorBitmapLocked = false;
                }
              


                // ================================================================
                // DepthFrame code block : À modifier et completer 
                // Remarque : Beaucoup de code à modifer/ajouter dans cette partie
                // ================================================================



                using (KinectBuffer depthBuffer = colorFrame.LockRawImageBuffer())
                {
                    // Lock the depthBitmap while we write in it.
                    this.depthBitmap.Lock();
                    isDepthBitmapLocked = true;

                    //-----------------------------------------------------------
                    // Effectuer la correspondance espace Profondeur---Couleur 
                    //-----------------------------------------------------------
                    //  Utiliser la ligne ci-dessous pour l'image de profondeur
                    Image<Gray, byte> depthImageGray = new Image<Gray, byte>(RAWCOLORWIDTH, RAWCOLORHEIGHT);

                    //-----------------------------------------------------------
                    // Traiter l'image de profondeur 
                    //-----------------------------------------------------------


                    // Une fois traitée convertir l'image en Bgra
                    Image<Bgra, byte> depthImageBgra = depthImageGray.Convert<Bgra, byte>();

                    //---------------------------------------------------------------------------------------------------------
                    //  Modifier le code pour que depthBitmap contienne depthImageBgra au lieu du contenu trame couleur actuel
                    //---------------------------------------------------------------------------------------------------------
                    if (colorDesc.Width == this.colorBitmap.Width && colorDesc.Height == this.colorBitmap.Height)
                    {
                        colorFrame.CopyConvertedFrameDataToIntPtr(this.depthBitmap.BackBuffer, (uint)(colorDesc.Width * colorDesc.Height * BYTESPERPIXELS), ColorImageFormat.Bgra);

                        // Mark the entire buffer as dirty to refresh the display
                        this.depthBitmap.AddDirtyRect(new Int32Rect(0, 0, colorDesc.Width, colorDesc.Height));
                    }



                    // Unlock the colorBitmap
                    this.depthBitmap.Unlock();
                    isDepthBitmapLocked = false;
                }


                // We are done with the depthFrame, dispose of it
                // depthFrame.Dispose();
                depthFrame = null;
                // We are done with the ColorFrame, dispose of it
                colorFrame.Dispose();
                colorFrame = null;

                // ===============================
                // ===============================



            }
            finally
            {
                if (isColorBitmapLocked) this.colorBitmap.Unlock();
                if (isDepthBitmapLocked) this.depthBitmap.Unlock();
                if (depthFrame != null) depthFrame.Dispose();
                if (colorFrame != null) colorFrame.Dispose();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            // Get a reference to the multi-frame
            var reference = e.FrameReference.AcquireFrame();
            ColorFrame colorFrame = null;
            BodyIndexFrame biFrame = null;
            DepthFrame depthFrame = null;
            try
            {
                rightImageFrameCount++;
                if (rightImageFrameCount == 1)
                {
                    colorFrame = reference.ColorFrameReference.AcquireFrame();
                if (colorFrame != null)
                {
                    ColorSection(colorFrame);
                }

                depthFrame = reference.DepthFrameReference.AcquireFrame();
                if (depthFrame != null)
                {
                    //DepthSection(depthFrame);
                    
                        GetRightImage(depthFrame, colorFrame);
                }
                    rightImageFrameCount = 0;
                }
            }
            finally
            {

                if (colorFrame != null)
                    colorFrame.Dispose();

                if (depthFrame != null)
                    depthFrame.Dispose();
            }

        }

        void DepthSection(DepthFrame frame)
        {
            using (KinectBuffer depthFrameData = frame.LockImageBuffer())
            {
                this.coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                    depthFrameData.UnderlyingBuffer,
                    depthFrameData.Size,
                    this.colorMappedToDepthPoints);
                //PictureBox2.Source = getImageSourceFromBitmap(DepthFrameToBitmap(frame));
            }
        }

        void GetRightImage(DepthFrame dFrame, ColorFrame cFrame)
        {
            byte[] disparity = GetDisparityInPixelsForImage(dFrame);
            ShowColorFrameWithDisparity(cFrame, disparity);
        }

        void ShowColorFrameWithDisparity(ColorFrame frame, byte[] disparity)
        {
            PixelFormat format = PixelFormats.Bgr32;
            int stride = frame.FrameDescription.Width * format.BitsPerPixel / 8;
            byte[] pixelData = new byte[RAWCOLORHEIGHT * stride];
            colorBitmap.CopyPixels(pixelData, stride, 0);

            byte[] rightImage = new byte[RAWCOLORHEIGHT * stride];
            for(int i = 0; i < frame.FrameDescription.Height; i++) {
                for(int j = 0; j < frame.FrameDescription.Width; j++) {
                    int index = i * stride + (j + disparity[i*((int)colorBitmap.Width)+j])*format.BitsPerPixel;
                    for (int k = 0; k < format.BitsPerPixel; k++) {
                        try
                        {
                           rightImage[index + k] = pixelData[i * stride + j * format.BitsPerPixel + k];
                        }
                        catch(Exception ex)
                        {
                        }
                    }
                }
            }
            BitmapSource image = BitmapSource.Create(Convert.ToInt32(this.colorBitmap.Width), Convert.ToInt32(this.colorBitmap.Height), 96, 96, this.colorBitmap.Format, null, rightImage, stride);
            PictureBox2.Source = image;

            /*BitmapSource image = BitmapSource.Create(Convert.ToInt32(this.colorBitmap.Width), Convert.ToInt32(this.colorBitmap.Height), 96, 96, this.colorBitmap.Format, null, pixelData, stride);
             Bitmap bmpRightImage = BitmapFromSource(image);
             Bitmap bmpLeftImage = BitmapFromSource(image);

             for(int y = 0; y < bmpLeftImage.Height; y++)
             {
                 for(int x = 0; x < bmpLeftImage.Width; x++)
                 {
                     try
                     {
                         bmpRightImage.SetPixel(x + disparity[y * bmpLeftImage.Width + x], y, bmpLeftImage.GetPixel(x, y));
                     }catch(Exception ex)
                     {

                     }
                 }
             }

             PictureBox2.Source = getImageSourceFromBitmap(bmpRightImage);*/

            /*this.depthBitmap.CopyPixels(frameData, RAWCOLORWIDTH, 0);
            this.depthBitmap.AddDirtyRect(new Int32Rect(0, 0, RAWCOLORWIDTH, RAWCOLORHEIGHT));*/



        }

        byte[] GetDisparityInPixelsForImage(DepthFrame frame)
        {
            double knear = 0.1;
            double kfar = 0.6;
            double tc = 0.065;//65mm
            double w = Screen.PrimaryScreen.Bounds.Width;
            double h = Screen.PrimaryScreen.Bounds.Height;
            double ratio = w / h;

            // T qui n'est pas fixe, j'ai prit celui de l'exemple dans les notes
            double T = 1.52;
            double W = 1.33;
            double H = 0.74;
            double D = 3 * H;
            double N = 1920;

            Bitmap bmp = DepthFrameToBitmap(frame);
            /*int stride = bmp.Width * PixelFormats.Bgr32.BitsPerPixel;
            byte[] pixelData = new byte[Convert.ToInt32(this.colorBitmap.Width) * Convert.ToInt32(this.colorBitmap.Height) * PixelFormats.Bgr32.BitsPerPixel];
            this.colorBitmap.CopyPixels(pixelData, stride, 0);
            BitmapSource image = BitmapSource.Create( Convert.ToInt32(this.colorBitmap.Width), Convert.ToInt32(this.colorBitmap.Height), 96, 96, this.colorBitmap.Format, null, pixelData, stride);
            Bitmap bmpRightImage = BitmapFromSource(image);
            Bitmap bmpLeftImage = BitmapFromSource(image);*/

            byte[] disparityArray = new byte[bmp.Width * bmp.Height];
            int height = bmp.Height;
            int width = bmp.Width;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    System.Drawing.Color c = bmp.GetPixel(x, y);
                    byte m = c.B;

                    double zp = W * (( m / (Math.Pow(2,N))) * (knear + kfar) - kfar);
                    double p = tc * (1 - ( (D)/(D - zp)));
                    double pix = (p * N) / W;
                    byte b = Convert.ToByte(pix);
                    disparityArray[y * bmp.Width + x] = b;
                }
            }
            //PictureBox2.Source = getImageSourceFromBitmap(bmpRightImage);
            return disparityArray;
        }

        void ColorSection(ColorFrame frame)
        {
            bool isColorBitmapLocked = false;

            try
            {
                // If the frame has expired or is invalid, return
                if (frame == null) return;
                FrameDescription colorDesc = frame.FrameDescription;
                // Using an IDisposable buffer to work with the color frame. Will be disposed automatically at the end of the using block.
                using (KinectBuffer colorBuffer = frame.LockRawImageBuffer())
                {
                    // Lock the colorBitmap while we write in it.
                    this.colorBitmap.Lock();
                    isColorBitmapLocked = true;

                    // Check for correct size
                    if (colorDesc.Width == this.colorBitmap.Width && colorDesc.Height == this.colorBitmap.Height)
                    {
                        //write the new color frame data to the display bitmap
                        frame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, (uint)(colorDesc.Width * colorDesc.Height * BYTESPERPIXELS), ColorImageFormat.Bgra);

                        // Mark the entire buffer as dirty to refresh the display
                        this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorDesc.Width, colorDesc.Height));
                    }

                    // Unlock the colorBitmap
                    this.colorBitmap.Unlock();
                    isColorBitmapLocked = false;
                }
            }
            finally
            {
                if (isColorBitmapLocked) this.colorBitmap.Unlock();
            }
        }

        public ImageSource ImageSource1
        {
            get
            {
                return this.colorBitmap;
            }
        }

        public ImageSource ImageSource2
        {
            get
            {
                return this.depthBitmap;
            }
        }

        public BitmapSource getImageSourceFromBitmap(Bitmap bmp)
        {
            BitmapSource imageSource = null;
            try
            {
                imageSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch {
                return null;
            }
            return imageSource;
        }

        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder bmpEnc = new BmpBitmapEncoder();
                bmpEnc.Frames.Add(BitmapFrame.Create(bitmapsource));
                bmpEnc.Save(outStream);
                bitmap = new Bitmap(outStream);
                outStream.Dispose();
            }
            return bitmap;
        }

        private Bitmap DepthFrameToBitmap(DepthFrame frame)
        {
            PixelFormat format = PixelFormats.Bgr32;

            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            int mapDepthToByte = 1000 / 256;
            ushort minDepth = (ushort)(frame.DepthMinReliableDistance);
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] depthData = new ushort[width * height];
            byte[] pixelData = new byte[width * height * (PixelFormats.Bgr32.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(depthData);

            int colorI = 0;
            for (int depthI = 0; depthI < depthData.Length; ++depthI)
            {
                ushort depth = depthData[depthI];
                byte intensity = (byte)(255 - (depth >= minDepth && depth <= maxDepth ? ((depth - minDepth) / mapDepthToByte) : 255));
                
                pixelData[colorI++] = intensity; // Blue
                pixelData[colorI++] = intensity; // Green
                pixelData[colorI++] = intensity; // Red

                colorI++;
            }

            int stride = width * format.BitsPerPixel / 8;
            BitmapSource image = BitmapSource.Create(width, height, 96, 96, format, null, pixelData, stride);
            double ratioX = (double)RAWCOLORWIDTH / (double)width;
            double ratioY = (double)RAWCOLORHEIGHT / (double)height;

            // to be replace by CoordinateMapper : How ??
            image = new TransformedBitmap(image, new ScaleTransform(ratioX, ratioY));

            Bitmap imageBitmap = BitmapFromSource(image);

            Image<Bgr, Byte> imageBGR = new Image<Bgr, byte>(imageBitmap);
            imageBitmap.Dispose();
            Image<Bgr, byte> imageSmooth = imageBGR.SmoothMedian(5);
            imageBGR.Dispose();
            return imageSmooth.ToBitmap();
        }

        private void InitializeComponentsSize()
        {
            // Get the screen size
            Screen[] screens = Screen.AllScreens;
            this.screenWidth = screens[0].Bounds.Width;
            this.screenHeight = screens[0].Bounds.Height;

            // Make the application full screen
            this.Width = this.screenWidth;
            this.Height = this.screenHeight;
            this.MainWindow1.Width = this.screenWidth;
            this.MainWindow1.Height = this.screenHeight;

            // Make the Grid container full screen
            this.Grid1.Width = this.screenWidth;
            this.Grid1.Height = this.screenHeight;

            // Make the PictureBox1 half the screen width and full screen height
            this.PictureBox1.Width = this.screenWidth;
            this.PictureBox1.Height = this.screenHeight / 2;

            // Make the PictureBox2 half the screen width and full screen height
            this.PictureBox2.Width = this.screenWidth;
            this.PictureBox2.Margin = new Thickness(0, 0, 0, 0);
            this.PictureBox2.Height = this.screenHeight / 2;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if(this.mfsReader != null)
            {
                this.mfsReader.Dispose();
                this.mfsReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
    }
}