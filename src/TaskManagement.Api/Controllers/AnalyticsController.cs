using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Api.DTOs;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly TaskManagementContext _context;

        public AnalyticsController(IAnalyticsService analyticsService, TaskManagementContext context)
        {
            _analyticsService = analyticsService;
            _context = context;
        }

        [HttpPost("calculate/daily")]
        public async Task<IActionResult> CalculateDailyMetrics([FromBody] DateTime date)
        {
            try
            {
                await _analyticsService.CalculateDailyMetricsAsync(date);
                return Ok(new { message = "Daily metrics calculated" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("calculate/weekly")]
        public async Task<IActionResult> CalculateWeeklyMetrics([FromBody] DateTime weekStart)
        {
            try
            {
                await _analyticsService.CalculateWeeklyMetricsAsync(weekStart);
                return Ok(new { message = "Weekly metrics calculated" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("calculate/monthly")]
        public async Task<IActionResult> CalculateMonthlyMetrics([FromBody] dynamic yearMonth)
        {
            try
            {
                int year = (int)yearMonth.year;
                int month = (int)yearMonth.month;
                await _analyticsService.CalculateMonthlyMetricsAsync(year, month);
                return Ok(new { message = "Monthly metrics calculated" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserMetrics(int userId, [FromQuery] string period = "Monthly")
        {
            try
            {
                var metrics = await _analyticsService.GetUserMetricsAsync(userId, period);
                var user = await _context.Users.FindAsync(userId);

                var dtos = metrics.Select(m => new UserPerformanceDto
                {
                    UserId = m.UserId,
                    UserName = user.FirstName + " " + user.LastName,
                    Department = user.Department.Name,
                    DateFrom = m.DateFrom,
                    DateTo = m.DateTo,
                    Period = m.Period,
                    TotalTasksAssigned = m.TotalTasksAssigned,
                    CompletedTasks = m.CompletedTasks,
                    PendingTasks = m.PendingTasks,
                    OnHoldTasks = m.OnHoldTasks,
                    CancelledTasks = m.CancelledTasks,
                    RejectedTasks = m.RejectedTasks,
                    ApprovedTasks = m.ApprovedTasks,
                    ApprovalRate = m.ApprovalRate,
                    RevisionCount = m.RevisionCount,
                    AverageCompletionTimeHours = m.AverageCompletionTimeHours,
                    OnTimeCompletions = m.OnTimeCompletions,
                    LateCompletions = m.LateCompletions,
                    HighPriorityCompleted = m.HighPriorityCompleted,
                    MediumPriorityCompleted = m.MediumPriorityCompleted,
                    LowPriorityCompleted = m.LowPriorityCompleted,
                    EfficiencyScore = m.EfficiencyScore,
                    PerformanceRating = GetPerformanceRating(m.EfficiencyScore)
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("top-performers/{departmentId}")]
        public async Task<IActionResult> GetTopPerformers(int departmentId, [FromQuery] string period = "Monthly", [FromQuery] int topCount = 10)
        {
            try
            {
                var metrics = await _analyticsService.GetTopPerformersAsync(departmentId, period, topCount);

                var dtos = metrics.Select((m, index) => new TopPerformerDto
                {
                    Rank = index + 1,
                    UserId = m.UserId,
                    UserName = m.User.FirstName + " " + m.User.LastName,
                    EfficiencyScore = m.EfficiencyScore,
                    CompletedTasks = m.CompletedTasks,
                    ApprovedTasks = m.ApprovedTasks,
                    ApprovalRate = m.ApprovalRate,
                    AverageCompletionTimeHours = m.AverageCompletionTimeHours
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("best-performer/{departmentId}")]
        public async Task<IActionResult> GetBestPerformer(int departmentId, [FromQuery] string period = "Monthly",
            [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                dateFrom ??= DateTime.Now.AddMonths(-1);
                dateTo ??= DateTime.Now;

                var metric = await _analyticsService.GetBestPerformerAsync(departmentId, period, dateFrom.Value, dateTo.Value);

                if (metric == null)
                    return NotFound("No performance data available");

                var dto = new UserPerformanceDto
                {
                    UserId = metric.UserId,
                    UserName = metric.User.FirstName + " " + metric.User.LastName,
                    Department = metric.Department.Name,
                    Period = metric.Period,
                    EfficiencyScore = metric.EfficiencyScore,
                    CompletedTasks = metric.CompletedTasks,
                    ApprovedTasks = metric.ApprovedTasks,
                    ApprovalRate = metric.ApprovalRate,
                    PerformanceRating = GetPerformanceRating(metric.EfficiencyScore)
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetDepartmentMetrics(int departmentId, [FromQuery] string period = "Monthly",
            [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                dateFrom ??= DateTime.Now.AddMonths(-1);
                dateTo ??= DateTime.Now;

                var metric = await _analyticsService.GetDepartmentMetricsAsync(departmentId, period, dateFrom.Value, dateTo.Value);
                var dept = await _context.Departments.FindAsync(departmentId);

                var dto = new DepartmentPerformanceDto
                {
                    DepartmentId = metric.DepartmentId,
                    DepartmentName = dept.Name,
                    DateFrom = metric.DateFrom,
                    DateTo = metric.DateTo,
                    Period = metric.Period,
                    TotalTasksAssigned = metric.TotalTasksAssigned,
                    CompletedTasks = metric.CompletedTasks,
                    ApprovedTasks = metric.ApprovedTasks,
                    RejectedTasks = metric.RejectedTasks,
                    ApprovalRate = metric.ApprovalRate,
                    TeamMembersActive = metric.TeamMembersActive,
                    TopPerformerName = metric.TopPerformer?.FirstName + " " + metric.TopPerformer?.LastName,
                    AverageEfficiencyScore = metric.AverageEfficiencyScore
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetPerformanceRating(decimal score)
        {
            return score switch
            {
                >= 90 => "Excellent",
                >= 75 => "Good",
                >= 60 => "Fair",
                _ => "Needs Improvement"
            };
        }
    }
}