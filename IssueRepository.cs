using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace MunicipalServicesApplication
{
    public static class RequestStatuses
    {
        public const string Pending = "Pending";
        public const string InProgress = "In Progress";
        public const string Resolved = "Resolved";
        public static string[] All { get { return new[] { Pending, InProgress, Resolved }; } }
    }

    // Single-process local store; snapshots protect validation and change notifications.
    // A null path provides isolated in-memory storage for tests.
    public sealed class IssueStore
    {
        private readonly List<IssueReport> issues = new List<IssueReport>();
        private readonly Dictionary<string, IssueReport> byReference =
            new Dictionary<string, IssueReport>(StringComparer.OrdinalIgnoreCase);
        private readonly string filePath;
        private bool loadFailed;
        public string StorageWarning { get; private set; }
        public event EventHandler Changed;

        public IssueStore(string filePath = null) { this.filePath = filePath; Load(); }

        public void AddIssue(IssueReport issue)
        {
            if (issue == null) throw new ArgumentNullException(nameof(issue));
            if (string.IsNullOrWhiteSpace(issue.Location) || string.IsNullOrWhiteSpace(issue.Category) ||
                string.IsNullOrWhiteSpace(issue.Description))
                throw new ArgumentException("Location, category and description are required.");
            string reference = (issue.ReferenceNumber ?? "").Trim();
            while (reference.Length == 0 || byReference.ContainsKey(reference))
                reference = Guid.NewGuid().ToString("N").ToUpperInvariant();
            issue.ReferenceNumber = reference;
            issue.Status = RequestStatuses.Pending;
            issue.LastUpdated = DateTime.Now;
            var stored = Copy(issue);
            issues.Add(stored);
            byReference.Add(reference, stored);
            SaveAndNotify();
        }

        public List<IssueReport> GetAllIssues() { return issues.Select(Copy).ToList(); }
        public IssueReport GetIssueByReference(string reference)
        {
            IssueReport found;
            return byReference.TryGetValue((reference ?? "").Trim(), out found) ? Copy(found) : null;
        }
        public bool UpdateStatus(string reference, string status)
        {
            if (!RequestStatuses.All.Contains(status)) throw new ArgumentException("Select a valid status.");
            IssueReport found;
            if (!byReference.TryGetValue((reference ?? "").Trim(), out found)) return false;
            found.Status = status;
            found.LastUpdated = DateTime.Now;
            SaveAndNotify();
            return true;
        }
        private static IssueReport Copy(IssueReport issue)
        {
            return new IssueReport { ReferenceNumber = issue.ReferenceNumber, Location = issue.Location,
                Category = issue.Category, Description = issue.Description, AttachmentPath = issue.AttachmentPath,
                DateReported = issue.DateReported, Status = issue.Status, LastUpdated = issue.LastUpdated };
        }
        private void Load()
        {
            if (filePath == null) return;
            try
            {
                using (var reader = XmlReader.Create(filePath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
                {
                    var loaded = (List<IssueReport>)new XmlSerializer(typeof(List<IssueReport>)).Deserialize(reader);
                    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (loaded == null || loaded.Any(i => i == null || string.IsNullOrWhiteSpace(i.ReferenceNumber) ||
                        !seen.Add(i.ReferenceNumber.Trim()) || !RequestStatuses.All.Contains(i.Status) ||
                        string.IsNullOrWhiteSpace(i.Location) || string.IsNullOrWhiteSpace(i.Category) ||
                        string.IsNullOrWhiteSpace(i.Description)))
                        throw new InvalidDataException("Invalid or duplicate request records.");
                    foreach (var issue in loaded)
                    {
                        issue.ReferenceNumber = issue.ReferenceNumber.Trim();
                        issues.Add(issue);
                        byReference.Add(issue.ReferenceNumber, issue);
                    }
                }
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (Exception ex) when (IsStorageError(ex))
            {
                loadFailed = true;
                StorageWarning = "Requests could not be loaded. Existing file is protected from overwrite. " +
                    "New changes are available only in this session. " + ex.Message;
            }
        }
        private void SaveAndNotify()
        {
            if (filePath != null && !loadFailed)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)));
                    string temporary = filePath + ".tmp";
                    using (var writer = new StreamWriter(temporary, false))
                        new XmlSerializer(typeof(List<IssueReport>)).Serialize(writer, issues);
                    if (File.Exists(filePath)) File.Replace(temporary, filePath, filePath + ".bak");
                    else File.Move(temporary, filePath);
                    StorageWarning = null;
                }
                catch (Exception ex) when (IsStorageError(ex))
                {
                    StorageWarning = "Change kept in this session, but could not be saved to disk. " + ex.Message;
                }
            }
            Changed?.Invoke(this, EventArgs.Empty);
        }
        private static bool IsStorageError(Exception ex)
        {
            return ex is IOException || ex is InvalidDataException || ex is UnauthorizedAccessException || ex is InvalidOperationException ||
                ex is XmlException || ex is System.Security.SecurityException || ex is ArgumentException ||
                ex is NotSupportedException;
        }
    }

    public static class IssueRepository
    {
        private static readonly IssueStore store = new IssueStore(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MunicipalServicesApplication", "requests.xml"));
        internal static IssueStore Store { get { return store; } }
        public static string StorageWarning { get { return store.StorageWarning; } }
        public static event EventHandler Changed { add { store.Changed += value; } remove { store.Changed -= value; } }
        public static void AddIssue(IssueReport issue) { store.AddIssue(issue); }
        public static List<IssueReport> GetAllIssues() { return store.GetAllIssues(); }
        public static IssueReport GetIssueByReference(string reference) { return store.GetIssueByReference(reference); }
        public static bool UpdateStatus(string reference, string status) { return store.UpdateStatus(reference, status); }
    }
}
