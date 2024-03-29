﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace ApiGenerator.Extensions;

public static class StringExtensions
{
    public static string RemoveSuffix(this string stringWithPotentialSuffix, string suffix)
    {
        if (stringWithPotentialSuffix.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return stringWithPotentialSuffix.Substring(0, stringWithPotentialSuffix.Length - suffix.Length);
        }

        return stringWithPotentialSuffix;
    }

    public static string PrettifyCode(this string uglyCode)
    {
        return CSharpSyntaxTree.ParseText(uglyCode).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
    }
}

