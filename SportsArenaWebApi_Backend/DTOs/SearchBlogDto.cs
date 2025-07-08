﻿namespace SportsArenaWebApi_Backend.DTOs
{
    public class SearchBlogDto
    {
        public string? SearchTerm { get; set; }
        public int? AuthorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
