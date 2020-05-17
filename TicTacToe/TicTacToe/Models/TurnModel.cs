using System;

namespace TicTacToe.Models
{
    public class TurnModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserModel User { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
