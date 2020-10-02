using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataConnection.Models
{
    internal class RollingCounter : IEnumerable<double>, IEnumerable, IReadOnlyCollection<double>
    {
        internal int Limit { get; set; }

        private List<double> LimitedList { get; set; }

        public int Slipped { get; private set; } = 0;

        public double Min { get; set; }

        public double Max { get; set; }

        public double Avg
        {
            get => LimitedList.Average();
        }

        public RollingCounter(int count)
        {
            Limit = count;
            LimitedList = new List<double>(Limit);
        }

        // The Count read-only property returns the number
        // of items in the collection.
        public int Count => LimitedList.Count;

        public void Slip(double obj)
        {
            while (LimitedList.Count >= Limit)
            {
                LimitedList.Remove(LimitedList.Last());
            }

            LimitedList.Insert(0, obj);

            Min = LimitedList.Min();
            Max = LimitedList.Max();
            Slipped++;
        }

        public void CopyTo(Array array, int index)
        {
            foreach (double i in LimitedList)
            {
                array.SetValue(i, index);
                index += 1;
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            return LimitedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LimitedList.GetEnumerator();
        }
    }
}
