﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingPageKHDN.Application.Services
{
    public interface INsfwService
    {
        Task<bool> IsSafeImageAsync(Stream imageStream);
    }
}
