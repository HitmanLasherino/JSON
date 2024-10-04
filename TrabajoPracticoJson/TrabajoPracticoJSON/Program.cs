using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;

public class Libro
{
    public string Nombre { get; set; }
    public int AnioPublicacion { get; set; }
    public string Editorial { get; set; }
}

public class Escritor
{
    public int Id { get; set; }
    public string Apellido { get; set; }
    public string Nombre { get; set; }
    public string Dni { get; set; }
    public List<Libro> Libros { get; set; }
}

class Program
{
    static void Main()
    {
        string connectionString = "tu_conexion_a_la_bd";
        List<Escritor> escritores = new List<Escritor>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = @"
                SELECT e.id, e.apellido, e.nombre, e.dni, l.nombre AS libroNombre, 
                l.año_publicacion, l.editorial 
                FROM Escritor e 
                LEFT JOIN Libro l ON e.id = l.idEscritor";

            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int escritorId = reader.GetInt32(0);
                    Escritor escritor = escritores.Find(e => e.Id == escritorId);

                    if (escritor == null)
                    {
                        escritor = new Escritor
                        {
                            Id = escritorId,
                            Apellido = reader.GetString(1),
                            Nombre = reader.GetString(2),
                            Dni = reader.GetString(3),
                            Libros = new List<Libro>()
                        };
                        escritores.Add(escritor);
                    }

                    if (!reader.IsDBNull(4)) // Verifica si tiene libros
                    {
                        escritor.Libros.Add(new Libro
                        {
                            Nombre = reader.GetString(4),
                            AnioPublicacion = reader.GetInt32(5),
                            Editorial = reader.GetString(6)
                        });
                    }
                }
            }
        }

        // Serializa los datos a JSON y los guarda en un archivo
        string json = JsonConvert.SerializeObject(escritores, Formatting.Indented);
        File.WriteAllText("escritores.json", json);

        Console.WriteLine("Datos escritos en escritores.json");
    }
}
