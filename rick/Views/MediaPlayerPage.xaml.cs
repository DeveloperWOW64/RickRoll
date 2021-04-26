﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace rick.Views
{
    public sealed partial class MediaPlayerPage : Page, INotifyPropertyChanged
    {
        // For more on the MediaPlayer and adjusting controls and behavior see https://docs.microsoft.com/windows/uwp/controls-and-patterns/media-playback
        private const string DefaultSource = "https://github.com/DeveloperWOW64/RickRoll/raw/main/.assets/rick.mp4";

        // The poster image is displayed until the video is started
        private const string DefaultPoster = "https://i.ytimg.com/vi/fTZtmsTaijY/hqdefault.jpg";

        // The DisplayRequest is used to stop the screen dimming while watching for extended periods
        private DisplayRequest _displayRequest = new DisplayRequest();
        private bool _isRequestActive = false;

        public MediaPlayerPage()
        {
            InitializeComponent();

            mpe.PosterSource = new BitmapImage(new Uri(DefaultPoster));
            mpe.Source = MediaSource.CreateFromUri(new Uri(DefaultSource));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            mpe.MediaPlayer.Pause();
            mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
            var mediaSource = mpe.Source as MediaSource;
            mediaSource?.Dispose();
            mpe.Source = null;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            if (sender is MediaPlaybackSession playbackSession && playbackSession.NaturalVideoHeight != 0)
            {
                if (playbackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    if (!_isRequestActive)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            _displayRequest.RequestActive();
                            _isRequestActive = true;
                        });
                    }
                }
                else
                {
                    if (_isRequestActive)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            _displayRequest.RequestRelease();
                            _isRequestActive = false;
                        });
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
