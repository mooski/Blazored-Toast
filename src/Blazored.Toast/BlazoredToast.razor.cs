using Blazored.Toast.Configuration;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blazored.Toast
{
    public partial class BlazoredToast : IDisposable
    {
        [CascadingParameter] private BlazoredToasts ToastsContainer { get; set; }

        [Parameter] public Guid ToastId { get; set; }
        [Parameter] public ToastSettings ToastSettings { get; set; }
        [Parameter] public int Timeout { get; set; }

        private string StateCssClass { get; set; }
        private string ProgressBarStyle { get; set; }
        private Timer CloseTimer { get; set; }

        private int _isClosing = 0;
        private bool _disposedValue;

        protected override void OnInitialized()
        {
            CloseTimer = new Timer((state) => { Close(); }, null, TimeSpan.FromSeconds(Timeout), TimeSpan.FromMilliseconds(-1));
            ProgressBarStyle = $"animation: blazored-toast-progressbar {Timeout}s linear";
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                StateCssClass = "blazored-toast-open";
            }
        }

        private async void Close()
        {
            if (Interlocked.Exchange(ref _isClosing, 1) == 0)
            {
                StateCssClass = "blazored-toast-closing";

                await InvokeAsync(StateHasChanged);

                await Task
                    .Delay(ToastSettings.CloseDelay ?? TimeSpan.FromSeconds(0))
                    .ContinueWith((task) =>
                    {
                        ToastsContainer.RemoveToast(ToastId);
                    });
            }
        }

        private string JoinCssClasses(params string[] cssClasses)
        {
            return string.Join(" ", cssClasses.Where(c => !string.IsNullOrEmpty(c)));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CloseTimer.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
