using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class SQLClientStatistics
    {
        public SQLClientStatistics(Type contextType, bool isLongRunning)
        {
            IsLongRunning = isLongRunning;
            Connections = new List<SQLConnectionStatistics>();
            ContextType = contextType;
        }

        public bool IsLongRunning { get; private set; }
        public string ContextTypeName { get { return ContextType.FullName; } }
        public Type ContextType { get; private set; }
        public List<SQLConnectionStatistics> Connections { get; private set; }
        public long TotalBytesSent
        {
            get
            {
                return Connections.Sum(x => x.TotalBytesSent);
            }
        }
        public long TotalBytesReceived
        {
            get
            {
                return Connections.Sum(x => x.TotalBytesReceived);
            }
        }
        public List<string> AllInitiatingQueries
        {
            get
            {
                return Connections.Select(x => x.InitiatingQuery).ToList();
            }
        }
        public long TotalTimeConnectedMilliseconds
        {
            get
            {
                try
                {
                    return Connections.Sum(x => x.ConnectionLengthMilliseconds);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public long TotalExecutionTimeMilliseconds
        {
            get
            {
                return Connections.Sum(x => x.ExecutionTimeMilliseconds);
            }
        }
        public int InlineMappingCount { get; internal set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Is Long Running: " + IsLongRunning);
            sb.AppendLine("Context Type: " + ContextType.FullName);
            sb.AppendLine("Total Time Connected (ms): " + TotalTimeConnectedMilliseconds);
            sb.AppendLine("Total Execution Time (ms): " + TotalExecutionTimeMilliseconds);
            sb.AppendLine("Conection Count: " + Connections.Count);
            sb.AppendLine("Total Bytes Sent: " + TotalBytesSent);
            sb.AppendLine("Total Bytes Received: " + TotalBytesReceived);
            sb.AppendLine("Inline Mappings Done: " + InlineMappingCount);
            return sb.ToString();
        }
    }

    public class SQLConnectionStatistics
    {
        public SQLConnectionStatistics()
        {
            DatabaseLogExcerpt = new List<string>();
        }

        public string InitiatingQuery { get; set; }
        public long ExecutionTimeMilliseconds { get; set; }
        public long ConnectionLengthMilliseconds { get; set; }
        public List<string> DatabaseLogExcerpt { get; private set; }
        public long TotalBytesSent { get; set; }
        public long TotalBytesReceived { get; set; }
        public System.Collections.IDictionary RawStatistics { get; set; }
    }
}
