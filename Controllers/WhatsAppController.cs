using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks; // Agregar esto para usar Task
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace WhatsAppMessageSender.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _twilioFromNumber;

        public WhatsAppController()
        {
            _twilioAccountSid =
                Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
                ?? throw new InvalidOperationException("TWILIO_ACCOUNT_SID no está definido.");
            _twilioAuthToken =
                Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN")
                ?? throw new InvalidOperationException("TWILIO_AUTH_TOKEN no está definido.");
            _twilioFromNumber = "whatsapp:+573112556050"; // Número de Twilio para WhatsApp
            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
        }

        [HttpPost("send-message-to-all")]
        public async Task<IActionResult> SendMessageToAllAsync()
        {
            try
            {
                // Conexión a la base de datos
                string connectionString =
                    "Server=localhost;Database=WhatsAppMessages;User Id=sa;Password=pazJc2601;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Consulta SQL para obtener todos los clientes
                    string query = "SELECT Nombre, Telefono FROM Clientes";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nombre = reader["Nombre"].ToString();
                                string telefono = reader["Telefono"].ToString();

                                if (
                                    !string.IsNullOrEmpty(telefono) && !string.IsNullOrEmpty(nombre)
                                )
                                {
                                    // Envía el mensaje a cada cliente
                                    await EnviarMensajeAsync(nombre, telefono);
                                }
                            }
                        }
                    }
                }

                return Ok("Mensajes enviados a todos los clientes.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al enviar los mensajes: {ex.Message}");
            }
        }

        // Método que envía el mensaje
        private async Task EnviarMensajeAsync(string nombre, string telefono)
        {
            var phoneNumberTo = new PhoneNumber($"whatsapp:{telefono}");

            string contentSid = "HX76e6a8c6eeb5aadbf2be8d0fe7fd4b82"; // SID de tu plantilla en Twilio

            var templateVariables = new Dictionary<string, object> { { "1", nombre } };

            var templateMessage = await MessageResource.CreateAsync(
                from: new PhoneNumber(_twilioFromNumber),
                to: phoneNumberTo,
                contentSid: contentSid,
                contentVariables: JsonConvert.SerializeObject(templateVariables)
            );

            Console.WriteLine($"Mensaje enviado a {nombre} ({telefono}): {templateMessage.Sid}");
        }

        [HttpPost("incoming-message")]
        public IActionResult IncomingMessage([FromForm] IncomingMessageRequest request)
        {
            try
            {
                Console.WriteLine($"Mensaje recibido de {request.From}: {request.Body}");

                // Procesa el mensaje recibido como lo necesites
                // Puedes responder al cliente si es necesario
                var responseMessage =
                    $"Gracias por tu mensaje, Este es un canal informativo. Si deseas mas información puedes escribirnos al 3183192913.";

                // Lógica adicional según lo que quieras hacer con el mensaje entrante

                return Ok(responseMessage);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar el mensaje entrante: {ex.Message}");
            }
        }

        /*private async Task SendMultimediaMessagesAsync(PhoneNumber phoneNumberTo, string nombre)
        {
            // Mensaje con imagen
            string mensajeImagen =
                $"Hola 😃 {nombre}, 🥳 Se viene el mejor evento del año ⚒️⛏️🔧 y queremos que tú seas parte.👏🏼";
            var mediaUrlImagen = new List<Uri>
            {
                new Uri(
                    "https://www.dropbox.com/scl/fi/bx50d6iqcln61i11ysxnr/expologistica.jpeg?rlkey=v7bjsfahykop0xw86ley80lz3&raw=1"
                ),
            };

            var imageMessage = await MessageResource.CreateAsync(
                from: new PhoneNumber(_twilioFromNumber),
                to: phoneNumberTo,
                body: mensajeImagen,
                mediaUrl: mediaUrlImagen
            );
            Console.WriteLine(
                $"Mensaje con imagen enviado a {nombre} ({phoneNumberTo}): {imageMessage.Sid}"
            );

            // Mensaje con video
            string mensajeVideo =
                $"Tenemos un invitado especial que te va a encantar. ¡Te esperamos!";
            var mediaUrlVideo = new List<Uri>
            {
                new Uri(
                    "https://www.dropbox.com/scl/fi/ihuu7ockz85i0ieft41wo/Susos.mp4?rlkey=8qxr1dvkufqgk1nsnile1hwlv&e=1&st=ei3qnoae&raw=1"
                ),
            };

            var videoMessage = await MessageResource.CreateAsync(
                from: new PhoneNumber(_twilioFromNumber),
                to: phoneNumberTo,
                body: mensajeVideo,
                mediaUrl: mediaUrlVideo
            );
            Console.WriteLine(
                $"Mensaje con video enviado a {nombre} ({phoneNumberTo}): {videoMessage.Sid}"
            );

            // Mensaje con audio
            var mediaUrlAudio = new List<Uri>
            {
                new Uri(
                    "https://www.dropbox.com/scl/fi/8m7e4uke8iz4rd4l3yxjk/Carrera-22.m4a?rlkey=v0mrlur1s7knflvvai2jizwsy&st=qusn7fwm&raw=1"
                ),
            };

            var audioMessage = await MessageResource.CreateAsync(
                from: new PhoneNumber(_twilioFromNumber),
                to: phoneNumberTo,
                body: "",
                mediaUrl: mediaUrlAudio
            );
            Console.WriteLine(
                $"Mensaje con audio enviado a {nombre} ({phoneNumberTo}): {audioMessage.Sid}"
            );
        }*/
    }

    public class IncomingMessageRequest
    {
        [FromForm(Name = "From")]
        public string From { get; set; } = string.Empty; // Asegúrate de que no sea NULL

        [FromForm(Name = "Body")]
        public string Body { get; set; } = string.Empty; // Asegúrate de que no sea NULL

        [FromForm(Name = "NumMedia")]
        public int NumMedia { get; set; } = 0; // Asegúrate de que sea un número entero

        [FromForm(Name = "MediaUrl0")]
        public string? MediaUrl0 { get; set; } // Permitir que sea NULL
    }
}
