﻿using Microsoft.AspNetCore.Mvc;

namespace TestingPlayground.Models
{
    public class NoPropertiesAttributedClass
    {
        [FromHeader]
        public string? ExampleString { get; set; }

        public int ExampleInteger { get; set; }
    }
}
