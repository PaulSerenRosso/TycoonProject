using System;
using System.Collections.Generic;
using Logger;

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
                        throw new Exception("Error handling event", ex);
                    }
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null)
            {
                Log.Default.Log(new LogEntry(LogLevel.Error, "Handler is null", typeof(EventBus).Name));
                return;
            }

            var eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }

            _handlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null)
            {
                Log.Default.Log(new LogEntry(LogLevel.Error, "Handler is null", typeof(EventBus).Name));
                return;
            }

            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
            else
            {
                Log.Default.Log(new LogEntry(LogLevel.Error, $"No subscribers found for event type: {eventType.Name}",
                    typeof(EventBus).Name));
            }
        }
    }
}
