namespace AFT.RegoV2.Core.Auth.Interface
{
    public enum ErrorsCodes
    {
        ActorDoesNotExist,
        ActorPasswordIsNotValid,
        ActorAlreadyCreated,
        UsernameIsEmpty,
        PasswordIsEmpty,
        ModuleOrPermissionNameIsEmpty,
        DuplicatePermission,
        PasswordsMatch,
        RoleAlreadyCreated,
        RoleNotFound,
        PermissionIsNotRegistered
    }
}