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
            System.IO.StreamWriter configFile = new System.IO.StreamWriter("../../../../x64/Debug/UserParameters.txt");
            configFile.WriteLine(lowFreq.Text);
            configFile.WriteLine(freqInc.Text);
            configFile.WriteLine(horizSteps.Text);
            configFile.WriteLine(stepTime.Text);
            //Trace.WriteLine(lf);
            configFile.Close();

        }

        private void compile_Click(object sender, RoutedEventArgs e)
        {
            string cmd = "C:/SONARmaster/SONARBackEnd/SONARBackEnd.sln";
            System.Diagnostics.Process.Start("msbuild", cmd);
        }
    }
}
