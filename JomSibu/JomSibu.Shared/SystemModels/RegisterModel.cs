using JomSibu.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JomSibu.Shared.SystemModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(70, ErrorMessage = "The name is too long")]
        public string FullName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        [MaxLength(255, ErrorMessage = "The email is too long")]
        public string Email { get; set; }
        /// <summary>
        /// phone number without any symbol
        /// </summary>
        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression(@"^01[02-46-9][0-9]{7}$|^01[1][0-9]{8}$", ErrorMessage = "Invalid phone number. Ex (014XXXXXXX)")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password length must be at least 6 characters long")]
        public string Password { get; set; }

        public int UserRoleId { get; set; }
        public int IsHalal { get; set; }
        public int IsVegeterian { get; set; }
        public int BudgetStatusId { get; set; }
        //public ICollection<PreferencesTable>? Preferences { get; set; }
    }
}
