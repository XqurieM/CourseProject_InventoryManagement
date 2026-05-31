using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using CourseProject_InventoryManagement.Domain.Entities;
using CourseProject_InventoryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers
{
    public class GetOdooAggregatedResultsQueryHandler : ICQRS.IGetOdooAggregatedResults
    {
        private readonly IAppDbContext _context;

        public GetOdooAggregatedResultsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<OdooInventoryAggregateDto>> GetOdooAggregatedResults(GetOdooAggregatedResultsQuery query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.Token))
            {
                return Result<OdooInventoryAggregateDto>.Invalid(new ValidationError { ErrorMessage = "Token cannot be empty." });
            }

            var inventory = await _context.Inventories
                .Include(x => x.Category)
                .Include(x => x.Fields)
                .FirstOrDefaultAsync(x => x.ApiToken == query.Token && !x.IsDeleted, cancellationToken);

            if (inventory is null)
            {
                return Result<OdooInventoryAggregateDto>.NotFound("No inventory found matching the provided integration token.");
            }

            var items = await _context.Items
                .Where(x => x.InventoryId == inventory.Id && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var itemIds = items.Select(x => x.Id).ToList();

            var fieldValues = await _context.ItemFieldValues
                .Where(x => itemIds.Contains(x.ItemId))
                .ToListAsync(cancellationToken);

            var commentCount = await _context.Comments.CountAsync(c => c.InventoryId == inventory.Id && !c.IsDeleted, cancellationToken);
            var likeCount = await _context.ItemLikes.CountAsync(l => itemIds.Contains(l.ItemId), cancellationToken);
            var explicitAccessCount = await _context.InventoryAccesses.CountAsync(a => a.InventoryId == inventory.Id, cancellationToken);
            var tags = await _context.InventoryTags
                .Where(t => t.InventoryId == inventory.Id)
                .Select(t => t.Tag.Name)
                .ToListAsync(cancellationToken);

            var responseDto = new OdooInventoryAggregateDto
            {
                InventoryId = inventory.Id,
                Title = inventory.Title,
                Description = inventory.Description,
                TotalItems = items.Count,
                CategoryName = inventory.Category?.Name ?? "No Category",
                Tags = tags,
                CommentCount = commentCount,
                FieldCount = inventory.Fields.Count(f => !f.IsDeleted),
                LikeCount = likeCount,
                ExplicitAccessCount = explicitAccessCount
            };

            var itemLikesGroup = await _context.ItemLikes
                .Where(l => itemIds.Contains(l.ItemId))
                .GroupBy(l => l.ItemId)
                .Select(g => new { ItemId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var itemLikeCount = itemLikesGroup.FirstOrDefault(g => g.ItemId == item.Id)?.Count ?? 0;
                
                var itemFieldVals = fieldValues
                    .Where(v => v.ItemId == item.Id)
                    .Select(v => {
                        var field = inventory.Fields.FirstOrDefault(f => f.Id == v.InventoryFieldId);
                        var fieldName = field?.Name ?? "Unknown";
                        var fieldType = field?.FieldType.ToString() ?? "SingleLineText";
                        string displayVal = "";
                        if (v.NumberValue.HasValue) displayVal = v.NumberValue.Value.ToString();
                        else if (v.BooleanValue.HasValue) displayVal = v.BooleanValue.Value.ToString();
                        else displayVal = v.StringValue ?? "";
                        
                        return new OdooItemFieldValueDto { 
                            FieldName = fieldName, 
                            Value = displayVal,
                            FieldType = fieldType 
                        };
                    })
                    .ToList();
                
                responseDto.Items.Add(new OdooItemDto
                {
                    ItemId = item.Id,
                    ItemName = item.ItemName,
                    CustomId = item.CustomId,
                    LikeCount = itemLikeCount,
                    CreatedAt = item.CreatedAtUtc,
                    UpdatedAt = item.UpdatedAtUtc,
                    FieldValues = itemFieldVals
                });
            }

            foreach (var field in inventory.Fields.Where(x => !x.IsDeleted))
            {
                var valuesForField = fieldValues.Where(x => x.InventoryFieldId == field.Id).ToList();

                var fieldAggregate = new OdooFieldAggregateDto
                {
                    FieldId = field.Id,
                    Name = field.Name,
                    FieldType = field.FieldType.ToString()
                };

                if (field.FieldType == InventoryFieldType.Number)
                {
                    var numericValues = valuesForField
                        .Where(x => x.NumberValue.HasValue)
                        .Select(x => x.NumberValue!.Value)
                        .ToList();

                    if (numericValues.Count > 0)
                    {
                        fieldAggregate.MinValue = numericValues.Min();
                        fieldAggregate.MaxValue = numericValues.Max();
                        fieldAggregate.AverageValue = numericValues.Average();
                    }
                }
                else if (field.FieldType == InventoryFieldType.Boolean)
                {
                    var booleanValues = valuesForField
                        .Where(x => x.BooleanValue.HasValue)
                        .Select(x => x.BooleanValue!.Value)
                        .ToList();

                    fieldAggregate.TrueCount = booleanValues.Count(x => x);
                    fieldAggregate.FalseCount = booleanValues.Count(x => !x);
                }
                else
                {
                    var popularValues = valuesForField
                        .Where(x => !string.IsNullOrWhiteSpace(x.StringValue))
                        .GroupBy(x => x.StringValue!.Trim())
                        .Select(g => new OdooStringValueCountDto
                        {
                            Value = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .Take(3)
                        .ToList();

                    fieldAggregate.MostPopularValues = popularValues;
                }

                responseDto.Fields.Add(fieldAggregate);
            }

            return Result<OdooInventoryAggregateDto>.Success(responseDto);
        }
    }
}
