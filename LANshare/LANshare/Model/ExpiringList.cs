using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LANshare.Model
{
    public class ExpiringDictionary<T>
    {
        private readonly ConcurrentDictionary<object,Tuple<T,DateTime>> innerList;
        private readonly System.Timers.Timer innerTimer;
        private readonly TimeSpan expireInterval;

        /// <summary>
        /// Event argument is list of expired elements
        /// </summary>
        public event EventHandler<List<T>> ElementsExpired;

        public ExpiringDictionary(int expiringDelayMilliseconds)
        {
            innerList = new ConcurrentDictionary<object,Tuple<T, DateTime>>();
            expireInterval = TimeSpan.FromMilliseconds(expiringDelayMilliseconds);
            innerTimer = new System.Timers.Timer(expiringDelayMilliseconds);
            innerTimer.AutoReset = true;
            innerTimer.Elapsed += TimerElapsed;
            innerTimer.Enabled = true;
            innerTimer.Start();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs args)
        {
            var expire = new List<T>();
            var list = innerList.ToList();
            bool expired = false;
            for (int i = 0; i < list.Count; i++)
            {
                if ((DateTime.Now - list[i].Value.Item2) >= expireInterval)
                {
                    Tuple<T, DateTime> tup;
                    if (innerList.TryRemove(list[i].Key, out tup))
                    {
                        expire.Add(list[i].Value.Item1);
                        expired = true;
                    }
                }
            }
            if (expired)
            {
                OnElementsExpired(expire);
            }
        }

        public bool Add(object key, T toAdd)
        {
            var newVal = new Tuple<T, DateTime>(toAdd, DateTime.Now);
            bool isNew = true;
            try
            {
                innerList.AddOrUpdate(key, newVal, (keyVal, oldVal) =>
                {
                    isNew = false;
                    return new Tuple<T, DateTime>(oldVal.Item1,DateTime.Now);
                });
            }
            catch (OverflowException)
            {
                innerList.Clear();
            }
            return isNew;
        }

        public void Remove(object key)
        {
            Tuple<T, DateTime> t;
            if (innerList.TryRemove(key, out t))
            {
                var l = new List<T>();
                l.Add(t.Item1);
                OnElementsExpired(l);
            }
        }

        public T Get(object key)
        {
            Tuple<T, DateTime> t;
            innerList.TryGetValue(key, out t);
            return t.Item1;
        }

        public List<T> GetAll()
        {
            return innerList.ToList().Select(a => a.Value.Item1).ToList();
        }
        public void Reset()
        {
            innerList.Keys.ToList().ForEach(k => Remove(k));
        }

        protected void OnElementsExpired(List<T> expired)
        {
            ElementsExpired?.Invoke(this, expired);
        }
    }
}
