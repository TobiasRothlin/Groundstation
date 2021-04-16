using Foundation;
using System;
using UIKit;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Microcharts;
using Microcharts.iOS;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;

namespace MissionControlCenter
{
    public partial class ViewController : UIViewController
    {
        private UITextView UIConsole = new UITextView();

        private UIProgressView DataAcquisitionProgress = new UIProgressView();

        private UILabel CurrentPitchLbl = new UILabel();
        private UILabel CurrentRollLbl = new UILabel();
        private UILabel CurrentYawLbl = new UILabel();
        private UILabel RunTimeLbl = new UILabel();
        private UILabel CurrentAcXLbl = new UILabel();
        private UILabel CurrentAcYLbl = new UILabel();
        private UILabel CurrentAcZLbl = new UILabel();
        private UILabel CurrentGyXLbl = new UILabel();
        private UILabel CurrentGyYLbl = new UILabel();
        private UILabel CurrentGyZLbl = new UILabel();

        private List<ChartEntry> pitchChartData = new List<ChartEntry>();
        private ChartView pitchChart = new ChartView();
        private LineChart pitchLine = new LineChart();

        private List<ChartEntry> rollChartData = new List<ChartEntry>();
        private ChartView rollChart = new ChartView();
        private LineChart rollLine = new LineChart();


        private List<ChartEntry> yawChartData = new List<ChartEntry>();
        private ChartView yawChart = new ChartView();
        private LineChart yawLine = new LineChart();



        private Rocket Ranger = new Rocket();


        private bool UIUpdateActive = true;


        public ViewController(IntPtr handle) : base(handle)
        {
        }




        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Ranger.TelemetryUpdate += this.updateUIValues;

            UIButton ButtonLaunch = new UIButton(UIButtonType.System);
            ButtonLaunch.Frame = new CoreGraphics.CGRect(155, 155, 100, 40);
            ButtonLaunch.SetTitle("Launch", UIControlState.Normal);
            ButtonLaunch.BackgroundColor = UIColor.FromRGB(220, 80, 95);
            ButtonLaunch.SetTitleColor(UIColor.FromRGB(65, 64, 64), UIControlState.Normal);
            ButtonLaunch.AddTarget(ButtonLaunchEventHandler, UIControlEvent.TouchUpInside);
            View.AddSubview(ButtonLaunch);

            UIButton ButtonTest = new UIButton(UIButtonType.System);
            ButtonTest.Frame = new CoreGraphics.CGRect(155, 100, 100, 40);
            ButtonTest.SetTitle("Test", UIControlState.Normal);
            ButtonTest.BackgroundColor = UIColor.FromRGB(115, 191, 148);
            ButtonTest.SetTitleColor(UIColor.FromRGB(65, 64, 64), UIControlState.Normal);
            ButtonTest.AddTarget(ButtonTestEventHandler, UIControlEvent.TouchUpInside);
            View.AddSubview(ButtonTest);

            UIButton ButtonGetStatus = new UIButton(UIButtonType.System);
            ButtonGetStatus.Frame = new CoreGraphics.CGRect(275, 155, 100, 40);
            ButtonGetStatus.SetTitle("Get Status", UIControlState.Normal);
            ButtonGetStatus.BackgroundColor = UIColor.FromRGB(49, 81, 122);
            ButtonGetStatus.SetTitleColor(UIColor.FromRGB(198, 200, 202), UIControlState.Normal);
            ButtonGetStatus.AddTarget(ButtonStatusEventHandler, UIControlEvent.TouchUpInside);
            View.AddSubview(ButtonGetStatus);

            UIButton ButtonClearOutput = new UIButton(UIButtonType.System);
            ButtonClearOutput.Frame = new CoreGraphics.CGRect(395, 155, 100, 40);
            ButtonClearOutput.SetTitle("Clear Output", UIControlState.Normal);
            ButtonClearOutput.BackgroundColor = UIColor.FromRGB(49, 81, 122);
            ButtonClearOutput.SetTitleColor(UIColor.FromRGB(198, 200, 202), UIControlState.Normal);
            ButtonClearOutput.AddTarget(ButtonClearOutputEventHandler, UIControlEvent.TouchUpInside);
            View.AddSubview(ButtonClearOutput);

