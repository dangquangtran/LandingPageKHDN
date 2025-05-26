using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Services
{
    public interface INSFWDetectionService
    {
        Task<bool> IsSafeImageAsync(Stream imageStream);
    }
}
