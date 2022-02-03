using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagination.WebApi.Contexts;
using Pagination.WebApi.Filter;
using Pagination.WebApi.Helpers;
using Pagination.WebApi.Models;
using Pagination.WebApi.Services;
using Pagination.WebApi.Wrappers;

namespace Pagination.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IUriService uriService;
        public CustomerController(ApplicationDbContext context, IUriService uriService)
        {
            this.context = context;
            this.uriService = uriService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
        {
            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(filter.FirstName, filter.Email,
                filter.PageNumber, filter.PageSize);

            // repository e gitmeli, geriye Task<List<Domain>> dönecek (pagedData)
            var pagedData = await context.Customers
               .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
               .Take(validFilter.PageSize)
               .ToListAsync();

            List<Customer> filteredData = pagedData
                .Where(x =>
                {
                    if (x.FirstName != null && x.FirstName == filter.FirstName)
                        return true;
                    else if (x.Email != null && x.Email == filter.Email)
                        return true;
                    return false;
                })
                .ToList();

            //pagedData = (retValue.Count > 0) ? retValue : pagedData;
            /*
            if (filter.FirstName != null && !string.IsNullOrEmpty(filter.FirstName))
            {
                pagedData = pagedData.Where(x => x.FirstName == filter.FirstName).ToList();
            }
            if (filter.Email != null && !string.IsNullOrEmpty(filter.Email))
            {
                pagedData = pagedData.Where(x => x.Email == filter.Email).ToList();
            }
            */
            var totalRecords = await context.Customers.CountAsync();

            // dönen değer buraya parametre olarak verilecek
            //var pagedReponse = PaginationHelper.CreatePagedReponse<Customer>(pagedData, validFilter, totalRecords, uriService,route);
            var pagedReponse = PaginationHelper.CreatePagedReponse<Customer>((filteredData.Count > 0) ? filteredData : pagedData, 
                validFilter, totalRecords, uriService,route);
            return Ok(pagedReponse);
        }
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var customer = await context.Customers.Where(a => a.Id == id).FirstOrDefaultAsync();
        //    return Ok(new Response<Customer>(customer));
        //}
    }
}