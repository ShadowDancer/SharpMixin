using System;
using System.CodeDom.Compiler;

namespace SharpMixin.Generator.Generator.Utils
{
    internal class Scope : IDisposable
    {
        private readonly IndentedTextWriter _indentedTextWriter;

        public Scope(IndentedTextWriter indentedTextWriter)
        {
            _indentedTextWriter = indentedTextWriter;
            _indentedTextWriter.WriteLine("{");
            _indentedTextWriter.Indent++;
        }

        public void Dispose()
        {
            _indentedTextWriter.Indent--;
            _indentedTextWriter.WriteLine("}");
        }
    }

    internal static class ScopeExtensions
    {
        public static IDisposable Scope(this IndentedTextWriter writer)
        {
            return new Scope(writer);
        }
    }
}