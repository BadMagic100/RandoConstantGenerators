using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace RandoConstantGenerators
{
    [Generator(LanguageNames.CSharp)]
    public class JsonConstsGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not MarkerAttributeReceiver mar)
            {
                return;
            }

            foreach (var (type, attr) in mar.Classes)
            {
                List<TypedConstant> args = attr.ConstructorArguments.ToList();
                string? datapath = args[0].Value as string;
                List<string?> values = args[1].Values.Select(c => c.Value as string).ToList();

                IEnumerable<string?> datafiles = context.AdditionalFiles
                    .Where(f => values.Where(v => v != null).Any(v => f.Path.EndsWith(v!)))
                    .Select(t => t.GetText(context.CancellationToken)?.ToString());

                JsonFileProcessor proc = new(datapath, datafiles);
                IEnumerable<ConstData> consts = proc.CollectConsts();
                proc.GenerateSources(type, consts, context);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            GenTimeDependencies.AddOnce();
            context.RegisterForSyntaxNotifications(() => new MarkerAttributeReceiver(AttributeGenerator.JsonConsts));
        }
    }
}
