using System;

class JTest {
	public static void Main() {
		DivideTester dt;
		int i;
		i = 1;
		while(1 == 1){
			Console.WriteLine("Enter Integer: ");
			i = Console.ReadInt();
			if(i==0){
				break;
			}
			int j;
			j = 1;
			int prime;
			prime = 1;
			while(j < (i/2)){
				int a;
				while(a <= (i/2)){
					if(a*j == i){
						prime = 0;
					}
					a++;
				}
				if(prime == 0){
					break;
				}
				j++;
			}
			if(prime == 0){
				Console.WriteLine("Number is not prime");
			}else{
				Console.WriteLine("Number is prime");
			}
			
		}
	} 
	
}