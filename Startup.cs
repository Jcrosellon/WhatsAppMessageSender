using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WhatsAppMessageSender.Controllers;

namespace WhatsAppMessageSender
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    
    // Registra MessageService
    services.AddSingleton<WhatsAppController>();

    // Registra el servicio en segundo plano
    services.AddHostedService<BackgroundMessageService>();
}


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Esto es importante para mapear los controladores
            });
        }
        
    }
}
