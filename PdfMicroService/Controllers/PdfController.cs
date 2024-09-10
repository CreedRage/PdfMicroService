using Microsoft.AspNetCore.Mvc;
using PdfMicroService.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfMicroService.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class PdfController : ControllerBase
    {
        private readonly ILogger<PdfController> _logger;

        public PdfController(ILogger<PdfController> logger)
        {
            _logger = logger;
        }

        [HttpPost("checkPdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> СheckPdf(IFormFile file)
        {
            // Проверка расширения файла
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var response = new ApiResponse();

            if (extension != ".pdf")
            {
                _logger.LogWarning("Файл {FileName} не является PDF.", file.FileName);
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Data = new ApiResponseData
                {
                    Type = "error",
                    Message = "Это не PDF файл."
                };
                return BadRequest(response);
            }

            try
            {
                // Проверка, что файл является действительным PDF
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0; // Сбросить позицию потока на начало

                    // Попытка открыть PDF файл
                    PdfDocument pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                }
            }
            catch (PdfReaderException)
            {
                _logger.LogError("Файл {FileName} не является действительным PDF.", file.FileName);
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Data = new ApiResponseData
                {
                    Type = "error",
                    Message = "Файл не является действительным PDF."
                };
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при обработке файла {FileName}.", file.FileName);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Data = new ApiResponseData
                {
                    Type = "error",
                    Message = "Произошла ошибка при обработке файла: " + ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            // Если всё в порядке, возвращаем 'OK'
            _logger.LogInformation("Файл {FileName} успешно загружен.", file.FileName);
            response.StatusCode = StatusCodes.Status200OK;
            response.Data = new ApiResponseData
            {
                Type = "success",
                Message = "Файл успешно загружен."
            };
            return Ok(response); // Возвращаем ответ
        }
    }
}