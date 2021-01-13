using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace MakeNewFtpClientGui.Models
{
    public class Settings
    {
        public string PathAD { get; set; }
        private string ftpPath;
        public string FtpPath {
            get => ftpPath;
            set => ftpPath = value.Replace("/", "\\");
        }

        private string fullPathToDictsSettings;
        public string FullPathToDictsSettings
        {
            get => fullPathToDictsSettings;
            set => fullPathToDictsSettings = value.Replace("/", "\\");
        }

        private string fullPathToResultServiceSettings;
        public string FullPathToResultServiceSettings
        {
            get => fullPathToResultServiceSettings;
            set => fullPathToResultServiceSettings = value.Replace("/", "\\");
        }

        private string fullPathToGftpRegistratorSettings;
        public string FullPathToGftpRegistratorSettings
        {
            get => fullPathToGftpRegistratorSettings;
            set => fullPathToGftpRegistratorSettings = value.Replace("/", "\\");
        }
        private string pathToDictsConsole;
        public string PathToDictsConsole
        {
            get => pathToDictsConsole;
            set => pathToDictsConsole = value.Replace("/", "\\");
        }

        public string DictionariesServer { get; set; }
        public string ConsoleStartsAccountLogin { get; set; }
        public string ConsoleStartsAccountPassword { get; set; }
        public string DictionariesFolderInScheduler { get; set; }
    }

    public class GetSettings
    {
        private static Settings settings;
        private static readonly object syncRoot = new object();
        public static Settings Get()
        {
            if (settings is null)
                try
                {
                    Monitor.TryEnter(syncRoot, TimeSpan.FromSeconds(2));
                    if (settings is null)
                    {
                        settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Settings.json")));
                    }
                }
                catch (Exception ex)
                {
                    settings = null;
                }
                finally
                {
                    Monitor.Exit(syncRoot);
                }
            return settings;
        }
    }
}
