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
    public class TaskVerificationController : ControllerBase
    {
        private readonly ITaskVerificationService _verificationService;
        private readonly TaskManagementContext _context;

        public TaskVerificationController(ITaskVerificationService verificationService, TaskManagementContext context)
        {
            _verificationService = verificationService;
            _context = context;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyTask([FromBody] VerifyTaskDto dto)
        {
            try
            {
                int verifierId = 1; // Get from auth token

                switch (dto.Action?.ToLower())
                {
                    case "approve":
                        await _verificationService.ApproveTaskAsync(dto.TaskAssignmentId, verifierId, dto.Comments);
                        return Ok(new { message = "Task approved successfully" });

                    case "reject":
                        await _verificationService.RejectTaskAsync(dto.TaskAssignmentId, verifierId, dto.Comments);
                        return Ok(new { message = "Task rejected and moved back to In Progress" });

                    case "requestrevision":
                    case "request_revision":
                        await _verificationService.RequestRevisionAsync(dto.TaskAssignmentId, verifierId, dto.Comments);
                        return Ok(new { message = "Revision requested" });

                    default:
                        return BadRequest("Invalid action");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("verification/{taskAssignmentId}")]
        public async Task<IActionResult> GetVerification(int taskAssignmentId)
        {
            try
            {
                var verification = await _verificationService.GetVerificationAsync(taskAssignmentId);
                if (verification == null)
                    return NotFound("Verification not found");

                var dto = new TaskVerificationDto
                {
                    Id = verification.Id,
                    TaskAssignmentId = verification.TaskAssignmentId,
                    Status = verification.Status.ToString(),
                    VerifiedByUserName = verification.VerifiedByUser?.FirstName + " " + verification.VerifiedByUser?.LastName,
                    VerificationComments = verification.VerificationComments,
                    VerifiedAt = verification.VerifiedAt,
                    RejectedAt = verification.RejectedAt,
                    RejectionReason = verification.RejectionReason,
                    RejectionCount = verification.RejectionCount
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("pending/{departmentId}")]
        public async Task<IActionResult> GetPendingVerifications(int departmentId)
        {
            try
            {
                var pending = await _verificationService.GetPendingVerificationsAsync(departmentId);

                var dtos = pending.Select(v => new PendingVerificationDto
                {
                    VerificationId = v.Id,
                    TaskAssignmentId = v.TaskAssignmentId,
                    TaskTitle = v.TaskAssignment.Task.Title,
                    EmployeeName = v.TaskAssignment.AssignedTo.FirstName + " " + v.TaskAssignment.AssignedTo.LastName,
                    EmployeeDepartment = v.TaskAssignment.AssignedTo.Department.Name,
                    CompletedAt = v.TaskAssignment.CompletedAt ?? DateTime.Now,
                    Priority = ((TaskPriority)v.TaskAssignment.Task.Priority).ToString()
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}