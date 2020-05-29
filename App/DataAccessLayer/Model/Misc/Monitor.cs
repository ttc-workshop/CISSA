using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Intersoft.CISSA.DataAccessLayer.Model.Misc
{
    public class Monitor: IDisposable
    {
        private static readonly List<MonitorNode> Items = new List<MonitorNode>();
        private static readonly ReaderWriterLock MonitorLock = new ReaderWriterLock();
        public const int LockTimeout = 100000;


        public static List<MonitorNode> GetItems()
        {
            MonitorLock.AcquireReaderLock(LockTimeout);
            try
            {
                return new List<MonitorNode>(Items);
            }
            finally
            {
                MonitorLock.ReleaseReaderLock();
            }
        }

        public static MonitorNode Find(params string[] pathName)
        {
            MonitorLock.AcquireReaderLock(LockTimeout);
            try
            {
                var items = Items;

                MonitorNode item = null;
                foreach (var name in pathName)
                {
                    item = FindIn(name, items);
                    if (item == null) return null;
                    items = item.Items;
                }
                return item;
            }
            finally
            {
                MonitorLock.ReleaseReaderLock();
            }
        }

        public static MonitorNode Find(MonitorNode parent, string name)
        {
            if (parent == null) return null;

            return FindIn(name, parent.Items);
        }

        public static MonitorNode CreateNode(params string[] pathName)
        {
            MonitorLock.AcquireReaderLock(LockTimeout);
            try
            {
                var items = Items;

                MonitorNode item = null;
                foreach (var name in pathName)
                {
                    item = FindIn(name, items);
                    if (item == null)
                    {
                        var lc = MonitorLock.UpgradeToWriterLock(LockTimeout);
                        try
                        {
                            item = FindIn(name, items);
                            if (item == null)
                            {
                                item = new MonitorNode(name);
                                items.Add(item);
                            }
                        }
                        finally
                        {
                            MonitorLock.DowngradeFromWriterLock(ref lc);
                        }
                    }
                    items = item.Items;
                }
                return item;
            }
            finally
            {
                MonitorLock.ReleaseReaderLock();
            }
        }

        public static MonitorNode CreateNode(MonitorNode parent, string name)
        {
            if (parent == null) return null;

            MonitorLock.AcquireReaderLock(LockTimeout);
            try
            {
                var item = Find(parent, name);
                if (item == null)
                {
                    var lc = MonitorLock.UpgradeToWriterLock(LockTimeout);
                    try
                    {
                        item = Find(parent, name);
                        if (item == null)
                        {
                            item = new MonitorNode(name);
                            parent.Items.Add(item);
                        }
                    }
                    finally
                    {
                        MonitorLock.DowngradeFromWriterLock(ref lc);
                    }
                }
                return item;
            }
            finally
            {
                MonitorLock.ReleaseReaderLock();
            }
        }

        private static MonitorNode FindIn(string name, IEnumerable<MonitorNode> items)
        {
            return items != null ? items.FirstOrDefault(i => String.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase)) : null;
        }

        public MonitorNode Node { get; private set; }
        private readonly object _writeLock = new object();
        private readonly ReaderWriterLock rwLock = new ReaderWriterLock();

        private long timeTicks;
        public DateTime Time { get {return new DateTime(timeTicks);} private set { timeTicks = value.Ticks; } }

        private MonitorData GetNodeData()
        {
            rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                if (Node != null)
                    return Node.Data ?? (Node.Data = new MonitorData());

                return null;
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
        }

        public Monitor(params string[] name)
        {
            Node = CreateNode(name);
            Time = DateTime.Now;

            var data = GetNodeData();

            if (data != null)
                // lock (_writeLock)
                {
                    var count = Interlocked.Read(ref data.sessionCount) + 1;
                    Interlocked.Increment(ref data.sessionCount);
                    var max = Interlocked.Read(ref data.maxSessionCount);
                    if (max <= count)
                        Interlocked.Exchange(ref data.maxSessionCount, count);
                }
        }

        public Monitor(Monitor parent, string name)
        {
            Node = CreateNode(parent.Node, name);
            Time = DateTime.Now;

            var data = GetNodeData();

            if (data != null)
                // lock (_writeLock)
                {
                    var count = Interlocked.Read(ref data.sessionCount) + 1;
                    Interlocked.Increment(ref data.sessionCount);
                    var max = Interlocked.Read(ref data.maxSessionCount);
                    if (max <= count)
                        Interlocked.Exchange(ref data.maxSessionCount, count);
                }
        }

        public DateTime Complete()
        {
            var endTime = DateTime.Now;

            var data = GetNodeData();

            if (data == null) return endTime;
            var time = new DateTime(Interlocked.Read(ref timeTicks));
            var spin = endTime.Subtract(time);

            // lock(_writeLock)
            {
                var count = Interlocked.Decrement(ref data.sessionCount); // data.SessionCount--;

                var total = new TimeSpan(Interlocked.Read(ref data.totalTimeTicks)).Add(spin);

                Interlocked.Increment(ref data.totalCount); //data.TotalCount = data.TotalCount + 1;
                Interlocked.Exchange(ref data.avgTimeTicks, new TimeSpan(total.Ticks/data.TotalCount).Ticks);
                Interlocked.Exchange(ref data.totalTimeTicks, total.Ticks);

                var t = Interlocked.Read(ref data.maxTimeTicks);
                if (t < spin.Ticks) Interlocked.Exchange(ref data.maxTimeTicks, spin.Ticks);
                t = Interlocked.Read(ref data.minTimeTicks);
                if (t > TimeSpan.Zero.Ticks && t > spin.Ticks) Interlocked.Exchange(ref data.minTimeTicks, spin.Ticks);
                //if (data.MinTime > TimeSpan.Zero && data.MinTime > spin) data.MinTime = spin;
            }
            return endTime;
        }

        public DateTime CompleteWithException(Exception e)
        {
            var endTime = DateTime.Now;

            var data = GetNodeData();

            if (data == null) return endTime;
            //var spin = endTime.Subtract(Time);
            var time = new DateTime(Interlocked.Read(ref timeTicks));
            var spin = endTime.Subtract(time);

            lock (_writeLock)
            {
                // data.SessionCount--;
                Interlocked.Decrement(ref data.sessionCount);
                
                // var total = data.TotalTime.Add(spin);
                var total = new TimeSpan(Interlocked.Read(ref data.totalTimeTicks)).Add(spin);
                // var exceptionTotal = data.ExceptionTime.Add(spin);
                var exceptionTotal = new TimeSpan(Interlocked.Read(ref data.exceptionTimeTicks)).Add(spin);
                // data.TotalCount = data.TotalCount + 1;
                var totalCount = Interlocked.Increment(ref data.totalCount);
                // data.ExceptionCount++;
                var exceptionCount = Interlocked.Increment(ref data.exceptionCount);
                //data.AvgTime = new TimeSpan(total.Ticks/data.TotalCount);
                Interlocked.Exchange(ref data.avgTimeTicks, new TimeSpan(total.Ticks/totalCount).Ticks);
                // data.ExceptionAvgTime = new TimeSpan(exceptionTotal.Ticks/data.ExceptionCount);
                Interlocked.Exchange(ref data.exceptionAvgTimeTicks,
                    new TimeSpan(exceptionTotal.Ticks/exceptionCount).Ticks);
                // data.TotalTime = total;
                Interlocked.Exchange(ref data.totalTimeTicks, total.Ticks);
                // data.ExceptionTime = exceptionTotal;
                Interlocked.Exchange(ref data.exceptionTimeTicks, exceptionTotal.Ticks);
                // if (data.MaxTime < spin) data.MaxTime = spin;
                var t = Interlocked.Read(ref data.maxTimeTicks);
                if (t < spin.Ticks) Interlocked.Exchange(ref data.maxTimeTicks, spin.Ticks);
                // if (data.MinTime > TimeSpan.Zero && data.MinTime > spin) data.MinTime = spin;
                t = Interlocked.Read(ref data.minTimeTicks);
                if (t > TimeSpan.Zero.Ticks && t > spin.Ticks) Interlocked.Exchange(ref data.minTimeTicks, spin.Ticks);
                // if (data.ExceptionMaxTime < spin) data.ExceptionMaxTime = spin;
                t = Interlocked.Read(ref data.exceptionMaxTimeTicks);
                if (t < spin.Ticks) Interlocked.Exchange(ref data.exceptionMaxTimeTicks, spin.Ticks);
                // if (data.ExceptionMinTime > TimeSpan.Zero && data.ExceptionMinTime > spin) data.ExceptionMinTime = spin;
                t = Interlocked.Read(ref data.exceptionMinTimeTicks);
                if (t > TimeSpan.Zero.Ticks && t > spin.Ticks) Interlocked.Exchange(ref data.exceptionMinTimeTicks, spin.Ticks);

            }
            return endTime;
        }

        public void Dispose()
        {
            Complete();
        }
    }

    [DataContract]
    public class MonitorData
    {
        internal long totalCount;
        internal long exceptionCount;

        [DataMember]
        public long TotalCount { get { return totalCount; } set { totalCount = value; } }
        [DataMember]
        public long ExceptionCount { get { return exceptionCount; } set { exceptionCount = value; } }

        internal long sessionCount;
        [DataMember]
        public long SessionCount { get { return sessionCount; } set { sessionCount = value; } }

        internal long maxSessionCount;
        [DataMember]
        public long MaxSessionCount { get { return maxSessionCount; } set { maxSessionCount = value; } }

        internal long totalTimeTicks;
        internal long maxTimeTicks;
        internal long minTimeTicks;
        internal long avgTimeTicks;
        internal long exceptionTimeTicks;
        internal long exceptionMaxTimeTicks;
        internal long exceptionMinTimeTicks;
        internal long exceptionAvgTimeTicks;
        [DataMember]
        public TimeSpan TotalTime { get { return new TimeSpan(totalTimeTicks); } set { totalTimeTicks = value.Ticks; } }
        [DataMember]
        public TimeSpan MaxTime { get { return new TimeSpan(maxTimeTicks); } set { maxTimeTicks = value.Ticks; } }
        [DataMember]
        public TimeSpan MinTime { get { return new TimeSpan(minTimeTicks); } set { minTimeTicks = value.Ticks; } }
        [DataMember]
        public TimeSpan AvgTime { get { return new TimeSpan(avgTimeTicks); } set { avgTimeTicks = value.Ticks; } }

        [DataMember]
        public TimeSpan ExceptionTime { get { return new TimeSpan(exceptionTimeTicks); } set { exceptionTimeTicks = value.Ticks; } }
        [DataMember]
        public TimeSpan ExceptionMaxTime { get { return new TimeSpan(exceptionMaxTimeTicks); } set { exceptionMaxTimeTicks = value.Ticks; } }
        [DataMember]
        public TimeSpan ExceptionMinTime { get { return new TimeSpan(exceptionMinTimeTicks); } set { exceptionMinTimeTicks = value.Ticks; } }
        [DataMember]
        public TimeSpan ExceptionAvgTime { get { return new TimeSpan(exceptionAvgTimeTicks); } set { exceptionAvgTimeTicks = value.Ticks; } }
    }

    [DataContract]
    public class MonitorNode 
    {
        [DataMember]
        public string Name { get; set; }

        public MonitorNode()
        {
            Items = new List<MonitorNode>();
        }
        public MonitorNode(string name) : this()
        {
            Name = name;
        }

        [DataMember]
        public List<MonitorNode> Items { get; set; }
        [DataMember]
        public MonitorData Data { get; set; }
    }
}
