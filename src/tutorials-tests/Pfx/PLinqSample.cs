namespace tutorials.Pfx
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.NetworkInformation;
	using System.Reflection;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class PLinqSample
	{
		[Test]
		public void Simple_parallel_prime()
		{
			var numbers = Enumerable.Range(3, 1000 - 3);
			var parallelQuery = from n in numbers.AsParallel()
			                    where Enumerable.Range(2, (int) Math.Sqrt(n)).All(i => n%i > 0)
			                    select n;

			int[] primes = parallelQuery.ToArray();

			foreach (var prime in primes)
			{
				Console.Write("{0} ", prime);
			}

		}

		[Test]
		public void As_parallel_out_of_sequence()
		{
			const string alpha = "abcdefgh";
			var pq = from c in alpha.AsParallel() select char.ToUpper(c);
			var uppered = pq.ToArray();
			foreach (var c in uppered)
			{
				Console.Write(c);
			}


		}

		[Test]
		public void Join_on_non_paprallel_squence_throws_exception()
		{
			var alpha = "abc".AsParallel();
			IEnumerable<char> beta = "def".AsEnumerable();

			Assert.Throws<NotSupportedException>(() => alpha.Union(beta));
		}

		[Test]
		public void Exception_in_parallel_processing_is_wrapped_in_aggregate_exception()
		{
			var list = new List<object> {1, 2, null};
			var parallelQuery = from o in list.AsParallel() select o.ToString();

			var aggregateException = Assert.Throws<AggregateException>(() => parallelQuery.ToArray());
			Assert.That(aggregateException.InnerException, Is.InstanceOf<NullReferenceException>());
		}

		[Test]
		public void Spellchecker_example()
		{
			const string wordlookupTxt = "WordLookup.txt";
			if (!File.Exists(wordlookupTxt))
			{
				new WebClient().DownloadFile("http://www.albahari.com/ispell/allwords.txt", wordlookupTxt);
			}

			var lookUp = new HashSet<string>(File.ReadAllLines(wordlookupTxt), StringComparer.CurrentCultureIgnoreCase);

			var random = new Random();

			var wordList = lookUp.ToArray();

			var wordsToTest = Enumerable.Range(0, 1000000).Select(i => wordList[random.Next(0, wordList.Length)]).ToArray();

			wordsToTest[12345] = "woozsh";
			wordsToTest[23456] = "wubsie";

			var query =
				wordsToTest.AsParallel()
				           .Select((word, index) => new IndexedWord {Word = word, Index = index})
				           .Where(iword => !lookUp.Contains(iword.Word))
				           .OrderBy(iword => iword.Index);

			query.Dump();
		}


		[Test]
		public void Parallel_side_effect()
		{
			int i = 0;
			var bad = from n in Enumerable.Range(0, 999).AsParallel() select n*i++;

			var badResults = bad.ToArray();
			foreach (var i1 in badResults)
			{
				Console.Write(i1 + " ");
			}

			var good = Enumerable.Range(0, 999).AsParallel().Select((n, j) => n*j);

			var goodResults = good.ToArray();
			foreach (var i1 in goodResults)
			{
				Console.Write(i1 + " ");
			}
		}

		[Test]
		public void Blocking_calls()
		{
			var query =
				from site in
					new string[]
						{
							"www.albahari.com", "www.linqpad.net", "www.oreilly.com", "www.takeonit.com", "stackoverflow.com",
							"www.rebeccarey.com", "1.1.1.1"
						}.AsParallel().WithDegreeOfParallelism(6) //hint for io intensive call
				let p = new Ping().Send(site)
				select new PingResult{Site= site, Result = p.Status, Time = p.RoundtripTime};

			query.Dump();

			
		}

		[Test]
		public void Cancel_with_break()
		{
			var million = Enumerable.Range(0, 1000000);
			int count = 0;
			var enumerable = million.AsParallel().Select(x =>
				{
					Thread.Sleep(10);
					Interlocked.Increment(ref count);
					return  x ;
				});
			
			foreach (var i in enumerable)
			{
				if (i > 1000) break; //count should be 	
			}

			Console.WriteLine("Count is " + count);
			
			Assert.That(count, Is.GreaterThan(1010));
			Assert.That(count, Is.LessThan(1050));
		}

		[Test]
		public void Cancel_with_token()
		{
			var cancelSource = new CancellationTokenSource();
			var mil = Enumerable.Range(3, 1000000);
			var parallelQuery =
				mil.AsParallel()
				   .WithCancellation(cancelSource.Token)
				   .Where(n => Enumerable.Range(2, (int) Math.Sqrt(n)).All(i => n % i > 0));

			new Thread(() => {Thread.Sleep(100);
			cancelSource.Cancel();}).Start();

			var primes = new List<int>();
			try
			{
				foreach (var prime in parallelQuery)
				{
					primes.Add(prime);
				}
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Query cancelled");
			}
			Console.WriteLine("Last prime is {0} at {1}", primes[primes.Count - 1], primes.Count);
		
		}

		[Test]
		public void NJustice_for_all()
		{
			"abcdefghijklmnopqrstuvwxyz".AsParallel().Select(c => char.ToUpper(c)).ForAll(Console.Write);
		}

		[Test]
		public void Range_partitioning_is_slower_for_primes()
		{
			var mil = Enumerable.Range(3, 10000000);
			var parallelQuery =
				mil.AsParallel()
				   .Where(n => Enumerable.Range(2, (int)Math.Sqrt(n)).All(i => n % i > 0));

			var clock = new Stopwatch();
			clock.Start();
			parallelQuery.ToArray();
			clock.Stop();

			Console.WriteLine("Chunck partitioning took {0} ms", clock.ElapsedMilliseconds);

			var milrange = Enumerable.Range(3, 10000000).ToArray();
			var parallelRangeQuery =
				milrange.AsParallel()
				   .Where(n => Enumerable.Range(2, (int)Math.Sqrt(n)).All(i => n % i > 0));

			clock.Reset();
			clock.Start();
			parallelRangeQuery.ToArray();
			clock.Stop();

			Console.WriteLine("Range partitioning took {0} ms", clock.ElapsedMilliseconds);

		}
	}

	public struct PingResult
	{
		public string Site;
		public IPStatus Result;
		public long Time;
	}

	public static class EnumExtentions
	{
		public static void Dump<T>( this IEnumerable<T> enumerable)
		{
			var type = typeof (T);

			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

			foreach (var fieldInfo in fields)
			{
				Console.Write(fieldInfo.Name.Substring(0, Math.Min(fieldInfo.Name.Length, 25)).PadRight(25));
				Console.Write(" ");
			}

			Console.WriteLine();

			foreach (var fieldInfo in fields)
			{
				Console.Write("".PadRight(25, '-'));
				Console.Write(" ");
			}

			Console.WriteLine();



			foreach (var x in enumerable)
			{
				foreach (var fieldInfo in fields)
				{
					var value = fieldInfo.GetValue(x).ToString();
					Console.Write(value.Substring(0,Math.Min(value.Length, 25)).PadRight(25));
					Console.Write(" ");
				}

				Console.WriteLine();
			}


		}
	}

	public struct IndexedWord
	{
		public string Word;
		public int Index;
	}

	

}