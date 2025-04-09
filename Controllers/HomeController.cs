using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Data.SqlClient;
using System.Configuration; // Para usar ConfigurationManager

namespace RelampagoSitios.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Subir()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Subir(HttpPostedFileBase archivoPdf)
        {
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
                    string query = "INSERT INTO Pdfs (Nombre, Contenido) VALUES (@nombre, @contenido)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nombre", System.IO.Path.GetFileName(archivoPdf.FileName));
                    cmd.Parameters.AddWithValue("@contenido", textoExtraido);
                    cmd.ExecuteNonQuery();
                }

                ViewBag.Mensaje = "PDF subido y contenido guardado en la base de datos.";
            }

            return View();
        }
    }
}
