using System;
using System.IO;
using System.Collections.Generic;


namespace FrontEnd {

public class NsVisitor: Visitor {

    public const int USINGLIST = 0; 
    public const int CLASSLIST = 1; 
    public const int MEMBERLIST = 2; 
    
    //public static NameSpace TopLevelNames = new NameSpace("");

    public NsVisitor() {

    }

    public override void Visit(AST_kary node, object data) {
        int arity = node.NumChildren;
        for( int i = 0; i < arity; i++ ) {
            AST ch = node[i];
            if (ch != null){
                switch(node.Tag){
                    case NodeType.UsingList:
                        ch.Accept(this, USINGLIST);
                        break;
                    case NodeType.ClassList:
                        ch.Accept(this, CLASSLIST); 
                        break; 
                    case NodeType.MemberList:
                        ch.Accept(this, MEMBERLIST); 
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public override void Visit(AST_leaf node, object data) {
        switch((int)data){

            case USINGLIST:
                if(node.Sval == "System"){
                    moveToTopLevel(node.Sval); 
                }else{
                    moveToTopLevel(node.Sval); 
                }

                break;
            default:
                break;
        }
    }

    public override void Visit( AST_nonleaf node, object data ) {
        int arity = node.NumChildren;
        for( int i = 0; i < arity; i++ ) {
            AST ch = node[i];
            if (ch != null)
                ch.Accept(this, -1);
        }
    }


    public void moveToTopLevel(String s){
        //I don't know what to do here. 
        NameSpace temp = new NameSpace(s);
        NameSpace.TopLevelNames.AddMember(temp);
        //Console.Print(s);
        //Console.Print('Added to Namespace');
    }


}
}