using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class CustomerModel
    {
        
        public CustomerModel()
        {

        }
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Please enter a Name")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Please enter an email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Please enter a password")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Please enter your mobile number.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Please enter a valid 10 digit mobile number.")]
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Please select a gender")]
        public string? Gender { get; set; }
        [Required(ErrorMessage = "Please enter your address")]
        public string? Address { get; set; }
        public int RoleId { get; set; }
        [Required(ErrorMessage = "Please select your state.")]
        public Nullable<int> StateId { get; set; }
        public string StateName { get; set; }

        public string CityName { get; set; }
        [Required(ErrorMessage = "Please select your City.")]
        public Nullable<int> CityId { get; set; }
        public int UserStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int Verified { get; set; }
        public string OTP { get; set; }
        public string EnteredOTP { get; set; }
        public IFormFile ProfilePic { get; set; }
    }

    public class RegisterResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
