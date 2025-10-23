using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagementSystem.Services;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportingController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        /// <summary>
        /// Get complete dashboard with all metrics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime? date = null)
        {
            try
            {
                date ??= DateTime.Now;
                var dashboard = await _reportingService.GetDashboardAsync(date.Value);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get daily metrics for a specific date
        /// </summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyMetrics([FromQuery] DateTime? date = null)
        {
            try
            {
                date ??= DateTime.Now;
                var metrics = await _reportingService.GetDailyMetricsAsync(date.Value);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get weekly metrics with daily breakdown
        /// </summary>
        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyMetrics([FromQuery] DateTime? weekStart = null)
        {
            try
            {
                weekStart ??= DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                var metrics = await _reportingService.GetWeeklyMetricsAsync(weekStart.Value);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly metrics with weekly breakdown
        /// </summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyMetrics([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                year ??= DateTime.Now.Year;
                month ??= DateTime.Now.Month;
                var metrics = await _reportingService.GetMonthlyMetricsAsync(year.Value, month.Value);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive performance report
        /// Supports Daily, Weekly, Monthly periods
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceReport(
            [FromQuery] string period = "Monthly",
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                if (!new[] { "Daily", "Weekly", "Monthly" }.Contains(period))
                    return BadRequest(new { error = "Period must be Daily, Weekly, or Monthly" });

                dateFrom ??= DateTime.Now.AddMonths(-1);
                dateTo ??= DateTime.Now;

                var report = await _reportingService.GeneratePerformanceReportAsync(period, dateFrom.Value, dateTo.Value);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}