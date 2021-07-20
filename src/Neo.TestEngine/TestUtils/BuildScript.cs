using Microsoft.CodeAnalysis;
using Neo.Compiler;
using Neo.IO.Json;
using Neo.SmartContract;
using System.Collections.Generic;
using System.IO;

namespace Neo.TestingEngine
{
    public class BuildScript
    {
        public bool Success => !FromCompilation || (Context != null && Context.Success);

        public UInt160 ScriptHash { get; private set; }
        public NefFile Nef { get; protected set; }
        public JObject Manifest { get; protected set; }
        public JObject DebugInfo { get; protected set; }
        public CompilationContext Context { get; protected set; }

        private bool FromCompilation { get; set; }

        public BuildScript(NefFile nefFile, JObject manifestJson, UInt160 originHash = null)
        {
            Nef = nefFile;
            Manifest = manifestJson;

            if (originHash is null && nefFile != null)
            {
                originHash = Nef.Script.ToScriptHash();
            }
            ScriptHash = originHash;
        }

        internal static BuildScript Build(List<MetadataReference> references = null, params string[] files)
        {
            BuildScript script;
            if (files.Length == 1 && Path.GetExtension(files[0]).ToLowerInvariant() == ".nef")
            {
                var filename = files[0];
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
                {
                    NefFile neffile = new NefFile();
                    neffile.Deserialize(reader);
                    var fileNameManifest = filename.Replace(".nef", ".manifest.json");
                    string manifestFile = File.ReadAllText(fileNameManifest);
                    script = new BuildScript(neffile, JObject.Parse(manifestFile))
                    {
                        FromCompilation = false
                    };
                }
            }
            else
            {
                NefFile nef = null;
                JObject manifest = null;
                JObject debuginfo = null;

                var options = new Options
                {
                    AddressVersion = ProtocolSettings.Default.AddressVersion
                };

                CompilationContext context;
                if (references != null && references.Count > 0)
                {
                    context = CompilationContext.CompileSources(files, references, options);
                }
                else
                {
                    context = CompilationContext.CompileSources(files, options);
                }

                if (context.Success)
                {
                    nef = context.CreateExecutable();
                    manifest = context.CreateManifest();
                    debuginfo = context.CreateDebugInformation();
                }

                script = new BuildScript(nef, manifest)
                {
                    FromCompilation = true,
                    Context = context,
                    DebugInfo = debuginfo
                };
            }

            return script;
        }
    }
}
