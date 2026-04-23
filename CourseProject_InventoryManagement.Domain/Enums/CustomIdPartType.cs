using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Enums
{
    public enum CustomIdPartType
    {
        StaticText = 1,
        Random20Bit,
        Random32Bit,
        Random6Digit,
        Random9Digit,
        Guid,
        DateTime,
        Sequence
    }
}
