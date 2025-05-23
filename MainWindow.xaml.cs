using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LiteDesk.Core;
using LiteDesk.Core.Models;
using LiteDesk.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LiteDesk
{
    public partial class MainWindow : Window
    {
        private readonly IDatabaseService _db;
        private List<Client>? AllClients;

        public MainWindow()
        {
            InitializeComponent();
            _db = App.Services!.GetService<IDatabaseService>()!;
            CallLogOverlay.RequestClosePanel += (_, _) => HideCallLogPanel();
            ClientOverlay.RequestClosePanel += (_, _) => HideClientPanel();
            ClientOverlay.ShowNotification = ShowNotification;
            ClientOverlay._db = _db;
            LoadClients();
            ClientOverlay.Clients = AllClients;
        }

        private async void LoadClients()
        {
            AllClients = await _db.GetClientsAsync();
            ClientsList.ItemsSource = AllClients.OrderBy(c => c.Name);
        }

        private async void ClientsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsList.SelectedItem is Client client)
            {
                var callLogs = await _db.GetCallLogsForClientAsync(client.Id);
                var groupedLogs = callLogs
                    .GroupBy(log => new { log.CallDate.Year, log.CallDate.Month })
                    .GroupBy(g => g.Key.Year)
                    .Select(yearGroup => new CallLogGroup
                    {
                        Year = yearGroup.Key.ToString(),
                        Months = new ObservableCollection<CallLogMonthGroup>(
                            yearGroup.Select(monthGroup => new CallLogMonthGroup
                            {
                                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key.Month),
                                Logs = new ObservableCollection<CallLog>(monthGroup.ToList())
                            }))
                    });
                CallLogs.ItemsSource = groupedLogs;
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            ShowClientPanel();
        }

        private void ShowCallLogPanel(CallLog callLog)
        {
            CallLogOverlay.Visibility = Visibility.Visible;
            CallLogOverlayBackground.Visibility = Visibility.Visible;
            CallLogOverlayBackground.Opacity = 0;
            CallLogOverlayBackground.IsHitTestVisible = true;

            var dimBackground = new DoubleAnimation(0, 0.5, TimeSpan.FromMilliseconds(300));
            CallLogOverlayBackground.BeginAnimation(UIElement.OpacityProperty, dimBackground);

            var slideIn = new DoubleAnimation
            {
                From = -600,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var transform = (TranslateTransform)CallLogOverlay.RenderTransform;
            transform.BeginAnimation(TranslateTransform.YProperty, slideIn);
            CallLogOverlay.PopulateCallLog(callLog);
        }

        private void ShowClientPanel(Client? client = null)
        {
            ClientOverlay.Visibility = Visibility.Visible;
            ClientOverlayBackground.Visibility = Visibility.Visible;
            ClientOverlayBackground.Opacity = 0;
            ClientOverlayBackground.IsHitTestVisible = true;

            var dimBackground = new DoubleAnimation(0, 0.5, TimeSpan.FromMilliseconds(300));
            ClientOverlayBackground.BeginAnimation(UIElement.OpacityProperty, dimBackground);

            var slideIn = new DoubleAnimation
            {
                From = -600,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var transform = (TranslateTransform)ClientOverlay.RenderTransform;
            transform.BeginAnimation(TranslateTransform.YProperty, slideIn);

            if (client != null)
                ClientOverlay.PopulateClient(client);
        }

        private void HideCallLogPanel()
        {
            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = -600,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            slideOut.Completed += (s, e) =>
            {
                CallLogOverlay.Visibility = Visibility.Collapsed;
                CallLogOverlayBackground.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)));
                CallLogOverlayBackground.Visibility = Visibility.Collapsed;
                CallLogOverlayBackground.IsHitTestVisible = false;

                var transform = (TranslateTransform)CallLogOverlay.RenderTransform;
                transform.Y = -600;
            };

            var transformOut = (TranslateTransform)CallLogOverlay.RenderTransform;
            transformOut.BeginAnimation(TranslateTransform.YProperty, slideOut);
        }

        private void HideClientPanel()
        {
            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = -600,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            slideOut.Completed += (s, e) =>
            {
                ClientOverlay.Visibility = Visibility.Collapsed;
                ClientOverlayBackground.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)));
                ClientOverlayBackground.Visibility = Visibility.Collapsed;
                ClientOverlayBackground.IsHitTestVisible = false;

                var transform = (TranslateTransform)ClientOverlay.RenderTransform;
                transform.Y = -600;
            };

            var transformOut = (TranslateTransform)ClientOverlay.RenderTransform;
            transformOut.BeginAnimation(TranslateTransform.YProperty, slideOut);
            LoadClients();
        }

        private void FilterClients_Click(object sender, RoutedEventArgs e)
        {
            var filterText = ClientsFilter.Text.ToLower();
            if (string.IsNullOrWhiteSpace(filterText))
            {
                ClientsList.ItemsSource = AllClients!.OrderBy(c => c.Name);
                return;
            }
            else
            {
                var filteredClients = AllClients!
                    .Where(c => filterText.Contains(c.Name, StringComparison.CurrentCultureIgnoreCase) || 
                        c.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
                ClientsList.ItemsSource = filteredClients.OrderBy(c => c.Name);
            }

        }

        private void RefreshClients_Click(object sender, RoutedEventArgs e)
        {
            LoadClients();
        }

        private async void AddCallLog_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsList.SelectedItem is not Client selectedClient)
            {
                ShowNotification("Please select a client first.");
                return;
            }

            var methodItem = ContactMethodInput.SelectedItem as ComboBoxItem;
            var method = methodItem?.Content?.ToString() ?? "Unknown";

            var textRange = new TextRange(SummaryInput.Document.ContentStart, SummaryInput.Document.ContentEnd);
            var summary = textRange.Text.Trim();

            if (string.IsNullOrWhiteSpace(summary))
            {
                ShowNotification("Summary is required.");
                return;
            }

            var callLog = new CallLog
            {
                ClientId = selectedClient.Id,
                CallDate = CallDateInput.SelectedDate ?? DateTime.Now,
                ContactMethod = method,
                Summary = summary,
                CreatedAt = DateTime.Now
            };

            await _db.AddCallLogAsync(callLog);
            var callLogs = await _db.GetCallLogsForClientAsync(selectedClient.Id);
            var groupedLogs = callLogs
                    .GroupBy(log => new { log.CallDate.Year, log.CallDate.Month })
                    .GroupBy(g => g.Key.Year)
                    .Select(yearGroup => new CallLogGroup
                    {
                        Year = yearGroup.Key.ToString(),
                        Months = new ObservableCollection<CallLogMonthGroup>(
                            yearGroup.Select(monthGroup => new CallLogMonthGroup
                            {
                                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key.Month),
                                Logs = new ObservableCollection<CallLog>(monthGroup.ToList())
                            }))
                    });
            CallLogs.ItemsSource = groupedLogs;

            SummaryInput.Document.Blocks.Clear();
        }

        private async void ShowNotification(string message)
        {
            NotificationText.Text = message;
            NotificationPopup.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            NotificationPopup.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(3000);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) => NotificationPopup.Visibility = Visibility.Collapsed;
            NotificationPopup.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void CallLogs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject? originalSource = e.OriginalSource as DependencyObject;

            while (originalSource != null && originalSource is not ListBoxItem)
            {
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }

            if (originalSource is ListBoxItem item && item.DataContext is CallLog callLog)
            {
                ShowCallLogPanel(callLog);
            }
        }

        private void ClientsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(ClientsList.SelectedItem is Client selectecClient)
            {
                ShowClientPanel(selectecClient);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string readme = "https://github.com/T2Dubs/LiteDesk/blob/master/README.md";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = readme,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowNotification($"Could not open help page: {ex.Message}");
            }
        }

        private void ClientsFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FilterClients_Click(FilterButton, new RoutedEventArgs());
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };

            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}