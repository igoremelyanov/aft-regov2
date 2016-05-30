namespace AFT.RegoV2.Shared.Caching
{
    public enum CacheType : byte
    {
        // please create different cache types for different cases

        Any = 0,
        Generic = 1, // let's refrain from using that as much as we can
        Test = 2, // to be used for tests

        GamesRequestToMemberApi = 3,

    }
}
