using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Obtener las credenciales de Twilio desde las variables de entorno
            string accountSid =
                Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
                ?? throw new InvalidOperationException("TWILIO_ACCOUNT_SID no está definido.");
            string authToken =
                Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN")
                ?? throw new InvalidOperationException("TWILIO_AUTH_TOKEN no está definido.");

            TwilioClient.Init(accountSid, authToken);
            Console.WriteLine("Conexión a Twilio exitosa.");

            // Obtener la cadena de conexión desde las variables de entorno
            string connectionString =
                Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
                ?? throw new InvalidOperationException("SQL_CONNECTION_STRING no está definido.");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Conexión a la base de datos exitosa.");

                string query = "SELECT Nombre, Telefono FROM Clientes";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("No se encontraron clientes en la base de datos.");
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            string nombre = reader["Nombre"]?.ToString() ?? "Cliente";
                            string telefono = reader["Telefono"]?.ToString() ?? "";

                            if (!string.IsNullOrEmpty(telefono))
                            {
                                var phoneNumberTo = new PhoneNumber($"whatsapp:{telefono}");
                                var phoneNumberFrom = new PhoneNumber("whatsapp:+14155238886");

                                // Primer mensaje con imagen personalizado
                                string mensajeImagen =
                                    $"Hola 😃 {nombre}, 🥳 Se viene el mejor evento del año ⚒️⛏️🔧 y queremos que tú seas parte.👏🏼";
                                var mediaUrlImagen = new List<Uri>()
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

                                // Segundo mensaje con video personalizado
                                string mensajeVideo =
                                    $"Tenemos un invitado especial que te va a encantar. ¡Te esperamos!";
                                var mediaUrlVideo = new List<Uri>()
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

                                // Tercer mensaje con audio SIN mensaje personalizado
                                var mediaUrlAudio = new List<Uri>()
                                {
                                    new Uri(
                                        "https://www.dropbox.com/scl/fi/8m7e4uke8iz4rd4l3yxjk/Carrera-22.m4a?rlkey=v0mrlur1s7knflvvai2jizwsy&st=qusn7fwm&raw=1"
                                    ),
                                };
                                var audioMessage = MessageResource.Create(
                                    from: phoneNumberFrom,
                                    to: phoneNumberTo,
                                    body: "", // Mensaje vacío o un texto genérico
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
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
