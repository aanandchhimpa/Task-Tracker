using System;
using System.Collections.Generic;

namespace TaskManagement.Api.DTOs
{
    // Task Workflow DTOs
    public class UpdateTaskStatusDto
    {
        public int AssignmentId { get; set; }
        public string NewStatus { get; set; } // NotStarted, Started, InProgress, Completed, OnHold, Cancelled
        public string Notes { get; set; }
    }

    public class TaskHistoryDto
    {
        public int Id { get; set; }
        public string PreviousStatus { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Notes { get; set; }
        public string ChangedByUserName { get; set; }
    }

    public class TaskAssignmentDetailDto
    {
        public int Id { get; set; }
        public string TaskTitle { get; set; }
        public string AssignedToName { get; set; }
        public string CurrentStatus { get; set; }
        public string Priority { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<TaskHistoryDto> History { get; set; }
    }

    // Task Verification DTOs
    public class VerifyTaskDto
    {
        public int TaskAssignmentId { get; set; }
        public string Action { get; set; } // "Approve", "Reject", "RequestRevision"
        public string Comments { get; set; }
    }

    public class TaskVerificationDto
    {
        public int Id { get; set; }
        public int TaskAssignmentId { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected, NeedsRevision
        public string VerifiedByUserName { get; set; }
        public string VerificationComments { get; set; }
        public DateTime VerifiedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }
        public int RejectionCount { get; set; }
    }

    public class PendingVerificationDto
    {
        public int VerificationId { get; set; }
        public int TaskAssignmentId { get; set; }
        public string TaskTitle { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDepartment { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Priority { get; set; }
    }

    // Analytics DTOs
    public class UserPerformanceDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Department { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Period { get; set; } // Daily, Weekly, Monthly

        // Task Statistics
        public int TotalTasksAssigned { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OnHoldTasks { get; set; }
        public int CancelledTasks { get; set; }
        public int RejectedTasks { get; set; }

        // Quality Metrics
        public int ApprovedTasks { get; set; }
        public decimal ApprovalRate { get; set; }
        public int RevisionCount { get; set; }

        // Time Metrics
        public double AverageCompletionTimeHours { get; set; }
        public int OnTimeCompletions { get; set; }
        public int LateCompletions { get; set; }

        // Priority Breakdown
        public int HighPriorityCompleted { get; set; }
        public int MediumPriorityCompleted { get; set; }
        public int LowPriorityCompleted { get; set; }

        // Overall Score
        public decimal EfficiencyScore { get; set; }
        public string PerformanceRating { get; set; } // Excellent, Good, Fair, Poor
    }

    public class TopPerformerDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal EfficiencyScore { get; set; }
        public int CompletedTasks { get; set; }
        public int ApprovedTasks { get; set; }
        public decimal ApprovalRate { get; set; }
        public double AverageCompletionTimeHours { get; set; }
    }

    public class DepartmentPerformanceDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Period { get; set; }

        public int TotalTasksAssigned { get; set; }
        public int CompletedTasks { get; set; }
        public int ApprovedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public decimal ApprovalRate { get; set; }

        public int TeamMembersActive { get; set; }
        public string TopPerformerName { get; set; }
        public decimal AverageEfficiencyScore { get; set; }
    }

    public class AnalyticsFilterDto
    {
        public int? DepartmentId { get; set; }
        public int? UserId { get; set; }
        public string Period { get; set; } // Daily, Weekly, Monthly
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}