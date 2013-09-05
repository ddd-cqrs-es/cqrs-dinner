namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;

	public class LazySample
	{
		 
	}

	public class Foo
	{
		public readonly Expensive Expensive = new Expensive();
	
		//or
		private Expensive _expensive;
		private object _expensiveLock= new object();
		public Expensive Exp
		{
			get
			{
				lock (_expensiveLock)
				{
					return _expensive ?? (_expensive = new Expensive());
				}
			}
		}


		private Lazy<Expensive> _lazyExp= new Lazy<Expensive>(()=> new Expensive(), true); 
		public Expensive LazyExp{get { return _lazyExp.Value; }}
	}

	public class Expensive
	{
		public Expensive()
		{
			Thread.Sleep(1000);
		}
	}


}