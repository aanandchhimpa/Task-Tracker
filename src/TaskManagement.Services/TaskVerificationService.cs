using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Services
{
    public interface ITaskVerificationService
    {
        Task ApproveTaskAsync(int taskAssignmentId, int verifierId, string comments = null);
        Task RejectTaskAsync(int taskAssignmentId, int verifierId, string rejectionReason);
        Task RequestRevisionAsync(int taskAssignmentId, int verifierId, string revisionNotes);
        Task<TaskVerification> GetVerificationAsync(int taskAssignmentId);
        Task<List<TaskVerification>> GetPendingVerificationsAsync(int departmentId);
    }

    public class TaskVerificationService : ITaskVerificationService
    {
        private readonly TaskManagementContext _context;
        private readonly ITaskWorkflowService _workflowService;

        public TaskVerificationService(TaskManagementContext context, ITaskWorkflowService workflowService)
        {
            _context = context;
            _workflowService = workflowService;
        }

        public async Task ApproveTaskAsync(int taskAssignmentId, int verifierId, string comments = null)
        {
            var verification = await _context.TaskVerifications
                .FirstOrDefaultAsync(x => x.TaskAssignmentId == taskAssignmentId);

            if (verification == null)
                throw new Exception("Verification record not found");

            verification.Status = VerificationStatus.Approved;
            verification.VerifiedAt = DateTime.UtcNow;
            verification.VerifiedByUserId = verifierId;
            verification.VerificationComments = comments;

            await _context.SaveChangesAsync();
        }

        public async Task RejectTaskAsync(int taskAssignmentId, int verifierId, string rejectionReason)
        {
            var assignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(x => x.Id == taskAssignmentId);

            if (assignment == null)
                throw new Exception("Task assignment not found");

            var verification = await _context.TaskVerifications
                .FirstOrDefaultAsync(x => x.TaskAssignmentId == taskAssignmentId);

            if (verification == null)
                throw new Exception("Verification record not found");

            verification.Status = VerificationStatus.Rejected;
            verification.RejectionCount++;
            verification.RejectedAt = DateTime.UtcNow;
            verification.RejectionReason = rejectionReason;
            verification.VerifiedByUserId = verifierId;

            // Move task back to InProgress
            await _workflowService.UpdateTaskStatusAsync(taskAssignmentId, TaskStatus.InProgress, verifierId,
                $"Rejected for revision: {rejectionReason}");

            await _context.SaveChangesAsync();
        }

        public async Task RequestRevisionAsync(int taskAssignmentId, int verifierId, string revisionNotes)
        {
            var verification = await _context.TaskVerifications
                .FirstOrDefaultAsync(x => x.TaskAssignmentId == taskAssignmentId);

            if (verification == null)
                throw new Exception("Verification record not found");

            verification.Status = VerificationStatus.NeedsRevision;
            verification.RejectionCount++;
            verification.RejectionReason = revisionNotes;
            verification.VerifiedByUserId = verifierId;

            await _context.SaveChangesAsync();
        }

        public async Task<TaskVerification> GetVerificationAsync(int taskAssignmentId)
        {
            return await _context.TaskVerifications
                .Include(x => x.VerifiedByUser)
                .FirstOrDefaultAsync(x => x.TaskAssignmentId == taskAssignmentId);
        }

        public async Task<List<TaskVerification>> GetPendingVerificationsAsync(int departmentId)
        {
            return await _context.TaskVerifications
                .Where(x => x.Status == VerificationStatus.Pending &&
                           x.TaskAssignment.AssignedTo.DepartmentId == departmentId)
                .Include(x => x.TaskAssignment)
                .ThenInclude(x => x.AssignedTo)
                .OrderBy(x => x.VerifiedAt)
                .ToListAsync();
        }
    }
}