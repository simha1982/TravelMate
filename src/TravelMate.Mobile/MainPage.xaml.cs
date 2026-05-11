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
        new("Hyderabad - Hussain Sagar lakefront", 17.4239, 78.4738, 8_000),
        new("Hyderabad - Birla Mandir viewpoint", 17.4062, 78.4691, 8_000),
        new("Nandi Hills", 13.3702, 77.6835, 5_000),
        new("Bengaluru - Chinnaswamy Stadium", 12.9788, 77.5996, 5_000),
        new("South Africa - St Lucia Wetlands", -28.0000, 32.4800, 5_000)
    ];
    private readonly List<DemoLocation> hyderabadDemoTrip =
    [
        new("Trip stop 1 - Charminar and Old City", 17.3616, 78.4747, 6_000),
        new("Trip stop 2 - Salar Jung and Musi River", 17.3713, 78.4804, 6_000),
        new("Trip stop 3 - Hussain Sagar lakefront", 17.4239, 78.4738, 7_000),
        new("Trip stop 4 - Birla Mandir viewpoint", 17.4062, 78.4691, 6_000),
        new("Trip stop 5 - Golconda and Qutb Shahi tombs", 17.3833, 78.4011, 7_000)
    ];

    private IReadOnlyList<NearbyStoryDto> currentStories = [];
    private StoryDetailDto? selectedStoryDetail;
    private NearbyStoryDto? selectedStory;
    private int selectedStoryIndex = -1;
    private int demoTripIndex = -1;
    private double currentLatitude = 17.3616;
    private double currentLongitude = 78.4747;
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

    private async void OnStartDemoTripClicked(object? sender, EventArgs e)
    {
        demoTripIndex = 0;
        await LoadDemoTripStopAsync();
    }

    private async void OnNextDemoStopClicked(object? sender, EventArgs e)
    {
        demoTripIndex = demoTripIndex < 0
            ? 0
            : (demoTripIndex + 1) % hyderabadDemoTrip.Count;

        await LoadDemoTripStopAsync();
    }

    private async Task LoadDemoTripStopAsync()
    {
        var stop = hyderabadDemoTrip[demoTripIndex];
        await LoadStoriesAsync(
            stop.Latitude,
            stop.Longitude,
            stop.RadiusMeters,
            stop.Name);
        StatusLabel.Text = $"Demo trip stop {demoTripIndex + 1} of {hyderabadDemoTrip.Count}: {stop.Name}.";
    }

    private async Task LoadStoriesAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        string locationName)
    {
        StatusLabel.Text = $"Looking near {latitude:0.0000}, {longitude:0.0000}...";
        currentLatitude = latitude;
        currentLongitude = longitude;
        LocationLabel.Text = $"{locationName} - {latitude:0.0000}, {longitude:0.0000}";
        UpdateMapPreview(latitude, longitude, locationName);

        var stories = await apiClient.GetNearbyStoriesAsync(
            latitude,
            longitude,
            radiusMeters,
            CancellationToken.None);

        currentStories = stories.ToArray();
        StoriesCollection.ItemsSource = currentStories;
        selectedStory = currentStories.FirstOrDefault();
        selectedStoryIndex = selectedStory is null ? -1 : 0;
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
        selectedStoryIndex = selectedStory is null ? -1 : currentStories.IndexOf(selectedStory);
        SetSelectedStory(selectedStory);
    }

    private async void SetSelectedStory(NearbyStoryDto? story)
    {
        if (story is null)
        {
            PlaceLabel.Text = "No story selected";
            StoryTitleLabel.Text = "";
            StoryDescriptionLabel.Text = "Load a demo location or find stories near you.";
            StoryDetailLabel.Text = "";
            selectedStoryDetail = null;
            return;
        }

        PlaceLabel.Text = story.PlaceName;
        StoryTitleLabel.Text = story.Title;
        StoryDescriptionLabel.Text = story.ShortDescription;
        StoryDetailLabel.Text = $"{story.DistanceMeters:0} m away - score {story.Score:0.00} - {story.SourceName}";
        selectedStoryDetail = await apiClient.GetStoryDetailAsync(story.StoryId, CancellationToken.None);
        if (selectedStoryDetail?.Story is not null)
        {
            StoryDetailLabel.Text = $"{story.DistanceMeters:0} m away - {string.Join(", ", selectedStoryDetail.Story.Categories)} - {selectedStoryDetail.Story.SourceName}";
        }
    }

    private void OnPreviousStoryClicked(object? sender, EventArgs e)
    {
        SelectStoryByOffset(-1);
    }

    private void OnNextStoryClicked(object? sender, EventArgs e)
    {
        SelectStoryByOffset(1);
    }

    private void SelectStoryByOffset(int offset)
    {
        if (currentStories.Count == 0)
        {
            StatusLabel.Text = "Load stories before navigating the list.";
            return;
        }

        selectedStoryIndex = selectedStoryIndex < 0
            ? 0
            : (selectedStoryIndex + offset + currentStories.Count) % currentStories.Count;
        selectedStory = currentStories[selectedStoryIndex];
        StoriesCollection.SelectedItem = selectedStory;
        SetSelectedStory(selectedStory);
        StatusLabel.Text = $"Selected {selectedStoryIndex + 1} of {currentStories.Count}.";
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
            StoryPlayer.Source = await CreatePlayableSourceAsync(audioUrl, selectedStory, CancellationToken.None);
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

    private async Task<MediaSource> CreatePlayableSourceAsync(
        string audioUrl,
        NearbyStoryDto story,
        CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(audioUrl, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https")
        {
            return MediaSource.FromUri(uri);
        }

        var localAudio = await apiClient.SynthesizeStoryToLocalFileAsync(story, cancellationToken);
        return MediaSource.FromFile(localAudio);
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        await SaveFeedbackAsync("Skipped");
    }

    private async void OnStoryPlayerMediaEnded(object? sender, EventArgs e)
    {
        if (selectedStory is null)
        {
            return;
        }

        try
        {
            await apiClient.SavePlaybackEventAsync(selectedStory.StoryId, "Completed", CancellationToken.None);
            StatusLabel.Text = "Story completed. TravelMate will reduce repeats in future recommendations.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not save completion event: {ex.Message}";
        }
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
            case "next":
                SelectStoryByOffset(1);
                break;
            case "previous":
            case "back":
                SelectStoryByOffset(-1);
                break;
            default:
                StatusLabel.Text = "Try yes, no, play, skip, next, or previous.";
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

    private async void OnSubmitContributionClicked(object? sender, EventArgs e)
    {
        var placeName = ContributionPlaceEntry.Text?.Trim();
        var title = ContributionTitleEntry.Text?.Trim();
        var storyText = ContributionTextEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(placeName)
            || string.IsNullOrWhiteSpace(title)
            || string.IsNullOrWhiteSpace(storyText))
        {
            StatusLabel.Text = "Enter place, title, and story text before submitting.";
            return;
        }

        try
        {
            var language = LanguagePicker.SelectedItem as string ?? "en";
            await apiClient.SubmitContributionAsync(
                placeName,
                title,
                storyText,
                currentLatitude,
                currentLongitude,
                language,
                CancellationToken.None);

            ContributionTitleEntry.Text = "";
            ContributionTextEditor.Text = "";
            StatusLabel.Text = "Submitted contribution for admin moderation.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not submit contribution: {ex.Message}";
        }
    }

    private async void OnOpenSourceClicked(object? sender, EventArgs e)
    {
        var sourceUrl = selectedStoryDetail?.Story.SourceUrl;
        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            StatusLabel.Text = "Source link is not available for this story.";
            return;
        }

        await Launcher.Default.OpenAsync(sourceUrl);
    }

    private async void OnOpenMapClicked(object? sender, EventArgs e)
    {
        if (selectedStoryDetail?.Place is null)
        {
            StatusLabel.Text = "Place map details are not loaded yet.";
            return;
        }

        var place = selectedStoryDetail.Place;
        await Map.Default.OpenAsync(
            place.Location.Latitude,
            place.Location.Longitude,
            new MapLaunchOptions { Name = place.Name });
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

public static class StoryListExtensions
{
    public static int IndexOf(this IReadOnlyList<NearbyStoryDto> stories, NearbyStoryDto story)
    {
        for (var index = 0; index < stories.Count; index++)
        {
            if (stories[index].StoryId == story.StoryId)
            {
                return index;
            }
        }

        return -1;
    }
}

public sealed record DemoLocation(string Name, double Latitude, double Longitude, double RadiusMeters);
