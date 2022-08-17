using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RandoConstantGenerators
{
    [Generator(LanguageNames.CSharp)]
    public class AttributeGenerator : ISourceGenerator
    {
        public const string JsonConsts = $"RandoConstantGenerators.Generate{nameof(JsonConsts)}Attribute";
        public const string Terms = $"RandoConstantGenerators.Generate{nameof(Terms)}Attribute";
        public const string LogicDefNames = $"RandoConstantGenerators.Generate{nameof(LogicDefNames)}Attribute";
        public const string MacroNames = $"RandoConstantGenerators.Generate{nameof(MacroNames)}Attribute";

        public const string AttributeSource = @$"
namespace RandoConstantGenerators
{{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal class Generate{nameof(JsonConsts)}Attribute : System.Attribute
    {{
        public string[] FilePathSuffixes {{ get; }}
        public string DataPath {{ get; }}

        public Generate{nameof(JsonConsts)}Attribute(string DataPath, params string[] FilePathSuffixes)
        {{
            this.FilePathSuffixes = FilePathSuffixes;
            this.DataPath = DataPath;
        }}
    }}

    internal class Generate{nameof(Terms)}Attribute : System.Attribute
    {{
        public string[] FilePathSuffixes {{ get; }}

        public Generate{nameof(Terms)}Attribute(params string[] FilePathSuffixes)
        {{
            this.FilePathSuffixes = FilePathSuffixes;
        }}
    }}

    internal class Generate{nameof(LogicDefNames)}Attribute : System.Attribute
    {{
        public string[] FilePathSuffixes {{ get; }}

        public Generate{nameof(LogicDefNames)}Attribute(params string[] FilePathSuffixes)
        {{
            this.FilePathSuffixes = FilePathSuffixes;
        }}
    }}

    internal class Generate{nameof(MacroNames)}Attribute : System.Attribute
    {{
        public string[] FilePathSuffixes {{ get; }}

        public Generate{nameof(MacroNames)}Attribute(params string[] FilePathSuffixes)
        {{
            this.FilePathSuffixes = FilePathSuffixes;
        }}
    }}
}}
";

        public void Execute(GeneratorExecutionContext context)
        {
            // nothing special here - we do everything in post-init
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("GeneratorAttributes.g.cs", SourceText.From(AttributeSource));
            });
        }
    }
}
