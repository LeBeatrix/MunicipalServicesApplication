using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalServicesApplication
{
    public static class EventSearch
    {
        public static List<MunicipalEvent> Filter(IEnumerable<MunicipalEvent> events, string keyword, string category, DateTime? date)
        {
            string term = (keyword ?? "").Trim();
            return events.Where(e =>
                (string.IsNullOrEmpty(category) || category == "All Categories" || string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase)) &&
                (!date.HasValue || e.EventDate.Date == date.Value.Date) &&
                (term.Length == 0 || (e.EventName ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 (e.Description ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(e => e.EventDate).ToList();
        }
        public static string Validate(MunicipalEvent item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.EventName) || string.IsNullOrWhiteSpace(item.Category) ||
                string.IsNullOrWhiteSpace(item.Location) || string.IsNullOrWhiteSpace(item.Description))
                return "Title, category, location and description are required.";
            if (item.EventDate.Date < DateTime.Today) return "Choose today or a future date.";
            if (item.Kind != "Event" && item.Kind != "Announcement") return "Choose Event or Announcement.";
            return null;
        }
    }
}
