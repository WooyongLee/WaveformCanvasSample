using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformCanvasSample
{
    public class WaveformCanvasVM : ViewModelBase
    {
        private int width;
        private double highValue;
        private double midValue;
        private double lowValue;

        public int Width
        {
            get { return width; }
            set { width = value; NotifyPropertyChanged("Width"); }
        }

        public double HighValue
        {
            get { return highValue; }
            set { highValue = value; NotifyPropertyChanged("HighValue"); }
        }

        public double MidValue
        {
            get { return midValue; }
            set { midValue = value; NotifyPropertyChanged("MidValue"); }
        }

        public double LowValue
        {
            get { return lowValue; }
            set { lowValue = value; NotifyPropertyChanged("LowValue"); }
        }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged 구현부
        public event PropertyChangedEventHandler PropertyChanged;

        // 각 Property 이름으로 지정해 놓고 UI 쪽으로 변경에 대한 이벤트 구현
        protected void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
