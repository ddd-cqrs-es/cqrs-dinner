namespace Garbage
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class Age_calculation
	{
		[Test]
		 public void Age_has_always_same_fraction_()
		 {
			 var ddn = new DateTime(1989, 5, 11);
			 var ddrap = new DateTime(2044, 6, 1);
			 var ddrsp = new DateTime(2054, 6, 1);
			
			
			var substract0n3Year = ddrap.Month < ddn.Month || (ddrap.Month == ddn.Month && ddrap.Day < ddn.Day);
			var years = ddrap.Year - ddn.Year + (substract0n3Year ? 1 : 0);
			var d55Ans = ddn.AddYears(years);
			double portionAnnee = (ddrap - d55Ans).TotalDays/365.25;




			Console.WriteLine("Age ap: {0} annees, {1} mois", years + portionAnnee, (years + portionAnnee)*12 );

			var substract0neYearSp = ddrsp.Month < ddn.Month || (ddrsp.Month == ddn.Month && ddrsp.Day < ddn.Day);
			var yearsSp = ddrsp.Year - ddn.Year + (substract0neYearSp ? 1 : 0);
			var d65Ans = ddn.AddYears(yearsSp);
			double portionAnneeSp = (ddrsp - d65Ans).TotalDays / 365.25;


			Console.WriteLine("Age sp: {0} annees, {1} mois", yearsSp + portionAnneeSp, (yearsSp + portionAnneeSp)*12);

		 }
	}
}