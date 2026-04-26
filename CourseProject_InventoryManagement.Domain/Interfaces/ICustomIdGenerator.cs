using CourseProject_InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Interfaces
{
    public interface ICustomIdGenerator
    {
        string Generate(List<InventoryCustomIdRule> rules, int currentCount);
    }
}
