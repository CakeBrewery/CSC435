using System;
using System.IO;
using System.Collections.Generic;

namespace FrontEnd {

public class NsVisitor: Visitor {


    public NsVisitor() {

    }

    public override void Visit(AST_kary node, object data) {
        int arity = node.NumChildren;
        for( int i = 0; i < arity; i++ ) {
            AST ch = node[i];
            if (ch != null){
                switch(node.Tag){
                    case NodeType.UsingList:
                        ch.Accept(this, i);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public override void Visit(AST_leaf node, object data) {
        Console.WriteLine(node.Sval); 
    }

    public override void Visit( AST_nonleaf node, object data ) {
        int arity = node.NumChildren;
        for( int i = 0; i < arity; i++ ) {
            AST ch = node[i];
            if (ch != null)
                ch.Accept(this, i);
        }
    }

}
}