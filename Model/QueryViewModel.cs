using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class QueryViewModel
    {
        public int PropertyId { get; set; }
        public int InqueryId { get; set; }
        public int ReceiverId { get; set; }
        public int SenderId { get; set; }
        public string CustomerEmail { get; set; }

        public string Message { get; set; }
        public DateTime SentOn { get; set; }
    }
}
