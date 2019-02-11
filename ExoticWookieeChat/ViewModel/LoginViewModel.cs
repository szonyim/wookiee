using System;
using ExoticWookieeChat.Util;
using System.ComponentModel.DataAnnotations;

namespace ExoticWookieeChat.ViewModel
{
    /// <summary>
    /// Model for login view
    /// </summary>
    public class LoginViewModel
    {
        [Display(Name = "User name")]
        [MinLength(4, ErrorMessage = "At least 4 character need")]
        public String UserName { get; set; }
        
        [MinLength(5, ErrorMessage = "At least 5 charachter need")]
        public String Password { get; set; }

        public String GetSHA512Password()
        {
            return PasswordUtil.GenerateSHA512String(Password);
        }

    }
}