﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Caliburn.Micro;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Setting;
using Ciribob.DCS.SimpleRadio.Standalone.Server.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Server.UI.ClientAdmin;
using NLog;
using LogManager = NLog.LogManager;

namespace Ciribob.DCS.SimpleRadio.Standalone.Server.UI.MainWindow
{
    public sealed class MainViewModel : Screen, IHandle<ServerStateMessage>
    {
        private readonly ClientAdminViewModel _clientAdminViewModel;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private DispatcherTimer _passwordDebounceTimer = null;

        public MainViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            ClientAdminViewModel clientAdminViewModel)
        {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _clientAdminViewModel = clientAdminViewModel;
            _eventAggregator.Subscribe(this);

            DisplayName = "DCS-SRS Server - " + UpdaterChecker.VERSION;

            Logger.Info("DCS-SRS Server Running - " + UpdaterChecker.VERSION);
        }

        public bool IsServerRunning { get; private set; } = true;

        public string ServerButtonText => IsServerRunning ? "Stop Server" : "Start Server";

        public int ClientsCount { get; private set; }

        public string RadioSecurityText
            =>
                ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.COALITION_AUDIO_SECURITY).BoolValue
                    ? "ON"
                    : "OFF";

        public string SpectatorAudioText
            =>
                ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.SPECTATORS_AUDIO_DISABLED).BoolValue
                    ? "DISABLED"
                    : "ENABLED";

        public string ExportListText
            =>
                ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.CLIENT_EXPORT_ENABLED).BoolValue
                    ? "ON"
                    : "OFF";

        public string LOSText
            => ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.LOS_ENABLED).BoolValue ? "ON" : "OFF";

        public string DistanceLimitText
            => ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.DISTANCE_ENABLED).BoolValue ? "ON" : "OFF";

        public string RealRadioText
            => ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.IRL_RADIO_TX).BoolValue ? "ON" : "OFF";

        public string IRLRadioRxText
            => ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.IRL_RADIO_RX_INTERFERENCE).BoolValue ? "ON" : "OFF";

        public string RadioExpansion
            => ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.RADIO_EXPANSION).BoolValue ? "ON" : "OFF";

        public string ExternalAWACSMode
            => ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.EXTERNAL_AWACS_MODE).BoolValue ? "ON" : "OFF";

        public bool IsExternalAWACSModeEnabled { get; set; } 
            = ServerSettings.Instance.GetGeneralSetting(ServerSettingsKeys.EXTERNAL_AWACS_MODE).BoolValue;

        private string _externalAWACSModeBluePassword = 
            ServerSettings.Instance.GetExternalAWACSModeSetting(ServerSettingsKeys.EXTERNAL_AWACS_MODE_BLUE_PASSWORD).StringValue;
        public string ExternalAWACSModeBluePassword
        {
            get { return _externalAWACSModeBluePassword; }
            set
            {
                _externalAWACSModeBluePassword = value.Trim();
                if (_passwordDebounceTimer != null)
                {
                    _passwordDebounceTimer.Stop();
                    _passwordDebounceTimer.Tick -= PasswordDebounceTimerTick;
                    _passwordDebounceTimer = null;
                }

                _passwordDebounceTimer = new DispatcherTimer();
                _passwordDebounceTimer.Tick += PasswordDebounceTimerTick;
                _passwordDebounceTimer.Interval = TimeSpan.FromMilliseconds(500);
                _passwordDebounceTimer.Start();

                NotifyOfPropertyChange(() => ExternalAWACSModeBluePassword);
            }
        }

        private string _externalAWACSModeRedPassword = 
            ServerSettings.Instance.GetExternalAWACSModeSetting(ServerSettingsKeys.EXTERNAL_AWACS_MODE_RED_PASSWORD).StringValue;
        public string ExternalAWACSModeRedPassword
        {
            get { return _externalAWACSModeRedPassword; }
            set
            {
                _externalAWACSModeRedPassword = value.Trim();
                if (_passwordDebounceTimer != null)
                {
                    _passwordDebounceTimer.Stop();
                    _passwordDebounceTimer.Tick -= PasswordDebounceTimerTick;
                    _passwordDebounceTimer = null;
                }

                _passwordDebounceTimer = new DispatcherTimer();
                _passwordDebounceTimer.Tick += PasswordDebounceTimerTick;
                _passwordDebounceTimer.Interval = TimeSpan.FromMilliseconds(500);
                _passwordDebounceTimer.Start();

                NotifyOfPropertyChange(() => ExternalAWACSModeRedPassword);
            }
        }

        public string ListeningPort
            => ServerSettings.Instance.GetServerSetting(ServerSettingsKeys.SERVER_PORT).StringValue;

        public void Handle(ServerStateMessage message)
        {
            IsServerRunning = message.IsRunning;
            ClientsCount = message.Count;
        }

        public void ServerStartStop()
        {
            if (IsServerRunning)
            {
                _eventAggregator.PublishOnBackgroundThread(new StopServerMessage());
            }
            else
            {
                _eventAggregator.PublishOnBackgroundThread(new StartServerMessage());
            }
        }

        public void ShowClientList()
        {
            IDictionary<string, object> settings = new Dictionary<string, object>
            {
                {"Icon", new BitmapImage(new Uri("pack://application:,,,/SR-Server;component/server-10.ico"))},
                {"ResizeMode", ResizeMode.CanMinimize}
            };
            _windowManager.ShowWindow(_clientAdminViewModel, null, settings);
        }

        public void RadioSecurityToggle()
        {
            var newSetting = RadioSecurityText != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.COALITION_AUDIO_SECURITY, newSetting);
            NotifyOfPropertyChange(() => RadioSecurityText);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void SpectatorAudioToggle()
        {
            var newSetting = SpectatorAudioText != "DISABLED";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.SPECTATORS_AUDIO_DISABLED, newSetting);
            NotifyOfPropertyChange(() => SpectatorAudioText);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void ExportListToggle()
        {
            var newSetting = ExportListText != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.CLIENT_EXPORT_ENABLED, newSetting);
            NotifyOfPropertyChange(() => ExportListText);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void LOSToggle()
        {
            var newSetting = LOSText != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.LOS_ENABLED, newSetting);
            NotifyOfPropertyChange(() => LOSText);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void DistanceLimitToggle()
        {
            var newSetting = DistanceLimitText != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.DISTANCE_ENABLED, newSetting);
            NotifyOfPropertyChange(() => DistanceLimitText);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void RealRadioToggle()
        {
            var newSetting = RealRadioText != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.IRL_RADIO_TX, newSetting);
            NotifyOfPropertyChange(() => RealRadioText);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void IRLRadioRxBehaviourToggle()
        {
            var newSetting = IRLRadioRxText != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.IRL_RADIO_RX_INTERFERENCE, newSetting);
            NotifyOfPropertyChange(() => IRLRadioRxText);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void RadioExpansionToggle()
        {
            var newSetting = RadioExpansion != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.RADIO_EXPANSION, newSetting);
            NotifyOfPropertyChange(() => RadioExpansion);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        public void ExternalAWACSModeToggle()
        {
            var newSetting = ExternalAWACSMode != "ON";
            ServerSettings.Instance.SetGeneralSetting(ServerSettingsKeys.EXTERNAL_AWACS_MODE, newSetting);

            IsExternalAWACSModeEnabled = newSetting;

            NotifyOfPropertyChange(() => ExternalAWACSMode);
            NotifyOfPropertyChange(() => IsExternalAWACSModeEnabled);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());
        }

        private void PasswordDebounceTimerTick(object sender, EventArgs e)
        {
            ServerSettings.Instance.SetExternalAWACSModeSetting(ServerSettingsKeys.EXTERNAL_AWACS_MODE_BLUE_PASSWORD, _externalAWACSModeBluePassword);
            ServerSettings.Instance.SetExternalAWACSModeSetting(ServerSettingsKeys.EXTERNAL_AWACS_MODE_RED_PASSWORD, _externalAWACSModeRedPassword);

            _eventAggregator.PublishOnBackgroundThread(new ServerSettingsChangedMessage());

            _passwordDebounceTimer.Stop();
            _passwordDebounceTimer.Tick -= PasswordDebounceTimerTick;
            _passwordDebounceTimer = null;
        }
    }
}