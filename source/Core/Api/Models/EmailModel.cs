﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.ComponentModel.DataAnnotations;

namespace Thinktecture.IdentityManager.Api.Models
{
    public class EmailModel
    {
        [Required]
        public string Email { get; set; }
    }
}