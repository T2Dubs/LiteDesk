using LiteDesk.Core;
using LiteDesk.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LiteDesk
{
    /// <summary>
    /// Interaction logic for ClientInfo.xaml
    /// </summary>
    public partial class ClientInfo : UserControl
    {
        public required IDatabaseService _db;
        public event EventHandler? RequestClosePanel;
        public Action<string>? ShowNotification;
        public List<Client>? Clients;
        private Client? _editingClient;

        public Client? EditingClient
        {
            get => _editingClient;
            set => _editingClient = value;
        }

        public ClientInfo()
        {
            InitializeComponent();
        }

        public void PopulateClient(Client client)
        {
            _editingClient = client;

            DeactivateButton.Visibility = Visibility.Visible;

            ClientName.Text = client.Name;
            ClientName.IsReadOnly = true;

            ClientPhone.Text = client.PhoneNumber;
            ClientEmail.Text = client.Email;
            Address1.Text = client.Address1;
            Address2.Text = client.Address2;
            City.Text = client.City;
            State.Text = client.State;
            Postal.Text = client.Postal;

            ClientNotes.Document.Blocks.Clear();
            ClientNotes.Document.Blocks.Add(new Paragraph(new Run(client.Notes ?? "")));
        }

        private async void SaveClient_Click(object sender, RoutedEventArgs e)
        {
            var textRange = new TextRange(ClientNotes.Document.ContentStart, ClientNotes.Document.ContentEnd);
            var notes = textRange.Text.Trim();

            if (_editingClient != null)
            {
                _editingClient.PhoneNumber = ClientPhone.Text;
                _editingClient.Email = ClientEmail.Text;
                _editingClient.Address1 = Address1.Text;
                _editingClient.Address2 = Address2.Text;
                _editingClient.City = City.Text;
                _editingClient.State = State.Text;
                _editingClient.Postal = Postal.Text;
                _editingClient.Notes = notes;

                await _db.UpdateClientAsync(_editingClient);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ClientName.Text))
                {
                    ShowNotification?.Invoke("Please enter a client name.");
                    return;
                }
                var client = new Client
                {
                    Name = ClientName.Text,
                    PhoneNumber = ClientPhone.Text,
                    Email = ClientEmail.Text,
                    Address1 = Address1.Text,
                    Address2 = Address2.Text,
                    City = City.Text,
                    State = State.Text,
                    Postal = Postal.Text,
                    Notes = notes,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                };

                await _db.AddClientAsync(client);
            }

            ClearFields();
            RequestClosePanel?.Invoke(this, EventArgs.Empty);
        }

        private void CancelClient_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            RequestClosePanel?.Invoke(this, EventArgs.Empty);
        }

        private void ClearFields()
        {
            _editingClient = null;
            ClientName.Clear();
            ClientName.IsReadOnly = false;
            ClientPhone.Clear();
            ClientEmail.Clear();
            Address1.Clear();
            Address2.Clear();
            City.Clear();
            State.Clear();
            Postal.Clear();
            DeactivateButton.Visibility = Visibility.Hidden;
            ClientNotes.Document.Blocks.Clear();
            DuplicateNameWarning.Text = "";
            DuplicatePhoneWarning.Text = "";
            DuplicateEmailWarning.Text = "";
        }

        private void DeactivateClient_Click(Object sender, RoutedEventArgs e)
        {
            ConfirmNameInput.Text = string.Empty;
            DeactivateConfirmOverlay.DataContext = EditingClient;
            DeactivateConfirmOverlay.Visibility = Visibility.Visible;
        }

        private void CancelDeactivate_Click(object sender, RoutedEventArgs e)
        {
            DeactivateConfirmOverlay.Visibility = Visibility.Collapsed;
        }

        private async void ProceedDeactivate_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmNameInput.Text.Trim().Equals(ClientName.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var client = _editingClient;
                client!.IsActive = false;
                await _db.UpdateClientAsync(client);

                DeactivateConfirmOverlay.Visibility = Visibility.Collapsed;
                ClearFields();
                RequestClosePanel?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ConfirmNameInput.Text = string.Empty;
                ShowNotification?.Invoke("Incorrect name entered.  Please try again.");
            }
        }

        private void ClientName_LostFocus(object sender, RoutedEventArgs e)
        {
            var enteredName = ClientName.Text.Trim();
            if(string.IsNullOrWhiteSpace(enteredName))
                return;

            var isDuplicate = Clients!.Any(c =>
                string.Equals(c.Name, enteredName, StringComparison.OrdinalIgnoreCase) &&
                (_editingClient == null || c.Id != _editingClient.Id));

            DuplicateNameWarning.Text = isDuplicate ? "⚠️ Duplicate client name" : "";
        }

        private void Phone_LostFocus(object sender, RoutedEventArgs e)
        {
            var enteredPhone = new string([.. ClientPhone.Text.Where(char.IsDigit)]);
            if (string.IsNullOrWhiteSpace(enteredPhone))
                return;

            var duplicateClient = Clients!.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.PhoneNumber) &&
                new string([.. c.PhoneNumber.Where(char.IsDigit)]) == enteredPhone &&
                (_editingClient == null || c.Id != _editingClient.Id));

            DuplicatePhoneWarning.Text = duplicateClient != null ? $"⚠️ Phone record exists on client {duplicateClient.Name}" : "";
        }

        private void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            var enteredEmail = ClientEmail.Text.Trim();
            if(string.IsNullOrWhiteSpace(enteredEmail))
                return;

            var duplicateEmail = Clients!.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.Email) &&
                string.Equals(c.Email, enteredEmail, StringComparison.OrdinalIgnoreCase) &&
                (_editingClient == null || c.Id != _editingClient.Id));

            DuplicateEmailWarning.Text = duplicateEmail != null ? $"⚠️ Email record exists on client {duplicateEmail.Name}" : "";
        }
    }
}