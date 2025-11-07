namespace FindMeHome.Dtos
{
    public class ResultDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }

        public static ResultDto Success(string? message = null)
            => new() { IsSuccess = true, Message = message };

        public static ResultDto Failure(string message)
            => new() { IsSuccess = false, Message = message };
    }
}
