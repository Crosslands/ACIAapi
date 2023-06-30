using Microsoft.AspNetCore.Mvc;
using ACIAapi.Models;
using NuGet.Packaging.Signing;
using Microsoft.Data.SqlClient;
using System.Data;
using NuGet.Protocol.Plugins;
using System.Net.Mail;
using System.Net;

namespace ACIAapi.Controllers
{
    [ApiController]
    [Route("api/controller")]
    public class Credenciales : ControllerBase
    {
        [HttpGet("IsEverythingOk")]
        public IActionResult Get()
        {
            return Ok("API is Running just fine");
        }
        [HttpGet("GetUsersByID")]
        public IActionResult GetUsersByID(int id)
        {
            var aciadb = new ACIADB();

            string connectionString = aciadb.getCs();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand("SPObtenerInfoUsuarioPorID", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@ID", id);

                var usuarios = new List<Usuario>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var usuario = new Usuario
                        {
                            ID_Usuario = (int)reader["ID_Usuario"],
                            Cedula_usuario = reader["Cedula_usuario"].ToString(),
                            Nombre_usuario = reader["Nombre_usuario"].ToString(),
                            Apellido_Paterno_usuario = reader["Apellido_Paterno_usuario"].ToString(),
                            Apellido_Materno_usuario = reader["Apellido_Materno_usuario"].ToString(),
                            FechaNacimiento_usuario = reader["FechaNacimiento_usuario"].ToString(),
                            ID_Credencial = reader["ID_Credencial"].ToString(),
                            ID_Seccion = reader["ID_Seccion"].ToString(),
                            Rol_ID = (int)reader["Rol_ID"]
                        };

                        usuarios.Add(usuario);
                    }
                }

                return Ok(usuarios);
            }
        }

        [HttpGet("GetPasswordByUser")]
        public IActionResult GetPasswordByUser(string usuario)
        {
            var aciadb = new ACIADB();
            string connectionString = aciadb.getCs();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand("spObtenerContraseñaPorUsuario", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Usuario", usuario);

                string contraseña = string.Empty;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        contraseña = reader["Clave"].ToString();
                    }
                }

                return Ok(contraseña);
            }
        }

        [HttpPost("InstertarCredencial")]
        public IActionResult InsertarUsuario(string usuario, string clave, int rolID, int Sesion = 1)
        {
            var aciadb = new ACIADB();
            string connectionString = aciadb.getCs();
            string consulta = "INSERT INTO Credencial_Usuario (Usuario, Clave, Rol_ID) VALUES (@Usuario, @Clave, @Rol_ID)";
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand comando = new SqlCommand(consulta, connection);
                comando.Parameters.AddWithValue("@Usuario", usuario);
                comando.Parameters.AddWithValue("@Clave", clave);
                comando.Parameters.AddWithValue("@Rol_ID", rolID);

                try
                {
                    connection.Open();
                    var log = new LogRepository();
                    int filasAfectadas = comando.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        log.InsertarLog("InstertarCredencial","0000",Sesion);
                        return Ok("Registro insertado exitosamente.");
                    }
                    else
                    {
                        log.InsertarLog("InstertarCredencial", "Standard ERROR", Sesion);
                        return BadRequest("Error al insertar el registro.");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Error: " + ex.Message);
                }
            }
        }

        [HttpGet("GetTokenResetPassword")]
        public IActionResult GetTokenResetPassword(string usuario)
        {
            var aciadb = new ACIADB();
            string connectionString = aciadb.getCs();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand("spObtenerContraseñaPorUsuario", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Usuario", usuario);

                string contraseña = string.Empty;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        contraseña = reader["Clave"].ToString();
                    }
                }

                // Generar token aleatorio
                string token = GenerateRandomToken();

                // Enviar correo electrónico con el token
                SendResetPasswordEmail(usuario, token);

                return Ok(token);
            }
        }

        private string GenerateRandomToken()
        {
            // Generar una cadena aleatoria para el token
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var token = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return token;
        }

        private void SendResetPasswordEmail(string usuario, string token)
        {
            // Configurar el correo electrónico
            string from = "bkeylog71@gmail.com";
            string to = "franciscoapvst@gmail.com";
            string subject = "Reset Password";
            string body = "Dear {usuario},\n\n"
                + "\nPlease use the following token to reset your password: {token}\n\n"
                + "Best regards,\nYour Application";

            // Configurar el cliente SMTP con conexión segura
            using (var client = new SmtpClient("smtp.gmail.com", 465))
            {
                client.EnableSsl = false;
                client.UseDefaultCredentials = true;
                client.Credentials = new NetworkCredential(from, "xriwmdsqvmwsopnh");

                // Enviar el correo electrónico
                try
                {
                    client.Send(from, to, subject, body);
                }
                catch (SmtpException ex)
                {
                    // Manejar la excepción del cliente SMTP
                    Console.WriteLine("Error sending email: " + ex.Message);
                    throw;
                }
            }
        }


        [HttpPost("EliminarCredencial")]
        public IActionResult EliminarUsuario(string usuario, string clave)
        {
            var aciadb = new ACIADB();
            string connectionString = aciadb.getCs();
            string consulta = "DELETE FROM Credencial_Usuario WHERE Usuario = @Usuario AND Clave = @Clave";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand comando = new SqlCommand(consulta, connection);
                comando.Parameters.AddWithValue("@Usuario", usuario);
                comando.Parameters.AddWithValue("@Clave", clave);

                try
                {
                    connection.Open();
                    var log = new LogRepository();
                    int filasAfectadas = comando.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        log.InsertarLog("EliminarCredencial", "0000");
                        return Ok("Usuario eliminado exitosamente.");
                    }
                    else
                    {
                        log.InsertarLog("EliminarCredencial", "Standard ERROR");
                        return BadRequest("No se encontró el usuario o la contraseña no coincide.");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Error: " + ex.Message);
                }
            }
        }
    }

}