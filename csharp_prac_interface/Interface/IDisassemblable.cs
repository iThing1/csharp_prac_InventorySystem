using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_prac_interface
{
    public interface IDisassemblable
    {
        List<Material> Disassemble();
    }
}
