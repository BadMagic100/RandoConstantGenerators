using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

    internal class TokenData
    {
        public readonly string tokenValue;
        public readonly int index;
        public readonly int len;

        public TokenData(string tokenValue, int index, int len)
        {
            this.tokenValue = tokenValue;
            this.index = index;
            this.len = len;
        }

        public bool Intersects(TokenData other)
        {
            if (index >= other.index && index < other.index + other.len)
            {
                return true;
            }
            if (index + len > other.index && index + len <= other.index + other.len)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{tokenValue} @ {index}-{index + len - 1}";
        }
    }

    internal class JsonFileProcessor
    {
        private readonly string? dataPath;
        private readonly IEnumerable<string?> dataFiles;

        private static readonly DiagnosticDescriptor NoConstantsFound = new(
            id: "RCG002",
            title: "No constants found",
            messageFormat: "No constants found for the class '{0}'. This may mean your AdditionalFiles paths are invalid or you've provided incorrect JsonPath.",
            category: "RandoConstantGenerators",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static string GetSafeName(string orig)
        {
            string val = orig;
            foreach (char underscoreReplacement in " -[")
            {
                val = val.Replace(underscoreReplacement, '_');
            }
            val = Regex.Replace(val, @"\W", "");
            val = Regex.Replace(val, @"_{2,}", "_");
            if (val.Length == 0 || char.IsDigit(val[0]))
            {
                return '_' + val;
            }
            return val;
        }

        private static string EscapeBackslashes(string orig)
        {
            return orig.Replace("\\", "\\\\");
        }

        private static string UnescapePath(string orig)
        {
            return orig.Replace("\\'", "'").Replace("\\\\", "\\");
        }

        private static IEnumerable<string> StringTokens(IEnumerable<JToken> tokens, bool queryKeys)
        {
            if (queryKeys)
            {
                return tokens.Where(t => !string.IsNullOrEmpty(t.Path)).Select(t =>
                {
                    List<TokenData> indexerTokens = new();
                    List<TokenData> namedTokens = new();
                    foreach (Match m in Regex.Matches(t.Path, @"\[('.+?(?<!\\)'|\d+?)\]"))
                    {
                        string val = m.Groups[1].Value;
                        if (val.StartsWith("'"))
                        {
                            val = UnescapePath(val.Substring(1, val.Length - 2));
                        }
                        indexerTokens.Add(new TokenData(val, m.Index, m.Length));
                    }
                    foreach (Match m in Regex.Matches(t.Path, @"(?<=^|\]\.|\.)(.+?)(?=$|[.\[])"))
                    {
                        string val = m.Groups[1].Value;
                        namedTokens.Add(new TokenData(val, m.Index, m.Length));
                    }
                    IEnumerable<TokenData> orderedPathTokens = indexerTokens
                        .Concat(namedTokens.Where(t => !indexerTokens.Any(u => t.Intersects(u))))
                        .OrderBy(t => t.index);
                    return orderedPathTokens.Last().tokenValue;
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
                result.AddRange(StringTokens(tokens, queryKeys).Select(t => new ConstData(GetSafeName(t), EscapeBackslashes(t))));
            }

            return result;
        }

        public void GenerateSources(INamedTypeSymbol type, IEnumerable<ConstData> consts, GeneratorExecutionContext context)
        {
            if (!consts.Any())
            {
                SyntaxReference sr = type.DeclaringSyntaxReferences.First();
                SyntaxToken classIdentifier = sr.GetSyntax().ChildTokens().First(t => t.IsKind(SyntaxKind.IdentifierToken));
                context.ReportDiagnostic(Diagnostic.Create(NoConstantsFound, Location.Create(sr.SyntaxTree, classIdentifier.Span), type.ToDisplayString()));
            }

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
