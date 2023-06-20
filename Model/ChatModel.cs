using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ChatModel
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int PropertyId { get; set; }
        
        public List<QueryViewModel> QueryList { get; set; } = new List<QueryViewModel>();
        public string Message { get; set; } = "";
    }

}
