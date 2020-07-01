using Mono.Cecil;
using Neo.SmartContract.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.Compiler
{
    public class FuncExport
    {
        public static readonly TypeReference Void = new TypeReference("System", "Void", ModuleDefinition.ReadModule(typeof(object).Assembly.Location, new ReaderParameters(ReadingMode.Immediate)), null);

        internal static string ConvType(TypeReference t)
        {
            if (t is null) return "Null";

            var type = t.FullName;

            TypeDefinition definition = t.Resolve();
            if (definition != null)
                foreach (var i in definition.Interfaces)
                {
                    if (i.InterfaceType.Name == nameof(IApiInterface))
                    {
                        return "IInteropInterface";
                    }
                }

            switch (type)
            {
                case "System.Boolean": return "Boolean";
                case "System.Char":
                case "System.Byte":
                case "System.SByte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Numerics.BigInteger": return "Integer";
                case "System.Byte[]": return "ByteArray";
                case "System.String": return "String";
                case "IInteropInterface": return "InteropInterface";
                case "System.Void": return "Void";
                case "System.Object": return "Any";
            }

            if (t.IsArray) return "Array";

            if (type.StartsWith("System.Action") || type.StartsWith("System.Func") || type.StartsWith("System.Delegate"))
                return $"Unknown:Pointers are not allowed to be public '{type}'";
            if (type.StartsWith("System.ValueTuple`") || type.StartsWith("System.Tuple`"))
                return "Array";

            return "Unknown:" + type;
        }

        public static string ComputeHash(byte[] script)
        {
            var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash256 = sha256.ComputeHash(script);
            var ripemd160 = new Neo.Cryptography.RIPEMD160Managed();
            var hash = ripemd160.ComputeHash(hash256);

            StringBuilder sb = new StringBuilder();
            sb.Append("0x");
            for (int i = hash.Length - 1; i >= 0; i--)
            {
                sb.Append(hash[i].ToString("x02"));
            }
            return sb.ToString();
        }

        public static MyJson.JsonNode_Object Export(NeoModule module, byte[] script, Dictionary<int, int> addrConvTable)
        {
            var outjson = new MyJson.JsonNode_Object();

            //hash
            outjson.SetDictValue("hash", ComputeHash(script));

            //functions
            var methods = new MyJson.JsonNode_Array();
            outjson["methods"] = methods;

            List<string> names = new List<string>();
            foreach (var function in module.mapMethods)
            {
                var mm = function.Value;
                if (mm.inSmartContract == false)
                    continue;
                if (mm.isPublic == false)
                    continue;

                var funcsign = new MyJson.JsonNode_Object();
                methods.Add(funcsign);
                funcsign.SetDictValue("name", function.Value.displayName);
                if (names.Contains(function.Value.displayName))
                {
                    throw new Exception("abi not allow same name functions");
                }
                names.Add(function.Value.displayName);
                var offset = addrConvTable?[function.Value.funcaddr] ?? function.Value.funcaddr;
                funcsign.SetDictValue("offset", offset.ToString());
                MyJson.JsonNode_Array funcparams = new MyJson.JsonNode_Array();
                funcsign["parameters"] = funcparams;
                if (mm.paramtypes != null)
                {
                    foreach (var v in mm.paramtypes)
                    {
                        var item = new MyJson.JsonNode_Object();
                        funcparams.Add(item);

                        item.SetDictValue("name", v.name);
                        item.SetDictValue("type", ConvType(v.type));
                    }
                }

                var rtype = ConvType(mm.returntype);
                if (rtype.StartsWith("Unknown:"))
                {
                    throw new Exception($"Unknown return type '{mm.returntype}' for method '{function.Value.name}'");
                }

                funcsign.SetDictValue("returnType", rtype);
            }

            //events
            var eventsigns = new MyJson.JsonNode_Array();
            outjson["events"] = eventsigns;
            foreach (var events in module.mapEvents)
            {
                var mm = events.Value;
                var funcsign = new MyJson.JsonNode_Object();
                eventsigns.Add(funcsign);

                funcsign.SetDictValue("name", events.Value.displayName);
                MyJson.JsonNode_Array funcparams = new MyJson.JsonNode_Array();
                funcsign["parameters"] = funcparams;
                if (mm.paramtypes != null)
                {
                    foreach (var v in mm.paramtypes)
                    {
                        var item = new MyJson.JsonNode_Object();
                        funcparams.Add(item);

                        item.SetDictValue("name", v.name);
                        item.SetDictValue("type", ConvType(v.type));
                    }
                }
                //event do not have returntype in nep3
                //var rtype = ConvType(mm.returntype);
                //funcsign.SetDictValue("returntype", rtype);
            }

            return outjson;
        }
    }
}
