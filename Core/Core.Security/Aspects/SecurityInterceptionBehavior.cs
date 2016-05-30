using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace AFT.RegoV2.Core.Security.Aspects
{
    public class SecurityInterceptionBehavior : IInterceptionBehavior
    {
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IAuthQueries _authQueries;

        public SecurityInterceptionBehavior(IActorInfoProvider actorInfoProvider, IAuthQueries authQueries)
        {
            _actorInfoProvider = actorInfoProvider;
            _authQueries = authQueries;
        }

        public IMethodReturn Invoke(IMethodInvocation input,
          GetNextInterceptionBehaviorDelegate getNext)
        {
            var methodBaseClass = input.MethodBase;
            if (input.MethodBase.DeclaringType != null && input.MethodBase.DeclaringType.IsInterface)
            {
                // Map interface method to implementing class method which contains the Permission attributes
                var targetType = input.Target.GetType();
                var parameterTypes = input.MethodBase.GetParameters().Select(par => par.ParameterType).ToArray();
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
                methodBaseClass = targetType.GetMethod(input.MethodBase.ReflectedType.FullName + "." + input.MethodBase.Name, flags, Type.DefaultBinder, parameterTypes, null) ??
                    targetType.GetMethod(input.MethodBase.Name, flags, Type.DefaultBinder, parameterTypes, null) ??
                        input.MethodBase;
            }

            var permissionAttrs = methodBaseClass.GetCustomAttributes(typeof(PermissionAttribute), true);

            if (permissionAttrs.Any())
            {
                if (_actorInfoProvider.IsActorAvailable == false)
                    throw new RegoException("Current user data is not accessible.");

                var userId = _actorInfoProvider.Actor.Id;
                var allowed = permissionAttrs.Select(p => (PermissionAttribute)p).Aggregate(false, (current, permission) => current
                                                                                                                             || _authQueries.VerifyPermission(userId,
                                                                                                                                 permission.Permission, permission.Module));

                if (!allowed)
                    throw new InsufficientPermissionsException(
                        string.Format("User \"{0}\" has insufficient permissions for the operation ", _actorInfoProvider.Actor.UserName));
            }

            // Invoke the next behavior in the chain.
            var result = getNext()(input, getNext);

            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }
}
