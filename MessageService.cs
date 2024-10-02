using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class MessageService
{
    private readonly string _twilioAccountSid;
    private readonly string _twilioAuthToken;
    private readonly string _connectionString;

    public MessageService()
    {
        _twilioAccountSid =
            Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
            ?? throw new InvalidOperationException("TWILIO_ACCOUNT_SID no está definido.");
        _twilioAuthToken =
            Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN")
            ?? throw new InvalidOperationException("TWILIO_AUTH_TOKEN no está definido.");
        _connectionString =
            Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
            ?? throw new InvalidOperationException("SQL_CONNECTION_STRING no está definido.");

        TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
    }

    public void EnviarMensajes()
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string query = "SELECT Nombre, Telefono FROM Clientes";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    Console.WriteLine("No se encontraron clientes en la base de datos.");
                    return;
                }

                while (reader.Read())
                {
                    string nombre = reader["Nombre"]?.ToString() ?? "Cliente";
                    string telefono = reader["Telefono"]?.ToString() ?? "";

                    if (!string.IsNullOrEmpty(telefono))
                    {
                        var phoneNumberTo = new PhoneNumber($"whatsapp:{telefono}");
                        var phoneNumberFrom = new PhoneNumber("whatsapp:+14155238886");

                        // Enviar el mensaje con imagen
                        var mensajeImagen = $"Hola {nombre}, evento importante.";
                        var mediaUrlImagen = new List<Uri> { new Uri("URL_IMAGEN") };

                        var imageMessage = MessageResource.Create(
                            from: phoneNumberFrom,
                            to: phoneNumberTo,
                            body: mensajeImagen,
                            mediaUrl: mediaUrlImagen
                        );

                        Console.WriteLine(
                            $"Mensaje con imagen enviado a {nombre} ({telefono}): {imageMessage.Sid}"
                        );
                    }
                }
            }
        }
    }
}
