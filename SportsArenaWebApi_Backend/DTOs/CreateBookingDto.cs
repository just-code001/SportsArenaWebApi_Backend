    namespace SportsArenaWebApi_Backend.DTOs
    {
        public class CreateBookingDto
        {
            public int SlotId { get; set; }
            public int UserId { get; set; }
            public decimal PayableAmount { get; set; }
        }
    }
