using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskManagementSystem.ViewModels
{
    // Main Dashboard ViewModel
    public class DashboardViewModel
    {
        public DailyMetricsViewModel DailyMetrics { get; set; }
        public WeeklyMetricsViewModel WeeklyMetrics { get; set; }
        public MonthlyMetricsViewModel MonthlyMetrics { get; set; }
        public List<TopPerformerViewModel> TopPerformers { get; set; }
        public List<DepartmentMetricViewModel> DepartmentMetrics { get; set; }
        public List<PendingTaskViewModel> PendingVerifications { get; set; }
        public TaskDistributionViewModel TaskDistribution { get; set; }
    }

    public class DailyMetricsViewModel
    {
        public DateTime Date { get; set; }
        public int TotalTasksAssigned { get; set; }
        public int CompletedToday { get; set; }
        public int InProgress { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public decimal AverageCompletionTime { get; set; }
        public decimal ApprovalRate { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
    }

    public class WeeklyMetricsViewModel
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int TotalCompletedTasks { get; set; }
        public int HighPriorityCompleted { get; set; }
        public int MediumPriorityCompleted { get; set; }
        public int LowPriorityCompleted { get; set; }
        public decimal AverageEfficiencyScore { get; set; }
        public int RejectedCount { get; set; }
        public decimal ApprovalRate { get; set; }
        public List<DailyDataPoint> DailyBreakdown { get; set; }
    }

    public class MonthlyMetricsViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalTasksAssigned { get; set; }
        public int CompletedTasks { get; set; }
        public int ApprovedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal AverageEfficiencyScore { get; set; }
        public int ActiveEmployees { get; set; }
        public List<WeeklyDataPoint> WeeklyBreakdown { get; set; }
        public Dictionary<string, int> PriorityDistribution { get; set; }
    }

    public class TopPerformerViewModel
    {
        public int Rank { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public decimal EfficiencyScore { get; set; }
        public int CompletedTasks { get; set; }
        public decimal ApprovalRate { get; set; }
        public double AvgCompletionHours { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int HighPriorityCompleted { get; set; }
        public string Badge { get; set; } // "Gold", "Silver", "Bronze"
    }

    public class DepartmentMetricViewModel
    {
        public string DepartmentName { get; set; }
        public int TeamSize { get; set; }
        public int TotalTasksAssigned { get; set; }
        public int CompletedTasks { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal AverageEfficiencyScore { get; set; }
        public string TopPerformerName { get; set; }
        public List<EmployeeMetricViewModel> EmployeeMetrics { get; set; }
    }

    public class EmployeeMetricViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int RejectedTasks { get; set; }
        public decimal EfficiencyScore { get; set; }
        public string PerformanceRating { get; set; }
        public decimal ApprovalRate { get; set; }
    }

    public class PendingTaskViewModel
    {
        public int TaskId { get; set; }
        public int AssignmentId { get; set; }
        public string TaskTitle { get; set; }
        public string EmployeeName { get; set; }
        public string Module { get; set; }
        public string Category { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Priority { get; set; }
        public int DaysWaitingForReview { get; set; }
    }

    public class TaskDistributionViewModel
    {
        public int NotStarted { get; set; }
        public int Started { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int OnHold { get; set; }
        public int Cancelled { get; set; }
        public int Total { get; set; }
    }

    public class DailyDataPoint
    {
        public DateTime Date { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
        public decimal EfficiencyScore { get; set; }
    }

    public class WeeklyDataPoint
    {
        public int Week { get; set; }
        public int Completed { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public decimal EfficiencyScore { get; set; }
    }

    // Performance Report
    public class PerformanceReportViewModel
    {
        public string ReportTitle { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string Period { get; set; } // Daily, Weekly, Monthly
        
        // Executive Summary
        public int TotalEmployees { get; set; }
        public int TotalTasksAssigned { get; set; }
        public int TotalCompleted { get; set; }
        public decimal OverallApprovalRate { get; set; }
        public decimal OverallEfficiencyScore { get; set; }

        // Detailed Data
        public List<EmployeeDetailedReportViewModel> EmployeeReports { get; set; }
        public List<DepartmentDetailedReportViewModel> DepartmentReports { get; set; }

        // Insights
        public string TopPerformer { get; set; }
        public string NeedsImprovement { get; set; }
        public List<string> Insights { get; set; }
    }

    public class EmployeeDetailedReportViewModel
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public int TasksAssigned { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksApproved { get; set; }
        public int TasksRejected { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal EfficiencyScore { get; set; }
        public double AvgCompletionHours { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
        public string Rating { get; set; }
        public List<TaskDetailViewModel> RecentTasks { get; set; }
    }

    public class DepartmentDetailedReportViewModel
    {
        public string DepartmentName { get; set; }
        public int TeamSize { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int ApprovedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal AverageEfficiencyScore { get; set; }
        public string TopPerformer { get; set; }
        public string BottomPerformer { get; set; }
    }

    public class TaskDetailViewModel
    {
        public string TaskTitle { get; set; }
        public string Module { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CompletedDate { get; set; }
        public bool IsOnTime { get; set; }
        public int RejectionCount { get; set; }
    }
}