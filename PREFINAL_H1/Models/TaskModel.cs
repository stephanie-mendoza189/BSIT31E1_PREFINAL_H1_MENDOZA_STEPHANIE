namespace PREFINAL_H1.Models
{
    public class TaskModel
    {
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class FilterModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string TargetDate { get; set; } = "2026-08-29";
        public string Owner { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
    }
}