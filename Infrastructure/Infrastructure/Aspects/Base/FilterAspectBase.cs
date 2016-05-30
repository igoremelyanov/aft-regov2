using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace AFT.RegoV2.Infrastructure.Aspects.Base
{
    public abstract class FilterAspectBase : IInterceptionBehavior
    {
        private readonly IActorInfoProvider _actorInfoProvider;

        protected FilterAspectBase(IActorInfoProvider actorInfoProvider)
        {
            _actorInfoProvider = actorInfoProvider;
        }

        protected IMethodReturn Filter(IMethodInvocation method, IMethodReturn unfilteredReturn)
        {
            var interfaces = GetType().GetInterfaces()
                .Where(i => i.Name == typeof(IFilter<>).Name);
            var methods = interfaces
                .SelectMany(i => i.GetMethods());
            var interfaceFilters =
                methods
                .Select(m => m.ToString());

            var filters =
                GetType().GetMethods()
                .Where(
                    m =>
                        interfaceFilters.Contains(m.ToString()));

            var appliedFilter = filters.FirstOrDefault(
                f =>
                    f.GetParameters().First().ParameterType.IsAssignableFrom((method.MethodBase as MethodInfo).ReturnType));

            var userId = _actorInfoProvider.Actor.Id;

            var result = appliedFilter != null ? appliedFilter.Invoke(this, new[] {unfilteredReturn.ReturnValue, userId}) :
                unfilteredReturn.ReturnValue;

            return method.CreateMethodReturn(result);
        }

        public IMethodReturn Invoke(IMethodInvocation input,
            GetNextInterceptionBehaviorDelegate getNext)
        {
            var filtered = input.MethodBase.GetCustomAttribute<FilteredAttribute>();

            if (filtered == null)
            {
                return getNext()(input, getNext);
            }

            if (!_actorInfoProvider.IsActorAvailable)
            {
                throw new RegoException("User must be logged in");
            }

            var unfiltered = getNext()(input, getNext);
            
            var result = Filter(input, unfiltered);

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
