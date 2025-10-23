using System;
using System.Collections.Generic;

namespace TaskManagement.Core.Entities
{
    // TaskStatus enum - Workflow states
    public enum TaskStatus
    {
        NotStarted = 0,
        Started = 1,
        InProgress = 2,
        Completed = 3,
        OnHold = 4,
        Cancelled = 5
    }

    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum VerificationStatus
    {
        Pending = 0,      // Waiting for verification
        Approved = 1,     // Verified and approved
        Rejected = 2,     // Sent back for revision
        NeedsRevision = 3 // Minor issues, resubmit
    }

    // TaskHistory - Track status changes
    public class TaskHistory
    {
        public int Id { get; set; }
        public int TaskAssignmentId { get; set; }
        public TaskStatus PreviousStatus { get; set; }
        public TaskStatus CurrentStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Notes { get; set; }
        public int ChangedByUserId { get; set; }

        public virtual TaskAssignment TaskAssignment { get; set; }
        public virtual User ChangedByUser { get; set; }
    }

    // TaskVerification - Verify completed tasks
    public class TaskVerification
    {
        public int Id { get; set; }
        public int TaskAssignmentId { get; set; }
        public VerificationStatus Status { get; set; }
        public int VerifiedByUserId { get; set; } // Admin/Leader
        public string VerificationComments { get; set; }
        public DateTime VerifiedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }
        public int RejectionCount { get; set; } // Track rejections

        public virtual TaskAssignment TaskAssignment { get; set; }
        public virtual User VerifiedByUser { get; set; }
    }

    // UserPerformanceMetric - Daily/Weekly/Monthly performance
    public class UserPerformanceMetric
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Period { get; set; } // "Daily", "Weekly", "Monthly"
        
        // Performance metrics
        public int TotalTasksAssigned { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OnHoldTasks { get; set; }
        public int CancelledTasks { get; set; }
        public int RejectedTasks { get; set; }
        
        // Quality metrics
        public int ApprovedTasks { get; set; }
        public decimal ApprovalRate { get; set; } // Percentage
        public int RevisionCount { get; set; }
        
        // Time metrics
        public double AverageCompletionTimeHours { get; set; }
        public int OnTimeCompletions { get; set; }
        public int LateCompletions { get; set; }
        
        // Priority breakdown
        public int HighPriorityCompleted { get; set; }
        public int MediumPriorityCompleted { get; set; }
        public int LowPriorityCompleted { get; set; }
        
        // Efficiency score (0-100)
        public decimal EfficiencyScore { get; set; }
        public DateTime CalculatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Department Department { get; set; }
    }

    // DepartmentPerformanceMetric - Department level analytics
    public class DepartmentPerformanceMetric
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Period { get; set; }
        
        public int TotalTasksAssigned { get; set; }
        public int CompletedTasks { get; set; }
        public int ApprovedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public decimal ApprovalRate { get; set; }
        
        public int TeamMembersActive { get; set; }
        public int TopPerformerId { get; set; }
        public decimal AverageEfficiencyScore { get; set; }
        
        public DateTime CalculatedAt { get; set; }

        public virtual Department Department { get; set; }
        public virtual User TopPerformer { get; set; }
    }
}