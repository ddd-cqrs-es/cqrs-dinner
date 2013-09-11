namespace Testing101
{
	public class A
	{
		public B B
		{
			get
			{
				throw new System.NotImplementedException();
			}
			set
			{
			}
		}
	
		 public void Foo(B b){}
	}

	public class B{}
}