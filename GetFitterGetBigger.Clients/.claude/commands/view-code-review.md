Perform a detailed code review of a specific View (Page/Screen) and its ViewModel against MVVM patterns and platform best practices.

Instructions:
1. Ask the user for the View/ViewModel pair to review (e.g., EquipmentPage/EquipmentViewModel)
2. Analyze both the View (XAML/UI) and ViewModel implementation
3. Check MVVM pattern compliance and data binding
4. Verify platform-specific UI implementations if applicable
5. Review navigation, commands, and state management
6. Provide actionable feedback with examples

Review checklist:
- **MVVM Architecture**: Proper separation, no business logic in views
- **Data Binding**: Two-way binding used appropriately, INotifyPropertyChanged
- **Commands**: ICommand implementation, async command handling
- **Navigation**: Using navigation service, parameter passing
- **State Management**: ObservableCollection usage, property change notifications
- **Platform UI**: Respects platform guidelines (iOS/Android/Desktop)
- **Responsive Design**: Adapts to different screen sizes
- **Accessibility**: Screen reader support, navigation aids
- **Performance**: Virtual lists for large data, image optimization
- **Error Handling**: User-friendly error messages, offline scenarios
- **Loading States**: Progress indicators for async operations
- **Memory Management**: Proper cleanup, no memory leaks

Output format:
1. View/ViewModel Overview (purpose, relationships, dependencies)
2. MVVM Architecture Assessment
3. Issues Found (Critical/High/Medium/Low)
4. Platform-Specific Concerns
5. Performance Analysis
6. Positive Aspects
7. Line-by-Line Feedback (both View and ViewModel)
8. Testing Recommendations
9. Overall Assessment (APPROVED/NEEDS_WORK)

Focus on cross-platform compatibility while maintaining platform-specific excellence.