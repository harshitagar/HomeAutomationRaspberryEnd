using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Sensors;
using Windows.Devices.Sensors.Custom;
using Windows.ApplicationModel.Core;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Home_automation_new
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        private const int Led_Pin1 = 5;
        private const int Led_Pin2 = 6;
        private const int Led_Pin3 = 13;
        private GpioPin pin1;
        private GpioPin pin2;
        private GpioPin pin3;
        private GpioPinValue pinValue1;
        private GpioPinValue pinValue2;
        private GpioPinValue pinValue3;
        private const int ECHO_PIN = 23;
        private const int TRIGGER_PIN = 18;
        private GpioPin pinEcho;
        private GpioPin pinTrigger;
        private DispatcherTimer timer;
        private Stopwatch sw;


        public MainPage()
        {
            this.InitializeComponent();
            

            InitGPIO();
            tts("Welcome to home automation remote system");
        }

        public void InitGPIO()
        {
            
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                pin1 = null;
                pin2 = null;
                pin3 = null;
                status.Text = "Gpio Pin Not Initialized";
                return;
            }
            pin1 = gpio.OpenPin(Led_Pin1);
            pin2 = gpio.OpenPin(Led_Pin2);
            pin3 = gpio.OpenPin(Led_Pin3);
            pinValue1 = GpioPinValue.High;
            pinValue2 = GpioPinValue.High;
            pinValue3 = GpioPinValue.High;
            pin1.Write(pinValue1);
            pin1.Write(pinValue1);
            pin1.Write(pinValue1);
            pin1.SetDriveMode(GpioPinDriveMode.Output);
            pin2.SetDriveMode(GpioPinDriveMode.Output);
            pin3.SetDriveMode(GpioPinDriveMode.Output);
            status.Text = "gpio pin initialized.";
        }

        private void speechRecognizerSwitch_Click(object sender, RoutedEventArgs e)
        {
            InitSpeechRecognizer();
            if ((speechRecognizerSwitch.Background as SolidColorBrush).Color != Windows.UI.Colors.Green)
                speechRecognizerSwitch.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            else
                speechRecognizerSwitch.Background = new SolidColorBrush(Windows.UI.Colors.Red);

        }

        public async void tts(string text)
        {
            var media = new MediaElement();
            var s = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            var stream = await s.SynthesizeTextToStreamAsync(text);
            media.SetSource(stream, stream.ContentType);
            media.Play();
        }

        private void ultrasonicSwitch_Click(object sender, RoutedEventArgs e)
        {
            InitUltrasonicSensor();

        }

        private void room1switch_Click(object sender, RoutedEventArgs e)
        {
            var gpio = GpioController.GetDefault();
            if (gpio != null)
            {
                switchLightRoom1();
            }

            if ((room1Switch.Background as SolidColorBrush).Color != Windows.UI.Colors.Green)
                room1Switch.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            else
                room1Switch.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            status.Text = "Room 1 light switched";
        }

        private void room2Switch_Click(object sender, RoutedEventArgs e)
        {
            var gpio = GpioController.GetDefault();
            if (gpio != null)
            {
                switchLightRoom2();
            }
            if ((room2Switch.Background as SolidColorBrush).Color != Windows.UI.Colors.Green)
                room2Switch.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            else
                room2Switch.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            status.Text = "Room 2 light switched";
        }

        public void switchLightRoom1()
        {
            if (pinValue1 == GpioPinValue.High)
            {
                pinValue1 = GpioPinValue.Low;
                pin1.Write(pinValue1);
            }
            else
            {
                pinValue1 = GpioPinValue.High;
                pin1.Write(pinValue1);
            }
        }

        public void switchLightRoom2()
        {
            if (pinValue2 == GpioPinValue.High)
            {
                pinValue2 = GpioPinValue.Low;
                pin2.Write(pinValue2);
            }
            else
            {
                pinValue2 = GpioPinValue.High;
                pin2.Write(pinValue2);
            }
        }

        public async void InitSpeechRecognizer()
        {
            SpeechRecognizer Rec = new SpeechRecognizer();
            //Event Handlers
            Rec.ContinuousRecognitionSession.ResultGenerated += Rec_ResultGenerated;

            StorageFile Store = await Package.Current.InstalledLocation.GetFileAsync(@"GrammerFile.xml");
            SpeechRecognitionGrammarFileConstraint constraint = new SpeechRecognitionGrammarFileConstraint(Store);
            Rec.Constraints.Add(constraint);
            SpeechRecognitionCompilationResult result = await Rec.CompileConstraintsAsync();
            if (result.Status == SpeechRecognitionResultStatus.Success)
            {
                status.Text = "Speech Recognition started.";
                tts(status.Text);
                Rec.UIOptions.AudiblePrompt = "Speech Recognition started.";
                await Rec.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async void Rec_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {

            switch (args.Result.Text)
            {
                case "switch light of room one":
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                      () =>
                      {
                          room1switch_Click(null, null);
                          tts("I have switched the light of room one");
                      });

                    break;
                case "switch light of room two":
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        room2Switch_Click(null, null);
                        tts("I have switched the light of room two");
                    });
                    break;
                case "switch all lights":
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        room1switch_Click(null, null);
                        tts("I have switched the light of both rooms");
                        room2Switch_Click(null, null);
                        status.Text = "All lights Switched";
                    });
                    break;
                default:
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       tts("Sorry I didn't get you.");
                   });
                    break;
            }
        }
        public async void InitUltrasonicSensor()
        {
            var gpio = GpioController.GetDefault();
            if (gpio != null)
            {
                pinEcho = gpio.OpenPin(ECHO_PIN);
                pinTrigger = gpio.OpenPin(TRIGGER_PIN);

                pinTrigger.SetDriveMode(GpioPinDriveMode.Output);
                pinEcho.SetDriveMode(GpioPinDriveMode.Input);

                pinTrigger.Write(GpioPinValue.Low);
                status.Text = "GPIO INITIALIZED";

                await Task.Delay(100);
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(400);
                timer.Tick += Timer_Tick;
                if (pinEcho != null && pinTrigger != null)
                {
                    timer.Start();
                }
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            distance.Text = "1";
            pinTrigger.Write(GpioPinValue.High);
            distance.Text = "3";
            Task.Delay(TimeSpan.FromMilliseconds(0.01)).Wait();

            pinTrigger.Write(GpioPinValue.Low);
            Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();

            distance.Text = "4";
            sw.Start();
            distance.Text = "444";
            distance.Text = pinEcho.Read().ToString();
            while (pinEcho.Read() == GpioPinValue.Low)
            {
                distance.Text = "no";
            }

            while (pinEcho.Read() == GpioPinValue.High)
            {
                distance.Text = "nonono";
            }
            sw.Stop();
            distance.Text = "no";

            var elapsed = sw.Elapsed.TotalSeconds;
            var distance1 = elapsed * 34000;

            distance1 /= 2;
            distance.Text = "Distance: " + distance1 + " cm";
        }


    }
}

        
    

