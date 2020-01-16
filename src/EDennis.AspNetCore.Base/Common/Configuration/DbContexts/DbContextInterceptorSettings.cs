﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EDennis.AspNetCore.Base {
    public class DbContextInterceptorSettings<TContext> : DbContextBaseSettings<TContext> 
        where TContext: DbContext{

        public bool Enabled { get; set; } = false;

        //public readonly static UserSource DEFAULT_USER_SOURCE = UserSource.JwtNameClaim;

        //public UserSource InstanceNameSource { get; set; } = DEFAULT_USER_SOURCE;

        public bool IsInMemory { get; set; }
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadUncommitted;
        public bool ResetSqlServerIdentities { get; set; } = false;
        public bool ResetSqlServerSequences { get; set; } = false;
    }
}
