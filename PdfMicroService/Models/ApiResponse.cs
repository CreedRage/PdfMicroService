namespace PdfMicroService.Models
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public ApiResponseData? Data { get; set; }
    }
}