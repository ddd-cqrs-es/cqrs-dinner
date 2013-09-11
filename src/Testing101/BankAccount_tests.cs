using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing101
{
	using NUnit.Framework;

	[TestFixture]
    public class BankAccount_tests
    {
		/// <summary>
		/// Name the test following the Given, When, The pattern
		/// </summary>
		[Test]
		public void Given_a_bank_account_with_a_deposit_of_100_when_30_are_withdrawn_the_new_balance_is_70()
		{
			//Arrange - Start with a known state
			var bankAccount = new BankAccount();
			bankAccount.Deposit(100);

			//Act - apply a state transition
			bankAccount.Withdraw(70);

			//Assert - verify that the now state is what is expected
			Assert.That(bankAccount.Balance, Is.EqualTo(30));
		}

		public class BankAccount
		{
			public void Deposit(decimal amount)
			{

			}

			public void Withdraw(decimal amount)
			{

			}

			public decimal Balance { get; private set; }
		}
    }


	
}
