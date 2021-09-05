using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SharpMixin.Generator.Generator.Utils;

namespace SharpMixin.Generator.Generator
{
    /// <summary>
    /// Generates constructor which assigns each parameter to field.
    /// </summary>
    internal class ConstructorGenerator
    {
        public record Param
        {
            public Param(string paramName, ITypeSymbol paramType, IImmutableList<string> fieldNames)
            {
                ParamName = paramName;
                ParamType = paramType;
                FieldNames = fieldNames;
            }

            public string ParamName { get; }
            public ITypeSymbol ParamType { get; }
            public IImmutableList<string> FieldNames { get; }
        }

        public static void GenerateConstructor(IndentedTextWriter sourceWriter, IImmutableList<Param> parameters,
            string typeName)
        {
            sourceWriter.Write($"public {typeName}(");
            var first = true;
            foreach (var parameter in parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sourceWriter.Write(", ");
                }

                string fullTypeName = parameter.ParamType.ToDisplayString(SymbolStyles.FullTypeName);
                sourceWriter.Write(fullTypeName);
                sourceWriter.Write(" ");
                sourceWriter.Write(parameter.ParamName);
            }

            sourceWriter.WriteLine(")");
            sourceWriter.WriteLine("{");

            sourceWriter.Indent++;
            foreach (var parameter in parameters)
            {
                foreach (var fieldName in parameter.FieldNames)
                {
                    sourceWriter.Write("this.");
                    sourceWriter.Write(fieldName);
                    sourceWriter.Write(" = ");
                    sourceWriter.Write(parameter.ParamName);
                    sourceWriter.WriteLine(";");
                }
            }

            sourceWriter.Indent--;
            sourceWriter.WriteLine("}");
            sourceWriter.WriteLine();
        }
    }
}