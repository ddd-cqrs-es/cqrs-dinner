namespace Dinner.Messaging
{
	using System;

	public class Narrower<TInput, TOutput> : IHandle<TInput>, IEquatable<Narrower<TInput, TOutput>> where TOutput : TInput
	{
		public bool Equals(Narrower<TInput, TOutput> other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return handler.Equals(other.handler);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Narrower<TInput, TOutput>) obj);
		}

		public override int GetHashCode()
		{
			return handler.GetHashCode();
		}

		private readonly IHandle<TOutput> handler;

		public Narrower(IHandle<TOutput> handler)
		{
			this.handler = handler;
		}

		public void Handle(TInput message)
		{
			try
			{
				handler.Handle((TOutput) message);
			}
			catch (InvalidCastException)
			{
				
			}
		}

		private TOutput ChangeType(TInput message)
		{
			try
			{
				return (TOutput) message;
			}
			catch
			{
				return default(TOutput);
			}
		}
	}
}