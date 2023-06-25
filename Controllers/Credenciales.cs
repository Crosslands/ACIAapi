using Microsoft.AspNetCore.Mvc;
using ACIAapi.Models;
using NuGet.Packaging.Signing;
using Microsoft.Data.SqlClient;
using System.Data;
using NuGet.Protocol.Plugins;

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
    }

}