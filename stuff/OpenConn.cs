using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.IO;

namespace Open
{
    public class Conn
    {
        private string connectionString = string.Empty;
        public string ConexionBDcs()
        {

            string currentDirectory = Directory.GetCurrentDirectory();

            string xmlFilePath = Path.Combine(currentDirectory, "AppConfig.xml");

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(xmlFilePath);

            string? aciaDbNode = xmlDoc.SelectSingleNode("ConnectionStrings/ACIADB").ToString();

            if (!string.IsNullOrEmpty(aciaDbNode))
            {
                connectionString = aciaDbNode;
                return connectionString;
            }
            else
            {
                return "No se encontro el string de conexion";            
            }
        }


            
 
    }

}
