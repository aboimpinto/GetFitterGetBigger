# Clients-Specific Code Quality Standards - GetFitterGetBigger

**🎯 PURPOSE**: Multi-platform client-specific code quality standards that extend the universal standards for the GetFitterGetBigger Clients projects. These standards apply to Web, Android, iOS, and Desktop applications.

## 📋 Prerequisites

This document extends the universal `CODE_QUALITY_STANDARDS.md`. Read that first, then apply these platform-specific standards.

---

## 🎯 Cross-Platform Patterns

### 1. **Platform Abstraction Layer**
Isolate platform-specific code:

```csharp
// ❌ BAD - Platform checks scattered throughout
public class DataService
{
    public async Task SaveData(string data)
    {
        if (Device.RuntimePlatform == Device.iOS)
        {
            await File.WriteAllTextAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "data.txt"), data);
        }
        else if (Device.RuntimePlatform == Device.Android)
        {
            await File.WriteAllTextAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "data.txt"), data);
        }
    }
}

// ✅ GOOD - Platform abstraction
public interface IFileService
{
    Task SaveDataAsync(string filename, string data);
    Task<string> LoadDataAsync(string filename);
}

// Platform implementations
public class IOSFileService : IFileService { }
public class AndroidFileService : IFileService { }
public class WindowsFileService : IFileService { }
```

### 2. **Responsive Design Pattern**
Design once, adapt everywhere:

```csharp
// ❌ BAD - Hardcoded dimensions
<Grid>
    <Frame Width="400" Height="600">
        <Label FontSize="16" />
    </Frame>
</Grid>

// ✅ GOOD - Responsive dimensions
<Grid>
    <Frame MaxWidth="400" HorizontalOptions="FillAndExpand">
        <Label FontSize="{OnPlatform iOS=16, Android=14, Default=16}"
               Style="{StaticResource BodyTextStyle}" />
    </Frame>
</Grid>
```

### 3. **Resource Management**
Centralize and organize resources:

```
/Resources/
├── Strings/
│   ├── AppResources.resx        # Default (English)
│   ├── AppResources.es.resx     # Spanish
│   └── AppResources.pt.resx     # Portuguese
├── Images/
│   ├── icon.png
│   ├── icon@2x.png              # iOS Retina
│   └── icon@3x.png              # iOS Retina HD
├── Styles/
│   ├── Colors.xaml
│   ├── Fonts.xaml
│   └── Themes/
│       ├── LightTheme.xaml
│       └── DarkTheme.xaml
```

---

## 🏗️ Architecture Standards

### 1. **MVVM Pattern (Mandatory)**
Strict Model-View-ViewModel separation:

```csharp
// ❌ BAD - Logic in code-behind
public partial class EquipmentPage : ContentPage
{
    private List<Equipment> _equipment;
    
    public EquipmentPage()
    {
        InitializeComponent();
        LoadData();
    }
    
    private async void LoadData()
    {
        var response = await HttpClient.GetAsync("api/equipment");
        _equipment = await response.Content.ReadAsAsync<List<Equipment>>();
        EquipmentList.ItemsSource = _equipment;
    }
    
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var id = button.CommandParameter.ToString();
        await HttpClient.DeleteAsync($"api/equipment/{id}");
        _equipment.RemoveAll(e => e.Id == id);
        EquipmentList.ItemsSource = _equipment;
    }
}

// ✅ GOOD - MVVM with proper separation
// View (XAML)
<ContentPage x:Class="GetFitterGetBigger.Views.EquipmentPage"
             BindingContext="{Binding Source={RelativeSource Self}}">
    <CollectionView ItemsSource="{Binding ViewModel.Equipment}"
                    IsRefreshing="{Binding ViewModel.IsRefreshing}"
                    RefreshCommand="{Binding ViewModel.RefreshCommand}">
        <CollectionView.ItemTemplate>
            <DataTemplate>
                <SwipeView>
                    <SwipeView.RightItems>
                        <SwipeItem Text="Delete"
                                   Command="{Binding Source={RelativeSource AncestorType={x:Type vm:EquipmentViewModel}}, 
                                            Path=DeleteCommand}"
                                   CommandParameter="{Binding Id}" />
                    </SwipeView.RightItems>
                    <Label Text="{Binding Name}" />
                </SwipeView>
            </DataTemplate>
        </CollectionView.ItemTemplate>
    </CollectionView>
</ContentPage>

// ViewModel
public class EquipmentViewModel : BaseViewModel
{
    private readonly IEquipmentService _equipmentService;
    private ObservableCollection<EquipmentDto> _equipment;
    
    public ObservableCollection<EquipmentDto> Equipment
    {
        get => _equipment;
        set => SetProperty(ref _equipment, value);
    }
    
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }
    
    public EquipmentViewModel(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
        RefreshCommand = new AsyncCommand(LoadEquipmentAsync);
        DeleteCommand = new AsyncCommand<string>(DeleteEquipmentAsync);
    }
    
    private async Task LoadEquipmentAsync()
    {
        var result = await _equipmentService.GetAllAsync();
        if (result.IsSuccess)
        {
            Equipment = new ObservableCollection<EquipmentDto>(result.Data);
        }
    }
}
```

