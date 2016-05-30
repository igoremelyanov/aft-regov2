namespace AFT.RegoV2.Core.Player.Interface.Data
{
    public static class PlayerEnums
    {
        public enum Gender
        {
            Male,
            Female
        }

        public enum Title
        {
            Mr,
            Ms,
            Mrs,
            Miss
        }

        public enum IdStatus
        {
            Verified,
            Unverified
        }

        public enum ContactMethod
        {
            Email,
            Chat,
            Phone,
            Sms
        }

        public enum TimeOut
        {
            _24Hrs,
            Week,
            Month,
            _6Weeks
        }

        public enum SelfExclusion
        {
            _6months,
            _1Year,
            _5Years,
            Permanent
        }
    }
}
