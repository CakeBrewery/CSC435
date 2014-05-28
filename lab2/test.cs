using LexScanner;
using System.IO;

public class test
{
	public static void Main(string[] args)
	{
	  Parser parser = new Parser();

	  FileStream file = new FileStream(args[0], FileMode.Open);
	  parser.openFile(file); 
	  System.Console.WriteLine("File: " + args[0]);

	  parser.Parse();
	}
}