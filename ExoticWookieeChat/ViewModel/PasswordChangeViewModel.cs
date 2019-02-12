using ExoticWookieeChat.Util;
using System;
using System.ComponentModel.DataAnnotations;

namespace ExoticWookieeChat.ViewModel
{
    /// <summary>
    /// View model for password change
    /// </summary>
    public class PasswordChangeViewModel
    {
        /// <summary>
        /// Id of selected user
        /// </summary>
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Old password")]
        public String OldPassword { get; set; }

        [Required]
        [Display(Name = "New password")]
        public String NewPassword { get; set; }

        [Required]
        [Display(Name = "Confirm new password")]
        public String ConfirmNewPassword { get; set; }

        /// <summary>
        /// Return with SHA512 hashed old password
        /// </summary>
        /// <returns></returns>
        public String GetHashedOldPassword()
        {
            return PasswordUtil.GenerateSHA512String(this.OldPassword);
        }

        /// <summary>
        /// Return with hashed new password
        /// </summary>
        /// <returns></returns>
        public String GetHashedNewPassword()
        {
            return PasswordUtil.GenerateSHA512String(this.NewPassword);
        }

        public bool ValidateOldaPassword(String hashedOldPassword)
        {
            if (PasswordUtil.GenerateSHA512String(this.OldPassword).Equals(hashedOldPassword))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validate new password
        /// </summary>
        /// <returns></returns>
        public bool ValidateNewPassword()
        {
            if (this.NewPassword.Equals(this.ConfirmNewPassword))
            {
                return true;
            }

            return false;
        }
    }
}