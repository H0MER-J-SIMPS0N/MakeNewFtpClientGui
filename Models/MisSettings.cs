using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace MakeNewFtpClientGui.Models
{
    public class MisSettings: IReactiveObject
    { 
        public string Name { get; private set; }
        public List<string> FoldersToCreate { get; private set; }


        private string selectedHub;
        public string SelectedHub
        {
            get => selectedHub;
            set => this.RaiseAndSetIfChanged(ref selectedHub, value);
        }


        private string login;
        public string Login
        {
            get => login;
            set => this.RaiseAndSetIfChanged(ref login, value);
        }

        private string accountId;
        public string AccountId
        {
            get => accountId;
            set => this.RaiseAndSetIfChanged(ref accountId, value);
        }

        public List<string> AccountIds
        {
            get => AccountId?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }


        private string contract;
        public string Contract
        {
            get => contract;
            set => this.RaiseAndSetIfChanged(ref contract, value);
        }
        public List<string> Contracts 
        {
            get => Contract?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }

        private string specialPropertiesRegion;
        public string SpecialPropertiesRegion
        {
            get => specialPropertiesRegion;
            set => this.RaiseAndSetIfChanged(ref specialPropertiesRegion, value);
        }


        private bool isByContract;
        public bool IsByContract
        {
            get => isByContract;
            set => this.RaiseAndSetIfChanged(ref isByContract, value);
        }

        public List<string> Emails
        {
            get => Email?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }

        private string email;
        public string Email
        {
            get => email;
            set => this.RaiseAndSetIfChanged(ref email, value);
        }


        private string labelRange;
        public string LabelRange
        {
            get => labelRange;
            set => this.RaiseAndSetIfChanged(ref labelRange, value);
        }


        private bool isDictionariesAdd;
        public bool IsDictionariesAdd
        {
            get => isDictionariesAdd;
            set => this.RaiseAndSetIfChanged(ref isDictionariesAdd, value);
        }


        private bool isOrdersAdd;
        public bool IsOrdersAdd
        {
            get => isOrdersAdd;
            set => this.RaiseAndSetIfChanged(ref isOrdersAdd, value);
        }

        private bool isResultsAdd;
        public bool IsResultsAdd
        {
            get => isResultsAdd;
            set => this.RaiseAndSetIfChanged(ref isResultsAdd, value);
        }

        private string password;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);                
        }

        private string scheduleStartTime;
        public string ScheduleStartTime
        {
            get => scheduleStartTime;
            set => this.RaiseAndSetIfChanged(ref scheduleStartTime, value);
         }

        public MisSettings()
        {

        }

        public MisSettings(string name, List<string> foldersToCreate, bool isByContract)
        {
            Name = name;
            FoldersToCreate = foldersToCreate;
            IsByContract = isByContract;
        }        

        public override string ToString()
        {
            return $"SelectedHub - {SelectedHub}\r\n" +
                $"Login - {Login}\r\n" +
                $"Password - {Password}\r\n" +
                $"AccountId - {AccountId}\r\n" +
                $"SpecialPropertiesRegion - {SpecialPropertiesRegion}\r\n" +
                $"LabelRange - {LabelRange}\r\n" +
                $"IsByContract - {IsByContract}\r\n" +
                $"IsDictionariesAdd - {IsDictionariesAdd}\r\n" +
                $"IsOrdersAdd - {IsOrdersAdd}\r\n" +
                $"IsResultsAdd - {IsResultsAdd}\r\n" +
                $"Email - {Email}\r\n";
        }

        public void RaisePropertyChanging(PropertyChangingEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
