using Microsoft.CodeAnalysis;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.VM.Types;
using System.ComponentModel;
using System.Linq;
using System.Numerics;

namespace Neo.Compiler
{
    static class Helper
    {
        public static ContractParameterType GetContractParameterType(this ITypeSymbol type)
        {
            //TODO: More types
            return type.SpecialType switch
            {
                SpecialType.System_Object => ContractParameterType.Any,
                SpecialType.System_Void => ContractParameterType.Void,
                SpecialType.System_Boolean => ContractParameterType.Boolean,
                SpecialType.System_Char => ContractParameterType.Integer,
                SpecialType.System_SByte => ContractParameterType.Integer,
                SpecialType.System_Byte => ContractParameterType.Integer,
                SpecialType.System_Int16 => ContractParameterType.Integer,
                SpecialType.System_UInt16 => ContractParameterType.Integer,
                SpecialType.System_Int32 => ContractParameterType.Integer,
                SpecialType.System_UInt32 => ContractParameterType.Integer,
                SpecialType.System_Int64 => ContractParameterType.Integer,
                SpecialType.System_UInt64 => ContractParameterType.Integer,
                SpecialType.System_String => ContractParameterType.String,
                _ => type.Name switch
                {
                    nameof(BigInteger) => ContractParameterType.Integer,
                    "UInt160" => ContractParameterType.Hash160,
                    "UInt256" => ContractParameterType.Hash256,
                    "ECPoint" => ContractParameterType.PublicKey,
                    "ByteString" => ContractParameterType.ByteArray,
                    _ => ContractParameterType.Any
                }
            };
        }

        public static StackItemType GetStackItemType(this ITypeSymbol type)
        {
            return type.SpecialType switch
            {
                SpecialType.System_Boolean => StackItemType.Boolean,
                SpecialType.System_Char => StackItemType.Integer,
                SpecialType.System_SByte => StackItemType.Integer,
                SpecialType.System_Byte => StackItemType.Integer,
                SpecialType.System_Int16 => StackItemType.Integer,
                SpecialType.System_UInt16 => StackItemType.Integer,
                SpecialType.System_Int32 => StackItemType.Integer,
                SpecialType.System_UInt32 => StackItemType.Integer,
                SpecialType.System_Int64 => StackItemType.Integer,
                SpecialType.System_UInt64 => StackItemType.Integer,
                _ => type.Name switch
                {
                    nameof(BigInteger) => StackItemType.Integer,
                    _ => StackItemType.Any
                }
            };
        }

        public static IFieldSymbol[] GetFields(this ITypeSymbol type)
        {
            return type.GetMembers().OfType<IFieldSymbol>().Where(p => !p.IsStatic).ToArray();
        }

        public static string GetDisplayName(this ISymbol symbol, bool lowercase = false)
        {
            AttributeData attribute = symbol.GetAttributes().FirstOrDefault(p => p.AttributeClass.Name == nameof(DisplayNameAttribute));
            if (attribute is not null) return (string)attribute.ConstructorArguments[0].Value;
            if (symbol is IMethodSymbol method)
            {
                switch (method.MethodKind)
                {
                    case MethodKind.Constructor:
                        symbol = method.ContainingType;
                        break;
                    case MethodKind.PropertyGet:
                        ISymbol property = method.AssociatedSymbol;
                        attribute = property.GetAttributes().FirstOrDefault(p => p.AttributeClass.Name == nameof(DisplayNameAttribute));
                        if (attribute is not null) return (string)attribute.ConstructorArguments[0].Value;
                        symbol = property;
                        break;
                    case MethodKind.StaticConstructor:
                        return "_initialize";
                }
            }
            if (lowercase)
                return symbol.Name[..1].ToLowerInvariant() + symbol.Name[1..];
            else
                return symbol.Name;
        }

        public static ContractParameterDefinition ToAbiParameter(this IParameterSymbol symbol)
        {
            return new ContractParameterDefinition
            {
                Name = symbol.Name,
                Type = symbol.Type.GetContractParameterType()
            };
        }
    }
}