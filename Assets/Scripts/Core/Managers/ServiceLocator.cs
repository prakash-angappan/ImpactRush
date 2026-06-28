using System;
using System.Collections.Generic;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Core.Managers
{
    /// <summary>
    /// Lightweight service registry used only at the composition root.
    /// Not a service singleton — consumers receive dependencies explicitly when possible.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<T>(T service) where T : class
        {
            Guard.AgainstNull(service, nameof(service));
            Services[typeof(T)] = service;
        }

        public static void Register(Type serviceType, object service)
        {
            Guard.AgainstNull(serviceType, nameof(serviceType));
            Guard.AgainstNull(service, nameof(service));
            Services[serviceType] = service;
        }

        public static T Get<T>() where T : class
        {
            if (TryGet<T>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var instance) && instance is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            Services.Remove(typeof(T));
        }

        public static void Clear()
        {
            Services.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Clear();
        }
    }
}
