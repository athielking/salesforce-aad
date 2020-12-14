using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Odata.Data;
using Odata.Models;

namespace Odata.Controllers
{
    public class BusinessTypeController : ControllerBase
    {
        private readonly OdataDbContext _context;

        public BusinessTypeController(OdataDbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public async Task<ActionResult<IEnumerable<BusinessType>>> Get()
        {
            return await _context.BusinessType.ToListAsync();
        }
    }
}
