namespace Neo.Compiler
{
    static class DiagnosticId
    {
        public const string NoEntryPoint = "NC1001";
        public const string ExternMethod = "NC1002";
        public const string NoParameterlessConstructor = "NC1003";
        public const string MultiplyContracts = "NC1004";
        public const string SyntaxNotSupported = "NC2001";
        public const string EventReturns = "NC2002";
        public const string NonStaticDelegate = "NC2003";
        public const string FloatingPointNumber = "NC2004";
        public const string MultiplyThrows = "NC2005";
        public const string MultiplyCatches = "NC2006";
        public const string CatchFilter = "NC2007";
        public const string MultidimensionalArray = "NC2008";
        public const string InterfaceCall = "NC2009";
        public const string ArrayRange = "NC2010";
        public const string InvalidToStringType = "NC2011";
        public const string AlignmentClause = "NC2012";
        public const string FormatClause = "NC2013";
        public const string InvalidInitialValueType = "NC3001";
        public const string InvalidMethodName = "NC3002";
        public const string InvalidAbi = "NC3003";
        public const string InvalidManifest = "NC3004";
    }
}
