﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MijnSauna.Common.Client.Interfaces;
using MijnSauna.Common.DataTransferObjects.Sessions;
using MijnSauna.Frontend.Phone.Enums;
using MijnSauna.Frontend.Phone.Helpers.Interfaces;
using MijnSauna.Frontend.Phone.Services.Interfaces;
using MijnSauna.Frontend.Phone.ViewModels.Base;
using Xamarin.Forms;

namespace MijnSauna.Frontend.Phone.ViewModels
{
    public class SaunaPageViewModel : ViewModelBase
    {
        #region <| Dependencies |>

        private readonly ISensorClient _sensorClient;
        private readonly ISessionClient _sessionClient;
        private readonly ISampleClient _sampleClient;
        private readonly ISoundService _soundService;

        #endregion

        #region <| Private Members |>

        private bool _isSaunaPending;
        private bool _isInfraredPending;
        private bool _isCancelPending;

        #endregion

        #region <| Properties - SessionState |>

        private SessionState _sessionState;

        public SessionState SessionState
        {
            get => _sessionState;
            set
            {
                _sessionState = value;
                OnPropertyChanged(nameof(SessionState));
            }
        }

        #endregion

        #region <| Properties - ActiveSession |>

        private GetActiveSessionResponse _activeSession;

        public GetActiveSessionResponse ActiveSession
        {
            get => _activeSession;
            set
            {
                _activeSession = value;
                OnPropertyChanged(nameof(ActiveSession));
            }
        }

        #endregion

        #region <| Properties - Temperatures |>

        private List<int> _temperatures;

        public List<int> Temperatures
        {
            get => _temperatures;
            set
            {
                _temperatures = value;
                OnPropertyChanged(nameof(Temperatures));
            }
        }

        #endregion

        #region <| Properties - Date |>

        private string _date;

        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        #endregion

        #region <| Properties - Time |>

        private string _time;

        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        #endregion

        #region <| Properties - Temperature |>

        private string _temperature;

        public string Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged(nameof(Temperature));
            }
        }

        #endregion

        #region <| Properties - Countdown |>

        private string _countdown;

        public string Countdown
        {
            get => _countdown;
            set
            {
                _countdown = value;
                OnPropertyChanged(nameof(Countdown));
            }
        }

        #endregion

        #region <| Properties - SaunaCaption |>

        private string _saunaCaption;

        public string SaunaCaption
        {
            get => _saunaCaption;
            set
            {
                _saunaCaption = value;
                OnPropertyChanged(nameof(SaunaCaption));
            }
        }

        #endregion

        #region <| Properties - InfraredCaption |>

        private string _infraredCaption;

        public string InfraredCaption
        {
            get => _infraredCaption;
            set
            {
                _infraredCaption = value;
                OnPropertyChanged(nameof(InfraredCaption));
            }
        }

        #endregion

        #region <| Properties - CancelCaption |>

        private string _cancelCaption;

        public string CancelCaption
        {
            get => _cancelCaption;
            set
            {
                _cancelCaption = value;
                OnPropertyChanged(nameof(CancelCaption));
            }
        }

        #endregion

        #region <| Commands |>

        public ICommand QuickStartSaunaCommand { get; set; }
        public ICommand QuickStartInfraredCommand { get; set; }
        public ICommand CancelCommand { get; }

        #endregion

        #region <| Construction |>

        public SaunaPageViewModel(
            ITimerHelper timerHelper,
            ISensorClient sensorClient,
            ISessionClient sessionClient,
            ISampleClient sampleClient,
            ISoundService soundService)
        {
            _sensorClient = sensorClient;
            _sessionClient = sessionClient;
            _sampleClient = sampleClient;
            _soundService = soundService;

            timerHelper.Start(OnTimer, 10000);
            timerHelper.Start(OnCountdown, 500);
            timerHelper.Start(OnProgress, 200);

            QuickStartSaunaCommand = new Command(async () => await OnQuickStartSauna());
            QuickStartInfraredCommand = new Command(async () => await OnQuickStartInfrared());
            CancelCommand = new Command(async () => await OnCancel());
        }

        #endregion

        #region <| Timer Handlers |>

        private Task OnTimer()
        {
            return RefreshActiveSession();
        }

        private Task OnCountdown()
        {
            return RefreshData();
        }

        private int _counter = 0;

        private Task OnProgress()
        {
            string pending = "\ue80d";

            if (_counter > 3)
            {
                _counter = 0;
            }

            if (_counter == 0)
            {
                pending = "\ue80d";
            }
            if (_counter == 1)
            {
                pending = "\ue80e";
            }
            if (_counter == 2)
            {
                pending = "\ue80f";
            }
            if (_counter == 3)
            {
                pending = "\u0e80";
            }

            _counter++;

            if (_isSaunaPending)
            {
                SaunaCaption = pending;
            }
            else
            {
                SaunaCaption = "\ue807";
            }

            if (_isInfraredPending)
            {
                InfraredCaption = pending;
            }
            else
            {
                InfraredCaption = "\ue80c";
            }

            if (_isCancelPending)
            {
                CancelCaption = pending;
            }
            else
            {
                CancelCaption = "\ue806";
            }

            return Task.CompletedTask;
        }

        #endregion

        #region <| Command Handlers |>

        private async Task OnQuickStartSauna()
        {
            _isSaunaPending = true;

            _soundService.PlayClickSound();
            await _sessionClient.QuickStartSession(new QuickStartSessionRequest
            {
                IsSauna = true
            });
            await Task.Delay(1000);
            await RefreshActiveSession();

            _isSaunaPending = false;
        }

        private async Task OnQuickStartInfrared()
        {
            _isInfraredPending = true;

            _soundService.PlayClickSound();
            await _sessionClient.QuickStartSession(new QuickStartSessionRequest
            {
                IsInfrared = true
            });
            await Task.Delay(1000);
            await RefreshActiveSession();

            _isInfraredPending = false;
        }

        private async Task OnCancel()
        {
            _isCancelPending = true;

            _soundService.PlayClickSound();
            if (_activeSession != null)
            {
                await _sessionClient.CancelSession(_activeSession.SessionId);
                await Task.Delay(1000);
                await RefreshActiveSession();
            }

            _isCancelPending = false;
        }

        #endregion

        #region <| Helper Methods |>

        private async Task RefreshActiveSession()
        {
            var currentDateAndTime = DateTime.Now;

            ActiveSession = await _sessionClient.GetActiveSession();
            SessionState = ActiveSession != null ? SessionState.Active : SessionState.None;

            var temperature = await _sensorClient.GetSaunaTemperature();

            Date = $"{currentDateAndTime:dddd d MMMM yyyy}";
            Time = $"{currentDateAndTime:HH:mm}";
            Temperature = $"{temperature.Temperature} °C";

            if (_activeSession != null)
            {
                var samples = await _sampleClient.GetSamplesForSession(_activeSession.SessionId);
                Temperatures = samples.Samples.Select(x => x.Temperature).ToList();
            }
        }

        private Task RefreshData()
        {
            if (_activeSession != null)
            {
                var timeDifference = _activeSession.End.ToLocalTime() - DateTime.Now;
                if (timeDifference > TimeSpan.Zero)
                {
                    Countdown = timeDifference > TimeSpan.FromHours(1) ? $"{timeDifference:hh\\:mm\\:ss}" : $"{timeDifference:mm\\:ss}";
                }
                else
                {
                    Countdown = "00:00";
                }
            }
            else
            {
                Countdown = string.Empty;
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}