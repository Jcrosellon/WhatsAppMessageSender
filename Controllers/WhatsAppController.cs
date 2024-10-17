using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
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
            _twilioAccountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
                ?? throw new InvalidOperationException("TWILIO_ACCOUNT_SID no está definido.");
            _twilioAuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN")
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
                string connectionString = "Server=pedidos.logisticaferretera.com.co,19433;Database=FERRETERIA;User Id=sa;Password=HpMl110g7*;";
                //string connectionString = "Server=localhost;Database=FERRETERIA;User Id=sa;Password=pazJc2601;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Consulta SQL para obtener todos los clientes
                    string query = "SELECT Cliente, TelefonoEnvio, Productos, Total FROM MensajesMasivosMensaje WHERE TelefonoEnvio IS NOT NULL";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Dentro del while que lee los datos
                            while (await reader.ReadAsync())
                            {
                                string? nombre = reader["Cliente"].ToString();
                                string? telefono = reader["TelefonoEnvio"].ToString();
                                int productos = Convert.ToInt32(reader["Productos"]); // Asumiendo que es un entero
                                decimal total = Convert.ToDecimal(reader["Total"]); // Asumiendo que es un decimal

                                // Agrega un registro para ver los datos obtenidos
                                Console.WriteLine($"Nombre: {nombre}, Teléfono: {telefono}, Productos: {productos}, Total: {total}");

                                if (!string.IsNullOrEmpty(telefono) && !string.IsNullOrEmpty(nombre))
                                {
                                    try
                                    {
                                        // Envía el mensaje a cada cliente
                                        await EnviarMensajeAsync(nombre, telefono, productos, total);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error al enviar mensaje a {nombre} ({telefono}): {ex.Message}");
                                    }
                                }
                            }
                        }
                    }

                    // Llamada al procedimiento almacenado después de enviar todos los mensajes
                    // Llamada al procedimiento almacenado después de enviar todos los mensajes
await EjecutarProcedimientoAlmacenado(connection);

                }

                return Ok("Mensajes enviados y procedimiento almacenado ejecutado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest($"Error al enviar los mensajes: {ex.Message}");
            }
        }

        private async Task EnviarMensajeAsync(string nombre, string telefono, int productos, decimal total)
        {
            var phoneNumberTo = new PhoneNumber($"whatsapp:{telefono}");
            string contentSid = "HX571d97ae1f3a906001e2682ebd0f54f7"; // SID de tu plantilla en Twilio

            // Crear un diccionario con las variables de la plantilla
            var templateVariables = new Dictionary<string, string>
            {
                { "1", nombre },                           // Nombre del cliente
                { "2", productos.ToString() },             // Total de productos como string
                { "3", total.ToString("N0").Replace(",", ".") }  // Total a pagar como string sin decimales
            };

            // Serializar el diccionario a JSON
            string contentVariablesJson = JsonConvert.SerializeObject(templateVariables);

            // Enviar el mensaje utilizando el SID de la plantilla y las variables
            var templateMessage = await MessageResource.CreateAsync(
                from: new PhoneNumber(_twilioFromNumber),
                to: phoneNumberTo,
                contentSid: contentSid,
                contentVariables: contentVariablesJson // Enviar la cadena JSON
            );

            Console.WriteLine($"Mensaje enviado a {nombre} ({telefono}): {templateMessage.Sid}");
        }

        // Método para ejecutar el procedimiento almacenado
        private async Task EjecutarProcedimientoAlmacenado(SqlConnection connection)
{
    using (SqlCommand command = new SqlCommand("GeneraMasivosWhatsApp", connection))
    {
        command.CommandType = CommandType.StoredProcedure;

        // Agregar los parámetros necesarios
        command.Parameters.AddWithValue("@IDPlantilla", 0); // Asumiendo que el valor es 0
        command.Parameters.AddWithValue("@ParaEnviar", 1);  // Asumiendo que el valor es 1

        await command.ExecuteNonQueryAsync();
        Console.WriteLine("Procedimiento almacenado 'MensajesMasivosProcesados' ejecutado con éxito.");
    }
}

    }
}
