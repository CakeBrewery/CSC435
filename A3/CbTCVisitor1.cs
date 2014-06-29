/*  CbTCVisitor1.cs

    Fills in Missing Type Information
    
    Based On:

    	Defines a Top-Level Symbol Table Visitor class for the CFlat AST
    
    	Author: Nigel Horspool
    
    	Dates: 2012-2014
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrontEnd {


// Traverses the AST to add top-level names into the top-level
// symbol table.
// Incomplete type descriptions of new classes are createdn too,
// these descriptions specify only the parent class and the names
// of members (but each is associated with a minimal field, method
// or const type description as appropriate).
public class TCVisitor1: Visitor {
    private Dictionary<string, AST> pendingClassDefns = null;

    // constructor
    public TCVisitor1( ) {
    }

	public override void Visit(AST_kary node, object data) {
	    Dictionary<string, AST> savedList;
        switch(node.Tag) {
        case NodeType.UsingList: //Do Nothing
	        Debug.Assert(data != null && data is NameSpace);
            break;
        case NodeType.ClassList:
	        Debug.Assert(data != null && data is NameSpace);
            // add each class to the current namespace, by continuing traversal
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
            }
            break;
        case NodeType.MemberList:
	        Debug.Assert(data != null && data is CbClass);
            // add each member to the current class, by continuing traversal
            for(int i=0; i<node.NumChildren; i++) {
                node[i].Accept(this, data);
            }
            break;
        default:
            throw new Exception("Unexpected tag: "+node.Tag);
        }
    }

	public override void Visit( AST_nonleaf node, object data ) {
        switch(node.Tag) {
        case NodeType.Program: //Modified in-line with hints
            Debug.Assert(data != null && data is NameSpace);
            node[1].Accept(this, data);  // visit class declarations
            break;
        case NodeType.Class: //Modified in-line with hints
            Debug.Assert(data != null && data is NameSpace);
            NameSpace ns = (NameSpace)data;
            AST_kary memberList = node[2] as AST_kary;
            // Look up CbType type description   
            AST_leaf classNameId = node[0] as AST_leaf;
            string className = classNameId.Sval;
            object ctd = ns.LookUp(className);
            Debug.Assert(ctd is CbClass);
            CbClass classTypeDefn = (CbClass)ctd;
 	    // Visit each member of the class, passing CbType as a parameter 
            for(int i=0; i<memberList.NumChildren; i++) {
                memberList[i].Accept(this,classTypeDefn);
            }
            break;
        case NodeType.Const: //Modified in-line with hints
	    Debug.Assert(data != null && data is CbClass);
	    CbClass c1 = (CbClass)data;
            // find const in class
            AST_leaf cid = (AST_leaf)(node[1]);
            CbConst cdef = c1.FindMember(cid.Sval);
            if(cdef != null){
            	cdef.Type = node[0].Type;
            }
            break;
        case NodeType.Field:
	    Debug.Assert(data != null && data is CbClass);
	    CbClass c1 = (CbClass)data;
            // find field in class
            AST_leaf cid = (AST_leaf)(node[1]); //FIXME: Node is in fact a list
            CbConst cdef = c1.FindMember(cid.Sval);
            if(cdef != null){
            	cdef.Type = node[0].Type;
            }
            break;
        case NodeType.Method:
	        Debug.Assert(data != null && data is CbClass);
	        CbClass c3 = (CbClass)data;
            // add method name to current class
            AST_leaf mid = (AST_leaf)(node[1]);
            AST attr = node[4];
            CbMethod mdef = new CbMethod(mid.Sval, attr.Tag==NodeType.Static, null, null);
            c3.AddMember(mdef);
            break;
        default:
            throw new Exception("Unexpected tag: "+node.Tag);
        }
    }

	public override void Visit(AST_leaf node, object data) {
	    throw new Exception("TLVisitor traversal should not have reached a leaf node");
    }

    private void openNameSpace( AST ns2open, NameSpace currentNS ) {
        string nsName = ((AST_leaf)ns2open).Sval;
        object r = currentNS.LookUp(nsName);
        if (r == null) {
            Start.SemanticError(ns2open.LineNumber, "namespace {0} not found", nsName);
            return;
        }
        NameSpace c = r as NameSpace;
        if (r == null) {
            Start.SemanticError(ns2open.LineNumber, "{1} is not a namespce", nsName);
            return;
        }
        foreach(object def in c.Members) {
            Debug.Assert(def is CbClass);
            currentNS.AddMember((CbClass)def);
        }
    }
 
}

}
