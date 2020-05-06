using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace TicTacToe.Models
{
    public class UserModel
    {
        public Guid ID { get; set; }

        [Display(Name ="FirstName")]
        [Required(ErrorMessage ="FirstNameRequired")]
        public string FirstName { get; set; }

        [Display(Name = "LastName")]
        [Required(ErrorMessage = "LastNameRequired")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "EmailRequired"),
            DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "PasswordRequired"),
            DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public System.DateTime? EmailCofirmationDate { get; set; }

        public int Score { get; set; }
    }
}
