namespace GetFitterGetBigger.Admin.Services.Validation;

public static class Validate
{
    public static ValidationBuilder<T> For<T>(T value) => ValidationBuilder<T>.For(value);
    
    public static ValidationBuilder<T> That<T>(T value) => ValidationBuilder<T>.For(value);
}