using System;
using System.IO;
using System.Collections.Generic;


namespace FrontEnd {



public class NsVisitor: Visitor {

    public Stack<CbClass> parents = new Stack<CbClass>();
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
                    NameSpace system = (NameSpace)NameSpace.TopLevelNames.LookUp("System");

                    CbClass obj = (CbClass) system.LookUp("Object");
                    NameSpace.TopLevelNames.AddMember(obj);

                    CbClass st = (CbClass) system.LookUp("String");
                    NameSpace.TopLevelNames.AddMember(st);
                    
                    CbClass console = (CbClass) system.LookUp("Console");
                    NameSpace.TopLevelNames.AddMember(console);

                    CbClass i32 = (CbClass) system.LookUp("Int32");
                    NameSpace.TopLevelNames.AddMember(i32);
                }

                break;
            default:
                break;
        }
    }

    public override void Visit( AST_nonleaf node, object data ) {
        NameSpace toplevel = NameSpace.TopLevelNames;
        int arity = node.NumChildren;

        switch((int)data){
            case CLASSLIST:
                if(node.Tag == NodeType.Class){
                    CbClass parent = null;
                    if (node[1] != null) {
                        parent = (CbClass)toplevel.LookUp(((AST_leaf)node[1]).Sval);
                    }
                    CbClass temp = new CbClass(((AST_leaf)node[0]).Sval, parent);
                    toplevel.AddMember(temp);
                    parents.Push(temp); 
                }

                break;


            case MEMBERLIST:
                CbType type; 
                if(node[0] == null){
                    type = CbType.Void;
                }else{
                    type = node[0].Type;
                }

                IList<CbType> param = new List<CbType>(); 



                if(node.Tag == NodeType.Method){

                    for(int i = 0; i < ((AST_kary)node[2]).NumChildren; i++){
                        Console.WriteLine("test"); 
                        CbType c_type;
                        if(node[2][i] == null){
                            c_type = CbType.Void;
                        }
                        else{
                            c_type = node[2][i].Type;
                        }

                        param.Add(c_type); 
                    }

                    CbMethod new_method = new CbMethod(((AST_leaf)node[1]).Sval, true, type, /*param (this doesn't work for some reason) */ new List<CbType> {CbType.Object});

                    parents.Peek().AddMember(new_method);
                }
                else if(node.Tag == NodeType.Const){
                    CbConst new_const = new CbConst(((AST_leaf)node[1]).Sval, type);
                    parents.Peek().AddMember(new_const);
                }
                break; 

            default:
                break;
        }


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