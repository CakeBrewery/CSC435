/*  CbTypeCheckVisitor2.cs

    Second stage of full type-checking on AST

    We now visit and type-check all the parts of the AST which were not
    checked in the first stage of full type-checking.
    
    Author: Nigel Horspool
    
    Dates: 2012-2014
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrontEnd {


public class TCVisitor2: Visitor {
    NameSpace ns;  // namespace for all top-level names and names opened with 'using' clauses
    CbClass currentClass;  // current class being checked (null if there isn't one)
    CbMethod currentMethod;  // current method being checked (null if there isn't one)
    SymTab sy;   // one instance of SymTab used for all method body checking
    int loopNesting;  // current depth of nesting of while loops

    // constructor
    public TCVisitor2( ) {
        ns = NameSpace.TopLevelNames;  // get the top-level namespace
        currentMethod = null;
        sy = new SymTab();
        loopNesting = 0;
    }
    
    // Note: the data parameter for the Visit methods is never used
    // It is always null (or whatever is passed on the initial call)

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
            // visit each statement or local declaration
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
            }
            sy.Exit();
            break;
        case NodeType.ActualList:
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
            }
            break;
        }
    }

    public override void Visit( AST_nonleaf node, object data ) {
        switch(node.Tag) {
        case NodeType.Program:
            node[1].Accept(this, data);  // visit class declarations
            break;
        case NodeType.Class:
            AST_leaf classNameId = node[0] as AST_leaf;
            string className = classNameId.Sval;
            currentClass = ns.LookUp(className) as CbClass;
            Debug.Assert(currentClass != null);
            performParentCheck(currentClass,node.LineNumber);  // check Object is ultimate ancestor
            // now check the class's members
            AST_kary memberList = node[2] as AST_kary;
            for(int i=0; i<memberList.NumChildren; i++) {
                memberList[i].Accept(this,data);
            }
            currentClass = null;
            break;
        case NodeType.Const:
            node[2].Accept(this,data);  // get type of value
            if (!isAssignmentCompatible(node[0].Type,node[2].Type))
                Start.SemanticError(node.LineNumber, "invalid initialization for const");
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
                SymTabEntry newBinding = sy.Binding(name, formal[1].LineNumber);
                newBinding.Type = formal[0].Type;
            }
            sy.Enter();
            // now type-check the method body
            node[3].Accept(this,data);
            // finally check that static/virtual/override are used correctly
            checkOverride(node);
            currentMethod = null;
            break;
        case NodeType.LocalDecl:
            node[0].Accept(this,data);  // get type for the locals
            AST_kary locals = node[1] as AST_kary;
            for(int i=0; i<locals.NumChildren; i++) {
                AST_leaf local = locals[i] as AST_leaf;
                string name = local.Sval;
                SymTabEntry en = sy.Binding(name, local.LineNumber);
                en.Type = node[0].Type;
            }
            break;
        case NodeType.Assign:
            node[0].Accept(this,data);
            node[1].Accept(this,data);
            if (node[0].Kind != CbKind.Variable)
                Start.SemanticError(node.LineNumber, "target of assignment is not a variable");
            if (!isAssignmentCompatible(node[0].Type, node[1].Type))
                Start.SemanticError(node.LineNumber, "invalid types in assignment statement");
            break;
        case NodeType.If:
            node[0].Accept(this,data);

            /* (DONE?) */
            if (node[0].Type != CbType.Bool){
                Start.SemanticError(node[0].LineNumber, "invalid if");
                node.Type = CbType.Error; 
            }

            node[1].Accept(this,data);
            node[2].Accept(this,data);
            break;
        case NodeType.While:
            node[0].Accept(this,data);

            /* (DONE?) */
            if(node[0].Type != CbType.Bool){
                Start.SemanticError(node[0].LineNumber, "invalid while"); 
                node.Type = CbType.Error; 
            }

            loopNesting++;
            node[1].Accept(this,data);
            loopNesting--;
            break;
        case NodeType.Return:
            if (node[0] == null) {
                if (currentMethod.ResultType != CbType.Void)
                    Start.SemanticError(node.LineNumber, "missing return value for method");
                break;
            }
            node[0].Accept(this,data);

            /* (DONE?) TODO ... check type of method result */
            if(!isAssignmentCompatible(currentMethod.ResultType, node[0].Type)){
                Start.SemanticError(node.LineNumber, "Invalid return type"); 
            }
            break;
        case NodeType.Call:
            node[0].Accept(this,data); // method name (could be a dotted expression)
            node[1].Accept(this,data); // actual parameters
            /* TODO ... check types */
            
            //Check parameters
            List<CbType> checkList = new List<CbType>();
            
            node.Type = CbType.Error;  // FIX THIS
            break;
        case NodeType.Dot:
            node[0].Accept(this,data);
            string rhs = ((AST_leaf)node[1]).Sval;
            CbClass lhstype = node[0].Type as CbClass;
            if (lhstype != null) {
                // rhs needs to be a member of the lhs class
                CbMember mem;
                if (lhstype.Members.TryGetValue(rhs,out mem)) {
                    node.Type = mem.Type;
                    if (mem is CbField)
                        node.Kind = CbKind.Variable;
                    if (node[0].Kind == CbKind.ClassName) {
                        // mem has to be a static member
                        if (!mem.IsStatic)
                            Start.SemanticError(node[1].LineNumber, "static member required");
                    } else {
                        // mem has to be an instance member
                        if (mem.IsStatic)
                            Start.SemanticError(node[1].LineNumber,
                                "member cannot be accessed via a reference, use classname instead");
                    }
                } else {
                    Start.SemanticError(node[1].LineNumber,
                        "member {0} not found in class {1}", rhs, lhstype.Name);
                    node.Type = CbType.Error;
                }
                break;
            }
            if (rhs == "Length") {
                // lhs has to be an array or a string
                if (node[0].Type != CbType.String && !(node[0].Type is CFArray))
                    Start.SemanticError(node[1].LineNumber, "member Length not found");
                node.Type = CbType.Int;
                break;
            }
            CbNameSpaceContext lhsns = node[0].Type as CbNameSpaceContext;
            if (lhsns != null) {
                lhstype = lhsns.Space.LookUp(rhs) as CbClass;
                if (lhstype != null) {
                    node.Type = lhstype;
                    node.Kind = CbKind.ClassName;
                    break;
                }
            }
            Start.SemanticError(node[1].LineNumber, "member {0} does not exist", rhs);
            node.Type = CbType.Error;
            break;
        case NodeType.Cast:
            checkTypeSyntax(node[0]);
            node[0].Accept(this,data);
            node[1].Accept(this,data);
            if (!isCastable(node[0].Type, node[1].Type))
                Start.SemanticError(node[1].LineNumber, "invalid cast");
            node.Type = node[0].Type;
            break;
        case NodeType.NewArray:
            node[0].Accept(this,data);
            node[1].Accept(this,data);


            /* (DONE?) TODO ... check types */
            //check array type
            if (node[0].Type != CbType.Int && node[0].Type != CbType.Char
                && node[0].Type != CbType.String){
                Start.SemanticError(node[0].LineNumber, "Invalid array type"); 
                node.Type = CbType.Error;
            }

            //check size type
            if(node[1].Type != CbType.Int){
                Start.SemanticError(node[1].LineNumber, "Array size must be an Int");
                node.Type = CbType.Error; 
            }


            else{
              node.Type = CbType.Array(node[0].Type);
            }
            break;
        case NodeType.NewClass:
            node[0].Accept(this,data);


            /* (DONE?) TODO ... check that operand is a class */
            if(!(node[0].Type is CbClass)){
                Start.SemanticError(node[0].LineNumber, "{0} is not a class type");
                node.Type = CbType.Error;
            }

            else{
                node.Type = node[0].Type;
            }
            break;
        case NodeType.PlusPlus:
        case NodeType.MinusMinus:
            node[0].Accept(this,data);

            /* (DONE?) TODO ... check types and operand must be a variable */
            //Make sure it's an integer type
            if(!isIntegerType(node[0].Type)){
                Start.SemanticError(node[0].LineNumber, "decrement and increment can only be done on integer variables"); 
                node.Type = CbType.Error; 
            }

            //make sure it's a variable
            else if(node[0].Kind != CbKind.Variable){
                Start.SemanticError(node[0].LineNumber, "decrement and increment can only be done on variables"); 
                node.Type = CbType.Error; 
            }

            else{
                node.Type = node[0].Type;
            }

            break;
        case NodeType.UnaryPlus:
        case NodeType.UnaryMinus:
            node[0].Accept(this,data);

            /* (DONE?) TODO ... check types */
            if(!isIntegerType(node[0].Type)){
                Start.SemanticError(node[0].LineNumber, "Unary minus can only be done on integers"); 
                node.Type = CbType.Error; 
            }

            else{
                node.Type = CbType.Int;
            }
            break;
        case NodeType.Index:
            node[0].Accept(this,data);
            node[1].Accept(this,data);

            /* TODO ... check types */
            /* ?????? */


            node.Type = CbType.Error;  // FIX THIS
            break;
        case NodeType.Add:
        case NodeType.Sub:
        case NodeType.Mul:
        case NodeType.Div:
        case NodeType.Mod:
            node[0].Accept(this,data);
            node[1].Accept(this,data);


            /* (DONE?) TODO ... check types */
            if(node[0].Type == CbType.Int && node[1].Type == CbType.Int){
                node.Type = CbType.Int; 
            }
            else if(node[0].Type == CbType.Char && node[1].Type == CbType.Char){
                node.Type = CbType.Char; 
            }
            else if(node[0].Type == CbType.Int && node[1].Type == CbType.Char){
                node.Type = CbType.Int; 
            }
            else if(node[0].Type == CbType.Char && node[1].Type == CbType.Int){
                node.Type = CbType.Int; 
            }
            else{
                Start.SemanticError(node[0].LineNumber, "Invalid operator types"); 
                node.Type = CbType.Error; 
            }
            
            break;
        case NodeType.Equals:
        case NodeType.NotEquals:
            node[0].Accept(this,data);
            node[1].Accept(this,data);
            /* (DONE?) TODO ... check types */
            if(node[0].Type == node[1].Type){
                node.Type = CbType.Bool;
            }
            else{
                Start.SemanticError(node[0].LineNumber, "compairson must be between operators of the same type");
            }
            break;
        case NodeType.LessThan:
        case NodeType.GreaterThan:
        case NodeType.LessOrEqual:
        case NodeType.GreaterOrEqual:
            node[0].Accept(this,data);
            node[1].Accept(this,data);
            node.Type = CbType.Bool;
            /* (DONE?) TODO ... check types */
            if(!isIntegerType(node[0].Type) || !isIntegerType(node[1].Type)){
                Start.SemanticError(node[0].LineNumber, "Comparison must be between integer types");
                node.Type = CbType.Error; 
            }
            break;
        case NodeType.And:
        case NodeType.Or:
            node[0].Accept(this,data);
            node[1].Accept(this,data);
            /* (DONE?) TODO ... check types */
            node.Type = CbType.Bool;

            if(node[0].Type != CbType.Bool || node[1].Type != CbType.Bool){
                Start.SemanticError(node[0].LineNumber, "&& and || must be done between boolean exppressions");
                node.Type = CbType.Error; 
            }
            break;
        default:
            throw new Exception("Unexpected tag: "+node.Tag);  
        }
    }

    public override void Visit(AST_leaf node, object data) {
        switch(node.Tag) {
        case NodeType.Ident:
            string name = node.Sval;
            SymTabEntry local = sy.LookUp(name);
            if (local != null) {
                node.Type = local.Type;
                node.Kind = CbKind.Variable;
                return;
            }
            CbMember mem;
            if (currentClass.Members.TryGetValue(name,out mem)) {
                node.Type = mem.Type;
                if (mem is CbField)
                    node.Kind = CbKind.Variable;
                break;
            }
            CbClass t = ns.LookUp(name) as CbClass;
            if (t != null) {
                node.Type = t;
                node.Kind = CbKind.ClassName;
                break;
            }
            NameSpace lhsns = ns.LookUp(name) as NameSpace;
            if (lhsns != null) {
                node.Type = new CbNameSpaceContext(lhsns);
                break;
            }
            node.Type = CbType.Error;;
            Start.SemanticError(node.LineNumber, "{0} is unknown", name);
            break;
        case NodeType.Break:
            if (loopNesting <= 0)
                Start.SemanticError(node.LineNumber, "break can only be used inside a loop");
            break;
        case NodeType.Null:
            node.Type = CbType.Null;
            break;
        case NodeType.IntConst:
            node.Type = CbType.Int;
            break;
        case NodeType.StringConst:
            node.Type = CbType.String;
            break;
        case NodeType.CharConst:
            node.Type = CbType.Char;
            break;
        case NodeType.Empty:
            break;
        case NodeType.IntType:
            node.Type = CbType.Int;
            break;
        case NodeType.CharType:
            node.Type = CbType.Char;
            break;
        case NodeType.StringType:
            node.Type = CbType.String;
            break;
        default:
            throw new Exception("Unexpected tag: "+node.Tag);
        }
    }

    private void performParentCheck(CbClass c, int lineNumber) { //(DONE ?)
        List<String> classPath = new List<String>();
        classPath.Add(c.Name);
        while(c.Name != "Object") {
                c = c.Parent;
                if (c == null) Start.SemanticError(lineNumber, "Class does not have ultimate ancestor Object");
                if (classPath.Contains(c.Name)) Start.SemanticError(lineNumber, "Class inheritance involves cycle");
                classPath.Add(c.Name);
            }
    
        /* TODO
           code to check that c's ultimate ancestor is Object.
           Be careful not to get stuck if the parent relationship
           contains a cycle.
           The lineNumber parameter is used in error messages.
        */
    }
    
    private bool isAssignmentCompatible(CbType dest, CbType src) {
        if (dest == CbType.Error || src == CbType.Error) return true;
        if (dest == src) return true;
        if (dest == CbType.Int) return isIntegerType(src);
        CbClass d = dest as CbClass;
        CbClass s = src as CbClass;
        if (d != null) {
            if (src == CbType.Null) return true;
            if (s == null) return false;
            if (isAncestor(d,s)) return true;
        }
        return false;
    }
    
    private void checkTypeSyntax(AST n) { //(DONE ?)
    	switch(n.Tag){
    	case NodeType.IntType:
    		break;
    	case NodeType.CharType:
    		break;
    	case NodeType.StringType:
    		break;
    	case NodeType.Ident:
    		String name = ((AST_leaf)n).Sval;
    		CbClass t = ns.LookUp(name) as CbClass;
    		if(t == null){
    			Start.SemanticError(n.LineNumber, "Invalid Cast");
    		}
    		break;
    	default:
    		Start.SemanticError(n.LineNumber, "Invalid Cast");     
    		break;
    	}
    }

    private bool isCastable(CbType dest, CbType src) {
        if (isIntegerType(dest) && isIntegerType(src)) return true;
        if (dest == CbType.Error || src == CbType.Error) return true;
        CbClass d = dest as CbClass;
        CbClass s = src as CbClass;
        if (isAncestor(d,s)) return true;
        if (isAncestor(s,d)) return true;
        return false;
    }
    
    // returns true if type t can be used where an integer is needed
    private bool isIntegerType(CbType t) {
        return t == CbType.Int || t == CbType.Char || t == CbType.Error;
    }
    
    // tests if T1 == T2 or T1 is an ancestor of T2 in hierarchy
    private bool isAncestor( CbClass T1, CbClass T2 ) {
        while(T1 != T2) {
            T2 = T2.Parent;
            if (T2 == null) return false;
        }
        return true;
    }
    
    private void checkOverride(AST_nonleaf node) {
        string name = currentMethod.Name;
        // search for a member in any ancestor with same name
        /* TODO
           code to check whether any ancestor class contains a member with
           the same name. If so, it has to be a method with the identical
           signature.
           If there is a method with the same signature, then neither method
           is allowed to be static. (Not part of Cb language.)
           Otherwise, currentMethod must be flagged as override (not virtual).
        */
    }
}

}