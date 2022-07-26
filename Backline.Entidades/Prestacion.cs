using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backline.Entidades
{
    public class Prestacion
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public int Valor { get; set; }
        public bool ValorLibre { get; set; }
    }
}
