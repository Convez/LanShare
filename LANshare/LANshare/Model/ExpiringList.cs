using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LANshare.Model
{
    public class ExpiringList<T>
    {
        private readonly SynchronizedCollection<Tuple<T,DateTime>> innerList;
        private readonly System.Timers.Timer innerTimer;
        private readonly TimeSpan expireInterval;
        public ExpiringList(int expiringDelayMilliseconds)
        {
            innerList = new SynchronizedCollection<Tuple<T, DateTime>>();
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
            for (int i = 0; i < list.Count; i++)
            {
                if ((list[i].Item2 - DateTime.Now)>= expireInterval)
                {
                    innerList.RemoveAt(i);
                }
            }
        }

        public void Add(T toAdd)
        {
            innerList.Add(new Tuple<T, DateTime>(toAdd, DateTime.Now));
        }
    }
}
