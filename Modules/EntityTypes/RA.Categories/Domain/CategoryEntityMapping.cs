using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Domain
{
    public class CategoryEntityMapping
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Guid EntityId { get; set; }
    }
}
