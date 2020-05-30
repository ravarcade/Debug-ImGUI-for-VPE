using System.Diagnostics;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI.Tools
{

    internal class ChartFloat
    {
        float[] _data;
        int _idx;
        int _size;
        float _max;
        float _min;
        int _height;
        bool _checkMax;
        float _treshold;
        public float val { get
            {
                return _data[_idx];
            } }

        public ChartFloat(int size, float min, float max, int height)
        {
            _size = size;
            _data = new float[size * 2];
            _idx = 0;
            _min = min;
            _max = max;
            _height = height;
            _checkMax = false;
            _treshold = 0.5f;
        }

        public void Add(float val)
        {
            ++_idx;
            if (_idx >= _size)
                _idx -= _size;

            float _tmax = _max * _treshold;
            _checkMax = val < _tmax && _data[_idx] < _tmax;

            _data[_idx] = _data[_idx + _size] = val;

            // update min/max on chart
            if (val > _max)
                _max = val;

            if (val < _min)
                _min = val;
        }

        public void Draw(string label, string overlay_text, string precision)
        {
            if (_checkMax)
            {
                _checkMax = false;
                float tmax = _data[_idx];
                for (int i = _idx + 1; i < _size; ++i)
                {
                    if (tmax < _data[_idx])
                        tmax = _data[_idx];
                }
                if (tmax < _max * _treshold)
                    _max = tmax;
            }
            
            if (precision != null)
                overlay_text += val.ToString(precision);

            ImGui.PlotLines(label, ref _data[_idx + 1], _size, 0, overlay_text, _min, _max, new System.Numerics.Vector2(0, _height));
        }

        public void Draw(string label, string overlay_text)
        {
            Draw(label, overlay_text, null);
        }
    };

    struct QueueBuffer<T>
    {
        T[] _buf;
        int _head;
        int _tail;
        int _used;
        int _bufSize;

        public int size { get => _used; }

        public QueueBuffer(int size)
        {
            _buf = new T[size];
            _head = 0;
            _tail = size-1;
            _used = 0;
            _bufSize = size;
        }

        public void Push(T val)
        {            
            ++_tail;
            if (_tail >= _bufSize)
                _tail = 0;

            _buf[_tail] = val;
            
            // check if we overwrited 
            if (_used < _bufSize)
            {
                ++_used;
            } else
            {
                ++_head;
                if (_head >= _bufSize)
                    _head = 0;
            }
        }

        public void Pop()
        {
            if (_used > 0)
            {
                --_used;
                ++_head;
                if (_head >= _bufSize)
                    _head = 0;
            }
        }

        public bool isEmpty { get { return _used == 0; } }
        public T front {  get { return _buf[_head]; } }
        public T back {  get { return _buf[_tail]; } }
    }

    struct FPSHelper
    {
        private bool _drawChart;
        private string _precision;
        private int _frames;
        private float _val;
        
        private ChartFloat _chart;
        private Stopwatch _watch;
        internal struct TickData
        {
            public double time;
            public int frame;
            public TickData(double t, int fr)
            {
                time = t;
                frame = fr;
            }
        }
        private QueueBuffer<TickData> _ticks;

        private double _now { get => _watch.Elapsed.TotalSeconds; }
        public int count { get => _frames; }

        public FPSHelper(bool drawChart, float min, float max, string precision = "n0", int ticksBufSize = 60)
        {
            _drawChart = drawChart;
            _precision = precision;
            _frames = 0;
            _val = 0;
            _ticks = new QueueBuffer<TickData>(ticksBufSize);
            _chart = new ChartFloat(100, min, max, 50);
            _watch = new Stopwatch();
            _watch.Start();
            
        }

        public void Tick(int numTicks = 1)
        {
            _frames += numTicks;
            _ticks.Push(new TickData(_now, _frames));
        }

        private void _Update()
        {
            double now = _now;
            while (_ticks.size > 2 && (now - 1.0) > _ticks.front.time) // remove older than 1 second values
                _ticks.Pop();

            if (_ticks.size >= 2)
            {
                _val = (float)((double)(_ticks.back.frame - _ticks.front.frame) / (_ticks.back.time - _ticks.front.time));
                if (_drawChart)
                    _chart.Add(_val);
            }
        }

        public void Draw(string label)
        {
            _Update();
            if (_drawChart)
            {
                _chart.Draw("", label + _val.ToString(_precision));
            }
            else
            {
                ImGui.Text(label + _val.ToString(_precision));
            }
        }
    }    
}