using LexScanner;
using System.IO;

public class test
{
	public static void Main(string[] args)
	{
	  Parser parser = new Parser();

	  parser.flg_token = false;
	  parser.flg_debug = false;

	  for(int i = 1; i < args.Length; i++){
	  	if(args[i] == "-tokens"){
	  		parser.flg_token = true;
	  		System.Console.WriteLine("Debug: ON");
	  	}
	  	if(args[i] == "-debug"){
	  		parser.flg_debug = true; 
	  		System.Console.WriteLine("Tokens: ON");
	  	}
	  }


	  FileStream file = new FileStream(args[0], FileMode.Open);
	  parser.openFile(file); 
	  System.Console.WriteLine("File: " + args[0]);

	  System.Console.WriteLine("");

	  parser.Parse();

	}
}