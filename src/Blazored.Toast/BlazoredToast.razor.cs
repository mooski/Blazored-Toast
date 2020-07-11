using Blazored.Toast.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Diagnostics;
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
        private System.Timers.Timer OpenTimer { get; set; }
        private System.Timers.Timer CloseTimer { get; set; }
        private Stopwatch CloseTimerStopwatch { get; set; } = new Stopwatch();

        private int _isClosing = 0;
        private bool _disposedValue;

        protected override void OnInitialized()
        {
            OpenTimer = new System.Timers.Timer(1) { AutoReset = false };
            OpenTimer.Elapsed += HandleOpenTimerElapsed;
            OpenTimer.Start();

            CloseTimer = new System.Timers.Timer(TimeSpan.FromSeconds(Timeout).TotalMilliseconds) { AutoReset = false };
            CloseTimer.Elapsed += HandleCloseTimerElapsed;
            CloseTimer.Start();
            CloseTimerStopwatch.Start();
            UpdateProgressBarStyle();
        }

        private void HandleOpenTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StateCssClass = "blazored-toast-open";
            InvokeAsync(StateHasChanged);
        }

        private void HandleCloseTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Close();
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

        private void UpdateProgressBarStyle()
        {
            ProgressBarStyle = $"animation: blazored-toast-progressbar {Timeout}s linear {(CloseTimer.Enabled ? "running" : "paused")};";
        }

        private void HandleMouseOver(MouseEventArgs args)
        {
            if (ToastSettings.PauseTimeoutOnMouseOver)
            {
                CloseTimer.Stop();
                CloseTimerStopwatch.Stop();
                UpdateProgressBarStyle();
            }
        }

        private void HandleMouseOut(MouseEventArgs args)
        {
            if (ToastSettings.PauseTimeoutOnMouseOver)
            {
                CloseTimer.Interval = TimeSpan.FromSeconds(Timeout).TotalMilliseconds - CloseTimerStopwatch.ElapsedMilliseconds;
                CloseTimer.Start();
                CloseTimerStopwatch.Start();
                UpdateProgressBarStyle();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    OpenTimer.Elapsed -= HandleOpenTimerElapsed;
                    OpenTimer.Dispose();
                    OpenTimer = null;

                    CloseTimer.Elapsed -= HandleCloseTimerElapsed;
                    CloseTimer.Dispose();
                    CloseTimer = null;
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
