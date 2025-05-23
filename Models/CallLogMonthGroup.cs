using LiteDesk.Core.Models;
using System.Collections.ObjectModel;

namespace LiteDesk.Models
{
    public class CallLogMonthGroup
    {
        public required string MonthName { get; set; }
        public ObservableCollection<CallLog>? Logs { get; set; }
        public bool IsExpanded { get; set; } = false;
    }
}
