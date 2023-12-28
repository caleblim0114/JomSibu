using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JomSibu.Shared.SystemModels
{
    public class EditProfileModels
    {
        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(70, ErrorMessage = "The name is too long")]
        public string FullName { get; set; }

        //[Required(ErrorMessage = "IC Number is required")]
        //[RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Invalid IC number. Ex (990XXXXXXXXX)")]
        //public string IcNumber { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression(@"^01[02-46-9][0-9]{7}$|^01[1][0-9]{8}$", ErrorMessage = "Invalid phone number. Ex (014XXXXXXX)")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

    }
}