### 2. **Service Layer Pattern**
Services handle all external communication:

```csharp
public interface IApiService
{
    Task<T> GetAsync<T>(string endpoint);
    Task<T> PostAsync<T>(string endpoint, object data);
    Task<T> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConnectivityService _connectivity;
    private readonly IAuthService _auth;
    
    public async Task<T> GetAsync<T>(string endpoint)
    {
        if (!_connectivity.IsConnected)
            throw new NoNetworkException();
            
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

### 3. **Navigation Pattern**
Centralized navigation service:

```csharp
// ❌ BAD - Direct navigation in ViewModels
public class LoginViewModel
{
    private async Task LoginAsync()
    {
        if (await AuthService.LoginAsync(Username, Password))
        {
            Application.Current.MainPage = new NavigationPage(new HomePage());
        }
    }
}

// ✅ GOOD - Navigation service
public interface INavigationService
{
    Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel;
    Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : BaseViewModel;
    Task NavigateBackAsync();
    Task SetRootAsync<TViewModel>() where TViewModel : BaseViewModel;
}

public class LoginViewModel
{
    private readonly INavigationService _navigation;
    
    private async Task LoginAsync()
    {
        if (await AuthService.LoginAsync(Username, Password))
        {
            await _navigation.SetRootAsync<HomeViewModel>();
        }
    }
}
```

---

## 📱 Platform-Specific Standards

### 1. **iOS Standards**
```csharp
// Info.plist considerations
// - Privacy descriptions for camera, location, etc.
// - App Transport Security settings
// - Background modes configuration

// Platform-specific code
[assembly: Dependency(typeof(IOSStatusBar))]
public class IOSStatusBar : IStatusBarService
{
    public void SetStatusBarColor(Color color)
    {
        UIApplication.SharedApplication.StatusBarStyle = 
            color.Luminosity > 0.5 ? UIStatusBarStyle.DarkContent : UIStatusBarStyle.LightContent;
    }
}
```

### 2. **Android Standards**
```csharp
// Manifest considerations
// - Permissions declared and requested at runtime
// - Proper activity configurations
// - Service declarations

// Platform-specific code
[assembly: Dependency(typeof(AndroidStatusBar))]
public class AndroidStatusBar : IStatusBarService
{
    public void SetStatusBarColor(Color color)
    {
        var activity = Platform.CurrentActivity;
        var window = activity.Window;
        window.SetStatusBarColor(color.ToAndroid());
    }
}
```

### 3. **Web Standards**
```javascript
// Progressive Web App requirements
// - Service worker for offline capability
// - Web app manifest
// - HTTPS required

// Platform detection
export const DeviceService = {
    isIOS: () => /iPad|iPhone|iPod/.test(navigator.userAgent),
    isAndroid: () => /Android/.test(navigator.userAgent),
    isMobile: () => /Mobile|Android|iPhone/.test(navigator.userAgent),
    
    getDeviceInfo: () => ({
        platform: DeviceService.getPlatform(),
        isMobile: DeviceService.isMobile(),
        screenSize: {
            width: window.innerWidth,
            height: window.innerHeight
        }
    })
};
```

---

## 🚀 Performance Standards

### 1. **Image Optimization**
```csharp
// ❌ BAD - Loading full resolution always
<Image Source="exercise_photo.jpg" />

// ✅ GOOD - Responsive image loading
<Image>
    <Image.Source>
        <UriImageSource Uri="{Binding ImageUrl}" 
                        CachingEnabled="True"
                        CacheValidity="7.00:00:00" />
    </Image.Source>
</Image>

// Or use FFImageLoading for advanced scenarios
<ffimageloading:CachedImage Source="{Binding ImageUrl}"
                           DownsampleToViewSize="True"
                           LoadingPlaceholder="placeholder.png"
                           ErrorPlaceholder="error.png" />
```

### 2. **List Virtualization**
```csharp
// ❌ BAD - Loading all items
<StackLayout>
    @foreach (var item in Items)
    {
        <Frame>
            <Label Text="@item.Name" />
        </Frame>
    }
</StackLayout>

// ✅ GOOD - Virtualized list
<CollectionView ItemsSource="{Binding Items}"
                RemainingItemsThreshold="5"
                RemainingItemsThresholdReachedCommand="{Binding LoadMoreCommand}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Frame>
                <Label Text="{Binding Name}" />
            </Frame>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### 3. **Offline Support**
