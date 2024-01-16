using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformCanvasSample
{
    public class LineMinMax
    {
        public int Min { get; set; }

        public int Max { get; set; }
    }

    // WaveForm 데이터를 구성하고 있는 프로퍼티를 정의함
    public class WaveFormItem
    {
        public int IData { get; set; }

        public int QData { get; set; }

        public int? Marker1 { get; set; } = null;

        public int? Marker2 { get; set; } = null;
    }

    public class CapturedIQ
    {
        public double IData { get; set; }

        public double QData { get; set; }
    }

    // Wave Form File로부터 파싱한 헤더 및 데이터를 정의 함
    public class WaveForm
    {
        // Header의 존재 유무
        public bool IsHeaderExist { get; set; }

        // HZ
        public ESamplingRate SamplingRate { get; set; }

        // PARP 
        public double PAPR;

        // Marker1,2 data 존재 여부
        public bool IsExistMarker1 { get; set; }
        public bool IsExistMarker2 { get; set; }

        // 5G 여부
        public bool Is5GWaveform { get; set; }

        // 5G 주파수, HZ
        public long Frequency5G { get; set; }

        // IQ Data Length
        public long WaveformLength { get; set; }

        // WaveForm Data
        public List<WaveFormItem> WaveFormData { get; set; }

        // Waveform 모든 데이터 저장(사용하지 않음) 
        // public List<WaveFormItem> WaveFormDataAll { get; set; }

        public List<LineMinMax> MinMaxList_I { get; set; }
        public List<LineMinMax> MinMaxList_Q { get; set; }

        public WaveForm(bool IsNotData = false)
        {
            if (!IsNotData)
            {
                this.WaveFormData = new List<WaveFormItem>();
                // this.WaveFormDataAll = new List<WaveFormItem>();

                this.MinMaxList_I = new List<LineMinMax>();
                this.MinMaxList_Q = new List<LineMinMax>();
            }

            this.Initialize();
        }

        public void Initialize()
        {
            IsHeaderExist = false;
            SamplingRate = ESamplingRate._7530000;
            PAPR = 0;
            IsExistMarker1 = false;
            IsExistMarker2 = false;
            Is5GWaveform = false;
            Frequency5G = 0;
            WaveformLength = 0;

            if (WaveFormData != null)
            {
                WaveFormData.Clear();
            }

            if (MinMaxList_I != null)
            {
                MinMaxList_I.Clear();
            }

            if (MinMaxList_Q != null)
            {
                MinMaxList_Q.Clear();
            }
        }
    }

    public class CapturedData
    {
        // Header의 존재 유무
        public bool IsHeaderExist { get; set; }

        // HZ
        public ESamplingRate SamplingRate { get; set; }

        // PAPR 
        public long TotalSamplePoint;

        // I-Q 리스트
        public List<CapturedIQ> IQList;

        public CapturedData(bool isNotData = false)
        {
            if (!isNotData)
            {
                this.IQList = new List<CapturedIQ>();
            }
        }

        public void Initialize()
        {
            IsHeaderExist = false;
            this.SamplingRate = ESamplingRate._7530000;
            this.TotalSamplePoint = 0;
            if (this.IQList != null)
            {
                this.IQList.Clear();
            }
        }
    }

    public abstract class FileParserBase : IDisposable
    {
        protected StreamReader reader;

        // Header의 길이, 각 Parser마다 정해져있는 값이 있음(ex. Waveform 8, captued 3)
        protected int HeaderLength { get; set; }

        protected long TotalFileLength; // 전체 파일 길이를 자체적으로 읽어서 받아온 값
        protected long TotalFileLines;  // 전체 파일의 라인 수를 읽어서 가져온 값
        protected long MaxDataLength = 2457600;

        // 강제중지 요청 시 해당 플래그 false로 변경, Parsing 시작할 때 True로 시작
        public bool IsProgressFileParse = false;

        #region Events
        // 파일 읽기 진행에 따라 호출하는 이벤트 정의
        public delegate void FileReading(object sender, EventArgs e);
        public event FileReading FileReadingEvent;

        protected virtual void OnFileReading(object sender, EventArgs e)
        {
            FileReading handler = FileReadingEvent;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
        #endregion

        // 프로그레스를 컨트롤 함
        protected bool ControlProgress(int curLineNum)
        {
            try
            {
                // 너무 자주 호출되면 성능상의 문제가 생길 수 있으므로 
                // [현재 읽은 위치] / [전체] 의 값을 나눈 
                // int cntOfWaveformData = WaveFormComponent.WaveFormData.Count;
                long total = TotalFileLines;
                if (total > MaxDataLength + 10)
                {
                    total = MaxDataLength + 10;
                }

                var ONE_THOUSAND = 1000;
                var ONE_HUNDERED = 100;

                if (total / ONE_THOUSAND >= 1)
                {
                    if (curLineNum % (total / ONE_THOUSAND) == 1)
                    {
                        // Percentage 를 인자로 올려줌 
                        var progressPercentage = (int)((double)curLineNum * (double)ONE_HUNDERED / (double)(total + 1));

                        // 100 은 못채우게 하기
                        if (progressPercentage > 100)
                        {
                            return true;
                        }

                        OnFileReading(progressPercentage, null);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                return false;
            }
        }

        // 파일 전체 길이 받아옴
        public long GetFileTotalLength(string path)
        {
            return new FileInfo(path).Length;
        }

        public long GetFileLineLength(string path)
        {
            return File.ReadLines(path).LongCount();
        }

        public virtual void Initialize()
        {
            IsProgressFileParse = true;
        }

        public abstract object GetInfo();

        protected abstract bool ParseHeader(int lineNum, string strLine);

        // Read Stream, Get Line Number
        protected abstract int ParseHeader(StreamReader sReader);

        public virtual bool ReadFile(string path, bool bReadData = false)
        {
            this.Initialize();

            return true;
        }

        public abstract void ReadFileData(string strLine, int lineNum);

        public abstract void Dispose();
    }

    // Waveform File을 읽어서 파싱함
    public class WaveformFileParser : FileParserBase
    {
        private WaveForm WaveFormComponent;

        public WaveformFileParser()
        {
            WaveFormComponent = new WaveForm();
            HeaderLength = 8;
            // MaxDataLength = 20000;
        }

        public override void Initialize()
        {
            if (this.WaveFormComponent != null)
            {
                this.WaveFormComponent.Initialize();
            }
        }

        #region 주요 Object들에 대한 Getter
        // 화면엔 도시하기 위한 WaveForm Info를 반환함
        public override object GetInfo()
        {
            WaveForm waveForm = new WaveForm();

            if (this.WaveFormComponent.IsHeaderExist)
            {
                waveForm.IsHeaderExist = true;
                waveForm.SamplingRate = this.WaveFormComponent.SamplingRate;

                // Total sample points
                waveForm.WaveformLength = this.WaveFormComponent.WaveformLength;

                // PAPR, Header의 0.01을 곱한 값으로 표시하기 위함
                waveForm.PAPR = this.WaveFormComponent.PAPR;

                // 5G Waveform 추가
                waveForm.Frequency5G = this.WaveFormComponent.Frequency5G;
                waveForm.Is5GWaveform = this.WaveFormComponent.Is5GWaveform;
            }

            // 기본 값 반환 , 22. 12. 08 :: 기본 값 유효한 값으로 변경
            else
            {
                waveForm.SamplingRate = ESamplingRate._7530000; // Default는 122.88 Mhz로 적용

                // 파일 길이를 카운트 해서 표시
                waveForm.WaveformLength = this.TotalFileLines;

                // PAPR
                waveForm.PAPR = 0.00;
            }

            return waveForm;
        }

        public List<WaveFormItem> GetWaveformData()
        {
            return this.WaveFormComponent.WaveFormData;
        }

        public List<WaveFormItem> GetSampledData(int maxCount)
        {
            int length = this.WaveFormComponent.WaveFormData.Count;
            List<WaveFormItem> sampledList = new List<WaveFormItem>();

            for (int i = 0; i < maxCount; i++)
            {
                var iLength = (long)i * (long)length;
                int indexOfData = (int)(iLength / maxCount);
                var item = this.WaveFormComponent.WaveFormData[indexOfData];

                sampledList.Add(item);
            }

            return sampledList;
        }

        //public List<WaveFormItem> GetWaveformDataAll()
        //{
        //    return this.WaveFormComponent.WaveFormDataAll;
        //}

        public List<LineMinMax> GetI_MinMax()
        {
            return WaveFormComponent.MinMaxList_I;
        }

        public List<LineMinMax> GetQ_MinMax()
        {
            return WaveFormComponent.MinMaxList_Q;
        }
        #endregion

        public void ClearIQMinMaxList()
        {
            WaveFormComponent.MinMaxList_I.Clear();
            WaveFormComponent.MinMaxList_Q.Clear();
        }


        protected override int ParseHeader(StreamReader reader)
        {
            var lineNum = 0;

            float version = 0.1f;
            var readLine = reader.ReadLine(); lineNum++;// Read Line 1
            if (readLine.Contains("7FFF7FFF"))
            {
                WaveFormComponent.IsHeaderExist = true;
            }

            else
            {
                WaveFormComponent.IsHeaderExist = false;
                version = -1;
            }

            readLine = reader.ReadLine(); lineNum++; // Read Line2
            WaveFormComponent.SamplingRate = SamplingRateStrConverter.GetSamplingRate(readLine);

            readLine = reader.ReadLine(); lineNum++; // Read Line3 ...
            WaveFormComponent.PAPR = double.Parse(readLine) / 100.0;

            readLine = reader.ReadLine(); lineNum++;
            WaveFormComponent.IsExistMarker1 = Convert.ToBoolean(int.Parse(readLine));

            readLine = reader.ReadLine(); lineNum++;
            WaveFormComponent.IsExistMarker2 = Convert.ToBoolean(int.Parse(readLine));

            readLine = reader.ReadLine(); lineNum++;
            WaveFormComponent.Is5GWaveform = Convert.ToBoolean(int.Parse(readLine));

            var strFreq5G = reader.ReadLine(); lineNum++;
            WaveFormComponent.Frequency5G = Convert.ToInt64(strFreq5G, 16);

            // 23. 10. 10 LWY :: index 1씩 밀어서 5G Frequency 의 Last 32Bit \n First 32Bit 으로 파싱
            if (version >= 0.1f)
            {
                // String Append
                strFreq5G = reader.ReadLine() + strFreq5G; lineNum++;
                WaveFormComponent.Frequency5G = Convert.ToInt64(strFreq5G, 16);
            }

            readLine = reader.ReadLine(); lineNum++;
            WaveFormComponent.WaveformLength = Convert.ToInt32(readLine, 16);

            return lineNum;
        }

        string temp5gFreq = string.Empty;
        bool isOldVersion = false;
        protected override bool ParseHeader(int lineNum, string strLine)
        {
            bool isNextDatafield = false;
            switch (lineNum)
            {
                // Header Exist
                case 1:
                    if (strLine.Contains("7FFF7FFF"))
                    {
                        WaveFormComponent.IsHeaderExist = true;
                        isOldVersion = true;
                    }
                    else if (strLine.Contains("8FFF8FFF"))
                    {
                        WaveFormComponent.IsHeaderExist = true;
                    }
                    else
                    {
                        WaveFormComponent.IsHeaderExist = false;
                        isNextDatafield = true;
                    }
                    break;

                // Sampling Rate
                case 2:
                    WaveFormComponent.SamplingRate = SamplingRateStrConverter.GetSamplingRate(strLine);
                    break;

                // * x 100 의 값을 나타냄​
                case 3:
                    WaveFormComponent.PAPR = double.Parse(strLine) / 100.0;
                    break;

                case 4:
                    WaveFormComponent.IsExistMarker1 = Convert.ToBoolean(int.Parse(strLine));
                    break;

                case 5:
                    WaveFormComponent.IsExistMarker2 = Convert.ToBoolean(int.Parse(strLine));
                    break;

                case 6:
                    WaveFormComponent.Is5GWaveform = Convert.ToBoolean(int.Parse(strLine));
                    break;

                case 7:
                    // Hexa -> decimal
                    temp5gFreq = strLine;
                    WaveFormComponent.Frequency5G = Convert.ToInt64(strLine, 16);
                    break;

                // 23. 10. 10 LWY :: index 1씩 밀어서 5G Frequency 의 Last 32Bit \n First 32Bit 으로 파싱
                case 8:
                    if (isOldVersion)
                    {
                        // Hexa -> decimal
                        WaveFormComponent.WaveformLength = Convert.ToInt64(strLine, 16);
                    }

                    else
                    {
                        // WaveFormComponent.Frequency5G = Convert.ToInt64(strLine, 16);
                        WaveFormComponent.Frequency5G = Convert.ToInt64(strLine + temp5gFreq, 16);
                    }

                    break;

                case 9:
                    if (isOldVersion)
                    {
                        isNextDatafield = true;
                    }
                    else
                    {
                        // Hexa -> decimal
                        WaveFormComponent.WaveformLength = Convert.ToInt64(strLine, 16);
                    }
                    break;

                default:
                    isNextDatafield = true;
                    break;

            }
            return isNextDatafield;
        }

        // Waveform 파일을 읽어 Header와 Data를 파싱함
        public override bool ReadFile(string path, bool bReadData = false)
        {
            if (path == null)
            {
                return false;
            }
            // base.ReadFile(path, bReadData);
            IsProgressFileParse = true;
            ClearIQMinMaxList();
            if (tempItemList != null)
            {
                tempItemList.Clear();
            }
            try
            {
                using (reader = new StreamReader(path, Encoding.UTF8))
                {
                    bool isDataField = false;
                    int lineNum = 0;
                    string strLine = string.Empty;
                    this.TotalFileLength = GetFileTotalLength(path);
                    this.TotalFileLines = GetFileLineLength(path);
                    while ((strLine = reader.ReadLine()) != null)
                    {
                        // [22. 10. 26 LWY] :: 중지 요청 시 파일읽기 강제중지
                        if (!IsProgressFileParse)
                        {
                            break;
                        }

                        // 한 줄씩 읽을 때 마다 라인 수 증가
                        lineNum++;

                        // Header를 먼저 파싱
                        if (!isDataField)
                        {
                            isDataField = this.ParseHeader(lineNum, strLine);
                        }

                        // 여부 확인 후 Data 파싱
                        if (isDataField)
                        {
                            // Header만 읽도록 하는 경우 반복이탈
                            if (!bReadData)
                            {
                                break;
                            }

                            ReadFileData(strLine, lineNum);
                        }

                        // line number가 Header를 고려한 최대 데이터 길이를 넘어갈 경우에 더 이상 읽지 않음
                        if (lineNum > MaxDataLength + 9)
                        {
                            break;
                        }
                    } // end while ((strLine = reader.ReadLine()) != null)   ////////                       

                    // Header에 LineLength를 파싱하지 않았다면, 최종 line 수가 전체 waveform의 length임
                    if (WaveFormComponent.WaveformLength <= 0)
                    {
                        WaveFormComponent.WaveformLength = TotalFileLines;
                    }

                    // 22. 05. 31 :: 파일 모두 읽은 후 닫기 처리 추가, 계속 점유하는 문제 발생함
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif

                return false;
            }

            #region IQ Data csv 저장 부분
            //if (tempItemList != null)
            //{
            //    if (tempItemList.Count <= 0)
            //    {
            //        return false;
            //    }
            //}

            // csv 저장
            //string csvfullPath = Path.GetDirectoryName(Directory.GetCurrentDirectory()) + "\\" + "waveformIQ.csv";
            //var di = Directory.GetParent(csvfullPath);
            //if (!di.Exists) di.Create();
            //if (!File.Exists(csvfullPath))
            //{
            //    using (StreamWriter file = new StreamWriter(csvfullPath))
            //    {
            //        foreach (var item in tempItemList)
            //            file.WriteLine(item.IData + "," + item.QData);
            //    }
            //}
            #endregion

            return true;
        }

        List<WaveFormItem> tempItemList = new List<WaveFormItem>();
        public override void ReadFileData(string strLine, int lineNum)
        {
            try
            {
                byte[] bytes = new byte[32];
                WaveFormItem item = new WaveFormItem();
                string[] splitedStr = strLine.Split(' ');

                // Marker 여부 확인하고, 있을 시 marker 값 저장하기
                if (splitedStr.Length == 2)
                {
                    if (WaveFormComponent.IsExistMarker1)
                    {
                        item.Marker1 = int.Parse(splitedStr[1]);
                    }

                    if (WaveFormComponent.IsExistMarker2)
                    {
                        item.Marker2 = int.Parse(splitedStr[1]);
                    }
                }

                else if (splitedStr.Length == 3)
                {
                    item.Marker1 = int.Parse(splitedStr[1]);
                    item.Marker2 = int.Parse(splitedStr[2]);
                }

                #region read line을 string 변환, binary로 변환 후 앞,뒤 16bit씩 읽는 방식 (주석)
                //string strBytes = Common.HexToBinary(splitedStr[0]).PadLeft(32, '0'); // 32 bit, 앞쪽 비어있을 경우 0으로 채워주기 

                //// 앞 16자리, 뒤 16자리 구분하기
                //string str_I = strBytes.Substring(1, 15);
                //string str_Q = strBytes.Substring(17, 15);

                //int sign_I = 1;
                //int sign_Q = 1;

                //// 상위 비트가 1 이면 부호 변경할 것
                //// I의 부호 변경 여부 확인
                //if (strBytes[0] == '1')
                //{
                //    sign_I = -1;
                //}

                //// Q의 부호 변경 여부 확인
                //if (strBytes[16] == '1')
                //{
                //    sign_Q = -1;
                //}

                //var value_I = Convert.ToInt32(str_I, 2) * sign_I;
                //var value_Q = Convert.ToInt32(str_Q, 2) * sign_Q;

                //item.IData = value_I;
                //item.QData = value_Q;
                #endregion

                #region 생으로 hex 값을 dec로 변환하는 방식 
                // 그냥 hex 값 앞 네자리 뒷 네자리 파싱하는 방식
                string strHexaIQ = splitedStr[0].Trim().PadLeft(8, '0');

                // 앞 4자리, 뒤 4자리 구분하기
                string str_I = strHexaIQ.Substring(0, 4);
                string str_Q = strHexaIQ.Substring(4, 4);

                // hex -> dec
                var value_I = Convert.ToInt16(str_I, 16);
                var value_Q = Convert.ToInt16(str_Q, 16);

                // -4000 이하일 때 로그 찍어보기(주석)
                //if ( value_I < -4000 || value_Q < -4000)
                //{
                //    string strLog = string.Format("line num = {2, 6}, I = {0, 5}, Q = {1, 5}", value_I, value_Q, lineNum);
                //    Console.WriteLine(strLog);
                //}

                item.IData = value_I;
                item.QData = value_Q;
                #endregion

                // 23. 02. 10 LWY :: MAX Count 지정
                WaveFormComponent.WaveFormData.Add(item);

                //else
                //{
                //    long addInterval = this.TotalFileLines / (long)this.MaxDataLength;
                //    if (addInterval == 0)
                //    {
                //        addInterval = 1;
                //    }

                //    #region 일정 간격마다 저장, Filtering 방법 
                //    if (lineNum % addInterval == 0)
                //    {
                //        WaveFormComponent.WaveFormData.Add(item);
                //    }
                //}

                // 모든 Waveform 불러오는 것 삭제
                //WaveFormComponent.WaveFormDataAll.Add(item);

                #region Min Max 방법 (주석)
                //if (tempItemList.Count <= WaveformMaxCount)
                //{
                //    tempItemList.Add(item);
                //}
                //if (lineNum % addInterval == 0)
                //{
                //    WaveFormComponent.MinMaxList_I.Add(new LineMinMax()
                //    {
                //        Max = tempItemList.Max(x => x.IData),
                //        Min = tempItemList.Min(x => x.IData)
                //    });

                //    WaveFormComponent.MinMaxList_Q.Add(new LineMinMax()
                //    {
                //        Max = tempItemList.Max(x => x.QData),
                //        Min = tempItemList.Min(x => x.QData)
                //    });
                //    tempItemList.Clear();
                //}
                #endregion

                ControlProgress(lineNum);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public override void Dispose()
        {
            if (this.reader != null)
            {
                this.reader.Dispose();
            }

            if (this.tempItemList != null)
            {
                this.tempItemList.Clear();
            }

            if (this.WaveFormComponent != null)
            {
                this.WaveFormComponent.Initialize();
            }

            this.ClearIQMinMaxList();
        }
    }

    public class CapturedDataParser : FileParserBase
    {
        private CapturedData CapturedDataComponent;

        public CapturedDataParser()
        {
            CapturedDataComponent = new CapturedData();
            HeaderLength = 3;
            //  MaxDataLength = 20000; /// Captured Data의 Max Data Length 조정
        }

        public List<CapturedIQ> GetCapturedIQs()
        {
            return this.CapturedDataComponent.IQList;
        }

        private void ClearPrevData()
        {
            CapturedDataComponent.IQList.Clear();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (CapturedDataComponent != null)
            {
                CapturedDataComponent.Initialize();
            }
        }

        // 화면엔 도시하기 위한 WaveForm Info를 반환함
        public override object GetInfo()
        {
            CapturedData captuedData = new CapturedData();

            if (this.CapturedDataComponent.IsHeaderExist)
            {
                captuedData.SamplingRate = this.CapturedDataComponent.SamplingRate;

                // Total sample points
                captuedData.TotalSamplePoint = this.CapturedDataComponent.TotalSamplePoint;
            }

            // 기본 값 반환
            else
            {
                captuedData.SamplingRate = ESamplingRate.None;

                // 파일 길이를 카운트 해서 표시
                captuedData.TotalSamplePoint = this.TotalFileLines;
            }

            return captuedData;
        }

        protected override int ParseHeader(StreamReader reader)
        {
            var lineNum = 0;

            var readLine = reader.ReadLine(); lineNum++;// Read Line 1
            if (readLine.Contains("-999999"))
            {
                CapturedDataComponent.IsHeaderExist = true;
            }
            else
            {
                CapturedDataComponent.IsHeaderExist = false;
            }

            readLine = reader.ReadLine(); lineNum++;// Read Line2
            CapturedDataComponent.SamplingRate = SamplingRateStrConverter.GetSamplingRate(readLine);

            readLine = reader.ReadLine(); lineNum++;// Read Line3
            CapturedDataComponent.TotalSamplePoint = Convert.ToInt32(readLine, 16);

            return lineNum;
        }

        protected override bool ParseHeader(int lineNum, string strLine)
        {
            bool isNextDatafield = false;
            switch (lineNum)
            {
                // Header Exist
                case 1:
                    //  해당 값의 포함 여부로 변경
                    if (strLine.Contains("-999999"))
                    {
                        CapturedDataComponent.IsHeaderExist = true;
                    }
                    else
                    {
                        CapturedDataComponent.IsHeaderExist = false;
                        isNextDatafield = true;
                    }
                    break;

                // Sampling Rate
                case 2:
                    CapturedDataComponent.SamplingRate = SamplingRateStrConverter.GetSamplingRate(strLine);
                    break;

                case 3:
                    CapturedDataComponent.TotalSamplePoint = Convert.ToInt32(strLine, 16);
                    isNextDatafield = true; // 마지막 Header Line 이후에는 모두 Data Field로 취급하기 위한 플래그 변경
                    break;

                default:
                    isNextDatafield = true;
                    break;
            }
            return isNextDatafield;
        }

        public override bool ReadFile(string path, bool bReadData = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!File.Exists(path))
            {
                return false;
            }

            base.ReadFile(path, bReadData);
            ClearPrevData();

            try
            {
                using (reader = new StreamReader(path, Encoding.UTF8))
                {
                    bool isDataField = false;
                    int lineNum = 0;
                    string strLine = string.Empty;
                    this.TotalFileLength = GetFileTotalLength(path);
                    this.TotalFileLines = GetFileLineLength(path);
                    while ((strLine = reader.ReadLine()) != null)
                    {
                        if (!IsProgressFileParse)
                        {
                            break;
                        }

                        // 한 줄씩 읽을 때 마다 라인 수 증가
                        lineNum++;

                        // Header 파싱
                        if (!isDataField)
                        {
                            isDataField = this.ParseHeader(lineNum, strLine);
                            continue;
                        }

                        // Data Field 여부 확인 후 Data 파싱
                        if (bReadData && isDataField)
                        {
                            ReadFileData(strLine, lineNum);
                        }

                        // line number가 Header를 고려한 최대 데이터 길이를 넘어갈 경우에 더 이상 읽지 않음
                        if (lineNum > MaxDataLength + 3)
                        {
                            break;
                        }
                    } // end while

                    reader.Close();
                } // end reader new Stream
            }
            catch (IOException e)
            {
                Console.WriteLine("File Read Exception : " + e.ToString());
                return false;
            }

            return true;
        }

        public override void ReadFileData(string strLine, int lineNum)
        {
            try
            {
                string[] splitedStr = strLine.Split(' ');

                // Split결과 Length 2가 아닌 경우에 대한 예외처리
                if (splitedStr.Length != 2)
                {
                    return;
                }
                string str_i = splitedStr[0].Trim().Replace("​", "");   // .Replace(".", ",")
                string str_q = splitedStr[1].Trim().Replace("​", ""); ;   // .Replace(".", ",")

                CapturedIQ item = new CapturedIQ();
                item.IData = double.Parse(str_i);
                item.QData = double.Parse(str_q);
                //item.IData = Convert.ToDouble(str_i);
                //item.QData = Convert.ToDouble(str_q);

                // 전체 파일 길이에서 지정된 Max DataLength 만큼 나눈 Collection 추가 간격을 부여하여
                //long addInterval = this.TotalFileLines / (long)this.MaxDataLength;
                //if (addInterval == 0)
                //{
                //    addInterval = 1;
                //}

                // IQ List에 데이터를 추가함
                #region 일정 간격마다 저장, Filtering 방법
                //if (lineNum % addInterval == 0)
                //{
                //    CapturedDataComponent.IQList.Add(item);
                //}
                // 23. 02. 10 LWY :: MAX Count 지정

                CapturedDataComponent.IQList.Add(item);
                #endregion

                ControlProgress(lineNum);
            }

            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public async Task<List<CapturedIQ>> GetRawIQsAsync(string filePath)
        {
            // IQ Data를 저장할 List
            List<CapturedIQ> list = new List<CapturedIQ>();

            var task = new Task(() =>
            {
                // Read File
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    string strLine = string.Empty;
                    int i = 0;
                    int MAX = 2457600;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        string[] splitedStr = strLine.Split(' ');

                        // Length 1보다 적은 경우 I, Q 데이터가 Pair로 재대로 들어가지 않았다는 것
                        int strLength = splitedStr.Length;
                        if (strLength <= 1)
                        {
                            continue;
                        }

                        string str_I = splitedStr[0];
                        string str_Q = splitedStr[strLength - 1];

                        var value_I = double.Parse(str_I);
                        var value_Q = double.Parse(str_Q);

                        list.Add(new CapturedIQ() { IData = value_I, QData = value_Q });

                        // 최대 제한 넘어가면 break
                        if (i++ >= MAX)
                        {
                            break;
                        }
                    } // end while ((strLine = sr.ReadLine()) != null)
                    sr.Close();
                } // end using (var sr = new StreamReader(filePath, Encoding.UTF8))
            });
            task.Start();
            await task;
            return list;
        }

        public override void Dispose()
        {
            if (this.reader != null)
            {
                this.reader.Dispose();
            }

            if (this.CapturedDataComponent != null)
            {
                this.CapturedDataComponent.Initialize();
            }
        }
    }
}
