using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services;
using System.ComponentModel.DataAnnotations;

namespace HouseRent.Controllers
{
    public class ValidateCredentialsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var userService = (UserService)validationContext.GetService(typeof(UserService));
            var email = (string)validationContext.ObjectType.GetProperty("Email").GetValue(validationContext.ObjectInstance);
           

            if (userService.UserAlreadyExists(email)) ;
            {
                return new ValidationResult("Invalid email or password.");
            }

            return ValidationResult.Success;
        }
    }

}
