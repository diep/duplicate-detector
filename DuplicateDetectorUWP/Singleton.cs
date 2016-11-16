using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDirectorUWP
{
    public class Singleton
    {
        public static string CreateGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
