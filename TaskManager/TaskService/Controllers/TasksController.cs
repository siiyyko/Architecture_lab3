using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.DTOs.EventDTOs;
using TaskService.Models;
using TaskService.DTOs;

using TaskStatus = TaskService.Models.TaskStatus;

namespace TaskService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskServiceContext _context;

        public TasksController(TaskServiceContext context)
        {
            _context = context;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTaskEntity()
        {
            return await _context.TaskEntity.ToListAsync();
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskEntity>> GetTaskEntity(Guid id)
        {
            var taskEntity = await _context.TaskEntity.FindAsync(id);

            if (taskEntity == null)
            {
                return NotFound();
            }

            return taskEntity;
        }

        // PUT: api/Tasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskEntity(Guid id, TaskEntity taskEntity)
        {
            if (id != taskEntity.Id)
            {
                return BadRequest();
            }

            _context.Entry(taskEntity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskEntityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TaskEntity>> PostTaskEntity(CreateTaskDto createTaskDto)
        {
            var taskCount = await _context.TaskEntity.CountAsync();
            var newCodeName = $"IF-{taskCount + 1}";

            var taskEntity = new TaskEntity
            {
                Name = createTaskDto.Name,
                Description = createTaskDto.Description,
                ReporterId = createTaskDto.ReporterId,
                AssigneeId = createTaskDto.AssigneeId,
                Priority = (TaskPriority)createTaskDto.Priority,
                Status = (TaskStatus)createTaskDto.Status,

                Id = Guid.NewGuid(),
                CodeName = newCodeName,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.TaskEntity.Add(taskEntity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTaskEntity", new { id = taskEntity.Id }, taskEntity);
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskEntity(Guid id)
        {
            var taskEntity = await _context.TaskEntity.FindAsync(id);
            if (taskEntity == null)
            {
                return NotFound();
            }

            _context.TaskEntity.Remove(taskEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: /api/Tasks/update-activity/5
        [HttpPatch("update-activity/{id}")]
        public async Task<IActionResult> UpdateTaskActivity(Guid id)
        {
            var task = await _context.TaskEntity.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            task.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Console.WriteLine($"--> (Sync Call) Task {id} activity updated.");

            return Ok();
        }

        private bool TaskEntityExists(Guid id)
        {
            return _context.TaskEntity.Any(e => e.Id == id);
        }

        // PATCH: api/tasks/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] TaskStatusUpdatedDto statusDto)
        {
            var task = await _context.TaskEntity.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            task.Status = (TaskStatus)statusDto.NewStatus;
            task.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
