using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MakeNewFtpClientGui.Models
{
    public static class Validate
    {
        public const string FORPASSWORD = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ01234565789!@#$%*{}()[]";
        public static bool Login(string login)
        {
            string regex = @"^([a-zA-Z0-9]{2,})$";
            return login.Length > 5 && new Regex(regex).IsMatch(login);
        }

        public static bool Password(string password)
        {
            string regex = @"^(.{0,7}|[^" + FORPASSWORD.Substring(0, 25) +
                @"]*|[^" + FORPASSWORD.Substring(25, 25) +
                @"]*|[^" + FORPASSWORD.Substring(50, 10) +
                @"]*|[^" + FORPASSWORD.Substring(60) +
                @"]*|[" + FORPASSWORD.Substring(0, 60) + @"]*)$";
            return password.Length > 7 && !new Regex(regex).IsMatch(password);
        }

        public static bool AccountId(string accountId)
        {
            string regex = @"^(\d{5,5})$";
            return new Regex(regex).IsMatch(accountId);
        }

        public static bool Contract(string contract)
        {
            string regex = @"^(C\d{9})$";
            return new Regex(regex).IsMatch(contract);
        }

        public static bool Email(string email)
        {
            string regex = @"^((([0-9A-Za-z]{1}[-0-9A-z.]{1,}[0-9A-Za-z]{1})|([0-9А-Яа-я]{1}[-0-9А-я.]{1,}[0-9А-Яа-я]{1}))@([-A-Za-z0-9]{1,}\.){1,2}[-A-Za-z]{2,})$";
            return new Regex(regex).IsMatch(email);
        }

        public static bool TimeInString(string timeInString)
        {
            bool validate = false;
            string[] timeInStringSplitted = timeInString.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (timeInStringSplitted.Length == 2)
            {
                int first;
                int.TryParse(timeInStringSplitted[0], out first);
                if (first > -1 && first < 25)
                {
                    int second;
                    int.TryParse(timeInStringSplitted[1], out second);
                    if (second > -1 && first < 61)
                    {
                        validate = true;
                    }
                }
            }
            else if (timeInStringSplitted.Length == 3)
            {
                int.TryParse(timeInStringSplitted[0], out int first);
                if (first > -1 && first < 25)
                {
                    int.TryParse(timeInStringSplitted[1], out int second);
                    if (second > -1 && second < 61)
                    {
                        int.TryParse(timeInStringSplitted[2], out int third);
                        if (third > -1 && third < 61)
                        {
                            validate = true;
                        }
                    }
                }
            }
            return validate;
        }

    }

}
