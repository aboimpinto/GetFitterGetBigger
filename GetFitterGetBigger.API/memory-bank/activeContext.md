# Active Context

## Application Overview
GetFitterGetBigger is a fitness application ecosystem consisting of:
1. A C# Minimal API with Swagger (this application)
2. A mobile app for clients to perform workouts
3. An admin app for Personal Trainers to manage content

## Current Status
This API serves as the central data processing hub for the GetFitterGetBigger ecosystem. It handles all database operations since neither the mobile app nor the admin app have direct database access.

## Key Responsibilities
- Process, record, retrieve, and update fitness data
- Serve as the intermediary between client applications and the database
- Provide endpoints for workout management
- Support exercise and workout plan creation by trainers
- Enable clients to access and track their fitness activities
