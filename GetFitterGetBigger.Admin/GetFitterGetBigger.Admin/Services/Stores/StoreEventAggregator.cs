namespace GetFitterGetBigger.Admin.Services.Stores
{
    public class StoreEventAggregator : IStoreEventAggregator
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly ILogger<StoreEventAggregator> _logger;

        public StoreEventAggregator(ILogger<StoreEventAggregator> logger)
        {
            _logger = logger;
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : class
        {
            var eventType = typeof(TEvent);
            _logger.LogDebug("Publishing event {EventType}: {@EventData}", eventType.Name, eventData);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers.ToList())
                {
                    try
                    {
                        ((Action<TEvent>)handler)(eventData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling event {EventType}", eventType.Name);
                    }
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);
            
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }
            
            _handlers[eventType].Add(handler);
            _logger.LogDebug("Subscribed to event {EventType}", eventType.Name);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);
            
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                _logger.LogDebug("Unsubscribed from event {EventType}", eventType.Name);
            }
        }
    }
}