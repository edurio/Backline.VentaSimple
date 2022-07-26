﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backline.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public string RutEmpresa { get; set; }
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public int Emp_Id { get; set; }
        public int Est_Id { get; set; }
        public bool EsAfecta { get; set; }
        public string NombreEmpresa { get; set; }
        public string NombreEstablecimiento { get; set; }
        public string Facturador { get; set; }
        public string Ambiente { get; set; }
        public string Usuario_FE { get; set; }
        public string Clave_FE { get; set; }

        public bool Administrador { get; set; }
    }
}
