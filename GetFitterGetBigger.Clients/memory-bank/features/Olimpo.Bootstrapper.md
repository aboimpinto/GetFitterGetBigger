# Olimpo.Bootstrapper

The Olimpo.Bootstrapper module is responsible for initializing and configuring the application at startup. It serves as the entry point for the application and handles the bootstrapping process, which includes:

1. **Dependency Injection**: Registering services and dependencies with the IoC container.
2. **Event Aggregator**: Setting up the event aggregator for communication between loosely coupled components.
3. **Navigation Manager**: Configuring the navigation manager for handling view navigation.
4. **Initial View**: Launching the initial view or splash screen of the application.

The Bootstrapper module acts as the orchestrator, ensuring that all the necessary components are properly initialized and wired together before the application starts running. It follows the Bootstrapper pattern, which separates the application's initialization logic from the main application code, promoting a more modular and maintainable architecture.
