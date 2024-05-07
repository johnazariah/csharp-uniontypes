using Microsoft.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace CSharp.UnionTypes.SourceGenerator
{
    [Generator]
    public class FileTransformGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pipeline = context.AdditionalTextsProvider
                .Where(static (text) => text.Path.EndsWith(".csunion"))
                .Select(static (text, cancellationToken) =>
                {
                    var name = Path.GetFileName(text.Path);
                    var code = Library.GenerateNamespace(text.GetText(cancellationToken)?.ToString());
                    return (name, code);
                });

            context.RegisterSourceOutput(pipeline,
                static (context, pair) =>
                    // Note: this AddSource is simplified. You will likely want to include the path in the name of the file to avoid
                    // issues with duplicate file names in different paths in the same project.
                    context.AddSource($"{pair.name}generated.cs", SourceText.From(pair.code, Encoding.UTF8)));
        }
    }

    internal class Generator
    {
    }
}
