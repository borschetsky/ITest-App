﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITestApp.Web.Models.DashboardViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<CategoryViewModel> Categories { get; set; }
    }
}