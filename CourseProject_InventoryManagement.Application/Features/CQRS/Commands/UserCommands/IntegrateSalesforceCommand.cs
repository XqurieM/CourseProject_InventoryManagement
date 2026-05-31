using System;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands
{
    public class IntegrateSalesforceCommand
    {
        public Guid UserId { get; set; }
        public string AccountName { get; set; } = null!;
        public string? AccountPhone { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ContactPhone { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
