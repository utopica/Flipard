using NToastNotify;

namespace Flipard.MVC.Services
{
    public class NToastNotifyService : INToastNotifyService
    {
        
        private readonly IToastNotification _toastNotification;

        public NToastNotifyService(IToastNotification toastNotification)
        {
            _toastNotification = toastNotification;
        }


        public void AddAlertToastMessage(string? message)
        {
            _toastNotification.AddAlertToastMessage(message);
        }

        public void AddErrorToastMessage(string? message)
        {
            _toastNotification.AddErrorToastMessage(message);
        }

        public void AddInfoToastMessage(string? message)
        {
           _toastNotification.AddInfoToastMessage(message);
        }

        public void AddSuccessToastMessage(string? message)
        {
            _toastNotification.AddSuccessToastMessage(message);
        }

        public void AddWarningToastMessage(string? message)
        {
           _toastNotification.AddWarningToastMessage(message);
        }
    }
}
