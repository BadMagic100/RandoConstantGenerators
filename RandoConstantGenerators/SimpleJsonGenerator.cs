using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace RandoConstantGenerators
{
    [Generator(LanguageNames.CSharp)]
    public sealed class TermGenerator : SimpleJsonGenerator
    {
        protected override string DataPath => "$[*]";
        protected override string MarkerAttributeName => AttributeGenerator.Terms;
    }

    [Generator(LanguageNames.CSharp)]
    public sealed class LogicDefNamesGenerator : SimpleJsonGenerator
    {
        protected override string DataPath => "$[*].name";
        protected override string MarkerAttributeName => AttributeGenerator.LogicDefNames;
    }

    [Generator(LanguageNames.CSharp)]
    public sealed class MacroNamesGenerator : SimpleJsonGenerator
    {
        protected override string DataPath => "$.*~";
        protected override string MarkerAttributeName => AttributeGenerator.MacroNames;
    }

    public abstract class SimpleJsonGenerator : ISourceGenerator
    {
        protected abstract string DataPath { get; }
        protected abstract string MarkerAttributeName { get; }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not MarkerAttributeReceiver mar)
            {
                return;
            }

            foreach (Diagnostic diag in mar.PreprocessorDiagnostics)
            {
                context.ReportDiagnostic(diag);
            }

            foreach (var (type, attr) in mar.Classes)
            {
                List<TypedConstant> args = attr.ConstructorArguments.ToList();
                List<string?> values = args[0].Values.Select(c => c.Value as string).ToList();

                IEnumerable<string?> datafiles = context.AdditionalFiles
                    .Where(f => values.Where(v => v != null).Any(v => f.Path.EndsWith(v!)))
                    .Select(t => t.GetText(context.CancellationToken)?.ToString());

                JsonFileProcessor proc = new(DataPath, datafiles);
                IEnumerable<ConstData> consts = proc.CollectConsts();
                proc.GenerateSources(type, consts, context);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MarkerAttributeReceiver(MarkerAttributeName));
        }
    }
}
