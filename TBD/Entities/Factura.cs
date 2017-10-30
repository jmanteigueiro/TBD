using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Entities
{
    class Factura
    {
        public Factura()
        {

        }

        public int FacturaID { get; set; }
        public int ClienteID { get; set; }
        public string Nome { get; set; }
        public string Morada { get; set; }

    }
}
