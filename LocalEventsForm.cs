
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MunicipalServicesApplication
{
    public partial class LocalEventsForm : Form
    {
        // Stores all available events for display and future searching.
        private List<MunicipalEvent> allEvents;

        private Dictionary<string, List<MunicipalEvent>> eventsByCategory =
            new Dictionary<string, List<MunicipalEvent>>();

        private SortedDictionary<DateTime, List<MunicipalEvent>> eventsByDate =
            new SortedDictionary<DateTime, List<MunicipalEvent>>();

        private Queue<MunicipalEvent> upcomingEventsQueue =
            new Queue<MunicipalEvent>();

        private Stack<string> searchHistory = new Stack<string>();


        public LocalEventsForm()
        {
            InitializeComponent();

            SetupEventGrid();
            LoadSampleEvents();
            BuildEventDataStructures();
            PopulateCategories();

            // Display upcoming events when the form opens.
            DisplayEvents(GetUpcomingEventsInDateOrder());

            // Display initial recommendations immediately.
            GenerateRecommendations();

            // Connect the search and clear buttons.
            btnSearch.Click += btnSearch_Click;
            btnClear.Click += btnClear_Click;
            btnViewDetails.Click += btnViewDetails_Click;
        }



        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchEvents();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFilters();
        }

        private void SetupEventGrid()
        {
            dgvEvents.AutoGenerateColumns = false;
            dgvEvents.Columns.Clear();

            dgvEvents.ReadOnly = true;
            dgvEvents.AllowUserToAddRows = false;
            dgvEvents.AllowUserToDeleteRows = false;
            dgvEvents.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvEvents.MultiSelect = false;
            dgvEvents.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvEvents.RowHeadersVisible = false;

            // Event name column
            DataGridViewTextBoxColumn nameColumn =
                new DataGridViewTextBoxColumn();
            nameColumn.Name = "colEventName";
            nameColumn.HeaderText = "Event Name";
            nameColumn.DataPropertyName = "EventName";
            nameColumn.FillWeight = 150;
            dgvEvents.Columns.Add(nameColumn);

            // Category column
            DataGridViewTextBoxColumn categoryColumn =
                new DataGridViewTextBoxColumn();
            categoryColumn.Name = "colCategory";
            categoryColumn.HeaderText = "Category";
            categoryColumn.DataPropertyName = "Category";
            categoryColumn.FillWeight = 90;
            dgvEvents.Columns.Add(categoryColumn);

            // Date column
            DataGridViewTextBoxColumn dateColumn =
                new DataGridViewTextBoxColumn();
            dateColumn.Name = "colEventDate";
            dateColumn.HeaderText = "Date";
            dateColumn.DataPropertyName = "EventDate";
            dateColumn.DefaultCellStyle.Format = "dd MMM yyyy";
            dateColumn.FillWeight = 90;
            dgvEvents.Columns.Add(dateColumn);

            // Location column
            DataGridViewTextBoxColumn locationColumn =
                new DataGridViewTextBoxColumn();
            locationColumn.Name = "colLocation";
            locationColumn.HeaderText = "Location";
            locationColumn.DataPropertyName = "Location";
            locationColumn.FillWeight = 120;
            dgvEvents.Columns.Add(locationColumn);
        }

        private void LoadSampleEvents()
        {
            allEvents = new List<MunicipalEvent>
            {
                new MunicipalEvent
                {
                    EventName = "Community Clean-up",
                    Category = "Community",
                    EventDate = DateTime.Today.AddDays(3),
                    Location = "Town Hall",
                    Description =
                        "Residents are invited to help clean public spaces."
                },

                new MunicipalEvent
                {
                    EventName = "Health Awareness Day",
                    Category = "Health",
                    EventDate = DateTime.Today.AddDays(7),
                    Location = "Civic Centre",
                    Description =
                        "A community event promoting health awareness."
                },

                new MunicipalEvent
                {
                    EventName = "Youth Skills Workshop",
                    Category = "Education",
                    EventDate = DateTime.Today.AddDays(10),
                    Location = "Public Library",
                    Description =
                        "A workshop introducing useful skills to young people."
                },

                new MunicipalEvent
                {
                    EventName = "Recycling Awareness Campaign",
                    Category = "Environment",
                    EventDate = DateTime.Today.AddDays(14),
                    Location = "Community Park",
                    Description =
                        "Learn about recycling and responsible waste disposal."
                },

                new MunicipalEvent
                {
                    EventName = "Small Business Information Session",
                    Category = "Business",
                    EventDate = DateTime.Today.AddDays(18),
                    Location = "Municipal Hall",
                    Description =
                        "Information for residents interested in small businesses."
                },

                new MunicipalEvent
                {
                    EventName = "Community Sports Day",
                    Category = "Sports",
                    EventDate = DateTime.Today.AddDays(21),
                    Location = "Sports Ground",
                    Description =
                        "A day of sports and activities for the community."
                },

                new MunicipalEvent
                {
                    EventName = "Public Safety Awareness",
                    Category = "Safety",
                    EventDate = DateTime.Today.AddDays(25),
                    Location = "Civic Centre",
                    Description =
                        "Learn about community safety and emergency preparedness."
                }
            };
        }

        private void DisplayEvents(List<MunicipalEvent> eventsToDisplay)
        {
            dgvEvents.DataSource = null;
            dgvEvents.DataSource = eventsToDisplay;
        }
        private void PopulateCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("All Categories");

            HashSet<string> categories = new HashSet<string>();

            foreach (MunicipalEvent municipalEvent in allEvents)
            {
                categories.Add(municipalEvent.Category);
            }

            foreach (string category in categories)
            {
                cmbCategory.Items.Add(category);
            }

            cmbCategory.SelectedIndex = 0;
        }

        private void SearchEvents()
        {
            List<MunicipalEvent> filteredEvents = new List<MunicipalEvent>();

            string selectedCategory =
                cmbCategory.SelectedItem?.ToString() ?? "All Categories";

            // Record the search using the Stack.
            string searchRecord = selectedCategory;

            if (chkFilterDate.Checked)
            {
                searchRecord += " | " +
                    dtpSearchDate.Value.ToShortDateString();
            }

            searchHistory.Push(searchRecord);

            // Use the Dictionary to retrieve events by category.
            IEnumerable<MunicipalEvent> eventsToSearch = allEvents;

            if (selectedCategory != "All Categories" &&
                eventsByCategory.ContainsKey(selectedCategory))
            {
                eventsToSearch = eventsByCategory[selectedCategory];
            }
            else if (selectedCategory != "All Categories")
            {
                eventsToSearch = new List<MunicipalEvent>();
            }

            foreach (MunicipalEvent municipalEvent in eventsToSearch)
            {
                bool matchesDate =
                    !chkFilterDate.Checked ||
                    municipalEvent.EventDate.Date == dtpSearchDate.Value.Date;

                if (matchesDate)
                {
                    filteredEvents.Add(municipalEvent);
                }
            }

            DisplayEvents(filteredEvents);
            GenerateRecommendations();

            if (filteredEvents.Count == 0)
            {
                MessageBox.Show(
                    "No events match your selected filters.",
                    "No Events Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }


        private void GenerateRecommendations()
        {
            lstRecommendations.Items.Clear();

            Dictionary<string, int> categorySearchCounts =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            // Count searches, with the most recent searches processed first.
            foreach (string search in searchHistory)
            {
                string category = search.Split('|')[0].Trim();

                if (category == "All Categories")
                {
                    continue;
                }

                if (categorySearchCounts.ContainsKey(category))
                {
                    categorySearchCounts[category]++;
                }
                else
                {
                    categorySearchCounts[category] = 1;
                }
            }

            EventPriorityQueue priorityQueue = new EventPriorityQueue();

            foreach (MunicipalEvent municipalEvent in allEvents)
            {
                if (municipalEvent.EventDate.Date < DateTime.Today)
                {
                    continue;
                }

                int preferenceScore = 0;

                if (categorySearchCounts.ContainsKey(municipalEvent.Category))
                {
                    preferenceScore = categorySearchCounts[municipalEvent.Category];
                }

                // When a specific category has been searched, recommend
                // matching categories. Otherwise show upcoming events.
                if (categorySearchCounts.Count > 0 && preferenceScore == 0)
                {
                    continue;
                }

                int daysUntilEvent =
                    (municipalEvent.EventDate.Date - DateTime.Today).Days;

                // Lower values have higher priority.
                // Preference is the primary factor; date breaks ties.
                int priority = -(preferenceScore * 100000) +
                               Math.Min(daysUntilEvent, 99999);

                priorityQueue.Enqueue(municipalEvent, priority);
            }

            int displayed = 0;

            while (!priorityQueue.IsEmpty() && displayed < 5)
            {
                MunicipalEvent recommendedEvent = priorityQueue.Dequeue();

                lstRecommendations.Items.Add(
                    recommendedEvent.EventName +
                    " | " + recommendedEvent.Category +
                    " | " + recommendedEvent.EventDate.ToString("dd MMM yyyy") +
                    " | " + recommendedEvent.Location);

                displayed++;
            }

            if (displayed == 0)
            {
                lstRecommendations.Items.Add(
                    "No upcoming recommendations are available.");
            }
        }

        private void ClearFilters()
        {
            cmbCategory.SelectedIndex = 0;
            chkFilterDate.Checked = false;

            // Restore upcoming events using the FIFO queue.
            DisplayEvents(GetUpcomingEventsFromQueue());

            // Refresh recommendations using search history.
            GenerateRecommendations();
        }


        private void BuildEventDataStructures()
        {
            eventsByCategory.Clear();
            eventsByDate.Clear();
            upcomingEventsQueue.Clear();

            // Sort events chronologically before building the queue.
            List<MunicipalEvent> chronologicalEvents =
                new List<MunicipalEvent>(allEvents);

            chronologicalEvents.Sort(
                (first, second) =>
                    first.EventDate.CompareTo(second.EventDate));

            foreach (MunicipalEvent municipalEvent in chronologicalEvents)
            {
                // Dictionary: group events by category.
                if (!eventsByCategory.ContainsKey(municipalEvent.Category))
                {
                    eventsByCategory[municipalEvent.Category] =
                        new List<MunicipalEvent>();
                }

                eventsByCategory[municipalEvent.Category].Add(municipalEvent);

                // SortedDictionary: group events by date.
                DateTime eventDate = municipalEvent.EventDate.Date;

                if (!eventsByDate.ContainsKey(eventDate))
                {
                    eventsByDate[eventDate] = new List<MunicipalEvent>();
                }

                eventsByDate[eventDate].Add(municipalEvent);

                // Queue: add upcoming events in chronological order.
                if (municipalEvent.EventDate.Date >= DateTime.Today)
                {
                    upcomingEventsQueue.Enqueue(municipalEvent);
                }
            }
        }

        private List<MunicipalEvent> GetUpcomingEventsInDateOrder()
        {
            List<MunicipalEvent> upcomingEvents =
                new List<MunicipalEvent>();

            // SortedDictionary automatically stores dates in sorted order.
            foreach (KeyValuePair<DateTime, List<MunicipalEvent>> entry
                in eventsByDate)
            {
                // Ignore dates that have already passed.
                if (entry.Key < DateTime.Today)
                {
                    continue;
                }

                foreach (MunicipalEvent municipalEvent in entry.Value)
                {
                    upcomingEvents.Add(municipalEvent);
                }
            }

            return upcomingEvents;
        }

        private List<MunicipalEvent> GetUpcomingEventsFromQueue()
        {
            // ToArray reads the queue in FIFO order without removing items.
            return new List<MunicipalEvent>(upcomingEventsQueue.ToArray());
        }


        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            // Check whether an event has been selected.
            if (dgvEvents.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select an event to view its details.",
                    "No Event Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // Retrieve the selected event from the DataGridView.
            MunicipalEvent selectedEvent =
                dgvEvents.SelectedRows[0].DataBoundItem as MunicipalEvent;

            if (selectedEvent == null)
            {
                MessageBox.Show(
                    "Unable to retrieve the selected event.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            // Display the event details.
            string details =
                "Event: " + selectedEvent.EventName + Environment.NewLine +
                "Category: " + selectedEvent.Category + Environment.NewLine +
                "Date: " + selectedEvent.EventDate.ToString("dd MMMM yyyy") +
                Environment.NewLine +
                "Location: " + selectedEvent.Location + Environment.NewLine +
                "Description: " + selectedEvent.Description;

            MessageBox.Show(
                details,
                "Event Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

    }

    // Represents one local event or announcement.
    public class MunicipalEvent
    {
        public string EventName { get; set; }

        public string Category { get; set; }

        public DateTime EventDate { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }
    }


    public class EventPriorityQueue
    {
        private readonly SortedDictionary<int, Queue<MunicipalEvent>> items =
            new SortedDictionary<int, Queue<MunicipalEvent>>();

        public int Count { get; private set; }

        public void Enqueue(MunicipalEvent municipalEvent, int priority)
        {
            if (!items.ContainsKey(priority))
            {
                items[priority] = new Queue<MunicipalEvent>();
            }

            items[priority].Enqueue(municipalEvent);
            Count++;
        }

        public MunicipalEvent Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException(
                    "The priority queue is empty.");
            }

            // The smallest priority value is processed first.
            int highestPriority = 0;

            foreach (int priority in items.Keys)
            {
                highestPriority = priority;
                break;
            }

            MunicipalEvent nextEvent = items[highestPriority].Dequeue();

            if (items[highestPriority].Count == 0)
            {
                items.Remove(highestPriority);
            }

            Count--;

            return nextEvent;
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }
    }

}
