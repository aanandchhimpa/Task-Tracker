using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Api.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskWorkflowController : ControllerBase
    {
        private readonly ITaskWorkflowService _workflowService;
        private readonly TaskManagementContext _context;

        public TaskWorkflowController(ITaskWorkflowService workflowService, TaskManagementContext context)
        {
            _workflowService = workflowService;
            _context = context;
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusDto dto)
        {
            try
            {
                if (!Enum.TryParse<TaskStatus>(dto.NewStatus, true, out var newStatus))
                    return BadRequest("Invalid status");

                var userId = 1; // TODO: replace with authenticated user
                await _workflowService.UpdateTaskStatusAsync(dto.AssignmentId, newStatus, userId, dto.Notes);
                return Ok(new { message = "Task status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history/{assignmentId}")]
        public async Task<IActionResult> GetTaskHistory(int assignmentId)
        {
            try
            {
                var history = await _workflowService.GetTaskHistoryAsync(assignmentId);
                var dtos = history.Select(h => new TaskHistoryDto
                {
                    Id = h.Id,
                    PreviousStatus = h.PreviousStatus.ToString(),
                    CurrentStatus = h.CurrentStatus.ToString(),
                    ChangedAt = h.ChangedAt,
                    Notes = h.Notes,
                    ChangedByUserName = h.ChangedByUser?.FirstName + " " + h.ChangedByUser?.LastName
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("task-details/{assignmentId}")]
        public async Task<IActionResult> GetTaskDetails(int assignmentId)
        {
            try
            {
                var assignment = await _context.TaskAssignments
                    .Include(x => x.Task)
                    .Include(x => x.AssignedTo)
                    .FirstOrDefaultAsync(x => x.Id == assignmentId);

                if (assignment == null)
                    return NotFound("Task assignment not found");

                var history = await _workflowService.GetTaskHistoryAsync(assignmentId);

                var dto = new TaskAssignmentDetailDto
                {
                    Id = assignment.Id,
                    TaskTitle = assignment.Task.Title,
                    AssignedToName = assignment.AssignedTo.FirstName + " " + assignment.AssignedTo.LastName,
                    CurrentStatus = assignment.Status.ToString(),
                    Priority = ((TaskPriority)assignment.Task.Priority).ToString(),
                    DueDate = assignment.Task.DueDate,
                    AssignedAt = assignment.AssignedAt,
                    CompletedAt = assignment.CompletedAt,
                    History = history.Select(h => new TaskHistoryDto
                    {
                        Id = h.Id,
                        PreviousStatus = h.PreviousStatus.ToString(),
                        CurrentStatus = h.CurrentStatus.ToString(),
                        ChangedAt = h.ChangedAt,
                        Notes = h.Notes,
                        ChangedByUserName = h.ChangedByUser?.FirstName + " " + h.ChangedByUser?.LastName
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}