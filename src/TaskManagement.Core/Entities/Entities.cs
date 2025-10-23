using System;
using System.Collections.Generic;

namespace TaskManagement.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int DepartmentId { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Department Department { get; set; }
        public virtual User CreatedByAdmin { get; set; }
        public virtual ICollection<TaskAssignment> AssignedTasks { get; set; } = new List<TaskAssignment>();
    }

    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }

    public class Module
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }

    public class Category
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Module Module { get; set; }
        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }

    public class SubCategory
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Category Category { get; set; }
    }

    public class DailyTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ModuleId { get; set; }
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public DateTime DueDate { get; set; }
        public int Priority { get; set; } // use TaskPriority enum (stored as int)
        public string Status { get; set; } // Pending, In Progress, Completed, On Hold
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Module Module { get; set; }
        public virtual Category Category { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public virtual ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
    }

    public class TaskAssignment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int AssignedToUserId { get; set; }
        public int AssignedByUserId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }

        // workflow fields
        public TaskStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual DailyTask Task { get; set; }
        public virtual User AssignedTo { get; set; }
        public virtual User AssignedBy { get; set; }
    }
}
