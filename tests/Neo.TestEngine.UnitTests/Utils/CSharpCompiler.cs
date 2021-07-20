using Neo.Compiler;
using Neo.IO;
using System;
using System.IO;

namespace Neo.TestEngine.UnitTests.Utils
{
    public class CSharpCompiler
    {
        public static void Compile(string filepath)
        {
            CompilationContext context = CompilationContext.CompileSources(new[] { filepath }, new Options
            {
                AddressVersion = ProtocolSettings.Default.AddressVersion
            });

            if (!context.Success)
            {
                throw new Exception(string.Format("Could not compile '{0}'", filepath));
            }
            var nefPath = filepath.Replace(".cs", ".nef");
            File.WriteAllBytes(nefPath, context.CreateExecutable().ToArray());

            var manifestPath = filepath.Replace(".cs", ".manifest.json");
            File.WriteAllBytes(manifestPath, context.CreateManifest().ToByteArray(false));
        }
    }
}
