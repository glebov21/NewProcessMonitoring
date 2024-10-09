using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewProcessMonitoring
{
    internal class ProcessTableItem
    {
        public ProcessTableItem(string name, string fullPath, DateTime firstFoundLocalDate)
        {
            Name = name;
            FullPath = fullPath;
            FirstFoundLocalDate = firstFoundLocalDate;
        }

        public string Name { get; set; }
        public string FullPath { get; set; }
        public DateTime FirstFoundLocalDate { get; set; }
    };
}
