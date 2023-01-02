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

            foreach (Diagnostic diag in mar.PreprocessorDiagnostics)
            {
                context.ReportDiagnostic(diag);
            }

            foreach (var (type, attr) in mar.Classes)
            {
                List<TypedConstant> args = attr.ConstructorArguments.ToList();
                string? datapath = args[0].Value as string;
                IEnumerable<string?> datafiles = FileUtil.GetFiles(args[1].Values, context);

                JsonFileProcessor proc = new(datapath, datafiles);
                IEnumerable<ConstData> consts = proc.CollectConsts();
                proc.GenerateSources(type, consts, context);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //GenTimeDependencies.AddOnce();
            context.RegisterForSyntaxNotifications(() => new MarkerAttributeReceiver(AttributeGenerator.JsonConsts));
        }
    }
}
