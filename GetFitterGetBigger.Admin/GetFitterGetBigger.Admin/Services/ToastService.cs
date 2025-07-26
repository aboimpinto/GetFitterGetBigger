using System;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IToastService
    {
        event Action<ToastMessage>? OnShow;
        event Action? OnHide;
        
        void ShowSuccess(string message, string? title = null);
        void ShowError(string message, string? title = null);
        void ShowWarning(string message, string? title = null);
        void ShowInfo(string message, string? title = null);
    }
    
    public class ToastService : IToastService
    {
        public event Action<ToastMessage>? OnShow;
        #pragma warning disable CS0067 // The event 'OnHide' is never used - but it is used via subscription in ToastContainer.razor
        public event Action? OnHide;
        #pragma warning restore CS0067
        
        public void ShowSuccess(string message, string? title = null)
        {
            Show(new ToastMessage
            {
                Type = ToastType.Success,
                Message = message,
                Title = title ?? "Success"
            });
        }
        
        public void ShowError(string message, string? title = null)
        {
            Show(new ToastMessage
            {
                Type = ToastType.Error,
                Message = message,
                Title = title ?? "Error"
            });
        }
        
        public void ShowWarning(string message, string? title = null)
        {
            Show(new ToastMessage
            {
                Type = ToastType.Warning,
                Message = message,
                Title = title ?? "Warning"
            });
        }
        
        public void ShowInfo(string message, string? title = null)
        {
            Show(new ToastMessage
            {
                Type = ToastType.Info,
                Message = message,
                Title = title ?? "Information"
            });
        }
        
        private void Show(ToastMessage message)
        {
            OnShow?.Invoke(message);
        }
    }
    
    public class ToastMessage
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; }
    }
    
    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info
    }
}