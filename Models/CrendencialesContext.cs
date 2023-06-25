using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace ACIAapi.Models
{
    public class LogRepository
    {
        private string connectionString = "Server = tcp:aciaserver.database.windows.net,1433; Initial Catalog = ACIA; Persist Security Info = False; User ID = aciaserver; Password = z8DNXKisri_Pba.; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";

        public void InsertarLog(string modulo, string codigoError, int? idSesion = 1)
        {
            string consulta = "EXEC SPInsertarLogLogIn " +
                "@Modulo, @Codigo_Error, @ID_Sesion";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand comando = new SqlCommand(consulta, connection);
                comando.Parameters.AddWithValue("@Modulo", modulo);
                comando.Parameters.AddWithValue("@Codigo_Error", codigoError);
                comando.Parameters.AddWithValue("@ID_Sesion", idSesion);

                try
                {
                    connection.Open();
                    comando.ExecuteNonQuery();
                    Console.WriteLine("Registro insertado exitosamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al insertar el registro: " + ex.Message);
                }
            }
        }

    }
    public class ACIADB
    {
        public string getCs()
        {
            return "Server = tcp:aciaserver.database.windows.net,1433; Initial Catalog = ACIA; Persist Security Info = False; User ID = aciaserver; Password = z8DNXKisri_Pba.; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
        }

    }

    public class Usuario // Nueva clase para representar la entidad Usuario
    {
        [Key]  public int ID_Usuario { get; set; }
        public string? Cedula_usuario { get; set; }
        public string? Nombre_usuario { get; set; }
        public string? Apellido_Paterno_usuario { get; set; }
        public string? Apellido_Materno_usuario { get; set; }
        public string? FechaNacimiento_usuario { get; set; }
        public string? ID_Credencial { get; set; }
        public string? ID_Seccion { get; set; }
        public int? Rol_ID { get; set; }
    }

    public class UsuariosContext : DbContext // Cambiado el nombre del contexto a UsuariosContext
    {
        public DbSet<Usuario> oUsers { get; set; } // Cambiado el nombre de la propiedad DbSet

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=tcp:aciaserver.database.windows.net,1433;Initial Catalog=ACIA;Persist Security Info=False;User ID=aciaserver;Password=z8DNXKisri_Pba.;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
}
