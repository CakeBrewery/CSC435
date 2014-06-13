using System;
using asdfdsaf;
using asdfadasf;
using adsfadsfdsafdas;
using asdfasdf;

class List {
    public const int x = 3; 
    public List next;
    public virtual void Print() {}
}

class Other: List {
    public char c;
    public override void Print() {
        Console.Write(' ');
        Console.Write(c);
        if (next != null) next.Print();
    }
}

class Digit: List {
    public int d;
    public override void Print() {
        Console.Write(' ');
        Console.Write(d);
        if (next != null) next.Print();
    }
    public override void Add(int i) {}
}

class Lists {
    public static void Main() {
        List ccc;
        string s;
        Console.WriteLine("enter some text => ");
        s = Console.ReadLine();
        ccc = null;
        int i;
        i = 0;
        while(i < s.Length) {
            char ch;
            ch = s[i];  i++;
            List elem;
            if (ch >= '0' && ch <= '9') {
		        Digit elemD;
                elemD = new Digit();  elemD.d = ch - '0';
                elem = elemD;
            } else {
		        Other elemO;
                elemO = new Other();  elemO.c = ch;
                elem = elemO;
            }
            elem.next = ccc;
            ccc = elem;
        }
        Console.WriteLine("\nReversed text =");
        ccc.Print();
        Console.WriteLine("\n");
    }
}
