namespace AFT.RegoV2.Core.Player.Validators
{
    public class CommonPlayerSettings
    {
        public static int PasswordMinLength => 6;
        public static int PasswordMaxLength => 12;

        public static int FirstNameMinLength => 1;
        public static int FirstNameMaxLength => 50;
        public static string FirstNamePattern => @"^[\p{L}0-9\-\'_\.]+$";

        public static int LastNameMinLength => 1;
        public static int LastNameMaxLength => 20;
        public static string LastNamePattern => @"^[\p{L}0-9\-\'_\.]+$";

        public static string EmailPattern => @"^([\!#\$%&'\*\+/\=?\^`\{\|\}~a-zA-Z0-9_-]+[\.]?)+[\!#\$%&'\*\+/\=?\^`\{\|\}~a-zA-Z0-9_-]+@{1}((([0-9A-Za-z_-]+)([\.]{1}[0-9A-Za-z_-]+)*\.{1}([A-Za-z]){1,6})|(([0-9]{1,3}[\.]{1}){3}([0-9]{1,3}){1}))$";

        public static string PhonePattern => @"^((\\+)|(00)|(\\*)|())[0-9]{3,14}((\\#)|())$";

        public static string UsernamePatter => @"^[A-Za-z0-9-_\.\']*$";
    }
}
