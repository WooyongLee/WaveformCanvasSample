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

namespace WaveformCanvasSample
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SizeChanged += MainWindow_SizeChanged;
        }

        private void LoadWfButton_Click(object sender, RoutedEventArgs e)
        {
            this.Clear();

            waveformCanvas_I.HighValue = 16000;
            waveformCanvas_I.MidValue= 0;
            waveformCanvas_I.LowValue= -16000;

            string fullPath = "LTE_DL_10MHz_QPSK.wf";

            FileParserBase FileParser = new WaveformFileParser();
            bool bReasSuccess = FileParser.ReadFile(fullPath, true);

            if (!bReasSuccess)
            {
                Console.WriteLine("File Read Error");
                return;
            }

            if (FileParser is WaveformFileParser)
            {
                WaveformFileParser waveParser = (WaveformFileParser)FileParser;

                #region 전체를 샘플링 해서 그리는 방법
                var waveData = waveParser.GetSampledData(10000);

                // Extract IData values into a new list
                List<int> idataList = waveData.Select(waveFormItem => waveFormItem.IData).ToList();

                Console.WriteLine(" MaxValue (Waveform) = " + waveData.Max(x => x.IData));

                // Draw on Canvas
                // waveformCanvas_I.OnWaveShow(waveData);

                waveformView.MinY = -16000;
                waveformView.MaxY = 16000;

                // Draw on GL Control
                waveformView.OnWaveShow(idataList);

                waveData.Clear();
                #endregion
            }
        }

        private void LoadCfButton_Click(object sender, RoutedEventArgs e)
        {
            this.Clear();

            waveformCanvas_I.HighValue = 1;
            waveformCanvas_I.MidValue = 0;
            waveformCanvas_I.LowValue = -1;

            string fullPath = "LTE_3072_10ms_10db.cf";

            FileParserBase FileParser = new CapturedDataParser();
            bool bReasSuccess = FileParser.ReadFile(fullPath, true);

            if (!bReasSuccess)
            {
                Console.WriteLine("File Read Error");
                return;
            }

            if (FileParser is CapturedDataParser)
            {
                CapturedDataParser capturedDataParser = (CapturedDataParser)FileParser;

                var capturedData = capturedDataParser.GetCapturedIQs();

                // Extract IData values into a new list
                List<double> idataList = capturedData.Select(capturedIQ => capturedIQ.IData).ToList();

                Console.WriteLine(" MaxValue (I/Q Captured) = " + capturedData.Max(x => x.IData));

                waveformView.MinY = -1;
                waveformView.MaxY = 1;

                waveformView.OnWaveShow(idataList);

                capturedData.Clear();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.Clear();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Size Changed 구현, Main Control의 Width, Height, Actual Width, Height변경
            var width = ActualWidth - 180;
            var height = ActualHeight - 600;

            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            waveformView.Width = width;
            waveformView.Height = height;

            waveformView.CurrentControlHeight = ActualHeight - 600;
            waveformView.CurrentControlWidth = ActualWidth - 180;

            Dispatcher.BeginInvoke(new Action(() => 
            {
                WindowWidthHeightTextBlock.Text = "current width = " + ActualWidth + ", height = " + ActualHeight;
            }));
            Console.WriteLine("current width = " + ActualWidth + ", height = " + ActualHeight);
        }

        private void Clear()
        {
            waveformCanvas_I.OnWaveShow(new List<WaveFormItem>());
            waveformView.OnWaveShow(new List<double>());
        }
    }
}
