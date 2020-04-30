using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Neo.Compiler.MSIL
{
    class DefLogger : ILogger
    {
        public void Log(string log)
        {
            Console.WriteLine(log);
        }
    }

    /// <summary>
    /// Convert IL to NeoVM opcode
    /// </summary>
    public partial class ModuleConverter
    {
        public ModuleConverter(ILogger logger)
        {
            if (logger == null)
            {
                logger = new DefLogger();
            }
            this.logger = logger;
        }

        private const int MAX_STATIC_FIELDS_COUNT = 255;
        private const int MAX_PARAMS_COUNT = 255;
        private const int MAX_LOCAL_VARIABLES_COUNT = 255;

        private readonly ILogger logger;
        public NeoModule outModule;
        private ILModule inModule;
        public Dictionary<ILMethod, NeoMethod> methodLink = new Dictionary<ILMethod, NeoMethod>();

        public NeoModule Convert(ILModule _in, ConvOption option = null)
        {
            this.inModule = _in;
            this.outModule = new NeoModule(this.logger)
            {
                option = option ?? ConvOption.Default
            };
            foreach (var t in _in.mapType)
            {
                if (t.Key.Contains("<")) continue;   //skip system type
                if (t.Key.Contains("_API_")) continue; // skip api
                if (t.Key.Contains(".My.")) continue; //vb system

                foreach (var m in t.Value.methods)
                {
                    if (m.Value.method == null) continue;
                    if (m.Value.method.IsAddOn || m.Value.method.IsRemoveOn) continue; // skip the code generated by event
                    if (m.Value.method.Is_ctor()) continue;
                    if (m.Value.method.Is_cctor())
                    {
                        //if cctor contains sth can not be as a const value.
                        //  then need 1.record these cctor's code.
                        //            2.insert them to main function
                        CctorSubVM.Parse(m.Value, this.outModule);
                        continue;
                    }

                    NeoMethod nm = new NeoMethod(m.Value);
                    this.methodLink[m.Value] = nm;
                    outModule.mapMethods[nm.name] = nm;
                }

                foreach (var e in t.Value.fields)
                {
                    if (e.Value.isEvent)
                    {
                        NeoEvent ae = new NeoEvent(e.Value);
                        outModule.mapEvents[ae.name] = ae;
                    }
                    else if (e.Value.field.IsStatic)
                    {
                        var _fieldindex = outModule.mapFields.Count;
                        var field = new NeoField(e.Key, e.Value.type, _fieldindex);
                        outModule.mapFields[e.Value.field.FullName] = field;
                    }
                }
            }

            var keys = new List<string>(_in.mapType.Keys);
            foreach (var key in keys)
            {
                var value = _in.mapType[key];
                if (key.Contains("<")) continue; // skip system typee
                if (key.Contains("_API_")) continue; // skip api
                if (key.Contains(".My.")) continue; //vb system

                foreach (var m in value.methods)
                {
                    if (m.Value.method == null) continue;
                    if (m.Value.method.Is_cctor()) continue;
                    if (m.Value.method.IsAddOn || m.Value.method.IsRemoveOn) continue; // skip the code generated by event

                    var nm = this.methodLink[m.Value];

                    //try
                    {
                        nm.returntype = m.Value.returntype;
                        try
                        {
                            var type = m.Value.method.ReturnType.Resolve();
                            foreach (var i in type.Interfaces)
                            {
                                if (i.InterfaceType.Name == "IApiInterface")
                                {
                                    nm.returntype = "IInteropInterface";
                                }
                            }
                        }
                        catch
                        {
                        }

                        foreach (var src in m.Value.paramtypes)
                        {
                            nm.paramtypes.Add(new NeoParam(src.name, src.type));
                        }

                        if (IsAppCall(m.Value.method, out byte[] outcall))
                            continue;
                        if (IsNonCall(m.Value.method))
                            continue;
                        if (IsMixAttribute(m.Value.method, out VM.OpCode[] opcodes, out string[] opdata))
                            continue;

                        if (m.Key.Contains("::Main("))
                        {
                            NeoMethod _m = outModule.mapMethods[m.Key];
                        }
                        this.ConvertMethod(m.Value, nm);
                    }
                }
            }

            if (this.outModule.mapFields.Count > MAX_STATIC_FIELDS_COUNT)
                throw new Exception("too much static fields");
            if (this.outModule.mapFields.Count > 0)
            {
                InsertInitializeMethod();
                logger.Log("Insert _initialize().");
            }

            var attr = outModule.mapMethods.Values.Where(u => u.inSmartContract).Select(u => u.type.attributes.ToArray()).FirstOrDefault();
            if (attr?.Length > 0)
            {
                outModule.attributes.AddRange(attr);
            }

            this.LinkCode();

            // this.findFirstFunc();// Need to find the first method
            // Assign func addr for each method
            // Then convert the call address

            return outModule;
        }

        private string InsertInitializeMethod()
        {
            string name = "::initializemethod";
            NeoMethod initialize = new NeoMethod
            {
                _namespace = "",
                name = "Initialize",
                displayName = "_initialize",
                inSmartContract = true
            };
            initialize.returntype = "System.Void";
            initialize.funcaddr = 0;
            if (!FillInitializeMethod(initialize))
            {
                return "";
            }
            outModule.mapMethods[name] = initialize;
            return name;
        }

        private bool FillInitializeMethod(NeoMethod to)
        {
            this.addr = 0;
            this.addrconv.Clear();

#if DEBUG
            Insert1(VM.OpCode.NOP, "this is a debug code.", to);
#endif
            InsertSharedStaticVarCode(to);
#if DEBUG
            Insert1(VM.OpCode.NOP, "this is a end debug code.", to);
#endif
            Insert1(VM.OpCode.RET, "", to);
            ConvertAddrInMethod(to);
            return true;
        }

        private void LinkCode()
        {
            this.outModule.totalCodes.Clear();
            int addr = 0;

            foreach (var m in this.outModule.mapMethods)
            {
                m.Value.funcaddr = addr;

                foreach (var c in m.Value.body_Codes)
                {
                    this.outModule.totalCodes[addr] = c.Value;
                    addr += 1;
                    if (c.Value.bytes != null)
                        addr += c.Value.bytes.Length;

                    // address offset
                    c.Value.addr += m.Value.funcaddr;
                }
            }

            foreach (var c in this.outModule.totalCodes.Values)
            {
                if (c.needfixfunc)
                { // Address convert required
                    var addrfunc = this.outModule.mapMethods[c.srcfunc].funcaddr;

                    if (c.bytes.Length > 4)
                    {
                        var len = c.bytes.Length - 4;
                        long wantaddr = (long)addrfunc - c.addr - len;
                        if (wantaddr < Int32.MinValue || wantaddr > Int32.MaxValue)
                        {
                            throw new Exception("addr jump is too far.");
                        }
                        var bts = BitConverter.GetBytes((int)wantaddr);
                        c.bytes[^4] = bts[0];
                        c.bytes[^3] = bts[1];
                        c.bytes[^2] = bts[2];
                        c.bytes[^1] = bts[3];
                    }
                    else if (c.bytes.Length == 4)
                    {
                        long wantaddr = (long)addrfunc - c.addr;
                        if (wantaddr < Int32.MinValue || wantaddr > Int32.MaxValue)
                        {
                            throw new Exception("addr jump is too far.");
                        }
                        c.bytes = BitConverter.GetBytes((int)wantaddr);
                    }
                    else
                    {
                        throw new Exception("not have right fill bytes");
                    }
                    c.needfixfunc = false;
                }
            }
        }

        private void FillMethod(ILMethod from, NeoMethod to, bool withReturn)
        {
            int skipcount = 0;
            foreach (var src in from.body_Codes.Values)
            {
                if (skipcount > 0)
                {
                    skipcount--;
                }
                else
                {
                    //Need clear arguments before return
                    if (src.code == CodeEx.Ret)//before return
                    {
                        if (!withReturn) break;
                    }
                    try
                    {
                        skipcount = ConvertCode(from, src, to);
                    }
                    catch (Exception err)
                    {
                        throw new Exception("error:" + from.method.FullName + "::" + src, err);
                    }
                }
            }

            ConvertAddrInMethod(to);
        }

        private void ConvertMethod(ILMethod from, NeoMethod to)
        {
            this.addr = 0;
            this.addrconv.Clear();

            // Insert a code that record the depth
            InsertBeginCode(from, to);

            FillMethod(from, to, true);
        }

        private readonly Dictionary<int, int> addrconv = new Dictionary<int, int>();
        private int addr = 0;
        private int ldloca_slot = -1;

        static int GetNumber(NeoCode code)
        {
            if (code.code <= VM.OpCode.PUSHINT256)
                return (int)new BigInteger(code.bytes);

            else if (code.code == VM.OpCode.PUSHM1) return -1;
            else if (code.code == VM.OpCode.PUSH0) return 0;
            else if (code.code == VM.OpCode.PUSH1) return 1;
            else if (code.code == VM.OpCode.PUSH2) return 2;
            else if (code.code == VM.OpCode.PUSH3) return 3;
            else if (code.code == VM.OpCode.PUSH4) return 4;
            else if (code.code == VM.OpCode.PUSH5) return 5;
            else if (code.code == VM.OpCode.PUSH6) return 6;
            else if (code.code == VM.OpCode.PUSH7) return 7;
            else if (code.code == VM.OpCode.PUSH8) return 8;
            else if (code.code == VM.OpCode.PUSH9) return 9;
            else if (code.code == VM.OpCode.PUSH10) return 10;
            else if (code.code == VM.OpCode.PUSH11) return 11;
            else if (code.code == VM.OpCode.PUSH12) return 12;
            else if (code.code == VM.OpCode.PUSH13) return 13;
            else if (code.code == VM.OpCode.PUSH14) return 14;
            else if (code.code == VM.OpCode.PUSH15) return 15;
            else if (code.code == VM.OpCode.PUSH16) return 16;
            else if (code.code == VM.OpCode.PUSHDATA1) return Pushdata1bytes2int(code.bytes);
            else
                throw new Exception("not support getNumber From this:" + code.ToString());
        }

        static int Pushdata1bytes2int(byte[] data)
        {
            byte[] target = new byte[4];
            for (var i = 1; i < data.Length; i++)
                target[i - 1] = data[i];
            var n = BitConverter.ToInt32(target, 0);
            return n;
        }

        private void ConvertAddrInMethod(NeoMethod to)
        {
            foreach (var c in to.body_Codes.Values)
            {
                if (c.needfix)
                {
                    try
                    {
                        var _addr = addrconv[c.srcaddr];
                        Int32 addroff = (Int32)(_addr - c.addr);
                        c.bytes = BitConverter.GetBytes(addroff);
                        c.needfix = false;
                    }
                    catch
                    {
                        throw new Exception("cannot convert addr in: " + to.name + "\r\n");
                    }
                }
            }
        }

        private int ConvertCode(ILMethod method, OpCode src, NeoMethod to)
        {
            int skipcount = 0;
            switch (src.code)
            {
                case CodeEx.Nop:
                    Convert1by1(VM.OpCode.NOP, src, to);
                    break;
                case CodeEx.Ret:
                    //return was handled outside
                    Insert1(VM.OpCode.RET, null, to);
                    break;
                case CodeEx.Pop:
                    Convert1by1(VM.OpCode.DROP, src, to);
                    break;

                case CodeEx.Ldnull:
                    Convert1by1(VM.OpCode.PUSHNULL, src, to);
                    break;

                case CodeEx.Ldc_I4:
                case CodeEx.Ldc_I4_S:
                    skipcount = ConvertPushI4WithConv(method, src.tokenI32, src, to);
                    break;
                case CodeEx.Ldc_I4_0:
                    ConvertPushNumber(0, src, to);
                    break;
                case CodeEx.Ldc_I4_1:
                    ConvertPushNumber(1, src, to);
                    break;
                case CodeEx.Ldc_I4_2:
                    ConvertPushNumber(2, src, to);
                    break;
                case CodeEx.Ldc_I4_3:
                    ConvertPushNumber(3, src, to);
                    break;
                case CodeEx.Ldc_I4_4:
                    ConvertPushNumber(4, src, to);
                    break;
                case CodeEx.Ldc_I4_5:
                    ConvertPushNumber(5, src, to);
                    break;
                case CodeEx.Ldc_I4_6:
                    ConvertPushNumber(6, src, to);
                    break;
                case CodeEx.Ldc_I4_7:
                    ConvertPushNumber(7, src, to);
                    break;
                case CodeEx.Ldc_I4_8:
                    ConvertPushNumber(8, src, to);
                    break;
                case CodeEx.Ldc_I4_M1:
                    skipcount = ConvertPushI4WithConv(method, -1, src, to);
                    break;
                case CodeEx.Ldc_I8:
                    skipcount = ConvertPushI8WithConv(method, src.tokenI64, src, to);
                    break;
                case CodeEx.Ldstr:
                    ConvertPushString(src.tokenStr, src, to);
                    break;
                case CodeEx.Stloc_0:
                    ConvertStLoc(src, to, 0);
                    break;
                case CodeEx.Stloc_1:
                    ConvertStLoc(src, to, 1);
                    break;
                case CodeEx.Stloc_2:
                    ConvertStLoc(src, to, 2);
                    break;
                case CodeEx.Stloc_3:
                    ConvertStLoc(src, to, 3);
                    break;
                case CodeEx.Stloc_S:
                    ConvertStLoc(src, to, src.tokenI32);
                    break;

                case CodeEx.Ldloc_0:
                    ConvertLdLoc(src, to, 0);
                    break;
                case CodeEx.Ldloc_1:
                    ConvertLdLoc(src, to, 1);
                    break;
                case CodeEx.Ldloc_2:
                    ConvertLdLoc(src, to, 2);
                    break;
                case CodeEx.Ldloc_3:
                    ConvertLdLoc(src, to, 3);
                    break;
                case CodeEx.Ldloc_S:
                    ConvertLdLoc(src, to, src.tokenI32);
                    break;

                case CodeEx.Ldarg_0:
                    ConvertLdArg(method, src, to, 0);
                    break;
                case CodeEx.Ldarg_1:
                    ConvertLdArg(method, src, to, 1);
                    break;
                case CodeEx.Ldarg_2:
                    ConvertLdArg(method, src, to, 2);
                    break;
                case CodeEx.Ldarg_3:
                    ConvertLdArg(method, src, to, 3);
                    break;
                case CodeEx.Ldarg_S:
                case CodeEx.Ldarg:
                case CodeEx.Ldarga:
                case CodeEx.Ldarga_S:
                    ConvertLdArg(method, src, to, src.tokenI32);
                    break;

                case CodeEx.Starg_S:
                case CodeEx.Starg:
                    ConvertStArg(src, to, src.tokenI32);
                    break;
                // Address convert required
                case CodeEx.Br:
                case CodeEx.Br_S:
                case CodeEx.Leave:
                case CodeEx.Leave_S:
                    {
                        var code = Convert1by1(VM.OpCode.JMP_L, src, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }

                    break;
                case CodeEx.Switch:
                    {
                        throw new Exception("need neo.VM update.");
                        //var addrdata = new byte[src.tokenAddr_Switch.Length * 2 + 2];
                        //var shortaddrcount = (UInt16)src.tokenAddr_Switch.Length;
                        //var data = BitConverter.GetBytes(shortaddrcount);
                        //addrdata[0] = data[0];
                        //addrdata[1] = data[1];
                        //var code = _Convert1by1(VM.OpCode.SWITCH, src, to, addrdata);
                        //code.needfix = true;
                        //code.srcaddrswitch = new int[shortaddrcount];
                        //for (var i = 0; i < shortaddrcount; i++)
                        //{
                        //    code.srcaddrswitch[i] = src.tokenAddr_Switch[i];
                        //}
                    }
                case CodeEx.Brtrue:
                case CodeEx.Brtrue_S:
                    {
                        var code = Convert1by1(VM.OpCode.JMPIF_L, src, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Brfalse:
                case CodeEx.Brfalse_S:
                    {
                        var code = Convert1by1(VM.OpCode.JMPIFNOT_L, src, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Beq:
                case CodeEx.Beq_S:
                    {
                        Convert1by1(VM.OpCode.NUMEQUAL, src, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Bne_Un:
                case CodeEx.Bne_Un_S:
                    {
                        Convert1by1(VM.OpCode.ABS, src, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.ABS, null, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.NUMNOTEQUAL, null, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Blt:
                case CodeEx.Blt_S:
                    {
                        Convert1by1(VM.OpCode.LT, src, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Blt_Un:
                case CodeEx.Blt_Un_S:
                    {
                        Convert1by1(VM.OpCode.ABS, src, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.ABS, null, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.LT, null, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Ble:
                case CodeEx.Ble_S:
                    {
                        Convert1by1(VM.OpCode.LE, src, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Ble_Un:
                case CodeEx.Ble_Un_S:
                    {
                        Convert1by1(VM.OpCode.ABS, src, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.ABS, null, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.LE, null, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Bgt:
                case CodeEx.Bgt_S:
                    {
                        Convert1by1(VM.OpCode.GT, src, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Bgt_Un:
                case CodeEx.Bgt_Un_S:
                    {
                        Convert1by1(VM.OpCode.ABS, src, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.ABS, null, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.GT, null, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Bge:
                case CodeEx.Bge_S:
                    {

                        Convert1by1(VM.OpCode.GE, src, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;
                case CodeEx.Bge_Un:
                case CodeEx.Bge_Un_S:
                    {
                        Convert1by1(VM.OpCode.ABS, src, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.ABS, null, to);
                        Convert1by1(VM.OpCode.SWAP, null, to);
                        Convert1by1(VM.OpCode.GE, null, to);
                        var code = Convert1by1(VM.OpCode.JMPIF_L, null, to, new byte[] { 0, 0, 0, 0 });
                        code.needfix = true;
                        code.srcaddr = src.tokenAddr_Index;
                    }
                    break;

                //Stack
                case CodeEx.Dup:
                    Convert1by1(VM.OpCode.DUP, src, to);
                    break;

                //Bitwise logic
                case CodeEx.And:
                    Convert1by1(VM.OpCode.AND, src, to);
                    break;
                case CodeEx.Or:
                    Convert1by1(VM.OpCode.OR, src, to);
                    break;
                case CodeEx.Xor:
                    Convert1by1(VM.OpCode.XOR, src, to);
                    break;
                case CodeEx.Not:
                    Convert1by1(VM.OpCode.INVERT, src, to);
                    break;

                //math
                case CodeEx.Add:
                case CodeEx.Add_Ovf:
                case CodeEx.Add_Ovf_Un:
                    Convert1by1(VM.OpCode.ADD, src, to);
                    break;
                case CodeEx.Sub:
                case CodeEx.Sub_Ovf:
                case CodeEx.Sub_Ovf_Un:
                    Convert1by1(VM.OpCode.SUB, src, to);
                    break;
                case CodeEx.Mul:
                case CodeEx.Mul_Ovf:
                case CodeEx.Mul_Ovf_Un:
                    Convert1by1(VM.OpCode.MUL, src, to);
                    break;
                case CodeEx.Div:
                case CodeEx.Div_Un:
                    Convert1by1(VM.OpCode.DIV, src, to);
                    break;
                case CodeEx.Rem:
                case CodeEx.Rem_Un:
                    Convert1by1(VM.OpCode.MOD, src, to);
                    break;
                case CodeEx.Neg:
                    Convert1by1(VM.OpCode.NEGATE, src, to);
                    break;
                case CodeEx.Shl:
                    Convert1by1(VM.OpCode.SHL, src, to);
                    break;
                case CodeEx.Shr:
                case CodeEx.Shr_Un:
                    Convert1by1(VM.OpCode.SHR, src, to);
                    break;

                //logic
                case CodeEx.Clt:
                case CodeEx.Clt_Un:
                    Convert1by1(VM.OpCode.LT, src, to);
                    break;
                case CodeEx.Cgt:
                case CodeEx.Cgt_Un:
                    skipcount = ConvertCgt(src, to);

                    break;
                case CodeEx.Ceq:
                    skipcount = ConvertCeq(src, to);
                    break;

                //call
                case CodeEx.Call:
                case CodeEx.Callvirt:
                    {
                        if (src.tokenMethod == "System.UInt32 <PrivateImplementationDetails>::ComputeStringHash(System.String)")
                        {
                            // this method maybe is a tag of switch
                            skipcount = ConvertStringSwitch(method, src, to);
                        }
                        else
                        {
                            ConvertCall(src, to);
                        }
                    }
                    break;

                // Use the previous argument as the array size, then new a array
                case CodeEx.Newarr:
                    skipcount = ConvertNewArr(method, src, to);
                    break;

                //array
                //Intent to use byte[] as array.....
                case CodeEx.Ldelem_U1:
                case CodeEx.Ldelem_I1:
                //_ConvertPush(1, src, to);
                //_Convert1by1(VM.OpCode.SUBSTR, null, to);
                //break;
                //now we can use pickitem for byte[]

                case CodeEx.Ldelem_Any:
                case CodeEx.Ldelem_I:
                //case CodeEx.Ldelem_I1:
                case CodeEx.Ldelem_I2:
                case CodeEx.Ldelem_I4:
                case CodeEx.Ldelem_I8:
                case CodeEx.Ldelem_R4:
                case CodeEx.Ldelem_R8:
                case CodeEx.Ldelem_Ref:
                case CodeEx.Ldelem_U2:
                case CodeEx.Ldelem_U4:
                    Convert1by1(VM.OpCode.PICKITEM, src, to);
                    break;
                case CodeEx.Ldlen:
                    Convert1by1(VM.OpCode.SIZE, src, to);
                    break;

                case CodeEx.Stelem_Any:
                case CodeEx.Stelem_I:
                case CodeEx.Stelem_I1:
                case CodeEx.Stelem_I2:
                case CodeEx.Stelem_I4:
                case CodeEx.Stelem_I8:
                case CodeEx.Stelem_R4:
                case CodeEx.Stelem_R8:
                case CodeEx.Stelem_Ref:
                    Convert1by1(VM.OpCode.SETITEM, src, to);
                    break;

                case CodeEx.Isinst://Support `as` expression
                    break;
                case CodeEx.Castclass:
                    ConvertCastclass(src, to);
                    break;

                case CodeEx.Box:
                case CodeEx.Unbox:
                case CodeEx.Unbox_Any:
                case CodeEx.Break:
                //Maybe we can use these for breakpoint debug
                case CodeEx.Conv_I:
                case CodeEx.Conv_I1:
                case CodeEx.Conv_I2:
                case CodeEx.Conv_I4:
                case CodeEx.Conv_I8:
                case CodeEx.Conv_Ovf_I:
                case CodeEx.Conv_Ovf_I_Un:
                case CodeEx.Conv_Ovf_I1:
                case CodeEx.Conv_Ovf_I1_Un:
                case CodeEx.Conv_Ovf_I2:
                case CodeEx.Conv_Ovf_I2_Un:
                case CodeEx.Conv_Ovf_I4:
                case CodeEx.Conv_Ovf_I4_Un:
                case CodeEx.Conv_Ovf_I8:
                case CodeEx.Conv_Ovf_I8_Un:
                case CodeEx.Conv_Ovf_U:
                case CodeEx.Conv_Ovf_U_Un:
                case CodeEx.Conv_Ovf_U1:
                case CodeEx.Conv_Ovf_U1_Un:
                case CodeEx.Conv_Ovf_U2:
                case CodeEx.Conv_Ovf_U2_Un:
                case CodeEx.Conv_Ovf_U4:
                case CodeEx.Conv_Ovf_U4_Un:
                case CodeEx.Conv_Ovf_U8:
                case CodeEx.Conv_Ovf_U8_Un:
                case CodeEx.Conv_U:
                case CodeEx.Conv_U1:
                case CodeEx.Conv_U2:
                case CodeEx.Conv_U4:
                case CodeEx.Conv_U8:
                    this.addrconv[src.addr] = addr;
                    break;

                ///////////////////////////////////////////////
                // Support for structure
                // Load a reference, but we change to load the value of `pos` position
                case CodeEx.Ldloca:
                case CodeEx.Ldloca_S:
                    ConvertLdLocA(method, src, to, src.tokenI32);
                    break;
                case CodeEx.Initobj:
                    ConvertInitObj(src, to);
                    break;
                case CodeEx.Newobj:
                    ConvertNewObj(method, src, to);
                    break;
                case CodeEx.Stfld:
                    ConvertStfld(src, to);
                    break;
                case CodeEx.Ldfld:
                    ConvertLdfld(src, to);
                    break;

                case CodeEx.Ldsfld:
                    {
                        Convert1by1(VM.OpCode.NOP, src, to);
                        var d = src.tokenUnknown as Mono.Cecil.FieldDefinition;
                        // If readdonly, pull a const value
                        if (
                            ((d.Attributes & Mono.Cecil.FieldAttributes.InitOnly) > 0) &&
                            ((d.Attributes & Mono.Cecil.FieldAttributes.Static) > 0)
                            )
                        {
                            var fname = d.FullName;// d.DeclaringType.FullName + "::" + d.Name;
                            var _src = outModule.staticfieldsWithConstValue[fname];
                            if (_src is byte[])
                            {
                                ConvertPushDataArray((byte[])_src, src, to);
                            }
                            else if (_src is int intsrc)
                            {
                                ConvertPushNumber(intsrc, src, to);
                            }
                            else if (_src is long longsrc)
                            {
                                ConvertPushNumber(longsrc, src, to);
                            }
                            else if (_src is bool bsrc)
                            {
                                ConvertPushBoolean(bsrc, src, to);
                            }
                            else if (_src is string strsrc)
                            {
                                ConvertPushString(strsrc, src, to);
                            }
                            else if (_src is BigInteger bisrc)
                            {
                                ConvertPushNumber(bisrc, src, to);
                            }
                            else if (_src is string[] strArray)
                            {
                                ConvertPushStringArray(strArray, src, to);
                            }
                            else
                            {
                                throw new Exception("not support type Ldsfld\r\n   in: " + to.name + "\r\n");
                            }
                            break;
                        }

                        //If this code was called by event, just find its name
                        if (d.DeclaringType.HasEvents)
                        {
                            foreach (var ev in d.DeclaringType.Events)
                            {
                                if (ev.Name == d.Name && ev.EventType.FullName == d.FieldType.FullName)
                                {

                                    Mono.Collections.Generic.Collection<Mono.Cecil.CustomAttribute> ca = ev.CustomAttributes;
                                    to.lastsfieldname = d.Name;
                                    foreach (var attr in ca)
                                    {
                                        if (attr.AttributeType.Name == "DisplayNameAttribute")
                                        {
                                            to.lastsfieldname = (string)attr.ConstructorArguments[0].Value;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var field = this.outModule.mapFields[d.FullName];
                            Convert1by1(VM.OpCode.LDSFLD, src, to, new byte[] { (byte)field.index });
                        }
                    }
                    break;
                case CodeEx.Stsfld:
                    {
                        var d = src.tokenUnknown as Mono.Cecil.FieldDefinition;
                        var field = this.outModule.mapFields[d.FullName];
                        Convert1by1(VM.OpCode.STSFLD, src, to, new byte[] { (byte)field.index });
                    }
                    break;
                case CodeEx.Throw:
                    {
                        Convert1by1(VM.OpCode.THROW, src, to);//throw suspends the vm
                        break;
                    }
                case CodeEx.Ldftn:
                    {
                        Insert1(VM.OpCode.DROP, "", to); // drop null
                        var c = Convert1by1(VM.OpCode.PUSHA, null, to, new byte[] { 5, 0, 0, 0 });
                        c.needfixfunc = true;
                        c.srcfunc = src.tokenMethod;
                        return 1; // skip create object
                    }
                default:
                    logger.Log("unsupported instruction " + src.code + "\r\n   in: " + to.name + "\r\n");
                    throw new Exception("unsupported instruction " + src.code + "\r\n   in: " + to.name + "\r\n");
            }

            return skipcount;
        }
    }
}
