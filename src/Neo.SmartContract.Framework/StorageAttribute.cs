using System;

namespace Neo.SmartContract.Framework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StorageAttribute : Attribute { }
}