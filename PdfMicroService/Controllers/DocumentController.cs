using Microsoft.AspNetCore.Mvc;
using PdfMicroService.Models;
using Spire.Doc;

namespace PdfMicroService.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(ILogger<DocumentController> logger)
        {
            _logger = logger;
        }


        [HttpPost("ConvertWordToPdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ConvertWordToPdf(IFormFile file)
        {
            // Проверка расширения файла
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var response = new ApiResponse();

            if (extension != ".doc" && extension != ".docx")
            {
                _logger.LogWarning("Файл {FileName} не является Word-документом.", file.FileName);
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Data = new ApiResponseData
                {
                    Type = "error",
                    Message = "Это не Word файл."
                };
                return BadRequest(response);
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0; // Сбросить позицию потока на начало

                    var pdfBytes = ConvertWordToPdf_Spire(stream);

                    // Возвращаем PDF файл
                    var pdfResult = new FileContentResult(pdfBytes, "application/pdf")
                    {
                        FileDownloadName = Path.ChangeExtension(file.FileName, ".pdf")
                    };

                    return pdfResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при обработке файла Word {FileName}. Exception: {Message}", file.FileName, ex.ToString());
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Data = new ApiResponseData
                {
                    Type = "error",
                    Message = "Произошла ошибка при обработке файла: " + ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        private static byte[] ConvertWordToPdf_Spire(Stream wordStream)
        {
            // Создаем новый экземпляр документа
            Document document = new Document();

            // Загружаем Word документ из потока
            document.LoadFromStream(wordStream, FileFormat.Auto);

            // Создаем MemoryStream для хранения PDF-выхода
            using (var pdfStream = new MemoryStream())
            {
                // Сохраняем документ в MemoryStream в формате PDF
                document.SaveToStream(pdfStream, FileFormat.PDF);

                // Возвращаем байтовый массив PDF
                return pdfStream.ToArray();
            }
        }
    }
}