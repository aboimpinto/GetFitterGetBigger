namespace GetFitterGetBigger.Admin.Services.Stores
{
    public interface IStoreEventAggregator
    {
        void Publish<TEvent>(TEvent eventData) where TEvent : class;
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    }
}