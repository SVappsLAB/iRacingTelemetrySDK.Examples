/**
 * Copyright (C)2024 Scott Velez
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.using Microsoft.CodeAnalysis;
**/

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SVappsLAB.iRacingTelemetrySDK;
using SVappsLAB.iRacingTelemetrySDK.Models;

namespace SpeedRPMGear.ViewModels
{
    [RequiredTelemetryVars(["isOnTrackCar", "gear", "rpm", "speed"])]
    internal class TelemetryViewModel : INotifyPropertyChanged
    {
        ITelemetryClient<TelemetryData>? _telemetryClient;
        CancellationTokenSource? _cts;
        Task? _monitoringTask;

        public TelemetryViewModel()
        {
            ResetState();
        }

        void StartMonitoring()
        {
            ResetState();


            ILogger logger = NullLogger.Instance;
            int PLAYBACK_SPEED_MULTIPLIER = 10;

            IBTOptions? iBTOptions = _isLiveSession ? null : new IBTOptions(IBTFileName, PLAYBACK_SPEED_MULTIPLIER);

            // if live session, pass null for iBTOptions
            // otherwise, pass the file name and playback speed multiplier
            _telemetryClient = TelemetryClient<TelemetryData>.Create(logger, iBTOptions);

            _telemetryClient.OnConnectStateChanged += OnConnectStateChange;
            _telemetryClient.OnSessionInfoUpdate += OnSessionInfoUpdate;
            _telemetryClient.OnTelemetryUpdate += OnTelemetryUpdate;

            _cts = new CancellationTokenSource();
            _monitoringTask = _telemetryClient.Monitor(_cts.Token);
        }
        async Task StopMonitoring()
        {
            if (_telemetryClient != null)
            {
                // tell client to stop monitoring
                _cts?.Cancel();
                await _monitoringTask;

                // clean up
                _telemetryClient.OnConnectStateChanged -= OnConnectStateChange;
                _telemetryClient.OnSessionInfoUpdate += OnSessionInfoUpdate;
                _telemetryClient.OnTelemetryUpdate -= OnTelemetryUpdate;
                _telemetryClient.Dispose();
                _telemetryClient = null;
            }
        }

        public bool IsReadyForMonitoring
        {
            get => _isReadyForMonitoring;
            set
            {
                _isReadyForMonitoring = value;
                OnPropertyChanged(nameof(IsReadyForMonitoring));
            }
        }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set
            {
                if (value && !_isMonitoring)
                {
                    StartMonitoring();
                    MonitorContentText = MONITORING_STOP;
                }
                else if (value == false && _isMonitoring)
                {
                    StopMonitoring();
                    MonitorContentText = MONITORING_START;
                }
                _isMonitoring = value;
                OnPropertyChanged(nameof(IsMonitoring));
            }
        }
        public string MonitorContentText
        {
            get => _monitorContentText;
            set
            {
                _monitorContentText = value;
                OnPropertyChanged(nameof(MonitorContentText));
            }
        }
        public bool IsLiveSession
        {
            get => _isLiveSession;
            set
            {
                _isLiveSession = value;
                OnPropertyChanged(nameof(IsLiveSession));
                calcMonitoringToggleState();
            }
        }
        public string IBTFileName
        {
            get => _ibtFileName;
            set
            {
                _ibtFileName = value;
                OnPropertyChanged(nameof(IBTFileName));
                calcMonitoringToggleState();
            }
        }
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }
        public string CarName
        {
            get => _carName;
            set
            {
                _carName = value;
                OnPropertyChanged(nameof(CarName));
            }
        }
        public bool IsCarOnTrack
        {
            get => _isCarOnTrack;
            set
            {
                _isCarOnTrack = value;
                OnPropertyChanged(nameof(IsCarOnTrack));
            }
        }
        public string Rpm
        {
            get => _rpm;
            set
            {
                _rpm = value;
                OnPropertyChanged(nameof(Rpm));
            }
        }
        public string Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }
        public string Gear
        {
            get => _gear;
            set
            {
                _gear = value;
                OnPropertyChanged(nameof(Gear));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnConnectStateChange(object? sender, ConnectStateChangedEventArgs e)
        {
            // if we are disconnecting, reset our monitoring state
            if (IsConnected && e.State == ConnectState.Disconnected)
            {
                IsMonitoring = false;
            }

            IsConnected = e.State == ConnectState.Connected;
        }
        private void OnSessionInfoUpdate(object? sender, TelemetrySessionInfo si)
        {
            // my driver info #
            var driverNum = si.DriverInfo.DriverCarIdx;
            // my car name
            CarName = si.DriverInfo.Drivers[driverNum].CarScreenNameShort;
        }
        private void OnTelemetryUpdate(object? sender, TelemetryData e)
        {
            IsCarOnTrack = e.IsOnTrackCar;
            Gear = e.Gear.ToString();
            Rpm = e.RPM.ToString("F0");
            Speed = e.Speed.ToString("F0");
        }

        void calcMonitoringToggleState()
        {
            if (IsLiveSession)
            {
                IsReadyForMonitoring = true;
            }
            else
            {
                IsReadyForMonitoring = !string.IsNullOrEmpty(IBTFileName);
            }
        }

        void ResetState()
        {
            const string INIT_VALUE = "unknown";
            CarName = INIT_VALUE;
            Gear = INIT_VALUE;
            Rpm = INIT_VALUE;
            Speed = INIT_VALUE;

        }
        private bool _isLiveSession = true;
        private bool _isReadyForMonitoring = true;
        private bool _isMonitoring;
        private const string MONITORING_START = "Start Monitoring";
        private const string MONITORING_STOP = "Stop Monitoring";
        private string _monitorContentText = MONITORING_START;
        private bool _isCarOnTrack;
        private string _ibtFileName;
        private bool _isConnected;

        private string _carName;
        private string _rpm;
        private string _speed;
        private string _gear;

    }
}
