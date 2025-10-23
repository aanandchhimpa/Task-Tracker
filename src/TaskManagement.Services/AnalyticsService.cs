using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Services
{
    public interface IAnalyticsService
    {
        Task CalculateDailyMetricsAsync(DateTime date);
        Task CalculateWeeklyMetricsAsync(DateTime weekStart);
        Task CalculateMonthlyMetricsAsync(int year, int month);
        Task<List<UserPerformanceMetric>> GetUserMetricsAsync(int userId, string period);
        Task<UserPerformanceMetric> GetBestPerformerAsync(int departmentId, string period, DateTime dateFrom, DateTime dateTo);
        Task<List<UserPerformanceMetric>> GetTopPerformersAsync(int departmentId, string period, int topCount = 10);
        Task<DepartmentPerformanceMetric> GetDepartmentMetricsAsync(int departmentId, string period, DateTime dateFrom, DateTime dateTo);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly TaskManagementContext _context;

        public AnalyticsService(TaskManagementContext context)
        {
            _context = context;
        }

        public async Task CalculateDailyMetricsAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var assignments = await _context.TaskAssignments
                .Where(x => x.AssignedAt >= startDate && x.AssignedAt < endDate)
                .Include(x => x.AssignedTo)
                .Include(x => x.Task)
                .ToListAsync();

            var groupedByUser = assignments.GroupBy(x => x.AssignedToUserId);

            foreach (var userGroup in groupedByUser)
            {
                var metric = new UserPerformanceMetric
                {
                    UserId = userGroup.Key,
                    DepartmentId = userGroup.First().AssignedTo.DepartmentId,
                    DateFrom = startDate,
                    DateTo = endDate,
                    Period = "Daily",
                    TotalTasksAssigned = userGroup.Count(),
                    CompletedTasks = userGroup.Count(x => x.Status == TaskStatus.Completed),
                    PendingTasks = userGroup.Count(x => x.Status == TaskStatus.NotStarted),
                    OnHoldTasks = userGroup.Count(x => x.Status == TaskStatus.OnHold),
                    CancelledTasks = userGroup.Count(x => x.Status == TaskStatus.Cancelled),
                    ApprovedTasks = userGroup.Count(x => x.Status == TaskStatus.Completed),
                    HighPriorityCompleted = userGroup.Count(x => x.Task.Priority == (int)TaskPriority.High && x.Status == TaskStatus.Completed),
                    MediumPriorityCompleted = userGroup.Count(x => x.Task.Priority == (int)TaskPriority.Medium && x.Status == TaskStatus.Completed),
                    LowPriorityCompleted = userGroup.Count(x => x.Task.Priority == (int)TaskPriority.Low && x.Status == TaskStatus.Completed),
                };

                metric.ApprovalRate = metric.TotalTasksAssigned > 0 
                    ? (decimal)metric.ApprovedTasks / metric.TotalTasksAssigned * 100 
                    : 0;

                metric.OnTimeCompletions = userGroup
                    .Where(x => x.Status == TaskStatus.Completed && x.CompletedAt <= x.Task.DueDate)
                    .Count();

                metric.AverageCompletionTimeHours = CalculateAverageCompletionTime(userGroup);
                metric.EfficiencyScore = CalculateEfficiencyScore(metric);

                _context.UserPerformanceMetrics.Add(metric);
            }

            await _context.SaveChangesAsync();
        }

        public async Task CalculateWeeklyMetricsAsync(DateTime weekStart)
        {
            weekStart = weekStart.AddDays(-(int)weekStart.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            await CalculatePeriodicMetricsAsync(weekStart, weekEnd, "Weekly");
        }

        public async Task CalculateMonthlyMetricsAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            await CalculatePeriodicMetricsAsync(startDate, endDate, "Monthly");
        }

        private async Task CalculatePeriodicMetricsAsync(DateTime startDate, DateTime endDate, string period)
        {
            var assignments = await _context.TaskAssignments
                .Where(x => x.AssignedAt >= startDate && x.AssignedAt < endDate)
                .Include(x => x.AssignedTo)
                .Include(x => x.Task)
                .ToListAsync();

            var groupedByUser = assignments.GroupBy(x => x.AssignedToUserId);

            foreach (var userGroup in groupedByUser)
            {
                var completedTasks = userGroup.Where(x => x.Status == TaskStatus.Completed).ToList();

                var metric = new UserPerformanceMetric
                {
                    UserId = userGroup.Key,
                    DepartmentId = userGroup.First().AssignedTo.DepartmentId,
                    DateFrom = startDate,
                    DateTo = endDate,
                    Period = period,
                    TotalTasksAssigned = userGroup.Count(),
                    CompletedTasks = completedTasks.Count(),
                    PendingTasks = userGroup.Count(x => x.Status == TaskStatus.NotStarted),
                    OnHoldTasks = userGroup.Count(x => x.Status == TaskStatus.OnHold),
                    CancelledTasks = userGroup.Count(x => x.Status == TaskStatus.Cancelled),
                    ApprovedTasks = completedTasks.Count(),
                    HighPriorityCompleted = completedTasks.Count(x => x.Task.Priority == (int)TaskPriority.High),
                    MediumPriorityCompleted = completedTasks.Count(x => x.Task.Priority == (int)TaskPriority.Medium),
                    LowPriorityCompleted = completedTasks.Count(x => x.Task.Priority == (int)TaskPriority.Low),
                };

                metric.ApprovalRate = metric.TotalTasksAssigned > 0 
                    ? (decimal)metric.ApprovedTasks / metric.TotalTasksAssigned * 100 
                    : 0;

                metric.OnTimeCompletions = completedTasks
                    .Where(x => x.CompletedAt <= x.Task.DueDate)
                    .Count();

                metric.LateCompletions = completedTasks
                    .Where(x => x.CompletedAt > x.Task.DueDate)
                    .Count();

                metric.AverageCompletionTimeHours = CalculateAverageCompletionTime(userGroup);
                metric.EfficiencyScore = CalculateEfficiencyScore(metric);

                _context.UserPerformanceMetrics.Add(metric);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<UserPerformanceMetric>> GetUserMetricsAsync(int userId, string period)
        {
            return await _context.UserPerformanceMetrics
                .Where(x => x.UserId == userId && x.Period == period)
                .OrderByDescending(x => x.DateFrom)
                .ToListAsync();
        }

        public async Task<UserPerformanceMetric> GetBestPerformerAsync(int departmentId, string period, DateTime dateFrom, DateTime dateTo)
        {
            return await _context.UserPerformanceMetrics
                .Where(x => x.DepartmentId == departmentId && 
                           x.Period == period && 
                           x.DateFrom >= dateFrom && 
                           x.DateFrom < dateTo)
                .OrderByDescending(x => x.EfficiencyScore)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserPerformanceMetric>> GetTopPerformersAsync(int departmentId, string period, int topCount = 10)
        {
            var lastDate = await _context.UserPerformanceMetrics
                .Where(x => x.DepartmentId == departmentId && x.Period == period)
                .MaxAsync(x => (DateTime?)x.DateFrom) ?? DateTime.Now;

            return await _context.UserPerformanceMetrics
                .Where(x => x.DepartmentId == departmentId && 
                           x.Period == period && 
                           x.DateFrom == lastDate)
                .OrderByDescending(x => x.EfficiencyScore)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<DepartmentPerformanceMetric> GetDepartmentMetricsAsync(int departmentId, string period, DateTime dateFrom, DateTime dateTo)
        {
            var userMetrics = await _context.UserPerformanceMetrics
                .Where(x => x.DepartmentId == departmentId && 
                           x.Period == period && 
                           x.DateFrom >= dateFrom && 
                           x.DateFrom < dateTo)
                .ToListAsync();

            var topPerformer = userMetrics.OrderByDescending(x => x.EfficiencyScore).FirstOrDefault();

            var deptMetric = new DepartmentPerformanceMetric
            {
                DepartmentId = departmentId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Period = period,
                TotalTasksAssigned = userMetrics.Sum(x => x.TotalTasksAssigned),
                CompletedTasks = userMetrics.Sum(x => x.CompletedTasks),
                ApprovedTasks = userMetrics.Sum(x => x.ApprovedTasks),
                RejectedTasks = userMetrics.Sum(x => x.RejectedTasks),
                TeamMembersActive = userMetrics.Count(),
                TopPerformerId = topPerformer?.UserId ?? 0,
                AverageEfficiencyScore = userMetrics.Any() 
                    ? userMetrics.Average(x => x.EfficiencyScore) 
                    : 0
            };

            deptMetric.ApprovalRate = deptMetric.TotalTasksAssigned > 0 
                ? (decimal)deptMetric.ApprovedTasks / deptMetric.TotalTasksAssigned * 100 
                : 0;

            return deptMetric;
        }

        private double CalculateAverageCompletionTime(IGrouping<int, TaskAssignment> userGroup)
        {
            var completedWithTime = userGroup
                .Where(x => x.Status == TaskStatus.Completed && x.CompletedAt.HasValue)
                .Select(x => (x.CompletedAt.Value - x.AssignedAt).TotalHours)
                .ToList();

            return completedWithTime.Any() ? completedWithTime.Average() : 0;
        }

        private decimal CalculateEfficiencyScore(UserPerformanceMetric metric)
        {
            if (metric.TotalTasksAssigned == 0)
                return 0;

            var completionRate = (decimal)metric.CompletedTasks / metric.TotalTasksAssigned;
            var approvalRate = metric.ApprovalRate / 100;
            var onTimeRate = metric.OnTimeCompletions > 0 
                ? (decimal)metric.OnTimeCompletions / metric.CompletedTasks 
                : 0;

            // Weighted score: Completion 40%, Approval 35%, On-Time 25%
            return (completionRate * 40 + approvalRate * 35 + onTimeRate * 25);
        }
    }
}