using System;
using System.IO;
using System.Collections.Generic;


namespace FrontEnd {



public class NsVisitor: Visitor {

    //a stack to keep track of parent classes
    public Stack<CbClass> parents;

    //#define statements
    public const int USINGLIST = 0; 
    public const int CLASSLIST = 1; 
    public const int MEMBERLIST = 2; 
    
    //public static NameSpace TopLevelNames = new NameSpace("");

    public NsVisitor() {
        //Constructor
        parents = new Stack<CbClass>();
    }

    public override void Visit(AST_kary node, object data) {
        int arity = node.NumChildren;
        for( int i = 0; i < arity; i++ ) {
            AST ch = node[i];
            if (ch != null){
                //Process the nodes according to tags
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

                //Copy contents of System to TopLevelNames
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

                //Store the parameter types of current method
                IList<CbType> param = new List<CbType>(); 

                //If current node is a method
                if(node.Tag == NodeType.Method){
                    for(int i = 0; i < ((AST_kary)node[2]).NumChildren; i++){

                        //Get type of current parameter
                        CbType c_type;
                        if(node[2][i] == null){
                            c_type = CbType.Void;
                        }
                        else{
                            c_type = node[2][i].Type;
                        }

                        //add type to parameter list
                        param.Add(c_type); 
                    }

                    //Create a new method
                    CbMethod new_method = new CbMethod(((AST_leaf)node[1]).Sval, true, type, new List<CbType> {CbType.Object});

                    //This should be the correct way, but it does not work for some reason. 
                    //CbMethod new_method = new CbMethod(((AST_leaf)node[1]).Sval, true, type, param);

                    //add this method to its parent class
                    parents.Peek().AddMember(new_method);
                }

                //Process constant declarations
                else if(node.Tag == NodeType.Const){
                    CbConst new_const = new CbConst(((AST_leaf)node[1]).Sval, type);

                    //add this const to its parent class
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