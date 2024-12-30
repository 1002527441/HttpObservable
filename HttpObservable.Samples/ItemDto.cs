using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpObservable_Samples
{
    public class ItemDto
    {
        public string ItemId { get; set; } = null!;
        public string Number { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? ParentItemId { get; set; }
        public string CategoryId { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Remark { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = default!;
    }
}
