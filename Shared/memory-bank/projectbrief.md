# GetFitterGetBigger Ecosystem

## Project Brief

The GetFitterGetBigger ecosystem is a comprehensive fitness platform designed to connect Personal Trainers (PTs) with their clients through technology. The system consists of three main components that work together to provide a seamless fitness experience.

## Core Components

1. **GetFitterGetBigger.API**
   - Central data processing hub
   - C# Minimal API with Swagger
   - Handles all database operations
   - Provides endpoints for both Admin and Client applications

2. **GetFitterGetBigger.Admin**
   - Trainer-facing Blazor web application
   - Used by Personal Trainers to create and manage fitness content
   - Allows creation of exercises, workouts, and training plans
   - Enables assignment of plans to specific clients
   - Communicates exclusively through the API layer

3. **GetFitterGetBigger.Clients**
   - Client-facing applications built with Avalonia UI
   - Cross-platform support (Android, iOS, Browser, Desktop)
   - Used by fitness enthusiasts to perform workouts
   - Tracks progress and provides feedback
   - Communicates through the API layer

4. **Shared Models and Code (This Project)**
   - Contains common models used across all components
   - Ensures data consistency throughout the ecosystem
   - Provides shared utilities and helpers

## System Interaction

The components interact in a structured manner:
- Personal Trainers use the Admin application to create fitness content
- This content is stored in the database through the API
- Clients access their assigned content through the Client applications
- Client progress and feedback flow back to Trainers through the API

## Project Goals

1. **Connectivity**: Create a seamless connection between Personal Trainers and their clients
2. **Accessibility**: Provide cross-platform access to fitness content
3. **Customization**: Enable Personal Trainers to create personalized training experiences
4. **Progress Tracking**: Allow both Trainers and clients to monitor fitness progress
5. **Data Consistency**: Ensure consistent data representation across all components

## Scope Boundaries

- The API handles all data persistence and business logic
- The Admin application focuses on content creation and management
- The Client applications focus on workout execution and progress tracking
- Shared models ensure consistent data representation across all components
