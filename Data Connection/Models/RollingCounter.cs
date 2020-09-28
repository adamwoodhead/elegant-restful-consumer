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

        private List<double> limitedList { get; set; }

        public int Slipped { get; private set; } = 0;

        public double Min { get; set; }

        public double Max { get; set; }

        public double Avg
        {
            get => limitedList.Average();
        }

        public RollingCounter(int count)
        {
            Limit = count;
            limitedList = new List<double>(Limit);
        }

        // The Count read-only property returns the number
        // of items in the collection.
        public int Count => limitedList.Count;

        public void Slip(double obj)
        {
            while (limitedList.Count >= Limit)
            {
                limitedList.Remove(limitedList.Last());
            }

            limitedList.Insert(0, obj);

            Min = limitedList.Min();
            Max = limitedList.Max();
            Slipped++;
        }

        public void CopyTo(Array array, int index)
        {
            foreach (double i in limitedList)
            {
                array.SetValue(i, index);
                index += 1;
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            return limitedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return limitedList.GetEnumerator();
        }
    }
}
