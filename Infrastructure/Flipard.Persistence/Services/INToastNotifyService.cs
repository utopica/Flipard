namespace Flipard.Persistence.Services
{
    public interface INToastNotifyService
    {
        void AddSuccessToastMessage(string? message);

        void AddInfoToastMessage(string? message);

        void AddAlertToastMessage(string? message);

        void AddWarningToastMessage(string? message);

        void AddErrorToastMessage(string? message);

       
        
    }
}
