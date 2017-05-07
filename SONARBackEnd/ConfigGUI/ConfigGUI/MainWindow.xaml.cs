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
using System.Diagnostics;

namespace ConfigGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string lf;
        string frInc;


        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            //System.IO.StreamWriter configFile = new System.IO.StreamWriter("../../../../x64/Debug/UserParameters.txt");
            System.IO.StreamWriter configFile = new System.IO.StreamWriter("./UserParameters.txt");
            if (Int32.Parse(lowFreq.Text) < 0)
            {
                lowFreq.Text = "0";
            }
            if(Int32.Parse(lowFreq.Text) > 600)
            {
                lowFreq.Text = "600";
            }

            if (Double.Parse(freqInc.Text) < 0.0)
            {
                freqInc.Text = "0.0";
            }
            if (Double.Parse(freqInc.Text) > 3.0)
            {
                freqInc.Text = "3.0";
            }

            if (Int32.Parse(horizSteps.Text) < 0)
            {
                horizSteps.Text = "0";
            }
            if (Int32.Parse(horizSteps.Text) > 100)
            {
                horizSteps.Text = "100";
            }

            if (Int32.Parse(stepTime.Text) < 0)
            {
                stepTime.Text = "0";
            }
            if (Int32.Parse(stepTime.Text) > 300)
            {
                stepTime.Text = "100";
            }

            if (Int32.Parse(audiospread.Text) < 0)
            {
                audiospread.Text = "0";
            }
            if (Int32.Parse(audiospread.Text) > 180)
            {
                audiospread.Text = "100";
            }

            if (Double.Parse(volrolloff.Text) < 0.0)
            {
                volrolloff.Text = "0.0";
            }
            if (Double.Parse(volrolloff.Text) > 2.0)
            {
                volrolloff.Text = "2.0";
            }

            configFile.WriteLine(lowFreq.Text);
            configFile.WriteLine(freqInc.Text);
            configFile.WriteLine(horizSteps.Text);
            configFile.WriteLine(stepTime.Text);
            configFile.WriteLine(audiospread.Text);
            configFile.WriteLine(volrolloff.Text);
            //Trace.WriteLine(lf);
            configFile.Close();

        }

        private void compile_Click(object sender, RoutedEventArgs e)
        {
            string cmd = "C:/SONARmaster/SONARBackEnd/SONARBackEnd.sln";
            System.Diagnostics.Process.Start("Msbuild.exe", cmd);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = sender as Slider;

            switch(s.Name)
            {
                case "fslider":
                    lowFreq.Text = Convert.ToInt32(s.Value).ToString();
                    break;
                case "incslider":
                    freqInc.Text = Math.Round(s.Value,2).ToString();
                    break;
                case "hstepslider":
                    horizSteps.Text = Convert.ToInt32(s.Value).ToString();
                    break;
                case "stimeslider":
                    stepTime.Text = Convert.ToInt32(s.Value).ToString();
                    break;
                case "audiospreadslider":
                    audiospread.Text = Convert.ToInt32(s.Value).ToString();
                    break;
                case "volrolloffslider":
                    volrolloff.Text = Math.Round(s.Value, 2).ToString();
                    break;
                default:
                    break;
            }
        }

        //private void recompile_Click(object sender, RoutedEventArgs e)
        //{
        //    submit_Click(sender,e);
        //    Debug.WriteLine(System.IO.Directory.GetCurrentDirectory());
        //    var p = new Process();
        //    string dir = string.Format(@"..\..\..\..\SONARBackEnd.sln");
        //   // string s = "C:\SONARmaster\SONARBackEnd\ConfigGUI\ConfigGUI\bin\Debug";
        //    p.StartInfo = new ProcessStartInfo(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe");
        //    // p.StartInfo.Arguments = string.Format(@"C:/SONARmaster/SONARBackEnd/SONARBackEnd.sln");
        //    p.StartInfo.Arguments = dir;
        //    p.Start();
        //   // System.Diagnostics.Process.Start("Msbuild.exe", cmd);
        //}
    }
}
