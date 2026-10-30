using System.Threading.Tasks;
using System.Windows.Controls;
using Toltech.App.Resources;
using Toltech.App.ViewModels;

namespace Toltech.App.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ContentPresenter _host;
        private MainViewModel _mainVM;
        public NotificationService()
        {
        }

        /// <summary>
        /// Displays a notification asynchronously with the specified message and error state.
        /// </summary>
        /// <param name="message">The message to display in the notification.</param>
        /// <param name="isError">true to indicate the notification is an error; otherwise, false.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ShowNotifAsync(string message, bool isError = false)
        {
            if(_mainVM==null)
                  _mainVM = App.MainVM;

            var notification = new NotificationControl(message);

            _mainVM.CurrentNotification = notification;

            await notification.ShowAsync(isError);

            _mainVM.CurrentNotification = null;
        }
    }
}
