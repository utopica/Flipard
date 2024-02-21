using NToastNotify;

namespace Flipard.MVC.Services
{
    public class NToastNotifyService
    {
        private readonly INToastNotifyService _notificationService;

        public NToastNotifyService(INToastNotifyService notificationService)
        {
            _notificationService = notificationService;
        }

        public void AddSuccessToastMessage(string? message = null) { }

        public void AddInfoToastMessage(string? message = null) { }
        public void AddAlertToastMessage(string? message = null) { }

        public void AddWarningToastMessage(string? message = null) { }

        public void AddErrorToastMessage(string? message = null) { }


    }
}
