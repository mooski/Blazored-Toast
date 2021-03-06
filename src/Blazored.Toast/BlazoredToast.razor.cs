﻿using Blazored.Toast.Configuration;
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
        private System.Timers.Timer OpenTimer { get; set; }

        private CountdownTimer _countdownTimer;
        private int _progress = 100;
        private int _isClosing = 0;

        protected override void OnInitialized()
        {
            OpenTimer = new System.Timers.Timer(1) { AutoReset = false };
            OpenTimer.Elapsed += HandleOpenTimerElapsed;
            OpenTimer.Start();

            _countdownTimer = new CountdownTimer(Timeout);
            _countdownTimer.OnTick += CalculateProgress;
            _countdownTimer.OnElapsed += () => { Close(); };
            _countdownTimer.Start();
        }

        private void HandleOpenTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StateCssClass = "blazored-toast-open";
            InvokeAsync(StateHasChanged);
        }

        private async void CalculateProgress(int percentComplete)
        {
            _progress = 100 - percentComplete;
            await InvokeAsync(StateHasChanged);
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

        public void Dispose()
        {
            OpenTimer.Elapsed -= HandleOpenTimerElapsed;
            OpenTimer.Dispose();
            OpenTimer = null;

            _countdownTimer.Dispose();
            _countdownTimer = null;
        }
    }
}
