namespace SportsArenaWebApi_Backend.DTOs
{
    public class SlotStatsDto
    {
        public int TotalSlots { get; set; }
        public int BookedSlots { get; set; }
        public int AvailableSlots { get; set; }
        public decimal UtilizationRate { get; set; }
    }
}
