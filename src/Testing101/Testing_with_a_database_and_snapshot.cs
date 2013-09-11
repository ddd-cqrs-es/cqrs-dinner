namespace Testing101
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public class Testing_with_a_database_and_snapshot
	{
		[Test]
		public void Given_a_bank_account_with_a_deposit_of_100_when_30_are_withdrawn_a_transaction_is_written_to_the_db()
		{
			using (new SnaphotScope("bank"))
			{
				using (var db = new SqlDatabase("bank")) //connect to existant db with known state
				{
					//Arrange - Create a database and bring it to a known state
					db.Insert(new TransactionRecord {Account = 1234, Amount = 100m});

					var bankAccount = new BankAccount(db);
					bankAccount.Load(1234);

					//Act
					bankAccount.Withdraw(30);

					//Assert
					var transactionRecords = db.Load<TransactionRecord>(x => x.Account == 1234 && x.Amount == -30);
					Assert.That(transactionRecords.Count(), Is.EqualTo(1));

				}
			}
		}

		public class BankAccount
		{
			private readonly SqlDatabase db;
			private IEnumerable<TransactionRecord> transactionRecords;

			public BankAccount(SqlDatabase db)
			{
				this.db = db;

			}

			public void Deposit(decimal amount)
			{

			}

			public void Withdraw(decimal amount)
			{
				
			}

			public decimal Balance { get; private set; }

			public void Load(int accoundNb)
			{
				transactionRecords = db.Load<TransactionRecord>(x => x.Account == accoundNb);
			}
		}

		public class SnaphotScope : IDisposable
		{
			public SnaphotScope(string bank)
			{
				//take snapshot
			}

			public void Dispose()
			{
				// Restore snaphot
			}
		}

		public class TransactionRecord
		{
			public int Account { get; set; }

			public decimal Amount { get; set; }
		}

		/// <summary>
		/// Stupid in memery database
		/// </summary>
		public class SqlDatabase : IDisposable
		{
			private SqlConnection cn;

			public SqlDatabase(string name)
			{
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
				builder.IntegratedSecurity = true;
				builder.DataSource = ".";
				builder.InitialCatalog = name;

				cn = new SqlConnection(builder.ConnectionString);
			}

			public void Insert<TRecord>(TRecord record)
			{
				//Some magical relexion to write to db
			}

			public IEnumerable<T> Load<T>(Func<T, bool> func)
			{
				//Load all the data then apply filter
				return Enumerable.Empty<T>();
			}

			public void Dispose()
			{
				//clear all the data
				cn.Dispose();
			}
		}
	}
}