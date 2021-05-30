using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityExpApi.Models
{
    public class PaginatedList<T> where T:new()
    {
        public long totalItems { get; set; }
        public long totalPages { get; set; }
        public int pageSize { get; set; }
        public int page { get; set; }
        public List<T> items { get; set; }
    }
}
