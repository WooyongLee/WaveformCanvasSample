using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WaveformCanvasSample
{
    /// <summary>
    /// WaveformCanvas.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WaveformCanvas : UserControl
    {
        WaveformCanvasVM vm = new WaveformCanvasVM();

        #region Dependency Properties
        public static readonly DependencyProperty HighValueProperty = DependencyProperty.Register("HighValue", typeof(double), typeof(WaveformCanvas),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHighValueChanged)));

        public static readonly DependencyProperty MidValueProperty = DependencyProperty.Register("MidValue", typeof(double), typeof(WaveformCanvas),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnMidValueChanged)));

        public static readonly DependencyProperty LowValueProperty = DependencyProperty.Register("LowValue", typeof(double), typeof(WaveformCanvas),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnLowValueChanged)));

        private static void OnHighValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WaveformCanvas uc = d as WaveformCanvas;
            uc.vm.HighValue = (double)e.NewValue;
        }

        private static void OnMidValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WaveformCanvas uc = d as WaveformCanvas;
            uc.vm.MidValue = (double)e.NewValue;
        }

        private static void OnLowValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WaveformCanvas uc = d as WaveformCanvas;
            uc.vm.LowValue = (double)e.NewValue;
        }

        public double HighValue
        {
            get { return (double)GetValue(WaveformCanvas.HighValueProperty); }
            set { SetValue(WaveformCanvas.HighValueProperty, value); }
        }

        public double MidValue
        {
            get { return (double)GetValue(WaveformCanvas.MidValueProperty); }
            set { SetValue(WaveformCanvas.MidValueProperty, value); }
        }

        public double LowValue
        {
            get { return (double)GetValue(WaveformCanvas.LowValueProperty); }
            set { SetValue(WaveformCanvas.LowValueProperty, value); }
        }

        #endregion

        public WaveformCanvas()
        {
            InitializeComponent();
        }
        // WaveForm Data를 통한 Wave 도시
        public void OnWaveShow(List<WaveFormItem> data)
        {
            // 23. 10. 10 LWY :: Right Margin에 대해 데이터 x값도 영향받도록 수정
            double xOffset = 40;
            double yOffset = 20;

            // I 또는 Q 데이터
            var waveForms = (List<WaveFormItem>)data;
            var length = data.Count;

            double canvasWidth = OuterCanvas.ActualWidth;
            double canvasHeight = OuterCanvas.ActualHeight;

            int waveformCount = waveForms.Count;

            double xScale = (OuterCanvas.ActualWidth - 10) / (double)waveformCount;
            double yScale = canvasHeight / ((vm.HighValue - vm.LowValue) * 1.26);

            int x = 0;
            double yMedium = -vm.LowValue;

            PointCollection collection = new PointCollection();

            foreach (var item in waveForms)
            {
                x++;

                double y = canvasHeight - ((item.IData + yMedium) * yScale + yOffset);
                collection.Add(new Point((double)x * xScale + xOffset, y));

                // 일정 Count 만 도시하도록 함(너무 많은 범위에서 도시하는데 문제 생김)
                if (x > waveformCount)
                {
                    break;
                }
            } // end foreach (var item in waveForms)

            Dispatcher.Invoke(new Action(() =>
            {
                if (this.InnerCanvas.Children != null)
                {
                    this.InnerCanvas.Children.Clear();

                    Polyline polyLine = MakePolylineFromMultiPoint(collection);
                    this.InnerCanvas.Children.Add(polyLine);
                }
            }));
        }

        private Polyline MakePolylineFromMultiPoint(PointCollection collection)
        {
            Polyline line = new Polyline();
            line.Points = collection;
            line.Stroke = new SolidColorBrush(Colors.DarkBlue);
            line.StrokeThickness = 1.3;
            return line;
        }
    }
}
