using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class PropertyModel
    {
        public PropertyModel()
        {

        }
        public int PropertyId { get; set; }
        public int UserId { get; set; }
        [Required(ErrorMessage = "Price is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Invalid price")]
        public string? Price { get; set; } = null;
        [Required(ErrorMessage = "Deposit is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Invalid deposit")]
        public string? Deposit { get; set; } = null;
        [Required(ErrorMessage = "Type is required")]
        public string? Type { get; set; } = null;
        [Required(ErrorMessage = "Rooms is required")]
        public string? Rooms { get; set; } = null;
        [Required(ErrorMessage = "Furnishing is required")]
        public string? Furnishing { get; set; } = null;
        [Required(ErrorMessage = "Area is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Invalid area")]
        public string? Area { get; set; } = null;
        [Required(ErrorMessage = "Property address is required")]
        public string? PropertyAddress { get; set; } = null;
        [Required(ErrorMessage = "Please ! select images of Property")]
        public List<IFormFile>? ImageUrl { get; set; } = null;
        public List<String>? ImageUrlString { get; set; } = null;
        [Required(ErrorMessage = "Property paper is required")]
        public IFormFile? PropertyPaper { get; set; } = null;
        [Required(ErrorMessage = "State name is required")]
        public string StateName { get; set; }

        [Required(ErrorMessage = "City name is required")]
        public string CityName { get; set; }
        public string? OwnerName { get; set; } = null;
        public int StateId { get; set; }
        public int CityId { get; set; }
        public int AdminAction { get; set; }
        public string? ActionComment { get; set; } = null;
        public int PropertyRented { get; set; }
        public DateTime CreatedAt { get; set; }

        public int SenderId { get; set; }
        public string Message { get; set; }
        public int QueryId { get; set; }

        public List<QueryViewModel> QueryList { get; set; }

        public int ReceiverId { get; set; }
        public CustomerModel Customer { get; set; } = new CustomerModel();


    }

    public class PropertyResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; } = null;
    }
}
