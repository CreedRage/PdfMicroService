namespace PdfMicroService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                //options.AddPolicy("AllowSpecificOrigin",
                //    builder => builder.WithOrigins("http://localhost:8000")
                //                      .AllowAnyMethod()
                //                      .AllowAnyHeader());
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseReDoc(c =>
                {
                    c.DocumentTitle = "REDOC API Documentation";
                    c.SpecUrl = "/swagger/v1/swagger.json";
                });
            }

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
