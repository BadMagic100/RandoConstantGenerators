using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandoConstantGenerators
{
    internal class ConstData
    {
        public readonly string Name;
        public readonly string Value;
        public ConstData(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    internal class JsonFileProcessor
    {
        private readonly string? dataPath;
        private readonly IEnumerable<string?> dataFiles;

        private static string GetSafeName(string orig)
        {
            string val = orig.Replace("'", "").Replace('-', '_').Replace("[", "_").Replace("]", "");
            if (val.Length == 0 || char.IsDigit(val[0]))
            {
                return '_' + val;
            }
            return val;
        }

        private static IEnumerable<string> StringTokens(IEnumerable<JToken> tokens, bool queryKeys)
        {
            if (queryKeys)
            {
                return tokens.Where(t => !string.IsNullOrEmpty(t.Path)).Select(t =>
                {
                    string[] pathParts = t.Path.Split(new[] { "[", "]", "." }, StringSplitOptions.RemoveEmptyEntries);
                    return pathParts[pathParts.Length - 1];
                });
            }
            return tokens.Where(t => t.Type == JTokenType.String).Select(t => (string)t!);
        }

        public JsonFileProcessor(string? dataPath, IEnumerable<string?> dataFiles)
        {
            this.dataPath = dataPath;
            this.dataFiles = dataFiles;
        }

        public IEnumerable<ConstData> CollectConsts()
        {
            if (dataPath is null)
            {
                return Enumerable.Empty<ConstData>();
            }

            List<ConstData> result = new();
            foreach (string? nullableFile in dataFiles)
            {
                if (nullableFile is not string file)
                {
                    continue;
                }

                JContainer? o = JsonConvert.DeserializeObject<JContainer>(file);
                if (o is null)
                {
                    continue;
                }

                bool queryKeys = false;
                string queryPath = dataPath;
                if (queryPath.EndsWith("~"))
                {
                    queryKeys = true;
                    queryPath = queryPath.Remove(dataPath.Length - 1);
                }
                IEnumerable<JToken> tokens = o.SelectTokens(queryPath);
                result.AddRange(StringTokens(tokens, queryKeys).Select(t => new ConstData(GetSafeName(t), t)));
            }

            return result;
        }

        public void GenerateSources(INamedTypeSymbol type, IEnumerable<ConstData> consts, GeneratorExecutionContext context)
        {
            StringBuilder source = new($@"
namespace {type.ContainingNamespace.ToDisplayString()}
{{
    {SyntaxFacts.GetText(type.DeclaredAccessibility)} static partial class {type.Name}
    {{
");
            const string indent = "        ";
            foreach (ConstData c in consts)
            {
                source.Append(indent);
                source.AppendLine($"public const string {c.Name} = \"{c.Value}\";");
            }
            source.AppendLine("    }");
            source.AppendLine("}");
            context.AddSource(type.Name + ".g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
        }
    }
}
