using System;
using System.Collections.Generic;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Core.Managers
{
    /// <summary>
    /// Marker for strongly typed game events published through the EventBus.
    /// </summary>
    public interface IGameEvent
    {
    }

    /// <summary>
    /// Strongly typed publish/subscribe event system. Avoids UnityEvents and string-based dispatch.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            Guard.AgainstNull(handler, nameof(handler));

            var eventType = typeof(T);
            if (!Subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                Subscribers[eventType] = handlers;
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            Guard.AgainstNull(handler, nameof(handler));

            if (Subscribers.TryGetValue(typeof(T), out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public static void Publish<T>(in T gameEvent) where T : struct, IGameEvent
        {
            if (!Subscribers.TryGetValue(typeof(T), out var handlers))
            {
                return;
            }

            var snapshot = handlers.ToArray();
            foreach (var handler in snapshot)
            {
                ((Action<T>)handler).Invoke(gameEvent);
            }
        }

        public static void Clear()
        {
            Subscribers.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Clear();
        }
    }
}