            UIConsole.Frame = new CoreGraphics.CGRect(155, 250, 400, 400);
            UIConsole.BackgroundColor = UIColor.FromRGB(65, 64, 64);
            UIConsole.TextColor = UIColor.FromRGB(255, 255, 255);
            UIConsole.TextAlignment = UITextAlignment.Left;
            UIConsole.Text = "";
            View.AddSubview(UIConsole);

            UILabel ConsoleLbl = new UILabel();
            ConsoleLbl.Frame = new CoreGraphics.CGRect(155, 210, 200, 50);
            ConsoleLbl.Text = "Raw Output:";
            ConsoleLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(ConsoleLbl);


            CurrentPitchLbl.Frame = new CoreGraphics.CGRect(155, 700, 200, 50);
            CurrentPitchLbl.Text = "Pitch: 0°";
            CurrentPitchLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentPitchLbl);


            CurrentRollLbl.Frame = new CoreGraphics.CGRect(355, 700, 200, 50);
            CurrentRollLbl.Text = "Roll: 0°";
            CurrentRollLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentRollLbl);


            CurrentYawLbl.Frame = new CoreGraphics.CGRect(555, 700, 200, 50);
            CurrentYawLbl.Text = "Yaw: 0°";
            CurrentYawLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentYawLbl);

            RunTimeLbl.Frame = new CoreGraphics.CGRect(400, 210, 200, 50);
            RunTimeLbl.Text = "Run Time: 0ms";
            RunTimeLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(RunTimeLbl);

            CurrentAcXLbl.Frame = new CoreGraphics.CGRect(155, 750, 200, 50);
            CurrentAcXLbl.Text = "AcX: 0m/s^2";
            CurrentAcXLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentAcXLbl);

            CurrentAcYLbl.Frame = new CoreGraphics.CGRect(355, 750, 200, 50);
            CurrentAcYLbl.Text = "AcY: 0m/s^2";
            CurrentAcYLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentAcYLbl);

            CurrentAcZLbl.Frame = new CoreGraphics.CGRect(555, 750, 200, 50);
            CurrentAcZLbl.Text = "AcZ: 0m/s^2";
            CurrentAcZLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentAcZLbl);

            CurrentGyXLbl.Frame = new CoreGraphics.CGRect(155, 800, 200, 50);
            CurrentGyXLbl.Text = "GyX: 0";
            CurrentGyXLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentGyXLbl);

            CurrentGyYLbl.Frame = new CoreGraphics.CGRect(355, 800, 200, 50);
            CurrentGyYLbl.Text = "GyY: 0";
            CurrentGyYLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentGyYLbl);

            CurrentGyZLbl.Frame = new CoreGraphics.CGRect(555, 800, 200, 50);
            CurrentGyZLbl.Text = "GyZ: 0";
            CurrentGyZLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(CurrentGyZLbl);

            pitchChart.Frame = new CoreGraphics.CGRect(700, 155, 500, 250);
            pitchChart.BackgroundColor = UIColor.FromRGB(198, 200, 202);
            pitchLine = new LineChart { Entries = pitchChartData, AnimationDuration = new System.TimeSpan(1) };
            pitchChart.Chart = pitchLine;
            View.AddSubview(pitchChart);

            UILabel pitchChartLbl = new UILabel();
            pitchChartLbl.Frame = new CoreGraphics.CGRect(700, 125, 200, 50);
            pitchChartLbl.Text = "Pitch Plot:";
            pitchChartLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(pitchChartLbl);

            rollChart.Frame = new CoreGraphics.CGRect(700, 425, 500, 250);
            rollChart.BackgroundColor = UIColor.FromRGB(198, 200, 202);
            rollLine = new LineChart { Entries = rollChartData, AnimationDuration = new System.TimeSpan(1) };
            rollChart.Chart = rollLine;
            View.AddSubview(rollChart);

            UILabel rollChartLbl = new UILabel();
            rollChartLbl.Frame = new CoreGraphics.CGRect(700, 395, 200, 50);
            rollChartLbl.Text = "Roll Plot:";
            rollChartLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(rollChartLbl);

            yawChart.Frame = new CoreGraphics.CGRect(700, 695, 500, 250);
            yawChart.BackgroundColor = UIColor.FromRGB(198, 200, 202);
            yawLine = new LineChart { Entries = yawChartData, AnimationDuration = new System.TimeSpan(1) };
            yawChart.Chart = yawLine;
            View.AddSubview(yawChart);

            UILabel yawChartLbl = new UILabel();
            yawChartLbl.Frame = new CoreGraphics.CGRect(700, 665, 200, 50);
            yawChartLbl.Text = "Yaw Plot:";
            yawChartLbl.TextColor = UIColor.FromRGB(0, 0, 0);
            View.AddSubview(yawChartLbl);

            DataAcquisitionProgress.Frame = new CoreGraphics.CGRect(155, 665, 400, 50);
            DataAcquisitionProgress.Progress = 0;
            DataAcquisitionProgress.ProgressTintColor = UIColor.FromRGB(49, 81, 122);
            View.AddSubview(DataAcquisitionProgress);
            Console.WriteLine("Ready");


        }


        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }


        public void ButtonClearOutputEventHandler(object sender, EventArgs e)
        {
            UIConsole.Text = "";

            pitchChartData.Clear();
            pitchLine.Entries = pitchChartData;

            rollChartData.Clear();
            rollLine.Entries = rollChartData;

            yawChartData.Clear();
            yawLine.Entries = yawChartData;

            DataAcquisitionProgress.Progress = 0;
        }

        public void ButtonLaunchEventHandler(object sender, EventArgs e)
        {
            DataAcquisitionProgress.Progress += 0.01f;
            UIUpdateActive = false;
            Ranger.launch();
        }

        public void ButtonStatusEventHandler(object sender, EventArgs e)
        {
            Ranger.getRocketStatus();
        }

        public void ButtonTestEventHandler(object sender, EventArgs e)
        {
            Ranger.test();
        }

        public void updateUIValues(RocketTelemetry telemetry)
        {
            Invoke(new Action(() =>
            {
                DataAcquisitionProgress.Progress += 0.01f;

                UIConsole.Text += "\n" + telemetry.rawData;
                var parsedData = telemetry.rawData.Split(',');

                CurrentPitchLbl.Text = "Pitch: " + parsedData[1] + "°";
                CurrentRollLbl.Text = "Roll: " + parsedData[2] + "°";
                CurrentYawLbl.Text = "Yaw: " + parsedData[3] + "°";
                RunTimeLbl.Text = "Run Time: " + parsedData[0] + "ms";
                CurrentAcXLbl.Text = "AcX: " + parsedData[4] + "m/s^2";
                CurrentAcYLbl.Text = "AcY: " + parsedData[5] + "m/s^2";
                CurrentAcZLbl.Text = "AcZ: " + parsedData[6] + "m/s^2";
                CurrentGyXLbl.Text = "GyX: " + parsedData[7] + "";
                CurrentGyYLbl.Text = "GyY: " + parsedData[8] + "";
                CurrentGyZLbl.Text = "GyZ: " + parsedData[9] + "";

                pitchChartData.Add(new ChartEntry(float.Parse(parsedData[1]))
                {
                    Color = SKColor.Parse("#dc505f")
                });
                pitchLine.Entries = pitchChartData;
                rollChartData.Add(new ChartEntry(float.Parse(parsedData[2]))
                {
                    Color = SKColor.Parse("#73bf94")
                });
                rollLine.Entries = rollChartData;
                yawChartData.Add(new ChartEntry(float.Parse(parsedData[3]))
                {
                    Color = SKColor.Parse("#31517a")
                });
                yawLine.Entries = yawChartData;
                
            }),0);
        }





        private async void startPeriodicUIUpdate()
        {
            while (true && UIUpdateActive == true)
            {
                await Task.Delay(1000);
            }
        }
    }
}