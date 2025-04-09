using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RelampagoSitios.Models
{
    public class Pdf
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}