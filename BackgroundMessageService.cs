using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TimeZoneConverter;
using WhatsAppMessageSender.Controllers;

public class BackgroundMessageService : BackgroundService
{
    private readonly WhatsAppController _whatsAppController;
    private readonly TimeZoneInfo _colombiaTimeZone;

    public BackgroundMessageService(WhatsAppController whatsAppController)
    {
        _whatsAppController = whatsAppController;
        _colombiaTimeZone = TZConvert.GetTimeZoneInfo("America/Bogota"); // Zona horaria de Colombia
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, _colombiaTimeZone);

            // Verifica si es las 6:00 p.m. en hora Colombia
            if (currentTime.Hour == 16 && currentTime.Minute == 15)
            {
                Console.WriteLine("Es la hora de enviar los mensajes.");
                await _whatsAppController.SendMessageToAllAsync(); // Llama a la acción del controlador para enviar los mensajes
            }

            // Espera un minuto antes de verificar de nuevo
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
