using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using MakeNewFtpClientGui.Models;
using System.IO;
using System.Linq;
using System.Threading;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MakeNewFtpClientGui.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        const string BACKGROUND_COLOR_GOOD = "#A9F5A9";
        const string BACKGROUND_COLOR_BAD = "#F5A9A9";
        //static readonly List<string> mis = new List<string>() { "АльфаЛаб", "Ангелы IT", "Архимед+", "Инфоклиника", "Медиалог", "Медофис", "Реновацио", "Селенда", "Симедклиника", "СмартКлиник", "УМЦ 1БИТ", "CRM soft" };
        static readonly List<string> hubs = new List<string>() { " ", "Ekb", "Krd", "Msk", "Spb" };
        static readonly List<Tuple<string, string>> specialPropertyRegions = new List<Tuple<string, string>>() {
            new Tuple<string, string>("0000000001", "Санкт-Петербург КК"),
            new Tuple<string, string>("0000000003", "Новосибирск"),
            new Tuple<string, string>("0000000004", "Екатеринбург"),
            new Tuple<string, string>("0000000008", "Псков"),
            new Tuple<string, string>("0000000011", "Москва зона 2"),
            new Tuple<string, string>("0000000012", "Череповец"),
            new Tuple<string, string>("0000000014", "Пермь"),
            new Tuple<string, string>("0000000017", "Тюмень"),
            new Tuple<string, string>("0000000018", "Челябинск"),
            new Tuple<string, string>("0000000019", "Санкт-Петербург ДЦ"),
            new Tuple<string, string>("0000000020", "Уфа"),
            new Tuple<string, string>("0000000022", "Санкт-Петербург CITO"),
            new Tuple<string, string>("0000000023", "Санкт-Петербург CITO 2"),
            new Tuple<string, string>("0000000028", "Омск"),
            new Tuple<string, string>("0000000032", "Лен. область ДЦ"),
            new Tuple<string, string>("0000000034", "Москва зона 1"),
            new Tuple<string, string>("0000000038", "Москва CITO"),
            new Tuple<string, string>("0000000039", "Москва зона 0"),
            new Tuple<string, string>("0000000040", "Великий Новгород"),
            new Tuple<string, string>("0000000043", "Магнитогорск"),
            new Tuple<string, string>("0000000044", "Москва зона 3"),
            new Tuple<string, string>("0000000045", "Екатеринбург CITO"),
            new Tuple<string, string>("0000000046", "Санкт-Петербург Сдайнакоронавирус.рф"),
            new Tuple<string, string>("0000000047", "Москва Сдайнакоронавирус.рф"),
            new Tuple<string, string>("0000000048", "Санкт-Петербург зона 2"),
            new Tuple<string, string>("0000000049", "Екатеринбург Сдайнакоронавирус.рф"),
            new Tuple<string, string>("0000000050", "Новосибирск Сдайнакоронавирус.рф"),
            new Tuple<string, string>("0000000051", "Краснодар Сдайнакоронавирус.рф"),
            new Tuple<string, string>("0000000052", "Сдайнакоронавирус.рф"),
            new Tuple<string, string>("0000000053", "Краснодар") };

        public ObservableCollection<MisSettings> Mis { get; }
        public ObservableCollection<string> Hubs { get; }
        public ObservableCollection<Tuple<string, string>> SpecialPropertyRegions { get; }
        public IObservable<bool> canExecuteCreate { get; set; }
        public ReactiveCommand<Unit, Unit> CreateCommand { get; set; }
        public IObservable<bool> canExecuteSendLetter { get; set; }
        public ReactiveCommand<Unit, Unit> SendLetterCommand { get; set; }

        private string scheduleStartTimeBackgroundColor = BACKGROUND_COLOR_BAD;
        public string ScheduleStartTimeBackgroundColor
        {
            get => scheduleStartTimeBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref scheduleStartTimeBackgroundColor, value);
        }

        private string scheduleStartTime;
        public string ScheduleStartTime
        {
            get => scheduleStartTime;
            set
            {
                this.RaiseAndSetIfChanged(ref scheduleStartTime, value);
                ScheduleStartTimeBackgroundColor = (IsDictionariesAdd && !string.IsNullOrEmpty(scheduleStartTime) && Validate.TimeInString(scheduleStartTime)
                    || !IsDictionariesAdd)
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private bool isCreateAdAccount = true;
        public bool IsCreateAdAccount
        {
            get => isCreateAdAccount;
            set
            {
                this.RaiseAndSetIfChanged(ref isCreateAdAccount, value);
                IsEnabledLogin = IsEnabled && value;
                IsEnabledPassword = IsEnabled && value;
            }
        }

        private bool isEnabledLogin = true;
        public bool IsEnabledLogin
        {
            get => isEnabledLogin;            
            set
            {
                this.RaiseAndSetIfChanged(ref isEnabledLogin, value);
                LoginBackgroundColor = IsCreateAdAccount && Validate.Login((SelectedHub + Login).Trim()) 
                    || !IsCreateAdAccount
                   ? BACKGROUND_COLOR_GOOD
                   : BACKGROUND_COLOR_BAD;
            }
        }

        private bool isEnabledPassword = true;
        public bool IsEnabledPassword
        {
            get => isEnabledPassword;
            set
            {
                this.RaiseAndSetIfChanged(ref isEnabledPassword, value);
                PasswordBackgroundColor = IsCreateAdAccount && Validate.Password(Password) || !IsCreateAdAccount
                   ? BACKGROUND_COLOR_GOOD
                   : BACKGROUND_COLOR_BAD;
            }
        }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref isEnabled, value);
                IsEnabledLogin = IsCreateAdAccount && value;
                IsEnabledPassword = IsCreateAdAccount && value;
            }
        }


        private MisSettings selectedMis;
        public MisSettings SelectedMis
        {
            get => selectedMis;
            set => this.RaiseAndSetIfChanged(ref selectedMis, value);
        }

        private string selectedHub;
        public string SelectedHub
        {
            get => selectedHub;
            set => this.RaiseAndSetIfChanged(ref selectedHub, value);
        }

        private Tuple<string, string> selectedSpecialPropertyRegions;
        public Tuple<string, string> SelectedSpecialPropertyRegions
        {
            get => selectedSpecialPropertyRegions;
            set => this.RaiseAndSetIfChanged(ref selectedSpecialPropertyRegions, value);
        }

        private string result;
        public string Result
        {
            get => string.IsNullOrEmpty(result) ? string.Empty : result;
            set => this.RaiseAndSetIfChanged(ref result, value);
        }

        private string letter;
        public string Letter
        {
            get => string.IsNullOrEmpty(letter) ? string.Empty : letter;
            set => this.RaiseAndSetIfChanged(ref letter, value);
        }

        private string loginBackgroundColor = BACKGROUND_COLOR_BAD;
        public string LoginBackgroundColor
        {
            get => loginBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref loginBackgroundColor, value);
        }

        private string login;
        public string Login
        {
            get => string.IsNullOrEmpty(login) ? string.Empty : login;            
            set
            {
                this.RaiseAndSetIfChanged(ref login, value);
                LoginBackgroundColor = IsCreateAdAccount && Validate.Login((SelectedHub + login).Trim())
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private string passwordBackgroundColor = BACKGROUND_COLOR_BAD;
        public string PasswordBackgroundColor
        {
            get => passwordBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref passwordBackgroundColor, value);                
        }

        private string password;
        public string Password
        {
            get => string.IsNullOrEmpty(password) ? string.Empty : password;
            set
            {
                this.RaiseAndSetIfChanged(ref password, value);
                PasswordBackgroundColor = IsCreateAdAccount && Validate.Password(value)
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private string letterMailsBackgroundColor = BACKGROUND_COLOR_BAD;
        public string LetterMailsBackgroundColor
        {
            get =>  letterMailsBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref letterMailsBackgroundColor, value);
        }

        private string letterMails;
        public string LetterMails
        {
            get => string.IsNullOrEmpty(letterMails) ? string.Empty : letterMails;
            set
            {
                this.RaiseAndSetIfChanged(ref letterMails, value);
                LetterMailsBackgroundColor = (!string.IsNullOrEmpty(letterMails) && letterMails.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(z => Validate.Email(z.Trim()))
                    .Count()
                    == letterMails.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Count())
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private string letterSubjectBackgroundColor = BACKGROUND_COLOR_BAD;
        public string LetterSubjectBackgroundColor
        {
            get =>  letterSubjectBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref letterSubjectBackgroundColor, value);
        }

        private string letterSubject;
        public string LetterSubject
        {
            get => string.IsNullOrEmpty(letterSubject) ? string.Empty : letterSubject;
            set
            {
                this.RaiseAndSetIfChanged(ref letterSubject, value);
                LetterSubjectBackgroundColor = string.IsNullOrEmpty(letterSubject) ? BACKGROUND_COLOR_BAD : BACKGROUND_COLOR_GOOD;
            }
        }

        private string accountIdBackgroundColor = BACKGROUND_COLOR_BAD;
        public string AccountIdBackgroundColor
        {
            get => accountIdBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref accountIdBackgroundColor, value);
        }

        private string accountId;
        public string AccountId
        {
            get => string.IsNullOrEmpty(accountId) ? string.Empty : accountId;
            set
            {
                this.RaiseAndSetIfChanged(ref accountId, value);
                AccountIdBackgroundColor = accountId.Length > 4 && accountId.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(z => Validate.AccountId(z.Trim()))
                    .Count()
                    == accountId.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Count()
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private string contractBackgroundColor = BACKGROUND_COLOR_BAD;
        public string ContractBackgroundColor
        {
            get => contractBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref contractBackgroundColor, value);
        }

        private string contract;
        public string Contract
        {
            get => string.IsNullOrEmpty(contract) ? string.Empty : contract;
            set
            {
                this.RaiseAndSetIfChanged(ref contract, value);
                ContractBackgroundColor = contract.Length > 9 && contract.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(z => Validate.Contract(z.Trim())).Count() == contract.Split(',', StringSplitOptions.RemoveEmptyEntries).Count()
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private string emailBackgroundColor = BACKGROUND_COLOR_GOOD;
        public string EmailBackgroundColor
        {
            get => emailBackgroundColor;
            set => this.RaiseAndSetIfChanged(ref emailBackgroundColor, value);
        }

        private string email;
        public string Email
        {
            get => string.IsNullOrEmpty(email) ? string.Empty : email;
            set
            {
                this.RaiseAndSetIfChanged(ref email, value);
                EmailBackgroundColor = (string.IsNullOrEmpty(email) 
                    || email.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Where(z => Validate.Email(z.Trim()))
                        .Count() 
                        == email.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Count())
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private string labelRange;
        public string LabelRange
        {
            get => string.IsNullOrEmpty(labelRange) ? string.Empty : labelRange;
            set => this.RaiseAndSetIfChanged(ref labelRange, value);
        }

        private bool isByContract;
        public bool IsByContract
        {
            get => isByContract;
            set => this.RaiseAndSetIfChanged(ref isByContract, value);
        }

        private bool isDictionariesAdd = true;
        public bool IsDictionariesAdd
        {
            get => isDictionariesAdd;
            set
            {
                this.RaiseAndSetIfChanged(ref isDictionariesAdd, value);
                ScheduleStartTimeBackgroundColor = IsDictionariesAdd && !string.IsNullOrEmpty(scheduleStartTime) && Validate.TimeInString(scheduleStartTime)
                    || !IsDictionariesAdd
                    ? BACKGROUND_COLOR_GOOD
                    : BACKGROUND_COLOR_BAD;
            }
        }

        private bool isOrdersAdd = true;
        public bool IsOrdersAdd
        {
            get => isOrdersAdd;
            set => this.RaiseAndSetIfChanged(ref isOrdersAdd, value);
        }

        private bool isResultsAdd = true;
        public bool IsResultsAdd
        {
            get =>  isResultsAdd;
            set => this.RaiseAndSetIfChanged(ref isResultsAdd, value);
        }

        public MainWindowViewModel()
        {
            GetSettings.Get();
            IsEnabled = true;
            Mis = new ObservableCollection<MisSettings>(new List<MisSettings>()
            {
                new MisSettings("CRM soft", new List<string>() { "In", "Out", "Dictionaries" }, true),
                new MisSettings("АльфаЛаб", new List<string>() { "In", "Out", "Dictionaries" }, false),
                new MisSettings("Ангелы IT", new List<string>() { "In", "Out", "Dictionaries" }, false),
                new MisSettings("Архимед+", new List<string>() { "In", "Out", "Dictionaries" }, false),
                new MisSettings("Инфоклиника", new List<string>() { "In", "Out", "Dictionaries" }, false),
                new MisSettings("Медиалог", new List<string>() { "In", "Out", "Dictionaries" }, true),
                new MisSettings("Медофис", new List<string>() { "In", "Out", "Dictionaries" }, true),
                new MisSettings("Реновацио", new List<string>() { "In", "Out", "Dictionaries" }, false),
                new MisSettings("Селенда", new List<string>() { "In", "Out", "Dictionaries", "ComplexEndings" }, true),
                new MisSettings("Симедклиника", new List<string>() { "In", "Out", "Dictionaries" }, true),
                new MisSettings("СмартКлиник", new List<string>() { "In", "Out", "Dictionaries", "ComplexEndings" }, true),
                new MisSettings("УМЦ 1БИТ", new List<string>() { "In", "Out", "Dictionaries", "Out\\Archive", "Out\\NotProcessed" }, false)
            });
            SelectedMis = Mis[0];
            Hubs = new ObservableCollection<string>(new List<string>(hubs));
            SelectedHub = Hubs[0];
            SpecialPropertyRegions = new ObservableCollection<Tuple<string, string>>(specialPropertyRegions);
            SelectedSpecialPropertyRegions = SpecialPropertyRegions[0];
            canExecuteCreate = this.WhenAnyValue(
                x => x.SelectedHub,
                x => x.LoginBackgroundColor,
                x => x.PasswordBackgroundColor,
                x => x.AccountIdBackgroundColor,
                x => x.ContractBackgroundColor,
                x => x.SelectedSpecialPropertyRegions.Item1,
                x => x.EmailBackgroundColor,
                x => x.IsDictionariesAdd,
                x => x.IsOrdersAdd,
                x => x.IsResultsAdd,
                x => x.ScheduleStartTimeBackgroundColor,
                (sh, l, p, ai, c, spr, e, ida, ioa, ira, sst) =>
                !string.IsNullOrEmpty(sh)
                && (ida || ioa || ira)
                && e == BACKGROUND_COLOR_GOOD
                && sst == BACKGROUND_COLOR_GOOD
                && l == BACKGROUND_COLOR_GOOD
                && p == BACKGROUND_COLOR_GOOD
                && ai == BACKGROUND_COLOR_GOOD
                && c == BACKGROUND_COLOR_GOOD
                );
            CreateCommand = ReactiveCommand.CreateFromTask(async () => await Task.Factory.StartNew(() => Create()), canExecuteCreate);
            canExecuteSendLetter = this.WhenAnyValue(
                x => x.LetterMailsBackgroundColor,
                x => x.LetterSubjectBackgroundColor,
                (lm, ls) =>
                lm == BACKGROUND_COLOR_GOOD
                && ls == BACKGROUND_COLOR_GOOD
                );
            SendLetterCommand = ReactiveCommand.CreateFromTask(() => Task.Factory.StartNew(() => SendLetter()), canExecuteSendLetter);
        }

        internal void GeneratePassword()
        {
            if (IsEnabledPassword)
            {
                StringBuilder pass = new StringBuilder();
                do
                {
                    pass.Clear();
                    for (int i = 0; i < 10; i++)
                    {
                        pass.Append(Validate.FORPASSWORD[new Random().Next(0, Validate.FORPASSWORD.Length - 1)]);
                    }
                }
                while (!Validate.Password(pass.ToString()));
                Password = pass.ToString();
            }
        }

        internal void Create()
        {
            IsEnabled = false;
            Letter = string.Empty;
            SelectedMis.SelectedHub = SelectedHub;
            SelectedMis.AccountId = AccountId;
            SelectedMis.Contract = Contract;
            SelectedMis.Login = Login;
            SelectedMis.Password = Password;
            SelectedMis.SpecialPropertiesRegion = SelectedSpecialPropertyRegions.Item1;
            SelectedMis.Email = Email;
            SelectedMis.LabelRange = LabelRange;
            SelectedMis.IsByContract = IsByContract;
            SelectedMis.IsDictionariesAdd = isDictionariesAdd;
            SelectedMis.IsOrdersAdd = IsOrdersAdd;
            SelectedMis.IsResultsAdd = IsResultsAdd;
            if (IsDictionariesAdd && !string.IsNullOrEmpty(ScheduleStartTime))
            {
                SelectedMis.ScheduleStartTime = ScheduleStartTime;
            }
            bool isCreated = true;
            if (IsCreateAdAccount)
            {
                isCreated = Make.AdAccount((SelectedMis.SelectedHub + SelectedMis.Login).Trim(), SelectedMis.Password, out string textResultMakeAdAccount);
                Result += textResultMakeAdAccount + "\r\n";
            }
            if (isCreated)
            {                
                if (Make.Directories(SelectedMis, out string textResultMakeDirectories))
                {
                    Result += textResultMakeDirectories + "\r\n";
                    if (SelectedMis.IsDictionariesAdd)
                    {
                        Result += Make.SetDictionariesSettings(SelectedMis) + "\r\n";
                    }
                    if (SelectedMis.IsOrdersAdd)
                    {
                        Result += Make.SetOrdersSettings(SelectedMis) + "\r\n";
                    }
                    if (SelectedMis.IsResultsAdd)
                    {
                        Result += Make.SetResultsSettings(SelectedMis) + "\r\n";
                    }
                    Letter = Make.CreateLetter(SelectedMis);
                }
                else
                {
                    Result += textResultMakeDirectories + "\r\n";
                }
            }            
            IsEnabled = true;
        }

        internal async void SendLetter()
        {
            Result += await Make.SendEmail(LetterMails, LetterSubject, Letter);
        }


    }
}
