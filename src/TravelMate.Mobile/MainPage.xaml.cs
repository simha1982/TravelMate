using CommunityToolkit.Maui.Views;
using TravelMate.Mobile.Models;
using TravelMate.Mobile.Services;

namespace TravelMate.Mobile;

public partial class MainPage : ContentPage
{
    private readonly TravelMateApiClient apiClient;
    private readonly List<DemoLocation> demoLocations =
    [
        new("Hyderabad - Charminar cluster", 17.3616, 78.4747, 15_000),
        new("Hyderabad - Golconda and tombs", 17.3833, 78.4011, 8_000),
        new("Nandi Hills", 13.3702, 77.6835, 5_000),
        new("Bengaluru - Chinnaswamy Stadium", 12.9788, 77.5996, 5_000),
        new("South Africa - St Lucia Wetlands", -28.0000, 32.4800, 5_000)
    ];

    private NearbyStoryDto? selectedStory;
    private string? lastAnswer;

    public MainPage(TravelMateApiClient apiClient)
    {
        this.apiClient = apiClient;
        InitializeComponent();
        DemoLocationPicker.ItemsSource = demoLocations;
        DemoLocationPicker.ItemDisplayBinding = new Binding(nameof(DemoLocation.Name));
        DemoLocationPicker.SelectedIndex = 0;
        LanguagePicker.ItemsSource = new List<string> { "en", "hi", "te", "de" };
        LanguagePicker.SelectedIndex = 0;
        UpdateMapPreview(demoLocations[0].Latitude, demoLocations[0].Longitude, demoLocations[0].Name);
    }

