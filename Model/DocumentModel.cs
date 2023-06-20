using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class DocumentModel
    {
        public int UserId { get; set; }

        public string? AadharUrl { get; set; } = null;
        public IFormFile? Aadhar { get; set; } = null;
        public DateTime CreatedAt { get; set; }
    }

    public class DocumentResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; } = null;
    }
}
