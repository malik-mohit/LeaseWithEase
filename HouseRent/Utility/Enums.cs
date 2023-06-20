using System.ComponentModel;

namespace HouseRent.Utility
{
    public class Enums
    {
        public enum UserRoles
        {
            [Description("Admin")]
            Admin = 1,
            [Description("Customer")]
            Customer = 2,
            [Description("Owner")]
            Owner = 3,
        }

        public static class ConstUserRoles
        {
            public const string Admin = "Admin";
            public const string Customer = "Customer";
            public const string Owner = "Owner";
        }
    }
}
