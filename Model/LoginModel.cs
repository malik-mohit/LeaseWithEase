using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Please enter an email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a password")]
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
        public string returnUrl { get; set; } = " ";
    }

    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public UserLoginInformation data { get; set; } = new UserLoginInformation();


    }

    public class UserLoginInformation
    {
        public int CustomerId { get; set; }
        public string? Email { get; set; }   
        public string? Name { get; set; }
        public int RoleId { get; set; }
    }
}
