using GLGraphLib;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace WaveformCanvasSample
{
    /// <summary>
    /// WaveformView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WaveformView : UserControl
    {
        #region Properties
        /// <summary>
        /// Sample Load 여부
        /// </summary>
        public bool IsLoadSample
        {
            get { return (bool)GetValue(IsLoadSampleProperty); }
            set { SetValue(IsLoadSampleProperty, value); }
        }

        /// <summary>
        /// Minumum X Axis Value
        /// </summary>
        public double MinX
        {
            get { return (double)GetValue(MinXProperty); }
            set { SetValue(MinXProperty, value); }
        }

        /// <summary>
        /// Minumum Y Axis Value
        /// </summary>
        public double MinY
        {
            get { return (double)GetValue(MinYProperty); }
            set { SetValue(MinYProperty, value); }
        }

        /// <summary>
        /// Maximum X Axis Value
        /// </summary>
        public double MaxX
        {
            get { return (double)GetValue(MaxXProperty); }
            set { SetValue(MaxXProperty, value); }
        }

        /// <summary>
        /// Maximum Y Axis Value
        /// </summary>
        public double MaxY
        {
            get { return (double)GetValue(MaxYProperty); }
            set { SetValue(MaxYProperty, value); }
        }

        /// <summary>
        /// 표현할 Constellation 행의 수
        /// </summary>
        public int NumOfRow
        {
            get { return (int)GetValue(NumOfRowProperty); }
            set { SetValue(NumOfRowProperty, value); }
        }

        /// <summary>
        /// 표현할 Constellation 열의 수
        /// </summary>
        public int NumOfColumn
        {
            get { return (int)GetValue(NumOfColumnProperty); }
            set { SetValue(NumOfColumnProperty, value); }
        }

        /// <summary>
        /// 수평 패딩
        /// </summary>
        public float PaddingHorizontal
        {
            get { return (float)GetValue(PaddingHorizontalProperty); }
            set { SetValue(PaddingHorizontalProperty, value); }
        }

        /// <summary>
        /// 수직 패딩
        /// </summary>
        public float PaddingVertical
        {
            get { return (float)GetValue(PaddingVerticalProperty); }
            set { SetValue(PaddingVerticalProperty, value); }
        }

        /// <summary>
        /// 현재 컨트롤의 너비
        /// </summary>
        public double CurrentControlWidth
        {
            get { return (double)GetValue(CurrentControlWidthProperty); }
            set { SetValue(CurrentControlWidthProperty, value); }
        }

        /// <summary>
        /// 현재 컨트롤의 높이
        /// </summary>
        public double CurrentControlHeight
        {
            get { return (double)GetValue(CurrentControlHeightProperty); }
            set { SetValue(CurrentControlHeightProperty, value); }
        }

        public RGBcolor BackgroundColor
        {
            get { return (RGBcolor)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public RGBcolor AxisColor
        {
            get { return (RGBcolor)GetValue(AxisColorProperty); }
            set { SetValue(AxisColorProperty, value); }
        }
        #endregion

        #region Define DependencyProperty from Properties
        public static readonly DependencyProperty IsLoadSampleProperty = DependencyProperty.Register(
            "IsLoadSample",
            typeof(bool),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty MinXProperty = DependencyProperty.Register(
            "MinX",
            typeof(double),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty MinYProperty = DependencyProperty.Register(
            "MinY",
            typeof(double),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty MaxXProperty = DependencyProperty.Register(
            "MaxX",
            typeof(double),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty MaxYProperty = DependencyProperty.Register(
            "MaxY",
            typeof(double),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty NumOfRowProperty = DependencyProperty.Register(
            "NumOfRow",
            typeof(int),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty NumOfColumnProperty = DependencyProperty.Register(
            "NumOfColumn",
            typeof(int),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty PaddingHorizontalProperty = DependencyProperty.Register(
            "PaddingHorizontal",
            typeof(float),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty PaddingVerticalProperty = DependencyProperty.Register(
            "PaddingVertical",
            typeof(float),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty CurrentControlWidthProperty = DependencyProperty.Register(
            "CurrentControlWidth",
            typeof(double),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty CurrentControlHeightProperty = DependencyProperty.Register(
            "CurrentControlHeight",
            typeof(double),
            typeof(WaveformView),
            null
            );

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            "BackgroundColor",
            typeof(RGBcolor),
            typeof(WaveformView),
            null
        );

        public static readonly DependencyProperty AxisColorProperty = DependencyProperty.Register(
            "AxisColor",
            typeof(RGBcolor),
            typeof(WaveformView),
            null
        );
        #endregion

        // binding data
        List<double> data = new List<double>();
        private int dataLength { get { return data.Count; } }

        object dataLockObject = new object();

        public WaveformView()
        {
            InitializeComponent();

            // Set fixed Properties
            BackgroundColor = new RGBcolor(1.0f, 1.0f, 1.0f);
            CurrentControlWidth = 1000;
            CurrentControlHeight = 120;
            
            this.openGLControl.OpenGLDraw += OpenGLControl_OpenGLDraw;
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            OpenGL gl = openGLControl.OpenGL;

            // Set the clear color and clear the color buffer and depth buffer
            gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // Set up the projection matrix (시점 좌표를 설정)
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            // 투상 좌표를 정의 right = width, top = height, near = -1, far = 1
            gl.Ortho(0, CurrentControlWidth, 0, CurrentControlHeight, -1, 1);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();

            DrawAxis(gl);

            DrawData(gl);

            DrawHorizontalAxis(gl);

            gl.PopMatrix();

            // Flush OpenGL
            gl.Flush();
        }

        private void DrawHorizontalAxis(OpenGL gl)
        {
            var LeftMargin = PaddingHorizontal + 2;

            // Draw horizontal center line
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(LeftMargin, CurrentControlHeight / 2);
            gl.Vertex(CurrentControlWidth - LeftMargin / 2, CurrentControlHeight / 2);
            gl.End();
        }

        // 축을 그림
        private void DrawAxis(OpenGL gl)
        {
            // Draw outer rectangular boundary with a margin of 1
            int BoundaryMargin = 1;
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.Begin(OpenGL.GL_LINE_LOOP);
            gl.Vertex(BoundaryMargin, BoundaryMargin);
            gl.Vertex(CurrentControlWidth - BoundaryMargin, BoundaryMargin);
            gl.Vertex(CurrentControlWidth - BoundaryMargin, CurrentControlHeight - BoundaryMargin);
            gl.Vertex(BoundaryMargin, CurrentControlHeight - BoundaryMargin);
            gl.End();

            var LeftMargin = PaddingHorizontal + 2;
            var VerticalMargin = 8;

            // Draw vertical line at the left end
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(LeftMargin, CurrentControlHeight - VerticalMargin);
            gl.Vertex(LeftMargin, VerticalMargin);
            gl.End();

            string FONT = "Microsoft Sans Serif";
            int TextMargin = 22;

            // Draw text at the top (Max)
            gl.DrawText((int)(LeftMargin - TextMargin), (int)(CurrentControlHeight- TextMargin / 2), 0.0f, 0.0f, 0.0f, FONT, 10.0f, string.Format("{0,5:N0}", MaxY));

            // Draw text at the middle (Mid)
            gl.DrawText((int)(LeftMargin - TextMargin), (int)(CurrentControlHeight / 2), 0.0f, 0.0f, 0.0f, FONT, 10.0f, string.Format("{0,5:N0}", (MinY +MaxY)));

            // Draw text at the bottom (Min)
            gl.DrawText((int)(LeftMargin - TextMargin), (int)(LeftMargin - TextMargin), 0.0f, 0.0f, 0.0f, FONT, 10.0f, string.Format("{0,5:N0}", MinY));
        }

        // 데이터를 도시함
        private void DrawData(OpenGL gl)
        {
            // Draw Line from points
            gl.Begin(OpenGL.GL_LINE_STRIP);
            gl.Color(0.0f, 0.4f, 0.9f);

            for (int i = 0; i < dataLength; i++)
            {
                float currentX = GetScreenX(i, dataLength, CurrentControlWidth, PaddingHorizontal);
                float currentY = GetScreenY(data[i], MinY, MaxY, CurrentControlHeight, PaddingVertical);

                // Set Vertex from current x, y
                gl.Vertex(currentX, currentY, 0);
            }

            gl.End();
        }

        public void OnWaveShow<T>(List<T> data) where T : IConvertible
        {
            lock (dataLockObject)
            {
                this.data.Clear();

                var count = data.Count;
                // Attempt to convert each element to double
                for (int i = 0; i < count; i++)
                {
                    // Attempt to convert each element to double
                    if (double.TryParse(data[i].ToString(), out double convertedValue))
                    {
                        this.data.Add(convertedValue);
                    }
                    else
                    {
                        // Handle the case where the conversion fails, or take alternative action
                        // For example, you could skip the element, log a warning, etc.
                        // this.data.Add(0.0); // Add a default value or handle the failure appropriately
                    }
                }
            }
        }

        public void OnWaveShow(List<WaveFormItem> data)
        {
            lock (dataLockObject)
            {
                this.data.Clear();

                var count = data.Count;
                for (int i = 0; i < count; i++)
                {
                    this.data.Add(data[i].IData);
                }
            }
        }

        public void OnWaveShow(List<CapturedIQ> data)
        {
            lock (dataLockObject)
            {
                this.data.Clear();

                var count = data.Count;
                for (int i = 0; i < count; i++)
                {
                    this.data.Add(data[i].IData);
                }
            }
        }

        // Data Index를 통해서 Screen X 좌표를 구함
        public static float GetScreenX(int xValue, int totalLength, double ControlWidth, double PaddingHorizontal)
        {
            return (float)((double)xValue / totalLength * (ControlWidth - PaddingHorizontal * 1.5)) + (float)PaddingHorizontal + 3;
        }

        // Data Index를 통해서 Screen Y 좌표를 구함
        public static float GetScreenY(double yValue, double minY, double maxY, double ControlHeight, double PaddingVertical)
        {
            // y value가 적정범위일 때
            if (yValue < minY)
            {
                return (float)PaddingVertical;
            }

            else if (yValue > maxY)
            {
                return (float)(ControlHeight - PaddingVertical);
            }

            else
            {
                return (float)((yValue - minY) / (maxY - minY) * (ControlHeight - PaddingVertical * 2.0)) + (float)PaddingVertical;
            }
        }
    }
}
