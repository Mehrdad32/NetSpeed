using System;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using NetSpeed.Properties;

namespace NetSpeed
{
    public partial class MainWindow : Window
    {
        readonly BackgroundWorker _worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            //(_plotModel.Series[0] as LineSeries).YAxis.IsAxisVisible = false;
            _worker.DoWork += (sender, args) =>
            {
                try
                {
                    var network = GetUpNetworkInterface();

                    while (true)
                    {
                        if (network == null)
                        {
                            Dispatcher.BeginInvoke((Action)(() => DownSpeedText.Text = "Error!"));
                            Thread.Sleep(5000);
                            network = GetUpNetworkInterface();
                        }
                        else
                        {
                            var beginValue = network.GetIPv4Statistics().BytesReceived;
                            var beginUpValue = network.GetIPv4Statistics().BytesSent;
                            var beginTime = DateTime.Now;

                            Thread.Sleep(1000);

                            var endValue = network.GetIPv4Statistics().BytesReceived;
                            var endUpValue = network.GetIPv4Statistics().BytesSent;
                            var endTime = DateTime.Now;

                            var receivedBytes = endValue - beginValue;
                            var uploadedBytes = endUpValue - beginUpValue;

                            var totalSeconds = (endTime - beginTime).TotalSeconds;

                            var bytesPerSecond = receivedBytes / totalSeconds;
                            var upBytesPerSecond = uploadedBytes / totalSeconds;

                            Dispatcher.BeginInvoke((Action)(() => DownSpeedText.Text = (int)bytesPerSecond / 1024 + ""));
                            Dispatcher.BeginInvoke((Action)(() => UpSpeedText.Text = (int)upBytesPerSecond / 1024 + ""));
                            Dispatcher.BeginInvoke((Action)(() => NetworkName.Text = network.Name));
                        }
                    }
                }
                catch
                {
                    //IGNORE
                }
            };
        }

        public NetworkInterface GetUpNetworkInterface()
        {
            try
            {
                var sp = NetworkInterface.GetAllNetworkInterfaces();
                return sp.FirstOrDefault(networkInterface => networkInterface.OperationalStatus == OperationalStatus.Up);
            }
            catch
            {
                return null;
            }
        }

        public void SetAlpha(double value)
        {
            try
            {
                Card.Background.Opacity = value;
                //if(DownSpeedText != null) DownSpeedText.Opacity = value;
                //if(UpSpeedText != null) UpSpeedText.Opacity = value;
                if (NetworkName != null) NetworkName.Opacity = value;
                Slider.Opacity = value;
                if(CloseButton != null) CloseButton.Opacity = value;
                if(RefreshButton != null) RefreshButton.Opacity = value;
            }
            catch 
            {
                //IGNORE!
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.GraphShow = Height > 95;
            Settings.Default.SliderValue = Slider.Value;
            Settings.Default.Save();

            //_worker.CancelAsync();
            //_worker = null;
            Application.Current.Shutdown();
        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetAlpha(Slider.Value / 100);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeAppLocation(AppLocation.BottomRight);

                //SetAlpha(Properties.Settings.Default.SliderValue);
                Slider.Value = Settings.Default.SliderValue;

                _worker.RunWorkerAsync();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error: " + exception.Message);
            }
        }

        private AppLocation _currentLocation;

        private void ChangeAppLocation(AppLocation location)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            _currentLocation = location;

            if (location == AppLocation.BottomRight)
            {
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
            } 
            else if (location == AppLocation.TopRight)
            {
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Top;
            }
            else if (location == AppLocation.TopLeft)
            {
                Left = desktopWorkingArea.Left;
                Top = desktopWorkingArea.Top;
            }
            else if (location == AppLocation.BottomLeft)
            {
                Left = desktopWorkingArea.Left;
                Top = desktopWorkingArea.Bottom - Height;
            }

            /*if (Settings.Default.GraphShow)
            {
                Top -= 100;
                Height = 190;
            }
            else
            {
                //Top += 100;
                Height = 100;
            }*/
        }

        private void Card_OnMouseEnter(object sender, MouseEventArgs e)
        {
            SetAlpha(1.0);
        }

        private void Card_OnMouseLeave(object sender, MouseEventArgs e)
        {
            SetAlpha(Slider.Value / 100);
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (_currentLocation)
                {
                    case AppLocation.TopRight:
                        ChangeAppLocation(AppLocation.BottomRight);
                        break;
                    case AppLocation.BottomRight:
                        ChangeAppLocation(AppLocation.BottomLeft);
                        break;
                    case AppLocation.BottomLeft:
                        ChangeAppLocation(AppLocation.TopLeft);
                        break;
                    case AppLocation.TopLeft:
                        ChangeAppLocation(AppLocation.TopRight);
                        break;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error: " + exception.Message);
            }
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }

    public enum AppLocation
    {
        TopRight, BottomRight, TopLeft, BottomLeft
    }
}