    private async void OnFindStoriesClicked(object? sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Finding your location...";
            var location = await Geolocation.Default.GetLastKnownLocationAsync()
                ?? await Geolocation.Default.GetLocationAsync(new GeolocationRequest(
                    GeolocationAccuracy.Medium,
                    TimeSpan.FromSeconds(10)));

            if (location is null)
            {
                StatusLabel.Text = "Location was not available. Showing Nandi Hills pilot location.";
                location = new Location(13.3702, 77.6835);
            }

            await LoadStoriesAsync(
                location.Latitude,
                location.Longitude,
                5_000,
                $"Current GPS {location.Latitude:0.0000}, {location.Longitude:0.0000}");
        }
        catch (FeatureNotEnabledException)
        {
            StatusLabel.Text = "Location is disabled. Use a demo location for desk testing.";
            await LoadSelectedDemoLocationAsync();
        }
        catch (PermissionException)
        {
            StatusLabel.Text = "Location permission was denied. Use a demo location for desk testing.";
            await LoadSelectedDemoLocationAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not load stories: {ex.Message}";
        }
    }

    private async void OnLoadDemoLocationClicked(object? sender, EventArgs e)
    {
        if (DemoLocationPicker.SelectedItem is not DemoLocation demoLocation)
        {
            StatusLabel.Text = "Choose a demo location first.";
            return;
        }

        await LoadSelectedDemoLocationAsync();
    }

    private async Task LoadSelectedDemoLocationAsync()
    {
        if (DemoLocationPicker.SelectedItem is not DemoLocation demoLocation)
        {
            StatusLabel.Text = "Choose a demo location first.";
            return;
        }

        await LoadStoriesAsync(
            demoLocation.Latitude,
            demoLocation.Longitude,
            demoLocation.RadiusMeters,
            demoLocation.Name);
    }

    private async Task LoadStoriesAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        string locationName)
    {
        StatusLabel.Text = $"Looking near {latitude:0.0000}, {longitude:0.0000}...";
        LocationLabel.Text = $"{locationName} - {latitude:0.0000}, {longitude:0.0000}";
        UpdateMapPreview(latitude, longitude, locationName);

        var stories = await apiClient.GetNearbyStoriesAsync(
            latitude,
            longitude,
            radiusMeters,
            CancellationToken.None);

        StoriesCollection.ItemsSource = stories;
        selectedStory = stories.FirstOrDefault();
        StoriesCollection.SelectedItem = selectedStory;

        if (selectedStory is null)
        {
            SetSelectedStory(null);
            StoryPlayer.Stop();
            StoryPlayer.Source = null;
            StatusLabel.Text = "No story returned by the API.";
            return;
        }

        SetSelectedStory(selectedStory);
        StatusLabel.Text = $"Found {stories.Count} nearby story option(s). Tap a card to choose one.";
    }

    private void OnStorySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        selectedStory = e.CurrentSelection.FirstOrDefault() as NearbyStoryDto;
        SetSelectedStory(selectedStory);
    }

    private void SetSelectedStory(NearbyStoryDto? story)
    {
        if (story is null)
        {
            PlaceLabel.Text = "No story selected";
            StoryTitleLabel.Text = "";
            StoryDescriptionLabel.Text = "Load a demo location or find stories near you.";
            return;
        }

        PlaceLabel.Text = story.PlaceName;
        StoryTitleLabel.Text = story.Title;
        StoryDescriptionLabel.Text = story.ShortDescription;
    }

    private async void OnInterestedClicked(object? sender, EventArgs e)
    {
        await SaveFeedbackAsync("Interested");
    }

    private async void OnPlayClicked(object? sender, EventArgs e)
    {
        await PlaySelectedStoryAsync();
    }

    private async Task PlaySelectedStoryAsync()
    {
        if (selectedStory is null)
        {
            StatusLabel.Text = "Find a story before playing audio.";
            return;
        }

        try
        {
            StatusLabel.Text = "Preparing story audio...";
            var audioUrl = selectedStory.AudioUrl;
            if (string.IsNullOrWhiteSpace(audioUrl))
            {
                var generated = await apiClient.GenerateStoryAudioAsync(selectedStory, CancellationToken.None);
                audioUrl = generated?.Url;
            }

            if (string.IsNullOrWhiteSpace(audioUrl))
            {
                StatusLabel.Text = "Audio is not available for this story yet.";
                return;
            }

            await apiClient.SavePlaybackEventAsync(selectedStory.StoryId, "Played", CancellationToken.None);
            StoryPlayer.Source = MediaSource.FromUri(audioUrl);
            StoryPlayer.MetadataTitle = selectedStory.Title;
            StoryPlayer.MetadataArtist = selectedStory.PlaceName;
            StoryPlayer.Play();
            StatusLabel.Text = "Playing story audio in TravelMate.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not play story audio: {ex.Message}";
        }
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        await SaveFeedbackAsync("Skipped");
    }

    private async void OnNotInterestedClicked(object? sender, EventArgs e)
    {
        await SaveFeedbackAsync("NotInterested");
    }

    private async void OnVoiceCommandClicked(object? sender, EventArgs e)
    {
        var command = VoiceCommandEntry.Text?.Trim().ToLowerInvariant();
        switch (command)
        {
            case "yes":
            case "interested":
            case "save":
                await SaveFeedbackAsync("Interested");
                break;
            case "no":
            case "skip":
                await SaveFeedbackAsync("Skipped");
                break;
            case "not interested":
            case "not":
                await SaveFeedbackAsync("NotInterested");
                break;
            case "play":
                await PlaySelectedStoryAsync();
                break;
            default:
                StatusLabel.Text = "Try yes, no, play, or skip.";
                break;
        }
    }

    private async Task SaveFeedbackAsync(string action)
    {
        if (selectedStory is null)
        {
            StatusLabel.Text = "Find a story before sending feedback.";
            return;
        }

        await apiClient.SavePlaybackEventAsync(selectedStory.StoryId, action, CancellationToken.None);
        StatusLabel.Text = $"Saved feedback: {action}.";
    }

    private async void OnAskClicked(object? sender, EventArgs e)
    {
        var question = QuestionEntry.Text;
        if (string.IsNullOrWhiteSpace(question))
        {
            AnswerLabel.Text = "Ask a question first.";
            return;
        }

        try
        {
            AnswerLabel.Text = "Thinking...";
            var answer = await apiClient.AskAsync(question, CancellationToken.None);
            lastAnswer = answer?.Answer;
            AnswerLabel.Text = lastAnswer ?? "No answer returned.";
        }
        catch (Exception ex)
        {
            AnswerLabel.Text = $"Could not answer yet: {ex.Message}";
        }
    }

    private async void OnPlayAnswerClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(lastAnswer))
        {
            StatusLabel.Text = "Ask TravelMate a question before playing an answer.";
            return;
        }

        try
        {
            StatusLabel.Text = "Preparing spoken answer...";
            var language = LanguagePicker.SelectedItem as string ?? "en";
            var audioFile = await apiClient.SynthesizeToLocalFileAsync(lastAnswer, language, CancellationToken.None);
            StoryPlayer.Source = MediaSource.FromFile(audioFile);
            StoryPlayer.MetadataTitle = "TravelMate answer";
            StoryPlayer.MetadataArtist = "TravelMate";
            StoryPlayer.Play();
            StatusLabel.Text = "Playing spoken answer.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not play answer audio: {ex.Message}";
        }
    }

    private async void OnSavePreferencesClicked(object? sender, EventArgs e)
    {
        var interests = new Dictionary<string, double>
        {
            ["history"] = HistoryPreference.IsChecked ? 0.9 : 0.15,
            ["nature"] = NaturePreference.IsChecked ? 0.85 : 0.15,
            ["architecture"] = ArchitecturePreference.IsChecked ? 0.8 : 0.15,
            ["cricket"] = CricketPreference.IsChecked ? 0.9 : 0.15,
            ["food"] = FoodPreference.IsChecked ? 0.8 : 0.15,
            ["culture"] = CulturePreference.IsChecked ? 0.85 : 0.15
        };

        try
        {
            var language = LanguagePicker.SelectedItem as string ?? "en";
            await apiClient.SavePreferencesAsync(interests, language, CancellationToken.None);
            StatusLabel.Text = "Saved demo preferences.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not save preferences: {ex.Message}";
        }
    }

    private async void OnSaveConsentClicked(object? sender, EventArgs e)
    {
        try
        {
            await apiClient.SaveConsentAsync(
                LocationConsentSwitch.IsToggled,
                VoiceConsentSwitch.IsToggled,
                PersonalizationConsentSwitch.IsToggled,
                CancellationToken.None);
            StatusLabel.Text = "Saved consent settings.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not save consent: {ex.Message}";
        }
    }

    private void UpdateMapPreview(double latitude, double longitude, string label)
    {
        var encodedLabel = System.Net.WebUtility.HtmlEncode(label);
        MapPreview.Source = new HtmlWebViewSource
        {
            Html = $$"""
                <!doctype html>
                <html>
                <body style="margin:0;font-family:Segoe UI,Arial,sans-serif;background:#eef5f5;">
                  <iframe
                    width="100%"
                    height="145"
                    style="border:0"
                    loading="lazy"
                    referrerpolicy="no-referrer-when-downgrade"
                    src="https://www.openstreetmap.org/export/embed.html?bbox={{longitude - 0.02}}%2C{{latitude - 0.02}}%2C{{longitude + 0.02}}%2C{{latitude + 0.02}}&layer=mapnik&marker={{latitude}}%2C{{longitude}}">
                  </iframe>
                  <div style="padding:6px 8px;color:#1f2937;font-size:12px;">
                    {{encodedLabel}} - {{latitude:0.0000}}, {{longitude:0.0000}}
                  </div>
                </body>
                </html>
                """
        };
    }
}

public sealed record DemoLocation(string Name, double Latitude, double Longitude, double RadiusMeters);
