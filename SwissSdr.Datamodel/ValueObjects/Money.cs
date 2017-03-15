using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public struct Money
	{
		public decimal Amount { get; private set; }
		/// <summary>
		/// The ISO 4217 three letter currency code.
		/// </summary>
		public string CurrencyCode { get; private set; }

		public Money(decimal amount, string currencyCode)
		{
			Amount = amount;
			CurrencyCode = currencyCode;
		}
	}
}
