using LogHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataConnection.Models
{
    public class RollingCounterCollection
    {
        internal Dictionary<Type, RollingCounter> TypeRollingCounters { get; private set; } = new Dictionary<Type, RollingCounter>();

        private int CollectionLimit { get; }

        public RollingCounterCollection(int limit)
        {
            CollectionLimit = limit;
        }

        public void Slip<T>(double val)
        {
            if (TypeRollingCounters.ContainsKey(typeof(T)))
            {
                TypeRollingCounters[typeof(T)].Slip(val);
            }
            else
            {
                TypeRollingCounters.Add(typeof(T), new RollingCounter(CollectionLimit));
                Slip<T>(val);
            }
        }

        public double Min<T>()
        {
            if (TypeRollingCounters.ContainsKey(typeof(T)))
            {
                return TypeRollingCounters[typeof(T)].Min;
            }
            else
            {
                TypeRollingCounters.Add(typeof(T), new RollingCounter(CollectionLimit));
                return Min<T>();
            }
        }

        public double Max<T>()
        {
            if (TypeRollingCounters.ContainsKey(typeof(T)))
            {
                return TypeRollingCounters[typeof(T)].Max;
            }
            else
            {
                TypeRollingCounters.Add(typeof(T), new RollingCounter(CollectionLimit));
                return Max<T>();
            }
        }

        public double Avg<T>()
        {
            if (TypeRollingCounters.ContainsKey(typeof(T)))
            {
                return TypeRollingCounters[typeof(T)].Avg;
            }
            else
            {
                TypeRollingCounters.Add(typeof(T), new RollingCounter(CollectionLimit));
                return Avg<T>();
            }
        }

        public double Count<T>()
        {
            if (TypeRollingCounters.ContainsKey(typeof(T)))
            {
                return TypeRollingCounters[typeof(T)].Count;
            }
            else
            {
                TypeRollingCounters.Add(typeof(T), new RollingCounter(CollectionLimit));
                return 0;
            }
        }

        public double Slipped<T>()
        {
            if (TypeRollingCounters.ContainsKey(typeof(T)))
            {
                return TypeRollingCounters[typeof(T)].Slipped;
            }
            else
            {
                TypeRollingCounters.Add(typeof(T), new RollingCounter(CollectionLimit));
                return 0;
            }
        }

        public double SlippedTotal()
        {
            return TypeRollingCounters.Sum(x => x.Value.Slipped);
        }

        public void Report<T>(string method, double elapsedMilliseconds)
        {
            try
            {
                Log.Verbose(
                $"[{method}] " +
                $"<{typeof(T).GetFriendlyName()}>".PadRight(TypeRollingCounters.Keys.Select(x => x.Name).Max(x => x.Length) + 2) + " : (ms)" +
                $" {elapsedMilliseconds:0000}" +
                $" : ({Slipped<T>():00000}) : ({SlippedTotal():000000})");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
