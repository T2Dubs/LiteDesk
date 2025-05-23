using System.Collections.ObjectModel;

namespace LiteDesk.Models
{
    public class CallLogGroup
    {
        public required string Year { get; set; }
        public ObservableCollection<CallLogMonthGroup>? Months { get; set; }
        public bool IsExpanded { get; set; } = false;
    }
}
