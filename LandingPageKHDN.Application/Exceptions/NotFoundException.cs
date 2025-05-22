using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message = "Không tìm thấy tài nguyên.")
            : base(message) { }
    }
}
