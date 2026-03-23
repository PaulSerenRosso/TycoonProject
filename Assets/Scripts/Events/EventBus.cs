using System;
using System.Collections.Generic;

namespace Core.Events
{
    public class EventBus 
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Publish<T>(T eventData) where T : class
        {
            if (eventData == null) return;

            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers.ToArray()) // ToArray to avoid modification during iteration
                {
                    try
                    {
                        ((Action<T>)handler)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error handling event {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            var eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }

            _handlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
        }
    }
}
