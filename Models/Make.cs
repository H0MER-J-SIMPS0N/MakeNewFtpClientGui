using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace MakeNewFtpClientGui.Models
{
    public enum Consoles { Dictionaries, GftpRegistrator, ResultService }

    public static class Make
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static bool AdAccount(string login, string password, out string textResult)
        {
            bool result;
            if (UserExists(login, "domain"))
            {
                textResult = $"Пользователь {login} уже существует\r\n";
                logger.Warn($"Пользователь {login} уже существует");
                result = true;
            }
            else
            {
                string path = GetSettings.Get().PathAD;

                PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, "domain", path);
                UserPrincipalEx user = new UserPrincipalEx(principalContext, login, password, true)
                {
                    //user.UserPrincipalName = login;
                    Cn = login,
                    DisplayName = login,
                    GivenName = login,
                    PhysicalDeliveryOfficeName = password,
                    UserPrincipalName = $"{login}@domain",
                    PasswordNeverExpires = true,
                    UserCannotChangePassword = true
                };
                try
                {
                    user.Save();
                    textResult = $"Создан пользователь {user.UserPrincipalName}\r\n";
                    logger.Info($"Создан пользователь {user.UserPrincipalName}");
                }
                catch (Exception ex)
                {
                    textResult = $"Пользователь не создан по причине:\r\n{ex}\r\n";
                    logger.Error($"Пользователь не создан по причине:\r\n{ex}");
                    return false;
                }
                GroupPrincipal group = GroupPrincipal.FindByIdentity(principalContext, "FTP Group");
                group.Members.Add(user);
                try
                {
                    group.Save();
                    user.Save();
                    textResult += "Пользователь добавлен в группу 'FTP Group'\r\n";
                    logger.Info("Пользователь добавлен в группу 'FTP Group'");
                }
                catch (Exception ex)
                {
                    textResult += $"Пользователь не добавлен в группу 'FTP Group' по причине:\r\n{ex}\r\n";
                    logger.Error($"Пользователь не добавлен в группу 'FTP Group' по причине:\r\n{ex}");
                    return false;
                }
                try
                {
                    user.PrimaryGroupId = "16076";
                    user.Save();
                    textResult += "\r\nУстановлена primary group для пользователя.\r\n";
                    logger.Info("Установлена primary group для пользователя.");
                    result = true;
                }
                catch (Exception ex)
                {
                    textResult += $"\r\nНе установлена primary group для пользователя по причине:\r\n{ex}\r\n";
                    logger.Error($"Не установлена primary group для пользователя по причине:\r\n{ex}");
                    result = false;
                }                
            }            
            return result;
        }

        private static bool UserExists(string userName, string domain)
        {
            using (var pc = new PrincipalContext(ContextType.Domain, domain))
            using (var p = Principal.FindByIdentity(pc, IdentityType.SamAccountName, userName))
            {
                return p != null;
            }
        }

        public static bool Directories(MisSettings misSettings, out string textResult)
        {
            textResult = string.Empty;
            bool result = true;
            bool res = CreateDirectory(Path.Combine(GetSettings.Get().FtpPath, (misSettings.SelectedHub + misSettings.Login).Trim()), out string tempResultTxt);
            Thread.Sleep(3000);
            if (res)
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(GetSettings.Get().FtpPath, (misSettings.SelectedHub + misSettings.Login).Trim()));
                    DirectorySecurity ds = dirInfo.GetAccessControl(AccessControlSections.Access);
                    FileSystemAccessRule addRule = new FileSystemAccessRule($@"domain\{(misSettings.SelectedHub + misSettings.Login).Trim()}", FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
                    ds.AddAccessRule(addRule);
                    dirInfo.SetAccessControl(ds);
                    textResult += $"Права на папку {Path.Combine(GetSettings.Get().FtpPath, (misSettings.SelectedHub + misSettings.Login).Trim())} установлены\r\n";
                    logger.Info($"Права на папку {Path.Combine(GetSettings.Get().FtpPath, (misSettings.SelectedHub + misSettings.Login).Trim())} установлены");
                }
                catch (Exception ex)
                {
                    textResult += $"Права на папку {Path.Combine(GetSettings.Get().FtpPath, (misSettings.SelectedHub + misSettings.Login).Trim())} не установлены по причине:\r\n{ex}\r\n";
                    logger.Error($"Права на папку {Path.Combine(GetSettings.Get().FtpPath, (misSettings.SelectedHub + misSettings.Login).Trim())} не установлены по причине:\r\n{ex}");
                }
            }
            else
            {
                textResult += tempResultTxt;
                return false;
            }
            for (int i = 0; i < misSettings.FoldersToCreate.Count; i++)
            {
                CreateDirectory(Path.Combine(GetSettings.Get().FtpPath, (misSettings.SelectedHub + misSettings.Login).Trim(), misSettings.FoldersToCreate[i]), out tempResultTxt);
                textResult += tempResultTxt;
            }
            return result;

            //return true;
        }

        private static bool CreateDirectory(string directoryPath, out string textResult)
        {
            textResult = string.Empty;
            bool result = true;
            try
            {
                Directory.CreateDirectory(directoryPath);
                textResult += $"Создана папка {directoryPath}\r\n";
                logger.Info($"Создана папка {directoryPath}");
            }
            catch (Exception ex)
            {
                textResult += $"Папка {directoryPath} не создана по причине:\r\n{ex}\r\n";
                logger.Error($"Папка {directoryPath} не создана по причине:\r\n{ex}");
                result = false;
            }
            return result;
        }

        public static string SetDictionariesSettings(MisSettings misSettings)
        {
            string result;
            try
            {
                if (File.ReadAllText(GetSettings.Get().FullPathToDictsSettings).ToUpper().Contains($"\"{(misSettings.SelectedHub + misSettings.Login).Trim().ToUpper()}\""))
                {
                    result = $"В файле {GetSettings.Get().FullPathToDictsSettings} уже есть настройки с логином {(misSettings.SelectedHub + misSettings.Login).Trim()}";
                    logger.Warn($"В файле {GetSettings.Get().FullPathToDictsSettings} уже есть настройки с логином {(misSettings.SelectedHub + misSettings.Login).Trim()}");
                    return result;
                }
            }
            catch
            {
                result = $"Не удалось прочитать файл настроек {GetSettings.Get().FullPathToDictsSettings} для проверки дубля логина. Попробуйте заново.";
                logger.Error($"Не удалось прочитать файл настроек {GetSettings.Get().FullPathToDictsSettings} для проверки дубля логина. Попробуйте заново.");
                return result;
            }
            string textSettingsFromTemplate = GetSettingsFromTemplate(
                Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Dictionaries", misSettings.Name + ".xml"),
                new Dictionary<string, string>()
                {
                    { "@ACCOUNT", (misSettings.SelectedHub + misSettings.Login).Trim() },
                    { "@PASSWORD", misSettings.Password },
                    { "@CONTRACTS", string.Join(";", misSettings.Contracts) },
                    { "@SPPROPREGION", misSettings.SpecialPropertiesRegion },
                    { "@ISBYCONTRACT", misSettings.IsByContract.ToString() }
                });
            if (textSettingsFromTemplate.Contains("Нужно внести настройки вручную"))
            {
                result = textSettingsFromTemplate + "\r\n";
            }
            else
            {
                result = "Настройки Dictionaries:\r\n" + textSettingsFromTemplate + "\r\n";
                logger.Info($"Настройки Dictionaries:\r\n{textSettingsFromTemplate}");
                result += WriteSettingsToMainFile(GetSettings.Get().FullPathToDictsSettings, textSettingsFromTemplate, Consoles.Dictionaries);
                result += CreateSchedulerTask(misSettings);
            }
            return result;
        }

        public static string SetOrdersSettings(MisSettings misSettings)
        {
            string result;
            try
            {
                if (File.ReadAllText(GetSettings.Get().FullPathToGftpRegistratorSettings).ToUpper().Contains($"\"{(misSettings.SelectedHub + misSettings.Login).Trim().ToUpper()}\""))
                {
                    result = $"В файле {GetSettings.Get().FullPathToGftpRegistratorSettings} уже есть настройки с логином {(misSettings.SelectedHub + misSettings.Login).Trim()}";
                    logger.Warn($"В файле {GetSettings.Get().FullPathToGftpRegistratorSettings} уже есть настройки с логином {(misSettings.SelectedHub + misSettings.Login).Trim()}");
                    return result;
                }
            }
            catch
            {
                result = $"Не удалось прочитать файл настроек {GetSettings.Get().FullPathToGftpRegistratorSettings} для проверки дубля логина. Попробуйте заново.";
                logger.Error($"Не удалось прочитать файл настроек {GetSettings.Get().FullPathToGftpRegistratorSettings} для проверки дубля логина. Попробуйте заново.");
                return result;
            }
            string textSettingsFromTemplate = GetSettingsFromTemplate(
                Path.Combine(Directory.GetCurrentDirectory(), "Templates", "GftpRegistrator", misSettings.Name + ".json"),
                new Dictionary<string, string>()
                {
                    { "@ACCOUNT", (misSettings.SelectedHub + misSettings.Login).Trim() },
                    { "@CONTRACTS", string.Join("\", \"", misSettings.Contracts) },
                    { "@EMAILSWITHERRORS", misSettings.Emails is null || misSettings.Emails.Count == 0  ? string.Empty : "\", \"" + string.Join("\", \"", misSettings.Emails) }
                });
            if (textSettingsFromTemplate.Contains("Нужно внести настройки вручную"))
            {
                result = textSettingsFromTemplate + "\r\n";
            }
            else
            {
                result = "Настройки GftpRegistrator:\r\n" + textSettingsFromTemplate + "\r\n";
                logger.Info($"Настройки GftpRegistrator:\r\n{textSettingsFromTemplate}");
                result += WriteSettingsToMainFile(GetSettings.Get().FullPathToGftpRegistratorSettings, textSettingsFromTemplate, Consoles.GftpRegistrator);
            }
            return result;
        }

        public static string SetResultsSettings(MisSettings misSettings)
        {
            string result;
            try
            {
                if (File.ReadAllText(GetSettings.Get().FullPathToResultServiceSettings).ToUpper().Contains($"/{(misSettings.SelectedHub + misSettings.Login).Trim().ToUpper()}/"))
                {
                    result = $"В файле {GetSettings.Get().FullPathToResultServiceSettings} уже есть настройки с логином {(misSettings.SelectedHub + misSettings.Login).Trim()}";
                    logger.Warn($"В файле {GetSettings.Get().FullPathToResultServiceSettings} уже есть настройки с логином {(misSettings.SelectedHub + misSettings.Login).Trim()}");
                    return result;
                }
            }
            catch
            {
                result = $"Не удалось прочитать файл настроек {GetSettings.Get().FullPathToResultServiceSettings} для проверки дубля логина. Попробуйте заново.";
                logger.Error($"Не удалось прочитать файл настроек {GetSettings.Get().FullPathToResultServiceSettings} для проверки дубля логина. Попробуйте заново.");
                return result;
            }
            string textSettingsFromTemplate = GetSettingsFromTemplate(
                Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ResultService", misSettings.Name + ".json"),
                new Dictionary<string, string>()
                {
                    { "@ACCOUNT", (misSettings.SelectedHub + misSettings.Login).Trim() },
                    { "@CONTRACTS", string.Join("\", \"", misSettings.Contracts) }
                });
            if (textSettingsFromTemplate.Contains("Нужно внести настройки вручную"))
            {
                result = textSettingsFromTemplate + "\r\n";
            }
            else
            {
                result = "Настройки ResultService:\r\n" + textSettingsFromTemplate + "\r\n";
                logger.Info($"Настройки ResultService:\r\n{textSettingsFromTemplate}");
                result += WriteSettingsToMainFile(GetSettings.Get().FullPathToResultServiceSettings, textSettingsFromTemplate, Consoles.ResultService);
            }            
            return result;
        }

        private static string GetSettingsFromTemplate(string filepath, Dictionary<string, string> replacingValues)
        {
            StringBuilder result = new StringBuilder();
            if (File.Exists(filepath))
            {
                try
                {
                    result.Append(File.ReadAllText(filepath));
                    foreach (var rValue in replacingValues)
                    {
                        result = result.Replace(rValue.Key, rValue.Value);
                    }
                }
                catch
                {
                    result.Append($"Файл настроек {filepath} не удалось прочитать. Нужно внести настройки вручную.");
                    logger.Error($"Файл настроек {filepath} не удалось прочитать. Нужно внести настройки вручную.");
                }
            }
            else
            {
                result.Append($"Файла настроек {filepath} не существует. Нужно внести настройки вручную.");
                logger.Error($"Файла настроек {filepath} не существует. Нужно внести настройки вручную.");
            }            
            return result.ToString();
        }

        private static string WriteSettingsToMainFile(string settingsFile, string currentSettings, Consoles console)
        {
            StringBuilder result = new StringBuilder();
            List<string> linesFromSettingsFile;
            try
            {
                File.Copy(settingsFile, settingsFile.Replace(".config", $"_{DateTime.Now:ddMMyyyy_hh-mm-ss}.config").Replace(".json", $"_{DateTime.Now:ddMMyyyy_hh-mm-ss}.json"));
                result.AppendLine($"Файл {settingsFile} скопирован в файл {settingsFile.Replace(".config", $"_{DateTime.Now:ddMMyyyy_hh-mm-ss}.config").Replace(".json", $"_{DateTime.Now:ddMMyyyy_hh-mm-ss}.json")}");
                logger.Info($"Файл {settingsFile} скопирован в файл {settingsFile.Replace(".config", $"_{DateTime.Now:ddMMyyyy_hh-mm-ss}.config").Replace(".json", $"_{DateTime.Now:ddMMyyyy_hh-mm-ss}.json")}");
            }
            catch (Exception ex)
            {
                result.AppendLine($"Не удалось скопировать файл настроек {settingsFile} по причине:\r\n{ex}");
                logger.Error($"Не удалось скопировать файл настроек {settingsFile} по причине:\r\n{ex}");
                return result.ToString();
            }
            try
            {
                linesFromSettingsFile = File.ReadAllLines(settingsFile).ToList();
                result.AppendLine($"Файл настроек {settingsFile} прочитан");
                logger.Info($"Файл настроек {settingsFile} прочитан");
            }
            catch (Exception ex)
            {
                result.AppendLine($"Не удалось прочитать файл настроек {settingsFile} по причине:\r\n{ex}");
                logger.Error($"Не удалось прочитать файл настроек {settingsFile} по причине:\r\n{ex}");
                return result.ToString();
            }
            string anchor = string.Empty;
            switch (console)
            {
                case Consoles.Dictionaries:
                    anchor = "<accountSettings>";
                    break;
                case Consoles.GftpRegistrator:
                    anchor = "},";
                    break;
                case Consoles.ResultService:
                    anchor = "*/";
                    break;
            }
            string res = WriteCurrentSettingsToFile(currentSettings, linesFromSettingsFile, settingsFile, anchor);
            result.AppendLine(res);
            logger.Info(res);
            return result.ToString();
        }

        private static string WriteCurrentSettingsToFile(string currentSettings, List<string> linesFromSettingsFile, string settingsFile, string anchor)
        {
            StringBuilder result = new StringBuilder();
            List<string> addLines = currentSettings.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            try
            {
                for (int i = 0; i < linesFromSettingsFile.Count; i++)
                {
                    if (linesFromSettingsFile[i].Contains(anchor))
                    {
                        linesFromSettingsFile.InsertRange(i + 1, addLines);
                        break;
                    }
                }
                File.WriteAllLines(settingsFile, linesFromSettingsFile);
                result.AppendLine($"Новые настройки сохранены в файле {settingsFile}");
                logger.Info($"Новые настройки сохранены в файле {settingsFile}");
            }
            catch (Exception ex)
            {
                result.AppendLine($"Новые настройки не сохранены в файле {settingsFile} по причине:\r\n{ex}");
                logger.Error($"Новые настройки не сохранены в файле {settingsFile} по причине:\r\n{ex}");
            }
            return result.ToString();
        }

        private static string CreateSchedulerTask(MisSettings misSettings)
        {
            StringBuilder result = new StringBuilder();
            DateTime startTime;
            DateTime.TryParse(misSettings.ScheduleStartTime.Replace('-', ':'), out startTime);
            if (startTime > DateTime.Now.Date)
            {
                string pathToTempSchedulerSettingsFile = Path.Combine(Directory.GetCurrentDirectory(), "dictXml.xml");
                string dateTimeStart = DateTime.Now.Date.Add(startTime.TimeOfDay).ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
                Dictionary<string, string> attributesForScheduleTask = new Dictionary<string, string>()
                    {
                        { "@CREATIONDATE", $"{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffffff}" },
                        { "@WHOMADETASK", $"{Environment.UserDomainName}\\{ Environment.UserName}" },
                        { "@STARTDATETIME", $"{dateTimeStart}" },
                        { "@PATHTOCONSOLE", "\"" + GetSettings.Get().PathToDictsConsole + "\"" },
                        { "@LOGIN", (misSettings.SelectedHub + misSettings.Login).Trim() }
                    };
                string shedulerTask = GetSettingsFromTemplate(Path.Combine(Directory.GetCurrentDirectory(), "Templates", "DictsSchedule.xml"), attributesForScheduleTask);
                try
                {
                    File.WriteAllText(pathToTempSchedulerSettingsFile, shedulerTask);
                }
                catch (Exception ex)
                {
                    result.AppendLine($"Не удалось записать временный файл настройки задания в планировщике по причине:\r\n{ex}");
                    logger.Error($"Не удалось записать временный файл настройки задания в планировщике по причине:\r\n{ex}");
                }
                string attributes = $"/create /s {GetSettings.Get().DictionariesServer} /tn \"{GetSettings.Get().DictionariesFolderInScheduler}\\{(misSettings.SelectedHub + misSettings.Login).Trim()}\" /ru {GetSettings.Get().ConsoleStartsAccountLogin} /rp \"{GetSettings.Get().ConsoleStartsAccountPassword}\" /xml {pathToTempSchedulerSettingsFile}";
                result.AppendLine(attributes);
                logger.Info(attributes);
                while (!File.Exists(pathToTempSchedulerSettingsFile))
                {
                    Thread.Sleep(10);
                }
                ProcessStartInfo procInfo = new ProcessStartInfo(@"C:\Windows\System32\schtasks.exe", attributes);
                try
                {
                    using Process p = Process.Start(procInfo);
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        result.AppendLine($"Задание {(misSettings.SelectedHub + misSettings.Login).Trim()} в планировщике создано");
                        logger.Info($"Задание {(misSettings.SelectedHub + misSettings.Login).Trim()} в планировщике создано");
                    }
                    else
                    {
                        result.AppendLine($"Задание в планировщике не создано по причине:\r\nErorCode - {p.ExitCode}");
                        logger.Error($"Задание в планировщике не создано по причине:\r\nErorCode - {p.ExitCode}");
                    }                    
                }
                catch (Exception ex)
                {
                    result.AppendLine($"Задание в планировщике не создано по причине:\r\n{ex}");
                    logger.Error($"Задание в планировщике не создано по причине:\r\n{ex}");
                }              
            }
            else
            {
                result.AppendLine("Некорректное время задания!");
                logger.Error("Некорректное время задания!");
            }
            return result.ToString();
        }

        public static string CreateLetter(MisSettings misSettings)
        {
            StringBuilder result = new StringBuilder();
            string text;
            try
            {
                text = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Letter.txt"));
            }
            catch (Exception ex)
            {
                result.AppendLine($"Не удалось прочитать файл шаблона письма {Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Letter.txt")} по причине:\r\n{ex}");
                logger.Error($"Не удалось прочитать файл шаблона письма {Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Letter.txt")} по причине:\r\n{ex}");
                return result.ToString();
            }
            StringBuilder accountIdsAndContracts = new StringBuilder();
            for (int i = 0; ; i++)
            {
                if (i < misSettings.AccountIds.Count)
                {
                    accountIdsAndContracts.AppendLine($"Код места забора – {misSettings.AccountIds[i]}");
                    if (i < misSettings.Contracts.Count)
                    {
                        accountIdsAndContracts.AppendLine($"Код контракта - {misSettings.Contracts[i]}\r\n");
                    }
                    else
                    {
                        accountIdsAndContracts.AppendLine($"Код контракта - {misSettings.Contracts[0]}\r\n");
                    }
                }
                else if (i < misSettings.Contracts.Count)
                {
                    accountIdsAndContracts.AppendLine($"Код места забора – {misSettings.AccountIds[0]}");
                    accountIdsAndContracts.AppendLine($"Код контракта - {misSettings.Contracts[i]}\r\n");
                }
                else
                {
                    break;
                }                    
            }
            string newText = text.Replace("@LOGIN", (misSettings.SelectedHub + misSettings.Login).Trim())
                .Replace("@PASSWORD", misSettings.Password)
                .Replace("@LABELRANGE", string.IsNullOrEmpty(misSettings.LabelRange) ? string.Empty : $"\r\nДиапазон ШК для тестирования - {misSettings.LabelRange}\r\n\r\nПросьба указывать ФИО тестового пациента: Тест Тест Тест\r\n")
                .Replace("@ACCOUNTSANDCONTRACTS", accountIdsAndContracts.ToString());
            return newText;
        }

        public static async Task<string> SendEmail(string sendTo, string subject, string messageBody)
        {
            try
            {
                MailMessage m = new MailMessage("mail@mail.ru", sendTo, subject, messageBody);                
                SmtpClient smtp = new SmtpClient("smtp.mail.ru");
                m.CC.Add("mail2@mail.ru");
                await smtp.SendMailAsync(m);
                logger.Info($"Письмо отправлено по адресу {sendTo}");
                return $"Письмо отправлено по адресу {sendTo}";
            }
            catch (Exception ex)
            {
                logger.Error($"Письмо не отправлено по адресу {sendTo} по причине:\r\n{ex}");
                return $"Письмо не отправлено по адресу {sendTo} по причине:\r\n{ex}";
            }
        }

    }
}
