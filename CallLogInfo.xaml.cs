using LiteDesk.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LiteDesk
{
    /// <summary>
    /// Interaction logic for CallLogInfo.xaml
    /// </summary>
    public partial class CallLogInfo : UserControl
    {
        public event EventHandler? RequestClosePanel;

        public CallLogInfo()
        {
            InitializeComponent();
        }

        public void PopulateCallLog(CallLog callLog)
        {
            Summary.Document.Blocks.Clear();
            Summary.Document.Blocks.Add(new Paragraph(new Run(callLog.Summary ?? "")));
            CallDate.Text = callLog.CallDate.ToShortDateString();
            CreatedDate.Text = callLog.CreatedAt.ToShortDateString();
            ContactMethod.Text = callLog.ContactMethod.ToString();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Summary.Document.Blocks.Clear();
            CallDate.Clear();
            CreatedDate.Clear();
            ContactMethod.Clear();
            RequestClosePanel?.Invoke(this, EventArgs.Empty);
        }
    }
}
