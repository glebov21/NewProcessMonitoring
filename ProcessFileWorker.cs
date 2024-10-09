using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewProcessMonitoring
{
    internal class ProcessFileWorker
    {
        public readonly string _filePath = string.Empty;
        public Dictionary<string, ProcessFileItem> ItemsByFullPath { get; private set; } = new Dictionary<string, ProcessFileItem>();
        public ProcessFileWorker(string logFilePath)
        {
            if (string.IsNullOrEmpty(logFilePath))
                throw new ArgumentNullException(nameof(logFilePath));

            Debug.WriteLine($"Process file: {logFilePath}");
            _filePath = logFilePath;
            CheckDir();

            LoadFromFile();
        }

       

        public void ClearAllAddedProcesses()
        {
            ItemsByFullPath.Clear();
            Save();
        }

        public void TryAddNewProcess(IEnumerable<ProcessFileItem> logItems)
        {
            bool isNeedSave = false;

            foreach (var logItem in logItems)
            {
                if (ItemsByFullPath.TryGetValue(logItem.FullPath, out ProcessFileItem existItem))
                {
                    if (existItem?.IsTrusted != logItem.IsTrusted)
                    {
                        ItemsByFullPath[logItem.FullPath] = logItem;
                        isNeedSave = true;
                    }
                }
                else
                {
                    ItemsByFullPath.Add(logItem.FullPath, logItem);
                    isNeedSave = true;
                }
            }

            if(isNeedSave)
                this.Save();
        }

        public void SetProcessToTrusted()
        {
            File.WriteAllText(_filePath, string.Empty); //already check for exists
        }

        public void Save()
        {
            Debug.WriteLine("Save file");
            var json = JsonConvert.SerializeObject(ItemsByFullPath.Values.ToList());
            File.WriteAllText(_filePath, json); //already check for exists
        }

        private void LoadFromFile()
        {
            ItemsByFullPath.Clear();
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var deserializedItems = JsonConvert.DeserializeObject<List<ProcessFileItem>>(json);
                        if (deserializedItems != null)
                        {
                            foreach (var item in deserializedItems) { 
                                ItemsByFullPath.Add(item.FullPath, item);
                            }
                        }
                    }
                    catch {
                        File.Delete(_filePath); //remove if can't deserialize
                    }
                }
            }
        }
        private void CheckDir()
        {
            var fileDir = Path.GetDirectoryName(_filePath);
            if (fileDir != null)
            {
                if (!Directory.Exists(fileDir))
                    Directory.CreateDirectory(fileDir);
            }
        }
    }
}
