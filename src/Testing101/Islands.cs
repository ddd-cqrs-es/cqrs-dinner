namespace Testing101
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class Island_tests
	{
		[Test]
		public void When_doing_some_hardcore_computation_with_the_real_data_the_result_is_5832()
		{
			// We are testing the algorithm with unknown state
			var actual = Island.SomeHardCoreComputation();

			Assert.That(actual, Is.EqualTo(5832.0d));
		}

		[Test]
		public void When_doing_some_hardcore_computation_with_some_fake_data_the_result_is_5832()
		{
			 // We are actually testing the algorithm with known state
			 var actual = Island.SomeHardCoreComputation(new TestData());
			 
			 Assert.That(actual, Is.EqualTo(5832.0d));
		}

		[Test]
		public void When_stroring_with_the_real_thing_how_can_i_know_what_happened()
		{
			Island.StoringABit(10);

			//I would have to query a db somewhere
		}

		[Test]
		public void When_stroring_with_the_fake_it_is_easy_to_know_what_happened()
		{
			var storage = new FakeStorage();
			Island.StoringABit(10, storage);
			Island.StoringABit(11, storage);

			Assert.That(storage.Stored[0], Is.EqualTo(10));
			Assert.That(storage.Stored[1], Is.EqualTo(-11));

			//I would have to query a db somewhere
		}




		public class TestData: IPovideSomeData
		{
			public double GetIt(int i)
			{
				return Math.Pow(i, i + 1);
			}
		}

		[Test]
		public void When_storing_a_bit_of_information()
		{
			// We are testing the algorithm with unknown state
			var actual = Island.SomeHardCoreComputation();

			Assert.That(actual, Is.EqualTo(5832.0d));
		}


	}

	public class FakeStorage : IStorage
	{
		public readonly IList<int> Stored = new List<int>();

		public void Store(int i)
		{
			Stored.Add(i);
		}
	}

	public class Island
	{
		public static double SomeHardCoreComputation()
		{
			var data = new TheRealDataStore();
			var result = 1.0;
			for (int i = 0; i < 10; i++)
			{
				var it = data.GetIt(i % 3) + 1;
				result *= it;
			}
			return result;
		}

		public static double SomeHardCoreComputation(IPovideSomeData data)
		{
			var result = 1.0;
			for (int i = 0; i < 10; i++)
			{
				var it = data.GetIt(i % 3) + 1;
				result *= it;
			}
			return result;
		}

		public static void StoringABit(int i)
		{
			var realStorage = new TheRealDataStore();

			if (i%2 == 0)
			{
				realStorage.Store(i);
			}
			else
			{
				realStorage.Store(-i);
			}

		}

		public static void StoringABit(int i, IStorage storage)
		{
			
			if (i % 2 == 0)
			{
				storage.Store(i);
			}
			else
			{
				storage.Store(-i);
			}

		}
	}

	public interface IStorage
	{
		void Store(int i);
	}

	public class TheRealDataStore
	{
		public double GetIt(int i)
		{
			Thread.Sleep(50); // we have to hit the disk somewhere
			return Math.Pow(i, i + 1);
		}

		public void Store(int i)
		{
			Thread.Sleep(50); // we have to hit the disk somewhere

		}
	}

	public interface IPovideSomeData
	{
		double GetIt(int i);
	}
}