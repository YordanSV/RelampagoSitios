using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using RelampagoSitios.Models;
using System.Text.RegularExpressions;

namespace RelampagoSitios.Controllers
{
    public class HomeController : Controller
    {
        // Definimos una clase interna para manejar los datos de PDF
        public class PdfData
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string Contenido { get; set; }
            public DateTime FechaSubida { get; set; }
        }

        // GET: Home/Subir
        public ActionResult Subir(string terminoBusqueda = null)
        {
            // Si hay un término de búsqueda, realizamos la búsqueda
            if (!string.IsNullOrEmpty(terminoBusqueda))
            {
                ViewBag.TerminoBusqueda = terminoBusqueda;
                List<PdfData> resultados = BuscarPdfs(terminoBusqueda);
                return View(resultados);
            }

            // Si no hay término de búsqueda, mostramos el formulario vacío
            // Esto mantiene el comportamiento original
            return View();
        }

        // POST: Home/Subir
        [HttpPost]
        public ActionResult Subir(HttpPostedFileBase archivoPdf)
        {
            // Esta parte mantiene exactamente la lógica original
            if (archivoPdf != null && archivoPdf.ContentLength > 0)
            {
                string textoExtraido = "";

                // Leer y extraer texto del PDF
                using (var reader = new PdfReader(archivoPdf.InputStream))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        textoExtraido += PdfTextExtractor.GetTextFromPage(reader, i) + "\n\n";
                    }
                }

                // Obtener conexión desde Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConexionPDF"].ConnectionString;

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Mantenemos la consulta original sin FechaSubida
                    string query = "INSERT INTO Pdfs (Nombre, Contenido) VALUES (@nombre, @contenido)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nombre", System.IO.Path.GetFileName(archivoPdf.FileName));
                    cmd.Parameters.AddWithValue("@contenido", textoExtraido);
                    cmd.ExecuteNonQuery();
                }

                ViewBag.Mensaje = "PDF subido y contenido guardado en la base de datos.";
            }

            // Mantenemos el return original
            return View();
        }

        // Método nuevo para buscar PDFs
        private List<PdfData> BuscarPdfs(string terminoBusqueda)
        {
            List<PdfData> resultados = new List<PdfData>();
            string connectionString = ConfigurationManager.ConnectionStrings["ConexionPDF"].ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Ajustamos la consulta según la estructura actual de tu tabla
                // Si no tienes FechaSubida, la omitimos
                string query = "SELECT Id, Nombre, Contenido FROM Pdfs WHERE Contenido LIKE @termino OR Nombre LIKE @termino";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@termino", "%" + terminoBusqueda + "%");

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var pdf = new PdfData
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Contenido = reader["Contenido"].ToString()
                        };

                        // Si tienes la columna FechaSubida, la incluimos
                        if (reader.GetSchemaTable().Select("ColumnName = 'FechaSubida'").Length > 0)
                        {
                            pdf.FechaSubida = Convert.ToDateTime(reader["FechaSubida"]);
                        }
                        else
                        {
                            // Si no existe la columna, usamos la fecha actual
                            pdf.FechaSubida = DateTime.Now;
                        }

                        resultados.Add(pdf);
                    }
                }
            }

            return resultados;
        }

        // Método nuevo para ver un PDF específico
        public ActionResult VerPdf(int id, string termino = null)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConexionPDF"].ConnectionString;
                Pdf pdf = null;

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, Nombre, Contenido FROM Pdfs WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string contenido = reader["Contenido"].ToString();

                            // Resaltar el término si se recibió
                            if (!string.IsNullOrEmpty(termino))
                            {
                                string marcado = $"<span style='background-color: yellow; font-weight: bold;'>{termino}</span>";
                                contenido = Regex.Replace(contenido, Regex.Escape(termino), marcado, RegexOptions.IgnoreCase);
                            }

                            pdf = new Pdf
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["Nombre"].ToString(),
                                Contenido = contenido,
                                FechaSubida = DateTime.Now
                            };
                        }
                    }
                }

                if (pdf == null)
                {
                    return HttpNotFound("PDF no encontrado.");
                }

                return View(pdf);
            }


    public ActionResult buscapalabras(string terminoBusqueda = null)
        {
            if (!string.IsNullOrEmpty(terminoBusqueda))
            {
                ViewBag.TerminoBusqueda = terminoBusqueda;
                var resultados = BuscarPdfs(terminoBusqueda);
                return View(resultados);
            }

            return View(new List<PdfData>());
        }


        public ActionResult levantar ()
        {
            return View();
        }

        public ActionResult login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Correo, string Contrasena)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConexionPDF"].ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Nombre, Correo FROM Usuarios WHERE Correo = @correo AND Contrasena = @contrasena";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@correo", Correo);
                cmd.Parameters.AddWithValue("@contrasena", Contrasena);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Guardamos la sesión
                        Session["UsuarioId"] = reader["Id"];
                        Session["Nombre"] = reader["Nombre"];
                        Session["Correo"] = reader["Correo"];

                        return RedirectToAction(""); // o a la vista protegida
                    }
                }
            }

            ViewBag.Mensaje = "Correo o contraseña incorrectos.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("");
        }


    }
}