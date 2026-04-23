namespace Orchestrate.Models
{
    public class PageRequest
    {
        public string Search { get; set; } = "";
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public int SortedIndex { get; set; } = 0;
        public byte sortType { get; set; } = 0;
    }
}
