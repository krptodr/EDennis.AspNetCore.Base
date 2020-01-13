﻿using Colors2.Models;
using EDennis.AspNetCore.Base.EntityFramework;
using EDennis.AspNetCore.Base.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Colors2Api.Lib.Controllers {
    public class HslController : SqlServerReadonlyController<Hsl, Color2DbContext> {
        public HslController(HslRepo repo, ILogger<SqlServerReadonlyController<Hsl, Color2DbContext>> logger) 
            : base(repo, logger) { }


        [HttpGet("RgbHslByColorName")]
        public RgbHsl GetRgbHslByColorName([FromQuery] string colorName) {
            var parameters = new Params().Add("colorName", colorName);
            var result = Repo.Context.GetSingleFromStoredProcedure<Color2DbContext, RgbHsl>(
                "RgbHslByColorName", parameters);
            return result;
        }

        [HttpGet("RgbHslByColorNameContains")]
        public List<RgbHsl> GetRgbHslByColorNameContains([FromQuery] string colorName) {
            var parameters = new Params().Add("colorNameContains", colorName);
            var result = Repo.Context.GetSingleFromStoredProcedure<Color2DbContext, List<RgbHsl>>(
                "RgbHslByColorNameContains", parameters);
            return result;
        }


    }
}
