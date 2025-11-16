using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunicationService.Data;
using CommunicationService.Models;
using CommunicationService.DTOs;
using CommunicationService.Services;
using CommunicationService.EventDTOs;
using System.Text.Json;
using System.Configuration;

namespace CommunicationService.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommunicationServiceContext _context;
        private readonly IMessageBusPublisher _messageBusPublisher;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CommentsController(CommunicationServiceContext context,
            IMessageBusPublisher messageBusPublisher,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _messageBusPublisher = messageBusPublisher;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: api/comments/task/b1234567-abcd-....
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForTask(Guid taskId)
        {
            return await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Select(c => new CommentDto // Використовуємо DTO
                {
                    Id = c.Id,
                    TaskId = c.TaskId,
                    AuthorId = c.AuthorId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .OrderBy(c => c.CreatedAt) // Сортуємо за датою
                .ToListAsync();
        }

        // POST: api/comments
        [HttpPost("async")]
        public async Task<ActionResult<CommentDto>> PostComment(CreateCommentDto createCommentDto)
        {
            var commentEntity = new CommentEntity
            {
                TaskId = createCommentDto.TaskId,
                AuthorId = createCommentDto.AuthorId,
                Content = createCommentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(commentEntity);
            await _context.SaveChangesAsync();

            try
            {
                var eventDto = new TaskUpdatedEventDto
                {
                    TaskId = commentEntity.TaskId,
                    Timestamp = commentEntity.CreatedAt
                };
                _messageBusPublisher.PublishTaskUpdated(eventDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> ERROR: Could not send message to RabbitMQ: {ex.Message}");
            }

            Console.WriteLine("--> TODO: Publish 'CommentAdded' event to RabbitMQ");

            var commentDto = new CommentDto
            {
                Id = commentEntity.Id,
                TaskId = commentEntity.TaskId,
                AuthorId = commentEntity.AuthorId,
                Content = commentEntity.Content,
                CreatedAt = commentEntity.CreatedAt
            };

            return CreatedAtAction(nameof(GetCommentsForTask), new { taskId = commentEntity.TaskId }, commentDto);
        }
        [HttpPost("sync")]
        public async Task<ActionResult<CommentDto>> PostCommentSync(CreateCommentDto createCommentDto)
        {
            var commentEntity = new CommentEntity
            {
                TaskId = createCommentDto.TaskId,
                AuthorId = createCommentDto.AuthorId,
                Content = createCommentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(commentEntity);
                await _context.SaveChangesAsync();

            Console.WriteLine("--> (Sync) Comment saved. Now calling TaskService...");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                var taskServiceUrl = _configuration["TaskServiceUrl"];
                var requestUrl = $"{taskServiceUrl}/api/tasks/update-activity/{commentEntity.TaskId}";

                var response = await httpClient.PatchAsync(requestUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("--> (Sync) TaskService updated successfully.");
                }
                else
                {
                    Console.WriteLine($"--> (Sync) TaskService call FAILED: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> (Sync) HTTP Client FAILED: {ex.Message}");
            }
            
            var commentDto = new CommentDto
            {
                Id = commentEntity.Id,
                TaskId = commentEntity.TaskId,
                AuthorId = commentEntity.AuthorId,
                Content = commentEntity.Content,
                CreatedAt = commentEntity.CreatedAt
            };

            return CreatedAtAction(nameof(GetCommentsForTask), new { taskId = commentEntity.TaskId }, commentDto);
        }
    }
}