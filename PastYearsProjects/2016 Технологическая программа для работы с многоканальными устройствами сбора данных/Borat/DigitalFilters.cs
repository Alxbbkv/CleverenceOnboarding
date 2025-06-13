using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borat
{
    public abstract class DigitalFilter
    {
        protected DigitalFilter()
        {
            Capacity = 50;
        }

        protected int _cursor;

        protected int _capacity;

        public int Capacity
        {
            get { return _capacity; }
            set
            {
                _capacity = value;
                data = new int[_capacity];
                _cursor = _capacity;
            }
        }

        protected int[] data;

        public int PutGet(int recentDatum)
        {
            Put(recentDatum);
            return Calculate();
        }

        public void Put(int recentDatum)
        {
            _cursor = GetNextCursor(_cursor);
            data[_cursor] = recentDatum;
        }

        protected int GetNextCursor(int curCursor)
        {
            curCursor++;
            if (curCursor >= _capacity) curCursor = 0;
            return curCursor;
        }

        protected int GetPredCursor(int curCursor)
        {
            curCursor--;
            if (curCursor < 0) curCursor = _capacity - 1;
            return curCursor;
        }

        protected abstract int Calculate();

        public int Max
        {
            get { return data.Max(); }
        }

        public int Min
        {
            get { return data.Min(); }
        }

        public int Mean
        {
            get { return (int)Math.Round((float)Sum / data.Length); }
        }

        public int Ampl
        {
            get { return Max - Min; }
        }

        public int Sum
        {
            get { return (int)Math.Round((float)data.Sum()); }
        }

        public int Median
        {
            get
            {
                var sorted = data.ToList();
                sorted.Sort();
                return sorted[_capacity / 2];
            }
        }

    }

    public class AmplitudeCalcer : DigitalFilter
    {
        #region Overrides of DigitalFilter

        protected override int Calculate()
        {
            return Ampl;
        }

        #endregion
    }

    public class Meaner : DigitalFilter
    {
        #region Overrides of DigitalFilter

        protected override int Calculate()
        {
            return Mean;
        }

        #endregion
    }

    public class Medianer : DigitalFilter
    {
        #region Overrides of DigitalFilter

        protected override int Calculate()
        {
            return Median;
        }

        #endregion
    }

    public class SimpleDifferenter : DigitalFilter
    {
        #region Overrides of DigitalFilter

        protected override int Calculate()
        {
            return data[_cursor] - data[GetPredCursor(_cursor)];
        }

        #endregion
    }
}
