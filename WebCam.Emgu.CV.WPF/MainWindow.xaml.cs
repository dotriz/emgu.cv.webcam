using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.IO;
using System.Threading;
using DirectShowLib;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.ComTypes;
using System.Collections.Generic;

namespace WebCam_Emgu_CV_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private VideoCapture _capture = null;
        private Mat _frame = new Mat();

        #region Public properties

        public ObservableCollection<Filter_Info> VideoDevices { get; set; }

        public Filter_Info CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }
        private Filter_Info _currentDevice;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.DataContext = this;
            GetVideoDevices();
        }

        private void GetVideoDevices()
        {
            //-> Find systems cameras with DirectShow.Net dll
            //DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            //VideoDevices = new ObservableCollection<Filter_Info>();
            //for (int i = 0; i < _SystemCamereas.Length; i++)
            //{
            //    Video_Device WebCam = new Video_Device() {
            //        Index = i,
            //        Name = _SystemCamereas[i].Name,
            //        Mon = _SystemCamereas[i].Mon,
            //        DevicePath = _SystemCamereas[i].DevicePath,
            //        ClassID = _SystemCamereas[i].ClassID
            //    };

            //    Filter_Info fi = new Filter_Info
            //    {
            //        Index = i,
            //        MonikerString = WebCam.Mon,
            //        Name = WebCam.Name
            //    };

            //    VideoDevices.Add(fi);
            //}

            VideoDevices = new ObservableCollection<Filter_Info>();
            List<CamList.Device> devices =  CamList.FindCameras();
            foreach(var device in devices)
            {

                Filter_Info fi = new Filter_Info
                {
                    Index = device.Index,
                    MonikerString = device.Mon,
                    Name = device.Name
                };

                VideoDevices.Add(fi);
            }

            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            StopCamera();
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame, 0);

                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    videoPlayer.Source = ConvertBitmap(_frame.Bitmap);
                }));
            }
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _capture = new VideoCapture(CurrentDevice.Index);
                _capture.ImageGrabbed += ProcessFrame;
                //_capture.FlipHorizontal = true;
                _capture.Start();
            }
        }

        private void StopCamera()
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Stop();
                _capture.Dispose();
                _capture.ImageGrabbed -= ProcessFrame;
            }
        }

        public BitmapImage ConvertBitmap(System.Drawing.Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion
    }

    public class Filter_Info
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public IMoniker MonikerString { get; set; }
    }

    internal class Video_Device
    {
        public int Index { get; set; }
        public IMoniker Mon { get; set; }
        public string Name { get; set; }
        public string DevicePath { get; set; }
        public Guid ClassID { get; set; }

        public Video_Device(int index, string name, IMoniker mon, string devicePath, Guid classID)
        {
            this.Index = index;
            this.Name = name;
            this.ClassID = classID;
            this.Mon = mon;
            this.DevicePath = devicePath;
        }

        public Video_Device(){ }
    }

}
