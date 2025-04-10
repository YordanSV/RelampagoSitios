using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RelampagoSitios.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
    }
}