using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Text;

namespace ApiGenerator;

public class CodeStringBuilder
{
    private int indentationCount = 0;
    private StringBuilder stringBuilder= new StringBuilder();

    public void OpenCurlyBracketLine()
    {
        this.AppendLine("{");
        this.IncreaseIndent();
    }

    public void CloseCurlyBracketLine()
    {
        this.DecreaseIndent();
        this.AppendLine("}");
    }

    public void AppendNewLine()
    {
        this.stringBuilder.Append(Environment.NewLine);
    }

    public void AppendFormat(string format, params object[] args)
    {
        this.stringBuilder.AppendFormat(format, args);
    }

    public void AppendLine(string text)
    {
        this.Append(text);
        this.AppendNewLine();
    }

    public override string ToString()
    {
        var text = this.stringBuilder.ToString();
        return string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : CSharpSyntaxTree.ParseText(text).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
    }

    public void Reset()
    {
        this.stringBuilder = new StringBuilder();
        this.indentationCount = 0;
    }

    private void Append(string text, bool indent = true)
    {
        if (indent)
        {
            this.AppendIndent();
        }

        this.stringBuilder.Append(text);
    }

    private void AppendIndent()
    {
        this.stringBuilder.Append(new string(' ', this.indentationCount * 4));
    }

    private void IncreaseIndent() => this.indentationCount++;

    private void DecreaseIndent() => this.indentationCount--;
}
