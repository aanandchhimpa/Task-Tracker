using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Services
{
    public interface IReportingService
    {
        Task<DashboardViewModel> GetDashboardAsync(DateTime date);
        Task<DailyMetricsViewModel> GetDailyMetricsAsync(DateTime date);
        Task<WeeklyMetricsViewModel> GetWeeklyMetricsAsync(DateTime weekStart);
        Task<MonthlyMetricsViewModel> GetMonthlyMetricsAsync(int year, int month);
        Task<PerformanceReportViewModel> GeneratePerformanceReportAsync(string period, DateTime dateFrom, DateTime dateTo);
        Task<List<TopPerformerViewModel>> GetTopPerformersDetailedAsync(int departmentId, string period, int topCount = 10);
    }

    public class ReportingService : IReportingService
    {
        private readonly TaskManagementContext _context;

        public ReportingService(TaskManagementContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardAsync(DateTime date)
        {
            var daily = await GetDailyMetricsAsync(date);
            var weekly = await GetWeeklyMetricsAsync(date.AddDays(-(int)date.DayOfWeek));
            var monthly = await GetMonthlyMetricsAsync(date.Year, date.Month);
            
            var topPerformers = await GetTopPerformersDetailedAsync(0, "Daily", 5);
            
            var departments = await _context.Departments
                .Include(d => d.Users)
                .ToListAsync();

            var departmentMetrics = new List<DepartmentMetricViewModel>();
            foreach (var dept in departments)
            {
                var deptMetric = new DepartmentMetricViewModel
                {
                    DepartmentName = dept.Name,
                    TeamSize = dept.Users.Count()
                };
                departmentMetrics.Add(deptMetric);
            }

            var pendingVerifications = await _context.TaskVerifications
                .Where(x => x.Status == VerificationStatus.Pending)
                .Include(x => x.TaskAssignment)
                .ThenInclude(x => x.Task)
                .Include(x => x.TaskAssignment)
                .ThenInclude(x => x.AssignedTo)
                .Take(10)
                .Select(x => new PendingTaskViewModel
                {
                    TaskId = x.TaskAssignment.Task.Id,
                    AssignmentId = x.TaskAssignment.Id,
                    TaskTitle = x.TaskAssignment.Task.Title,
                    EmployeeName = x.TaskAssignment.AssignedTo.FirstName + " " + x.TaskAssignment.AssignedTo.LastName,
                    Priority = ((TaskPriority)x.TaskAssignment.Task.Priority).ToString(),
                    CompletedAt = x.TaskAssignment.CompletedAt ?? DateTime.Now,
                    DaysWaitingForReview = (int)(DateTime.Now - (x.TaskAssignment.CompletedAt ?? DateTime.Now)).TotalDays
                })
                .ToListAsync();

            var taskDistribution = await GetTaskDistributionAsync();

            return new DashboardViewModel
            {
                DailyMetrics = daily,
                WeeklyMetrics = weekly,
                MonthlyMetrics = monthly,
                TopPerformers = topPerformers,
                DepartmentMetrics = departmentMetrics,
                PendingVerifications = pendingVerifications,
                TaskDistribution = taskDistribution
            };
        }

        public async Task<DailyMetricsViewModel> GetDailyMetricsAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var assignments = await _context.TaskAssignments
                .Where(x => x.AssignedAt >= startDate && x.AssignedAt < endDate)
                .Include(x => x.Task)
                .ToListAsync();

            var completedToday = assignments.Count(x => x.Status == TaskStatus.Completed);
            var onTime = assignments.Count(x => 
                x.Status == TaskStatus.Completed && x.CompletedAt <= x.Task.DueDate);
            var late = completedToday - onTime;

            var avgCompletionTime = assignments
                .Where(x => x.Status == TaskStatus.Completed && x.CompletedAt.HasValue)
                .Select(x => (x.CompletedAt.Value - x.AssignedAt).TotalHours)
                .DefaultIfEmpty(0)
                .Average();

            return new DailyMetricsViewModel
            {
                Date = date,
                TotalTasksAssigned = assignments.Count(),
                CompletedToday = completedToday,
                InProgress = assignments.Count(x => x.Status == TaskStatus.InProgress),
                Pending = assignments.Count(x => x.Status == TaskStatus.NotStarted),
                Rejected = assignments.Count(x => x.Status == TaskStatus.Cancelled),
                AverageCompletionTime = (decimal)avgCompletionTime,
                OnTimeDeliveries = onTime,
                LateDeliveries = late,
                ApprovalRate = completedToday > 0 ? (onTime * 100m / completedToday) : 0
            };
        }

        public async Task<WeeklyMetricsViewModel> GetWeeklyMetricsAsync(DateTime weekStart)
        {
            weekStart = weekStart.AddDays(-(int)weekStart.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var metrics = await _context.UserPerformanceMetrics
                .Where(x => x.DateFrom >= weekStart && x.DateFrom < weekEnd && x.Period == "Daily")
                .ToListAsync();

            var dailyBreakdown = new List<DailyDataPoint>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                var dayMetrics = metrics.Where(x => x.DateFrom.Date == date.Date).ToList();
                
                dailyBreakdown.Add(new DailyDataPoint
                {
                    Date = date,
                    Completed = dayMetrics.Sum(x => x.CompletedTasks),
                    Rejected = dayMetrics.Sum(x => x.RejectedTasks),
                    EfficiencyScore = dayMetrics.Any() ? (decimal)dayMetrics.Average(x => x.EfficiencyScore) : 0
                });
            }

            var totalCompleted = metrics.Sum(x => x.CompletedTasks);

            return new WeeklyMetricsViewModel
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                TotalCompletedTasks = totalCompleted,
                HighPriorityCompleted = metrics.Sum(x => x.HighPriorityCompleted),
                MediumPriorityCompleted = metrics.Sum(x => x.MediumPriorityCompleted),
                LowPriorityCompleted = metrics.Sum(x => x.LowPriorityCompleted),
                AverageEfficiencyScore = metrics.Any() ? (decimal)metrics.Average(x => x.EfficiencyScore) : 0,
                RejectedCount = metrics.Sum(x => x.RejectedTasks),
                ApprovalRate = metrics.Sum(x => x.TotalTasksAssigned) > 0 
                    ? (totalCompleted * 100m / metrics.Sum(x => x.TotalTasksAssigned)) 
                    : 0,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<MonthlyMetricsViewModel> GetMonthlyMetricsAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var metrics = await _context.UserPerformanceMetrics
                .Where(x => x.DateFrom >= startDate && x.DateFrom < endDate)
                .ToListAsync();

            var weeklyBreakdown = new List<WeeklyDataPoint>();
            var weeks = 4;

            for (int w = 0; w < weeks; w++)
            {
                var weekStart = startDate.AddDays(w * 7);
                var weekEnd = weekStart.AddDays(7);
                var weekMetrics = metrics.Where(x => x.DateFrom >= weekStart && x.DateFrom < weekEnd).ToList();

                weeklyBreakdown.Add(new WeeklyDataPoint
                {
                    Week = w + 1,
                    Completed = weekMetrics.Sum(x => x.CompletedTasks),
                    Approved = weekMetrics.Sum(x => x.ApprovedTasks),
                    Rejected = weekMetrics.Sum(x => x.RejectedTasks),
                    EfficiencyScore = weekMetrics.Any() ? (decimal)weekMetrics.Average(x => x.EfficiencyScore) : 0
                });
            }

            var totalTasksAssigned = metrics.Sum(x => x.TotalTasksAssigned);
            var completedTasks = metrics.Sum(x => x.CompletedTasks);

            return new MonthlyMetricsViewModel
            {
                Month = month,
                Year = year,
                TotalTasksAssigned = totalTasksAssigned,
                CompletedTasks = completedTasks,
                ApprovedTasks = metrics.Sum(x => x.ApprovedTasks),
                RejectedTasks = metrics.Sum(x => x.RejectedTasks),
                ApprovalRate = totalTasksAssigned > 0 ? (completedTasks * 100m / totalTasksAssigned) : 0,
                AverageEfficiencyScore = metrics.Any() ? (decimal)metrics.Average(x => x.EfficiencyScore) : 0,
                ActiveEmployees = metrics.Select(x => x.UserId).Distinct().Count(),
                WeeklyBreakdown = weeklyBreakdown,
                PriorityDistribution = new Dictionary<string, int>
                {
                    { "High", metrics.Sum(x => x.HighPriorityCompleted) },
                    { "Medium", metrics.Sum(x => x.MediumPriorityCompleted) },
                    { "Low", metrics.Sum(x => x.LowPriorityCompleted) }
                }
            };
        }

        public async Task<PerformanceReportViewModel> GeneratePerformanceReportAsync(string period, DateTime dateFrom, DateTime dateTo)
        {
            var metrics = await _context.UserPerformanceMetrics
                .Where(x => x.Period == period && x.DateFrom >= dateFrom && x.DateFrom < dateTo)
                .Include(x => x.User)
                .Include(x => x.Department)
                .ToListAsync();

            var employeeReports = new List<EmployeeDetailedReportViewModel>();
            var groupedByEmployee = metrics.GroupBy(x => x.UserId);

            foreach (var employeeGroup in groupedByEmployee)
            {
                var empMetrics = employeeGroup.ToList();
                var totalAssigned = empMetrics.Sum(x => x.TotalTasksAssigned);
                var totalCompleted = empMetrics.Sum(x => x.CompletedTasks);

                employeeReports.Add(new EmployeeDetailedReportViewModel
                {
                    Name = empMetrics.First().User.FirstName + " " + empMetrics.First().User.LastName,
                    Department = empMetrics.First().Department.Name,
                    TasksAssigned = totalAssigned,
                    TasksCompleted = totalCompleted,
                    TasksApproved = empMetrics.Sum(x => x.ApprovedTasks),
                    TasksRejected = empMetrics.Sum(x => x.RejectedTasks),
                    CompletionRate = totalAssigned > 0 ? (totalCompleted * 100m / totalAssigned) : 0,
                    ApprovalRate = empMetrics.Average(x => x.ApprovalRate),
                    EfficiencyScore = empMetrics.Average(x => x.EfficiencyScore),
                    AvgCompletionHours = empMetrics.Average(x => x.AverageCompletionTimeHours),
                    OnTimeDeliveries = empMetrics.Sum(x => x.OnTimeCompletions),
                    LateDeliveries = empMetrics.Sum(x => x.LateCompletions),
                    Rating = GetRating(empMetrics.Average(x => x.EfficiencyScore))
                });
            }

            var departmentReports = new List<DepartmentDetailedReportViewModel>();
            var groupedByDept = metrics.GroupBy(x => x.DepartmentId);

            foreach (var deptGroup in groupedByDept)
            {
                var deptMetrics = deptGroup.ToList();
                var totalTasks = deptMetrics.Sum(x => x.TotalTasksAssigned);
                var completedTasks = deptMetrics.Sum(x => x.CompletedTasks);

                departmentReports.Add(new DepartmentDetailedReportViewModel
                {
                    DepartmentName = deptMetrics.First().Department.Name,
                    TeamSize = deptMetrics.Select(x => x.UserId).Distinct().Count(),
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    ApprovedTasks = deptMetrics.Sum(x => x.ApprovedTasks),
                    RejectedTasks = deptMetrics.Sum(x => x.RejectedTasks),
                    ApprovalRate = totalTasks > 0 ? (completedTasks * 100m / totalTasks) : 0,
                    AverageEfficiencyScore = deptMetrics.Average(x => x.EfficiencyScore)
                });
            }

            var totalAssignedAll = metrics.Sum(x => x.TotalTasksAssigned);
            var totalCompletedAll = metrics.Sum(x => x.CompletedTasks);

            return new PerformanceReportViewModel
            {
                ReportTitle = $"{period} Performance Report",
                GeneratedOn = DateTime.Now,
                Period = period,
                TotalEmployees = employeeReports.Count,
                TotalTasksAssigned = totalAssignedAll,
                TotalCompleted = totalCompletedAll,
                OverallApprovalRate = metrics.Average(x => x.ApprovalRate),
                OverallEfficiencyScore = metrics.Average(x => x.EfficiencyScore),
                EmployeeReports = employeeReports.OrderByDescending(x => x.EfficiencyScore).ToList(),
                DepartmentReports = departmentReports,
                TopPerformer = employeeReports.OrderByDescending(x => x.EfficiencyScore).FirstOrDefault()?.Name,
                NeedsImprovement = employeeReports.OrderBy(x => x.EfficiencyScore).FirstOrDefault()?.Name
            };
        }

        public async Task<List<TopPerformerViewModel>> GetTopPerformersDetailedAsync(int departmentId, string period, int topCount = 10)
        {
            var query = _context.UserPerformanceMetrics
                .Include(x => x.User)
                .AsQueryable();

            if (departmentId > 0)
                query = query.Where(x => x.DepartmentId == departmentId);

            var metrics = await query
                .Where(x => x.Period == period)
                .OrderByDescending(x => x.EfficiencyScore)
                .Take(topCount)
                .ToListAsync();

            return metrics.Select((m, index) => new TopPerformerViewModel
            {
                Rank = index + 1,
                EmployeeName = m.User.FirstName + " " + m.User.LastName,
                Department = m.Department.Name,
                EfficiencyScore = m.EfficiencyScore,
                CompletedTasks = m.CompletedTasks,
                ApprovalRate = m.ApprovalRate,
                AvgCompletionHours = m.AverageCompletionTimeHours,
                OnTimeDeliveries = m.OnTimeCompletions,
                HighPriorityCompleted = m.HighPriorityCompleted,
                Badge = index switch
                {
                    0 => "🥇 Gold",
                    1 => "🥈 Silver",
                    2 => "🥉 Bronze",
                    _ => ""
                }
            }).ToList();
        }

        private async Task<TaskDistributionViewModel> GetTaskDistributionAsync()
        {
            var assignments = await _context.TaskAssignments.ToListAsync();

            return new TaskDistributionViewModel
            {
                NotStarted = assignments.Count(x => x.Status == TaskStatus.NotStarted),
                Started = assignments.Count(x => x.Status == TaskStatus.Started),
                InProgress = assignments.Count(x => x.Status == TaskStatus.InProgress),
                Completed = assignments.Count(x => x.Status == TaskStatus.Completed),
                OnHold = assignments.Count(x => x.Status == TaskStatus.OnHold),
                Cancelled = assignments.Count(x => x.Status == TaskStatus.Cancelled),
                Total = assignments.Count()
            };
        }

        private string GetRating(decimal score)
        {
            return score switch
            {
                >= 90 => "⭐⭐⭐⭐⭐ Excellent",
                >= 75 => "⭐⭐⭐⭐ Good",
                >= 60 => "⭐⭐⭐ Fair",
                _ => "⭐ Needs Improvement"
            };
        }
    }
}