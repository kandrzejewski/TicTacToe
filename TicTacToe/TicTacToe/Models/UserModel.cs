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
        [Required()]
        public string FirstName { get; set; }
        [Required()]
        public string LastName { get; set; }
        [Required(), DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(), DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public System.DateTime? EmailCofirmationDate { get; set; }
        public int Score { get; set; }
    }
}
