﻿using EDennis.AspNetCore.Base.EntityFramework;
using Hr.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hr.RazorApp.AddressPages {
    public class IndexModel : PageModel
    {
        private readonly HrApi _api;

        public IndexModel(HrApi api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)] public string Where { get; set; }
        [BindProperty(SupportsGet = true)] public string OrderBy { get; set; }
        [BindProperty(SupportsGet = true)] public string Select { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = -1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        [BindProperty(SupportsGet = true)] public int RowCount { get; set; } = -1;

        [BindProperty] public PagingData PagingData { get; set; }


        public IList<Address> Addresses { get;set; }

        public async Task OnGetAsync()
        {
            PagingData = new PagingData {
                 
                RowCount = RowCount,
                CurrentPage = CurrentPage,
                PageSize = PageSize
            };

            DynamicLinqResult<Address> result = await _api.GetAddressesAsync(
                skip:(PagingData.CurrentPage - 1)*PagingData.PageCount,
                take:PagingData.PageSize
                );

            Addresses = result.Data;

        }
    }
}
