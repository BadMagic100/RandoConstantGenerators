A source generator that aids in randomizer connection development by pulling constants from embedded JSON resources.

### Usage

1. Add `<AdditionalFileItemNames>EmbeddedResource</AdditionalFileItemNames>` to a PropertyGroup in your csproj
1. Add a nuget dependency on the analyzer package
1. Create a static partial class to hold your class, and mark it with any one of the marker attributes in the `RandoConstantGenerators` namespace.