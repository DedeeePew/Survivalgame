using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Lightweight Service Locator pattern. 
    /// Register services on Awake, resolve them anywhere.
    /// No external DI framework needed.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting service: {type.Name}");
            }
            _services[type] = service;
            Debug.Log($"[ServiceLocator] Registered: {type.Name}");
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            Debug.LogError($"[ServiceLocator] Service not found: {type.Name}");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (_services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered: {type.Name}");
            }
        }

        /// <summary>
        /// Call on scene unload or game quit to prevent stale refs.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            Debug.Log("[ServiceLocator] All services cleared.");
        }
    }
}
