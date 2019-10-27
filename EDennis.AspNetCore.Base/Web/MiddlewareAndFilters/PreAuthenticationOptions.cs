﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.AspNetCore.Base.Web {

    public enum PreAuthenticationType {
        None,
        MockClient,
        AutoLogin
    }

    public class PreAuthenticationOptions {
        public PreAuthenticationType PreAuthenticationType { get; set; }
    }
}