using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Integrations;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    public class CreateSupportTicketCommandHandler : ICQRS.ICreateSupportTicket
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IDropboxService _dropboxService;
        private readonly ILogger<CreateSupportTicketCommandHandler> _logger;

        public CreateSupportTicketCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            IDropboxService dropboxService,
            ILogger<CreateSupportTicketCommandHandler> logger)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _dropboxService = dropboxService;
            _logger = logger;
        }

        public async Task<Result<UploadedFileResultDto>> CreateSupportTicket(
            CreateSupportTicketCommand command,
            CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return Result<UploadedFileResultDto>.Unauthorized();
            }

            var currentUser = userResult.Value;

            
            string? inventoryTitle = null;
            if (command.InventoryId.HasValue)
            {
                var inventory = await _context.Inventories
                    .Where(i => i.Id == command.InventoryId.Value && !i.IsDeleted)
                    .Select(i => i.Title)
                    .FirstOrDefaultAsync(cancellationToken);
                
                inventoryTitle = inventory;
            }

            
            var adminEmails = await _context.Users
                .Where(u => u.IsAdmin && !u.IsDeleted)
                .Select(u => u.Email)
                .ToListAsync(cancellationToken);

            
            var ticketPayload = new
            {
                reportedBy = currentUser.Email,
                inventory = inventoryTitle,
                link = command.Link,
                priority = command.Priority,
                summary = command.Summary,
                adminEmails = adminEmails,
                createdAt = DateTime.UtcNow.ToString("o")
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var ticketJson = JsonSerializer.Serialize(ticketPayload, jsonOptions);

            
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"ticket_{timestamp}_{Guid.NewGuid().ToString("N").Substring(0, 8)}.json";

            _logger.LogInformation("Support ticket JSON generated. File name: {FileName}. Admin count: {AdminCount}", fileName, adminEmails.Count);

            
            var uploadResult = await _dropboxService.UploadFileAsync(fileName, ticketJson, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                _logger.LogWarning("Dropbox upload failed. Logging ticket details locally for safety:\n{TicketJson}", ticketJson);
                
                return Result<UploadedFileResultDto>.Error($"Bulut depolamaya yükleme başarısız oldu: {string.Join(" | ", uploadResult.Errors)}");
            }

            // 6. Return the success mapped result DTO
            var uploadPath = uploadResult.Value;
            return Result<UploadedFileResultDto>.Success(new UploadedFileResultDto
            {
                FileName = fileName,
                RelativePath = uploadPath,
                Url = $"https://www.dropbox.com/home{uploadPath}" 
            });
        }
    }
}
