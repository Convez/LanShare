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

        public event EventHandler<bool> ElementsExpired;

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
            
            var list = innerList.ToList();
            bool expired = false;
            for (int i = 0; i < list.Count; i++)
            {
                if ((list[i].Value.Item2 - DateTime.Now)>= expireInterval)
                {
                    Tuple<T, DateTime> tup;
                    if (innerList.TryRemove(list[i].Key, out tup))
                    {
                        expired = true;
                    }
                }
            }
            if (expired)
            {
                OnElementsExpired(expired);
            }
        }

        public bool Add(object key, T toAdd)
        {
            var newVal = new Tuple<T, DateTime>(toAdd, DateTime.Now);
            bool alreadyPresent = false;
            try
            {
                
                innerList.AddOrUpdate(key, newVal, (keyVal, oldVal) =>
                {
                    alreadyPresent = true;
                    return newVal;
                });
            }
            catch (OverflowException)
            {
                innerList.Clear();
            }
            return alreadyPresent;
        }

        public void Remove(object key)
        {
            Tuple<T, DateTime> t;
            innerList.TryRemove(key,out t);
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

        protected void OnElementsExpired(bool expired)
        {
            EventHandler<bool> handler = ElementsExpired;
            if (handler != null)
            {
                handler(this, expired);
            }
        }
    }
}