```csharp
public class OfflineEquipmentService : IEquipmentService
{
    private readonly IApiService _apiService;
    private readonly ILocalDatabase _localDb;
    private readonly IConnectivityService _connectivity;
    
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync()
    {
        if (_connectivity.IsConnected)
        {
            try
            {
                var result = await _apiService.GetAsync<IEnumerable<EquipmentDto>>("api/equipment");
                await _localDb.SaveEquipmentAsync(result);
                return ServiceResult<IEnumerable<EquipmentDto>>.Success(result);
            }
            catch (Exception ex)
            {
                // Fall back to local data
            }
        }
        
        var localData = await _localDb.GetEquipmentAsync();
        return ServiceResult<IEnumerable<EquipmentDto>>.Success(localData);
    }
}
```

---

## 🎨 UI/UX Standards

### 1. **Platform UI Guidelines**
Follow platform-specific design guidelines:
- **iOS**: Human Interface Guidelines
- **Android**: Material Design
- **Windows**: Fluent Design System

```csharp
// Platform-specific styling
<Style TargetType="Button">
    <Setter Property="CornerRadius">
        <OnPlatform x:TypeArguments="x:Int32">
            <On Platform="iOS" Value="8" />
            <On Platform="Android" Value="4" />
            <On Platform="UWP" Value="2" />
        </OnPlatform>
    </Setter>
</Style>
```

### 2. **Accessibility**
```csharp
// Always include accessibility properties
<Label Text="{Binding ExerciseName}"
       SemanticProperties.Description="Exercise name"
       SemanticProperties.HeadingLevel="H2" />

<Button Text="Add Exercise"
        SemanticProperties.Hint="Adds a new exercise to your workout"
        Command="{Binding AddExerciseCommand}" />
```

### 3. **Theming Support**
```csharp
public class ThemeService : IThemeService
{
    public void SetTheme(Theme theme)
    {
        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
        mergedDictionaries.Clear();
        
        switch (theme)
        {
            case Theme.Light:
                mergedDictionaries.Add(new LightTheme());
                break;
            case Theme.Dark:
                mergedDictionaries.Add(new DarkTheme());
                break;
            case Theme.System:
                var systemTheme = Application.Current.RequestedTheme;
                mergedDictionaries.Add(systemTheme == OSAppTheme.Dark ? new DarkTheme() : new LightTheme());
                break;
        }
    }
}
```

---

## 📊 Client-Specific Review Checklist

### ✅ Architecture
- [ ] MVVM pattern properly implemented
- [ ] No business logic in views
- [ ] ViewModels are testable (no UI dependencies)
- [ ] Services properly abstracted
- [ ] Platform-specific code isolated

### ✅ Performance
- [ ] Images optimized and cached
- [ ] Lists virtualized for large datasets
- [ ] Animations don't block UI thread
- [ ] Memory leaks prevented (event cleanup)
- [ ] Offline scenarios handled

### ✅ UI/UX
- [ ] Follows platform design guidelines
- [ ] Responsive to different screen sizes
- [ ] Loading states for all async operations
- [ ] Error states handled gracefully
- [ ] Accessibility properties included

### ✅ Cross-Platform
- [ ] Code shared where possible
- [ ] Platform-specific features properly wrapped
- [ ] Resources organized and localized
- [ ] Navigation works consistently
- [ ] Permissions handled correctly

### ✅ Testing
- [ ] ViewModels have unit tests
- [ ] UI tests for critical flows
- [ ] Platform-specific code tested
- [ ] Offline scenarios tested
- [ ] Different screen sizes tested

---

## 🧪 Testing Standards

### Unit Tests (ViewModels)
```csharp
[Fact]
public async Task LoadEquipment_Success_PopulatesCollection()
{
    // Arrange
    var mockService = new Mock<IEquipmentService>();
    var equipment = new[] { new EquipmentDto { Id = "1", Name = "Barbell" } };
    mockService.Setup(s => s.GetAllAsync())
        .ReturnsAsync(ServiceResult<IEnumerable<EquipmentDto>>.Success(equipment));
    
    var viewModel = new EquipmentViewModel(mockService.Object);
    
    // Act
    await viewModel.LoadEquipmentCommand.ExecuteAsync();
    
    // Assert
    Assert.Single(viewModel.Equipment);
    Assert.Equal("Barbell", viewModel.Equipment.First().Name);
}
```

### UI Tests
```csharp
[Test]
public void Equipment_CRUD_Flow()
{
    app.WaitForElement("AddButton");
    app.Tap("AddButton");
    
    app.EnterText("NameEntry", "Test Equipment");
    app.Tap("SaveButton");
    
    app.WaitForElement(e => e.Text("Test Equipment"));
    
    app.SwipeRightToLeft("Test Equipment");
    app.Tap("Delete");
    
    app.WaitForNoElement(e => e.Text("Test Equipment"));
}
```

---

## 🔗 Related Documents

- Universal: `CODE_QUALITY_STANDARDS.md`
- Process: `CODE_REVIEW_PROCESS.md`
- Platform Guidelines:
  - iOS: https://developer.apple.com/design/human-interface-guidelines/
  - Android: https://material.io/design
  - Windows: https://docs.microsoft.com/en-us/windows/apps/design/

---

Remember: Write once, run everywhere - but respect platform differences for the best user experience.