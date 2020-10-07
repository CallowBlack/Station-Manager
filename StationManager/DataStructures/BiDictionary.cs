using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationManager.DataStructures
{
    // Modified
    // Taken from StackOverflow 
    // https://stackoverflow.com/questions/255341/getting-multiple-keys-of-specified-value-of-a-generic-dictionary/255638#255638

    class BiDictionary<TFirst, TSecond>
    {
        IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        public int Count { get => firstToSecond.Count; }

        public void Add(TFirst first, TSecond second)
        {
            if (!firstToSecond.ContainsKey(first) && !secondToFirst.ContainsKey(second))
            {
                firstToSecond.Add(first, second);
                secondToFirst.Add(second, first);
            }
        }

        public void Clear()
        {
            firstToSecond.Clear();
            secondToFirst.Clear();
        }

        public TSecond this[TFirst first]
        {
            get { TSecond value; 
                return TryGetByFirst(first, out value) ? value : default; }
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }
    }
}
