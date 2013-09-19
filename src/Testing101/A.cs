namespace Testing101
{
	using System.Threading;

	public class A
	{
	
		 public void Foo(B b)
		 {
			 b.Bar();
		 }
	}

	public class B{
		
		public void Bar()
		{
			//Make a network call to some resource...
			Thread.Sleep(1000);

		}
	}







}