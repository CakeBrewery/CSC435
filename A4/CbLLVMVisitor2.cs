/*  CbLLVMVisitor2.cs

    Second stage of LLVM intermediate code generation
    
    This generates code for every method in the program.
    
    Author: Nigel Horspool
    
    Dates: 2012-2014
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrontEnd {


public class LLVMVisitor2: Visitor {
    NameSpace ns;
    CbClass currentClass;
    CbMethod currentMethod;
    LLVM llvm;
    SymTab sy;
    IList<string> LoopLabels;
    CbMethod writeMethod, writeLineMethod, readLineMethod, parseMethod;
    LLVMValue lastValueLocation;  // used to remember where a visit left a value
    SymTabEntry lastLocalVariable;
    string lastBBLabel; // used to remember label on last basic block processed
    LLVMValue thisPointer;
    Stack<LLVMValue> actualParameters;

    // constructor
    public LLVMVisitor2( LLVM llvm ) {
        ns = NameSpace.TopLevelNames;  // get the top-level namespace
        currentClass = null;
        currentMethod = null;
        this.llvm = llvm;
        sy = new SymTab();
        LoopLabels = new List<string>();
        NameSpace sys = ns.LookUp("System") as NameSpace;
        CbClass c = sys.LookUp("Console") as CbClass;
        writeMethod = c.Members["Write"] as CbMethod;
        writeLineMethod = c.Members["WriteLine"] as CbMethod;
        readLineMethod = c.Members["ReadLine"] as CbMethod;
        c = sys.LookUp("Int32") as CbClass;
        parseMethod = c.Members["Parse"] as CbMethod;
        actualParameters = new Stack<LLVMValue>();
        lastValueLocation = null;
        lastLocalVariable = null;
        thisPointer = null;
        lastBBLabel = null;
    }

	public override void Visit(AST_kary node, object data) {
        switch(node.Tag) {
        case NodeType.ClassList:
            // visit each class declared in the program
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
            }
            break;
        case NodeType.MemberList:
            // visit each member of the current class
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
            }
            break;
        case NodeType.Block:
            sy.Enter();
            // translate each statement or local declaration
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
            }
            sy.Exit();
            lastValueLocation = null;
            break;
        case NodeType.ActualList:
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
                actualParameters.Push(lastValueLocation); // remember the argument
            }
            break;
        }
    }

	public override void Visit( AST_nonleaf node, object data ) {
	    LLVMValue savedValue;
        switch(node.Tag) {
        case NodeType.Program:
            node[1].Accept(this, data);  // visit class declarations
            break;
        case NodeType.Class:
            string className = ((AST_leaf)node[0]).Sval;
            currentClass = ns.LookUp(className) as CbClass;
            thisPointer = new LLVMValue(llvm.GetTypeDescr(currentClass), "%this", false);
            // now visit the class's members -- only methods & consts matter
            AST_kary memberList = node[2] as AST_kary;
            for(int i=0; i<memberList.NumChildren; i++) {
                memberList[i].Accept(this,data);
            }
            currentClass = null;
            thisPointer = null;
            break;
        case NodeType.Const:
            // already processed in first pass
            break;
        case NodeType.Field:
            break;
        case NodeType.Method:
            // get the method's type description
            string methname = ((AST_leaf)(node[1])).Sval;
            currentMethod = currentClass.Members[methname] as CbMethod;
            sy.Empty();
            // add each formal parameter to the symbol table
            AST_kary formals = (AST_kary)node[2];
            for(int i=0; i<formals.NumChildren; i++) {
                AST_nonleaf formal = (AST_nonleaf)formals[i];
                string name = ((AST_leaf)formal[1]).Sval;
                SymTabEntry se = sy.Binding(name, formal[1].LineNumber);
                se.Type = formal[0].Type;
                se.SSAName = "%" + name;
            }
            sy.Enter();
            // generate code for the method start
            llvm.WriteMethodStart(currentClass, currentMethod, node);
            // now generate code for the method body
            lastBBLabel = "entry";
            llvm.WriteLabel(lastBBLabel);
            node[3].Accept(this,data);
            // generate code to terminate the method
            llvm.WriteMethodEnd(currentMethod);
            currentMethod = null;
            lastBBLabel = null;
            break;
        case NodeType.LocalDecl:
            AST_kary locals = node[1] as AST_kary;
            for(int i=0; i<locals.NumChildren; i++) {
                AST_leaf local = locals[i] as AST_leaf;
                SymTabEntry en = sy.Binding(local.Sval, local.LineNumber);
                en.Type = node[0].Type;
                en.SSAName = "%" + local.Sval;
            }
            break;
        case NodeType.Assign:
            node[0].Accept(this,data);
            savedValue = lastValueLocation;
            SymTabEntry savedDest = lastLocalVariable;
            node[1].Accept(this,data);
            if (savedValue.IsReference)
            {
                llvm.Store(lastValueLocation, savedValue);
            } 
            else
            {  // it was a local variable on the LHS
               // we generate no code, just remember a new name for that LHS
                lastValueLocation = llvm.Coerce(lastValueLocation, node[1].Type, node[0].Type);
                savedDest.SSAName = lastValueLocation.LLValue;
                lastLocalVariable = null;
            }
            lastValueLocation = null;
            break;
        case NodeType.If:
            string TL = llvm.CreateBBLabel("iftrue");
            string FL = llvm.CreateBBLabel("ifelse");
            string JL = llvm.CreateBBLabel("ifend");
            node[0].Accept(this,data);
            llvm.WriteCondBranch(lastValueLocation, TL, FL);
            SymTab syCopy = sy.Clone();
            llvm.WriteLabel(TL);
            lastBBLabel = TL;
            node[1].Accept(this,data);
            string thenEnd = lastBBLabel;
            llvm.WriteBranch(JL);
            llvm.WriteLabel(FL);
            lastBBLabel = FL;
            SymTab sySaved = sy;
            sy = syCopy;
            node[2].Accept(this,data);
            string elseEnd = lastBBLabel;
            llvm.WriteBranch(JL);
            llvm.WriteLabel(JL);
            lastBBLabel = JL;
            sy = llvm.Join(thenEnd, sySaved, elseEnd, sy);
            lastValueLocation = null;
            break;
        case NodeType.While:
            /*  TODO
            node[0].Accept(this,data);
            node[1].Accept(this,data);
            */
            lastValueLocation = null;
            break;
        case NodeType.Return:
            if (node[0] == null) {
                llvm.WriteInst("ret void");
            } else {
                node[0].Accept(this,data);
                savedValue = llvm.Coerce(lastValueLocation, node[0].Type, currentMethod.ResultType);
                llvm.WriteInst("ret {0}", savedValue);
            }
            lastValueLocation  = null;
            break;
        case NodeType.Call:
            node[0].Accept(this,data); // method name (could be a dotted expression)
            savedValue = lastValueLocation;
            node[1].Accept(this,data); // actual parameters
            CbMethodType m = node[0].Type as CbMethodType;
            // We have several special cases for the supported API methods
            if (m.Method == writeMethod || m.Method == writeLineMethod) {
                string name;
                CbType argt = node[1][0].Type;
                savedValue = actualParameters.Pop();
                if (m.Method == writeLineMethod) {
                    if (argt == CbType.Int) name = "@Console.WriteLineInt";
                    else if (argt == CbType.Char) name = "@Console.WriteLineChar";
                    else name = "@Console.WriteLineString";
                } else {
                    if (argt == CbType.Int) name = "@Console.WriteInt";
                    else if (argt == CbType.Char) name = "@Console.WriteChar";
                    else name = "@Console.WriteString";
                }
                if (argt == CbType.Int || argt == CbType.Char)
                    savedValue = llvm.Dereference(savedValue);
                llvm.CallBuiltInMethod(CbType.Void, name, savedValue);
            } else
            if (m.Method == readLineMethod) {
                lastValueLocation = llvm.CallBuiltInMethod(CbType.String, "@Console.ReadLine", null);
            } else
            if (m.Method == parseMethod) {
                lastValueLocation = llvm.CallBuiltInMethod(CbType.Int, "@Int32.Parse", actualParameters.Pop());
            } else {
                // and here is where we handle a call to a method in the Cb program
                int nc = node[1].NumChildren;
                LLVMValue[] actuals = new LLVMValue[nc];
                while(nc-- > 0) {
                    LLVMValue v = actualParameters.Pop();
                    v = llvm.Coerce(v, node[1][nc].Type, m.Method.ArgType[nc]);
                    actuals[nc] = v;
                }
                if (m.Method.IsStatic)
                    lastValueLocation = llvm.CallStaticMethod(m.Method, actuals);
                else
                    lastValueLocation = llvm.CallVirtualMethod(m.Method, savedValue, actuals);
            }
            break;
        case NodeType.Dot:
            node[0].Accept(this,data);
            string rhs = ((AST_leaf)node[1]).Sval;
            if (node.Kind == CbKind.Variable) {
                // access a field with name rhs
                CbClass lhstype = node[0].Type as CbClass;
                while (lhstype != null)
                {
                    CbMember mem = null;
                    if (lhstype.Members.TryGetValue(rhs, out mem))
                    {
                        CbField fld = mem as CbField;
                        // it's an instance field
                        lastValueLocation = llvm.RefClassField(lastValueLocation, fld);
                        break;
                    }
                    lhstype = lhstype.Parent;
                }
            } else
            if (node.Kind == CbKind.ClassName) {
                // do nothing
            } else if (node[0].Type is CFArray && rhs == "Length") {
            } else if (node[0].Type == CbType.String && rhs == "Length") {
            } else {
                // node.Kind == CbKind.None, it's a const or a method
                CbClass lhstype = node[0].Type as CbClass;
                CbConst mem = lhstype.Members[rhs] as CbConst;
                if (mem != null) {
                    string name = "@"+lhstype.Name+"."+rhs;
                    lastValueLocation = new LLVMValue(llvm.GetTypeDescr(node.Type),name,true);
                }
            }
            break;
        case NodeType.Cast:
            node[1].Accept(this,data);
            lastValueLocation = llvm.Coerce(lastValueLocation, node[1].Type, node.Type);
            break;
        case NodeType.NewArray:
            node[1].Accept(this,data);
            // TODO
            break;
        case NodeType.NewClass:
            lastValueLocation = llvm.NewClassInstance((CbClass)(node[0].Type));
            break;
        case NodeType.PlusPlus:
        case NodeType.MinusMinus:
            node[0].Accept(this,data);
            // TODO 
            lastValueLocation = null;
            break;
        case NodeType.UnaryPlus:
        case NodeType.UnaryMinus:
            node[0].Accept(this,data);
            // TODO
            break;
        case NodeType.Index:
            node[0].Accept(this,data);
            savedValue = lastValueLocation;
            node[1].Accept(this,data);
            lastValueLocation = llvm.ForceIntValue(lastValueLocation);
            // TODO
            break;
        case NodeType.Add:
        case NodeType.Sub:
        case NodeType.Mul:
        case NodeType.Div:
        case NodeType.Mod:
            node[0].Accept(this,data);
            savedValue = llvm.ForceIntValue(lastValueLocation);
            node[1].Accept(this,data);
            lastValueLocation = llvm.ForceIntValue(lastValueLocation);
            lastValueLocation = llvm.WriteIntInst(node.Tag, savedValue, lastValueLocation);
            break;
        case NodeType.Equals:
        case NodeType.NotEquals:
        case NodeType.LessThan:
        case NodeType.GreaterThan:
        case NodeType.LessOrEqual:
        case NodeType.GreaterOrEqual:
            node[0].Accept(this,data);
            savedValue = lastValueLocation;
            node[1].Accept(this,data);
            node.Type = CbType.Bool;
            lastValueLocation = llvm.WriteCompInst(node.Tag, savedValue, lastValueLocation);
            break;
        case NodeType.And:
        case NodeType.Or:
            node[0].Accept(this,data);
            savedValue = lastValueLocation;
            node[1].Accept(this,data);
            // TODO
            break;
        default:
            throw new Exception("Unexpected tag: "+node.Tag);  
        }
    }

	public override void Visit(AST_leaf node, object data) {
	    switch(node.Tag) {
        case NodeType.IntConst:
        case NodeType.CharConst:
            lastValueLocation = llvm.GetIntVal(node);
            break;
        case NodeType.StringConst:
            lastValueLocation = llvm.WriteStringConstant(node);
            break;
        case NodeType.Ident:
            string name = node.Sval;
            SymTabEntry local = sy.LookUp(name);
            if (local != null) {
                lastValueLocation = new LLVMValue(llvm.GetTypeDescr(local.Type), local.SSAName, false);
                lastLocalVariable = local;
                return;
            }
            CbMember mem;
            CbClass c = currentClass;
            while (c != null)
            {
                if (c.Members.TryGetValue(name, out mem))
                {
                    // found in class c
                    node.Type = mem.Type;
                    if (mem is CbField) {
                        lastValueLocation = llvm.RefClassField(thisPointer, (CbField)mem);
                    }
                    if (mem is CbConst) {
                        lastValueLocation = llvm.AccessClassConstant((CbConst)mem);
                    }
                    return;  // it must be a method name?
                }
                c = c.Parent;
            }
            // it's a class name or namespace name
            break;
        case NodeType.Break:
            // TODO
            break;
        case NodeType.Null:
        case NodeType.Empty:
        case NodeType.IntType:
        case NodeType.CharType:
        case NodeType.StringType:
            break;
        default:
            throw new Exception("Unexpected tag: "+node.Tag);
        }
    }

}

}
