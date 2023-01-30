using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;

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

    public static string SanitizeClassTypeString(this string inputString)
    {
        var result = string.Empty;

        if (inputString.Contains("<"))
        {
            var split = inputString.Split('<');
            result += split.FirstOrDefault() + "<";

            if (split[1].Contains(','))
            {
                var subSplit = split[1].Split(',');

                for (int j = 0; j < subSplit.Length; j++)
                {
                    result += subSplit[j].Split('.').LastOrDefault();

                    if (j < subSplit.Length - 1)
                    {
                        result += ", ";
                    }
                }
            }
            else
            {
                result += split[1].Split('.').LastOrDefault();
            }

            for (int i = 2; i < split.Length; i++)
            {
                if (i != split.Length)
                {
                    result += "<";
                }

                result += split[i].Split('.').LastOrDefault();
            }

            return result;
        }

        return inputString.Split('.').LastOrDefault();
    }
}

