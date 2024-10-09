using System;

namespace NewProcessMonitoring
{
    internal class ProcessFileItem
    {
        public string Title { get; set; }
        public string FullPath { get; set; }
        public DateTime FirstFoundUtcDate { get; set; }
        public bool IsTrusted;

        public ProcessFileItem(string title, string fullPath, DateTime firstFoundUtcDate, bool isTrusted)
        {
            Title = title;
            FullPath = fullPath;
            FirstFoundUtcDate = firstFoundUtcDate;
            IsTrusted = isTrusted;
        }
    }
}
