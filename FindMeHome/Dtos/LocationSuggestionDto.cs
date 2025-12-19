namespace FindMeHome.Dtos
{
    public class LocationSuggestionDto
    {
        public string Name { get; set; }
        public string Type { get; set; } // "City" or "Neighborhood"
        public int Count { get; set; }
    }
}
