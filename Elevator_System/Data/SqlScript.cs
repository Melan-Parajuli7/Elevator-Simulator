using System;
using System.Collections.Generic;
using System.Text;

namespace Lift.Data
{
    /// <summary>Splits a SQL script into batches. A line that contains only GO ends a batch (as in SSMS).</summary>
    public static class SqlScript
    {
        public static IList<string> SplitBatches(string script)
        {
            var batches = new List<string>();
            if (string.IsNullOrWhiteSpace(script)) return batches;

            var current = new StringBuilder();
            string[] lines = script.Replace("\r\n", "\n").Split('\n');

            foreach (string line in lines)
            {
                if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
                {
                    AddIfNotEmpty(batches, current);
                }
                else
                {
                    current.AppendLine(line);
                }
            }

            AddIfNotEmpty(batches, current);
            return batches;
        }

        private static void AddIfNotEmpty(List<string> batches, StringBuilder current)
        {
            string text = current.ToString().Trim();
            if (text.Length > 0) batches.Add(text);
            current.Clear();
        }
    }
}
