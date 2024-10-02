using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace WhatsAppMessageSender.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        [HttpPost("send-message")]
        public IActionResult SendMessage()
        {
            try
            {
                // Inicializar Twilio
                string accountSid =
                    Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
                    ?? throw new InvalidOperationException("TWILIO_ACCOUNT_SID no está definido.");
                string authToken =
                    Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN")
                    ?? throw new InvalidOperationException("TWILIO_AUTH_TOKEN no está definido.");

                TwilioClient.Init(accountSid, authToken);

                // Conectar a la base de datos
                using (
                    SqlConnection connection = new SqlConnection(
                        Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
                    )
                )
                {
                    connection.Open();

                    // Consultar clientes
                    string query = "SELECT Nombre, Telefono FROM Clientes";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("No se encontraron clientes en la base de datos.");
                            return Ok("No se encontraron clientes.");
                        }

                        while (reader.Read())
                        {
                            string nombre = reader["Nombre"]?.ToString() ?? "Cliente";
                            string telefono = reader["Telefono"]?.ToString() ?? "";

                            if (!string.IsNullOrEmpty(telefono))
                            {
                                var phoneNumberTo = new PhoneNumber($"whatsapp:{telefono}");
                                var phoneNumberFrom = new PhoneNumber("whatsapp:+14155238886");

                                // Mensaje con imagen
                                string mensajeImagen =
                                    $"Hola 😃 {nombre}, 🥳 Se viene el mejor evento del año ⚒️⛏️🔧 y queremos que tú seas parte.👏🏼";
                                var mediaUrlImagen = new List<Uri>
                                {
                                    new Uri(
                                        "https://www.dropbox.com/scl/fi/bx50d6iqcln61i11ysxnr/expologistica.jpeg?rlkey=v7bjsfahykop0xw86ley80lz3&raw=1"
                                    ),
                                };
                                var imageMessage = MessageResource.Create(
                                    from: phoneNumberFrom,
                                    to: phoneNumberTo,
                                    body: mensajeImagen,
                                    mediaUrl: mediaUrlImagen
                                );
                                Console.WriteLine(
                                    $"Mensaje con imagen enviado a {nombre} ({telefono}): {imageMessage.Sid}"
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
                                var videoMessage = MessageResource.Create(
                                    from: phoneNumberFrom,
                                    to: phoneNumberTo,
                                    body: mensajeVideo,
                                    mediaUrl: mediaUrlVideo
                                );
                                Console.WriteLine(
                                    $"Mensaje con video enviado a {nombre} ({telefono}): {videoMessage.Sid}"
                                );

                                // Mensaje con audio
                                var mediaUrlAudio = new List<Uri>
                                {
                                    new Uri(
                                        "https://www.dropbox.com/scl/fi/8m7e4uke8iz4rd4l3yxjk/Carrera-22.m4a?rlkey=v0mrlur1s7knflvvai2jizwsy&st=qusn7fwm&raw=1"
                                    ),
                                };
                                var audioMessage = MessageResource.Create(
                                    from: phoneNumberFrom,
                                    to: phoneNumberTo,
                                    body: "", // Mensaje vacío o texto genérico
                                    mediaUrl: mediaUrlAudio
                                );
                                Console.WriteLine(
                                    $"Mensaje con audio enviado a {nombre} ({telefono}): {audioMessage.Sid}"
                                );
                            }
                            else
                            {
                                Console.WriteLine(
                                    $"El cliente {nombre} no tiene un número de teléfono válido."
                                );
                            }
                        }
                    }
                }
                return Ok("Mensajes enviados exitosamente.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

    [HttpPost("incoming-message")]
    public IActionResult ReceiveMessage([FromBody] IncomingMessageRequest incomingRequest)
    {
        try
        {
            // Lógica para procesar el mensaje entrante
            var phoneNumberFrom = new PhoneNumber($"whatsapp:{incomingRequest.From}");
            var responseMessage =
                $"Gracias por tu mensaje, {incomingRequest.Body}. ¡Te responderemos pronto!";

            // Enviar respuesta automática
            var message = MessageResource.Create(
                from: new PhoneNumber("whatsapp:+14155238886"),
                to: phoneNumberFrom,
                body: responseMessage
            );

            return Ok($"Respuesta enviada: {message.Sid}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }
}
