using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AForge.Video;
using AForge.Video.DirectShow;

namespace AforgeTrials
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Install NuGet Package Manager
    /// 
    /// Using the NuGet Package Manager download 
    /// AForge, AForge.Video and AForge.VideoDirectShow
    /// </summary>
    public partial class MainWindow : Window
    {
        FilterInfoCollection fic = null;
        VideoCaptureDevice vcd = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method will start the video camera of the selected device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            vcd = new VideoCaptureDevice(fic[cmbCamera.SelectedIndex].MonikerString);
            vcd.NewFrame += Vcd_NewFrame;
            vcd.Start();
        }

        /// <summary>
        /// This updates the content of the display source
        /// In our case the Image box with frames coming from the camerasource
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void Vcd_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //pic.Source = eventArgs.Frame.Clone();

            // This line allows the event to modify the content of the image box
            // without this block of code, the thread holding the Image box and the thread
            // holding the image will not talk to each other
            this.Dispatcher.Invoke(() =>
            {
                /*
                 * This convoluted piece of code will convert the bitmap image of the video
                 * source (Your selected webcam) into something the Imagebox can load.
                 * I suggest not to change this section of the code.
                 */
                pic.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ((Bitmap)eventArgs.Frame.Clone()).GetHbitmap(),
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight((int)pic.Width, (int)pic.Height));
            });

            //MessageBox.Show(eventArgs.Frame.Clone().ToString());
        }

        /// <summary>
        /// To be perfectly honest, this could also be done in the Window_Loaded event
        /// and will perform exactly the same task
        /// 
        /// This event will add all the possible camera sources your computer has access to
        /// and by default select the first one it detects/sees
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo fi in fic)
                cmbCamera.Items.Add(fi.Name);
            cmbCamera.SelectedIndex = 0;
            vcd = new VideoCaptureDevice();
        }

        /// <summary>
        /// This method will convert the image displayed in the Image box into a PNG file
        /// </summary>
        /// <param name="filePath"></param>
        public void ImageToFile(string filePath)
        {
            var image = pic.Source;
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image));
                encoder.Save(fileStream);
            }
        }

        /// <summary>
        /// This event, upon clicking will stop the webcam from running
        /// Save the file in the desired location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCaptureImage_Click(object sender, RoutedEventArgs e)
        {
            //if (vcd.IsRunning)
            //{
            //    ImageToFile("Test.png");
            //    vcd.WaitForStop();
            //    vcd = null;
            //    //vcd.Stop();
            //}
            ImageToFile("Test.png");
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (vcd.IsRunning)
            {
                //ImageToFile("Test.png");
                //vcd.WaitForStop();
                vcd.SignalToStop(); // tells camera to stop working
                //vcd.WaitForStop();
                vcd = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                JustAnotherWindow jaw = new JustAnotherWindow();
                jaw.Show();
            }
        }
    }
}
