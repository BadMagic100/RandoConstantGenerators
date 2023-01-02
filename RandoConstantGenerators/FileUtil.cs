using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace RandoConstantGenerators
{
    internal static class FileUtil
    {
        public static IEnumerable<string?> GetFiles(IEnumerable<TypedConstant> searchPaths, GeneratorExecutionContext context)
        {
            List<string> values = searchPaths
                    .Select(c => c.Value as string)
                    .Where(s => s != null)
                    .Select(s => s!.Replace('\\', '/'))
                    .ToList();

            return context.AdditionalFiles
                .Where(f => values.Any(v => f.Path.Replace('\\', '/').EndsWith(v)))
                .Select(t => t.GetText(context.CancellationToken)?.ToString());
        }
    }
}
