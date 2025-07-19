Review platform-specific implementations to ensure they properly implement shared interfaces and follow platform guidelines.

Instructions:
1. Ask which platform implementation to review (iOS/Android/Web/Desktop)
2. Identify the shared interface/abstraction being implemented
3. Review the platform-specific code for correctness
4. Verify platform guideline compliance
5. Check for proper dependency injection setup
6. Ensure feature parity across platforms

Review checklist:
- **Interface Implementation**: All methods properly implemented
- **Platform Guidelines**: 
  - iOS: Human Interface Guidelines, Swift/Objective-C bridge if needed
  - Android: Material Design, proper Android lifecycle
  - Web: Progressive Web App standards, browser compatibility
  - Desktop: Native OS integration, windowing behavior
- **Resource Access**: Proper permissions, platform APIs used correctly
- **Performance**: Platform-optimized implementations
- **Error Handling**: Platform-specific error scenarios handled
- **Native Features**: Camera, GPS, notifications implemented correctly
- **Storage**: Platform-appropriate storage mechanisms
- **Security**: Platform security features utilized
- **Build Configuration**: Platform-specific build settings correct

Output format:
1. Platform Implementation Overview
2. Interface Compliance Check
3. Platform Guideline Adherence
4. Native Feature Usage
5. Issues Found (with platform impact)
6. Performance Considerations
7. Security Assessment
8. Recommendations
9. Overall Assessment (APPROVED/NEEDS_WORK)

Compare with other platform implementations to ensure consistency.