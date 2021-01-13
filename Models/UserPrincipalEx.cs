using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace MakeNewFtpClientGui.Models
{
    [DirectoryObjectClass("user")]
    [DirectoryRdnPrefix("CN")]
    public class UserPrincipalEx : UserPrincipal
    {
        public UserPrincipalEx(PrincipalContext context) : base(context) { }
        public UserPrincipalEx(PrincipalContext context, string samAccountName, string password, bool enabled) : base(context, samAccountName, password, enabled) { }

        [DirectoryProperty("cn")]
        public string Cn
        {
            get
            {
                if (ExtensionGet("cn").Length != 1)
                    return string.Empty;
                return (string)ExtensionGet("cn")[0];
            }
            set { ExtensionSet("physicalDeliveryOfficeName", value); }
        }

        [DirectoryProperty("physicalDeliveryOfficeName")]
        public string PhysicalDeliveryOfficeName
        {
            get
            {
                if (ExtensionGet("physicalDeliveryOfficeName").Length != 1)
                    return string.Empty;
                return (string)ExtensionGet("physicalDeliveryOfficeName")[0];
            }
            set { ExtensionSet("physicalDeliveryOfficeName", value); }
        }

        [DirectoryProperty("primaryGroupId")]
        public string PrimaryGroupId
        {
            get
            {
                if (ExtensionGet("primaryGroupId").Length != 1)
                    return string.Empty;
                return (string)ExtensionGet("primaryGroupId")[0];
            }
            set { ExtensionSet("primaryGroupId", value); }
        }

        public static new UserPrincipalEx FindByIdentity(PrincipalContext context, string identityValue)
        {
            return (UserPrincipalEx)FindByIdentityWithType(context, typeof(UserPrincipalEx), identityValue);
        }

        public static new UserPrincipalEx FindByIdentity(PrincipalContext context, IdentityType identityType, string identityValue)
        {
            return (UserPrincipalEx)FindByIdentityWithType(context, typeof(UserPrincipalEx), identityType, identityValue);
        }
    }
}
