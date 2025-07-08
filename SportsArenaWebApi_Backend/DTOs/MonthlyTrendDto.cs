namespace SportsArenaWebApi_Backend.DTOs
{
    public class MonthlyTrendDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }
}
