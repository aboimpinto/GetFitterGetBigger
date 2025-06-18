# Olimpo.EventAggregator

## Overview
The `Olimpo.EventAggregator` is part of the Olimpo developer suite and implements the event aggregator pattern to decouple modules by allowing them to communicate via messages.

## Key Components
- `IEventAggregator`: Interface for subscribing and publishing messages.
- `EventAggregator`: Implementation of `IEventAggregator`.
- `IHandle<T>`: Interface for synchronous message handling.
- `IHandleAsync<T>`: Interface for asynchronous message handling.

## Usage
### Subscribers
1. Implement `IHandle<T>` or `IHandleAsync<T>` to handle messages of type `T`.
2. Inject `IEventAggregator` and call `eventAggregator.Subscribe(this)` to subscribe to messages.

### Publishers
1. Inject `IEventAggregator` and call `eventAggregator.PublishAsync(message)` to publish a message.

## Example
```csharp
// Subscriber
public class MySubscriber : IHandle<MyMessage>
{
    public MySubscriber(IEventAggregator eventAggregator)
    {
        eventAggregator.Subscribe(this);
    }

    public void Handle(MyMessage message)
    {
        // Handle the message
    }
}

// When the class Handles an Event from EventAggregator, it's necessary to have in contructor the eventAggregator.Subscribe(this).

// Publisher
public class MyPublisher
{
    private readonly IEventAggregator _eventAggregator;

    public MyPublisher(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public async Task PublishMessageAsync()
    {
        await _eventAggregator.PublishAsync(new MyMessage());
    }
}
