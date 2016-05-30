namespace AFT.RegoV2.AdminApi.Interface.Auth
{
    public class Permission
    {
        public string Name { get; set; }
        public string Module { get; set; }

        public Permission(string name, string module)
        {
            Name = name;
            Module = module;
        }
    }
}