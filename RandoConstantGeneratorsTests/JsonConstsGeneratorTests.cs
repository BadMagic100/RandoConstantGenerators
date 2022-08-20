using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using VerifyCS = CSharpSourceGeneratorVerifier<RandoConstantGenerators.JsonConstsGenerator>;

namespace RandoConstantGenerators.Tests
{
    [TestClass()]
    public class JsonConstsGeneratorTests
    {
        [TestMethod()]
        public async Task NotPartialStaticDiagnostic()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$[*]"", ""list.json"")]
    public static class MakeMeSomeMagicConstants
    {
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\list.json", "[ \"T1\", \"T2\" ]")
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("RCG001", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
                            .WithSpan(5, 25, 5, 49)
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task NoConstantsDiagnostic()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$[*]"", ""list.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("RCG002", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
                            .WithSpan(5, 33, 5, 57)
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task SimpleGeneration()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$[*]"", ""list.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string T1 = ""T1"";
        public const string T2 = ""T2"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\list.json", "[ \"T1\", \"T2\" ]")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task DictionaryValueGeneration()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$.*"", ""dict.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string v1 = ""v1"";
        public const string v2 = ""v2"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\dict.json", "{ \"k1\": \"v1\", \"k2\": \"v2\" }")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task SimpleKeyGeneration()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$.*~"", ""dict.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string k1 = ""k1"";
        public const string k2 = ""k2"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\dict.json", "{ \"k1\": \"v1\", \"k2\": \"v2\" }")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task ComplexKeyGeneration()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$.*[*].*~"", ""dict.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string k1 = ""k1"";
        public const string k2 = ""k2"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\dict.json", "{ \"k0\": [{ \"k1\": \"v1\", \"k2\": \"v2\" }] }")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task ListKeyGeneration()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$[*]~"", ""list.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string _0 = ""0"";
        public const string _1 = ""1"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\list.json", "[ \"T1\", \"T2\" ]")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task NonStarPathGeneration()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$[*].name"", ""objs.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string FirstThing = ""FirstThing"";
        public const string SecondThing = ""SecondThing"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\objs.json", "[ { \"name\": \"FirstThing\", \"value\": \"asdfasdf\" }, { \"name\": \"SecondThing\", \"value\": \"asdfasdf\" } ]")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task SpecialCharacterKeys()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$.*~"", ""data.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string Lever_Queens_Garden_Stag = ""Lever-Queen's_Garden_Stag"";
        public const string Nailmasters_Oro_Mato = ""Nailmasters_Oro_&_Mato"";
        public const string Cliffs_01_right4 = ""Cliffs_01[right4]"";
        public const string Thing_With_Backslashes = ""Thing_\\With\\_Backslashes"";
        public const string keywith_specialcharactersandescaped_ = ""key.with[special.characters.and].escaped_'"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\data.json", "{ \"Lever-Queen's_Garden_Stag\": \"Lever-Queen's_Garden_Stag\", \"Nailmasters_Oro_&_Mato\": \"Nailmasters_Oro_&_Mato\", \"Cliffs_01[right4]\": \"Cliffs_01[right4]\", \"Thing_\\\\With\\\\_Backslashes\": \"Thing_\\\\With\\\\_Backslashes\", \"key.with[special.characters.and].escaped_'\": \"key.with[special.characters.and].escaped_'\" }")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }

        [TestMethod()]
        public async Task SpecialCharacterValues()
        {
            var code = @"
namespace Test
{
    [RandoConstantGenerators.GenerateJsonConsts(""$.*"", ""data.json"")]
    public static partial class MakeMeSomeMagicConstants
    {
    }
}
";
            var gen = @"
namespace Test
{
    public static partial class MakeMeSomeMagicConstants
    {
        public const string Lever_Queens_Garden_Stag = ""Lever-Queen's_Garden_Stag"";
        public const string Nailmasters_Oro_Mato = ""Nailmasters_Oro_&_Mato"";
        public const string Cliffs_01_right4 = ""Cliffs_01[right4]"";
        public const string Thing_With_Backslashes = ""Thing_\\With\\_Backslashes"";
        public const string keywith_specialcharactersandescaped_ = ""key.with[special.characters.and].escaped_'"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code, AttributeGenerator.AttributeSource },
                    AdditionalFiles =
                    {
                        ("Resources\\data.json", "{ \"Lever-Queen's_Garden_Stag\": \"Lever-Queen's_Garden_Stag\", \"Nailmasters_Oro_&_Mato\": \"Nailmasters_Oro_&_Mato\", \"Cliffs_01[right4]\": \"Cliffs_01[right4]\", \"Thing_\\\\With\\\\_Backslashes\": \"Thing_\\\\With\\\\_Backslashes\", \"key.with[special.characters.and].escaped_'\": \"key.with[special.characters.and].escaped_'\" }")
                    },
                    GeneratedSources =
                    {
                        (typeof(JsonConstsGenerator), "MakeMeSomeMagicConstants.g.cs", SourceText.From(gen, Encoding.UTF8))
                    }
                }
            }.RunAsync();
        }
    }
}