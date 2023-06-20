using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class FormModel
    {
        public int PropertyId { get; set; }
        public int CustomerId { get; set; }
        public int FlaggedBy { get; set; }
        public string Comment { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class FormResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
    }
}
