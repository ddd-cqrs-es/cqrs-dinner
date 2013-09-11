namespace Testing101
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class Testing_with_a_database
	{
		[Test]
		public void Given_a_bank_account_with_a_deposit_of_100_when_30_are_withdrawn_a_transaction_is_written_to_the_db()
		{
			
			using (var db = new Database("bank"))
			{
				//Arrange - Create a database and bring it to a known state
				db.Insert(new TransactionRecord {Account = 1234, Amount = 100m});

				var bankAccount = new BankAccount(db);
				bankAccount.Load(1234);

				//Act
				bankAccount.Withdraw(30);
				
				//Assert
				db.Load<TransactionRecord>(x => x.Account== 1234 && x.Amount == -30);
			}
		}

		public class BankAccount
		{
			private readonly Database db;

			public BankAccount(Database db)
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
				db.Load<TransactionRecord>(x => x.Account == accoundNb);
			}
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
	public class Database : IDisposable
	{
		public Database(string name)
		{
			
		}

		public void Insert<TRecord>(TRecord record)
		{
			
		}

		public void Load<T>(Func<T, bool> func)
		{
			
		}

		public void Dispose()
		{
			//clear all the data
		}
	}
}