using TravelMate.Mobile.Models;
using TravelMate.Mobile.Services;

namespace TravelMate.Mobile;

public partial class MainPage : ContentPage
{
    private readonly TravelMateApiClient apiClient;
    private NearbyStoryDto? selectedStory;

    public MainPage(TravelMateApiClient apiClient)
    {
        this.apiClient = apiClient;
        InitializeComponent();
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

            StatusLabel.Text = $"Looking near {location.Latitude:0.0000}, {location.Longitude:0.0000}...";
            var stories = await apiClient.GetNearbyStoriesAsync(
                location.Latitude,
                location.Longitude,
                5_000,
                CancellationToken.None);

            selectedStory = stories.FirstOrDefault();
            if (selectedStory is null)
            {
                PlaceLabel.Text = "No nearby story found";
                StoryTitleLabel.Text = "";
                StoryDescriptionLabel.Text = "Try again when you are near a pilot location.";
                StatusLabel.Text = "No story returned by the API.";
                return;
            }

            PlaceLabel.Text = selectedStory.PlaceName;
            StoryTitleLabel.Text = selectedStory.Title;
            StoryDescriptionLabel.Text = selectedStory.ShortDescription;
            StatusLabel.Text = $"Found {stories.Count} nearby story option(s).";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Could not load stories: {ex.Message}";
        }
    }

    private async void OnInterestedClicked(object? sender, EventArgs e)
    {
        await SaveFeedbackAsync("Interested");
    }

    private async void OnPlayClicked(object? sender, EventArgs e)
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
            await Launcher.Default.OpenAsync(new Uri(audioUrl, UriKind.RelativeOrAbsolute));
            StatusLabel.Text = "Opened story audio.";
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
            AnswerLabel.Text = answer?.Answer ?? "No answer returned.";
        }
        catch (Exception ex)
        {
            AnswerLabel.Text = $"Could not answer yet: {ex.Message}";
        }
    }
}
