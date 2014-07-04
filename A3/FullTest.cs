// test of everything, but without a using clause

class Foo {
    public const int theAnswer = 42;
    public const string hiThere = "hello";
    public const char itsAnX = 'x';

    public int a;
    public string b;
    public char c;

    public static void Main() {
        Foo f;
        f = new Bar();
        int r;
        r = f.Ummm(3,4);
        int x;
        x = (int)r;
        x = (x)r;
        r = f[2];
        c = b[a];
    }

    public virtual int Ummm( int a, int b ) {
        System.Console.WriteLine("This is Foo");
        return a+b;
    }

}


class Bar : Foo {
    public int x;

    public override int Ummm( int aa, int bb ) {
        System.Console.WriteLine("This is Bar");
        return a-b;
    }
    
    public static int Ummm(int a, int b) {
    return a*b;
    }
}
