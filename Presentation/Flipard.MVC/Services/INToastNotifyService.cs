using NToastNotify;

namespace Flipard.MVC.Services
{
    public interface INToastNotifyService
    {
        void AddSuccessToastMessage(string? message = null, LibraryOptions? toastOptions = null);

        void AddInfoToastMessage(string? message = null, LibraryOptions? toastOptions = null);

        void AddAlertToastMessage(string? message = null, LibraryOptions? toastOptions = null);

        void AddWarningToastMessage(string? message = null, LibraryOptions? toastOptions = null);

        void AddErrorToastMessage(string? message = null, LibraryOptions? toastOptions = null);

        IEnumerable<IToastMessage> GetToastMessages();

        IEnumerable<IToastMessage> ReadAllMessages();

        void RemoveAll();
        /*
        void AddSuccessToastMessage(string? message = null);

        void AddInfoToastMessage(string? message = null);

        void AddAlertToastMessage(string? message = null);

        void AddWarningToastMessage(string? message = null);

        void AddErrorToastMessage(string? message = null);
        */
    }
}
