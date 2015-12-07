using System;

namespace aQuery.Amibroker
{
    public struct AnalysisProgress
    {
        public AnalysisProgress(string progressText)
        {
            var tokens = progressText.Split(',');
            Percent = int.Parse(tokens[0].Substring(0, tokens[0].IndexOf('%')).Trim());
            Threads = int.Parse(tokens[1].Substring(0, tokens[1].IndexOf("thread", StringComparison.Ordinal)).Trim());
        }

        public int Threads { get; }
        public int Percent { get;  }
    }
}