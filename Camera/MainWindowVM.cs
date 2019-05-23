using AForge.Video;
using AForge.Video.DirectShow;
using Camera.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Camera
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        #region Private properties
        private IVideoSource videoSource;
        private BitmapImage latestFrame;
        #endregion

        #region Properties
        private ObservableCollection<FilterInfo> _videoDevices = new ObservableCollection<FilterInfo>();
        public ObservableCollection<FilterInfo> VideoDevices
        {
            get => _videoDevices;
            set
            {
                if (value != null)
                {
                    _videoDevices = value;
                    OnPropertyChanged(nameof(VideoDevices));
                }
            }
        }

        private FilterInfo _currentDevice;
        public FilterInfo CurrentDevice
        {
            get => _currentDevice;
            set
            {
                _currentDevice = value;
                OnPropertyChanged(nameof(CurrentDevice));
            }
        }

        private BitmapImage _display;
        public BitmapImage Display
        {
            get => _display;
            set
            {
                _display = value;
                OnPropertyChanged(nameof(Display));
            }
        }

        #endregion

        #region Constructor
        public MainWindowVM()
        {
            GetVideoDevices();
        }
        #endregion

        #region Commands
        public RelayCommand TakePictureCommand
        {
            get
            {
                return new RelayCommand(param =>
                {
                    Debug.WriteLine("Pressed Take Picture Btn");
                    TakePicture();
                });
            }
        }

        public RelayCommand StartCommand
        {
            get
            {
                return new RelayCommand(param =>
                {
                    Debug.WriteLine("Starting recording");
                    StartCamera();
                });
            }
        }

        public RelayCommand StopCommand
        {
            get
            {
                return new RelayCommand(param =>
                {
                    Debug.WriteLine("Stopping recording");
                    StopCamera();
                });
            }
        }

        #endregion

        #region Helpers
        private void GetVideoDevices()
        {
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
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

        private void StartCamera()
        {
            if (CurrentDevice != null && videoSource == null) // ensure their isnt already a source
            {
                videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                videoSource.NewFrame += VideoNewFrame;
                videoSource.Start();
            }
        }

        private void StopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= new NewFrameEventHandler(VideoNewFrame);
            }
        }

        private void TakePicture()
        {
            try
            {
                Bitmap bm = BitmapImageToBitmap(latestFrame);
                string fileName = GetLastFile();
                bm.Save(Settings.Default.ImagesPath + fileName + ".png", ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "  " + ex.InnerException);
            }

        }

        private void VideoNewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    latestFrame = bi;
                    Display = bi;
                }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private string GetLastFile()
        {
            List<string> sorted = Directory.GetFiles(Settings.Default.ImagesPath).OrderBy(x => new FileInfo(x).CreationTime).ToList();

            if ((sorted != null) && sorted.Count > 0)
            {
                string fileName = sorted.Last().Split('/').Last();
                int getFileNumber = Convert.ToInt32(fileName.Split('.').First());
                getFileNumber++;
                return getFileNumber.ToString();
            }
            else if (sorted.Count == 0)
                return "1";

            return "error";
        }

        #endregion

        #region Notify Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Tells the event, PropertyChanged, that a property has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
