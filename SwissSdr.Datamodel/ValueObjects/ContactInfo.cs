using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	/// <summary>
	/// Contains phone numbers, E-Mail addresses, links and physical addresses
	/// </summary>
	public class ContactInfo
	{
		public ICollection<PhoneNumber> PhoneNumbers { get; } = new Collection<PhoneNumber>();
		public ICollection<Email> Emails { get; } = new Collection<Email>();
		public ICollection<Url> Urls { get; } = new Collection<Url>();
		public ICollection<Address> Addresses { get; } = new Collection<Address>();

		public void AddPhoneNumber(string phoneNumber, PhoneNumberType numberType, Affiliation affiliation) => PhoneNumbers.Add(new PhoneNumber() { Value = phoneNumber, Affiliation = affiliation, Type = numberType});
		public void AddEmail(string email, Affiliation affiliation) => Emails.Add(new Email() { Value = email, Affiliation = affiliation});
		public void AddUrl(string url, string title, UrlType type) => Urls.Add(new Url() { Value = url, Title = title, Type = type });
		public void AddAddress(IEnumerable<string> addressLines, string postalCode, string locality, string region, string country, GeoCoordinate coordinates, Affiliation affiliation) => Addresses.Add(new Address(addressLines, postalCode, locality, region, country, coordinates, affiliation));

		public class Url
		{
			public UrlType Type { get; set; }
			public string Title { get; set; }
			public string Value { get; set; }
		}

		public class PhoneNumber
		{
			public Affiliation Affiliation { get; set; }
			public PhoneNumberType Type { get; set; }
			public string Value { get; set; }
		}

		public class Email
		{
			public Affiliation Affiliation { get; set; }
			public string Value { get; set; }
		}

		public class Address
		{
			public Affiliation Affiliation { get; set; }

			public IList<string> AddressLines { get; }
			public string PostalCode { get; set; }
			public string Locality { get; set; }
			public string Region { get; set; }
			public string Country { get; set; }
			public GeoCoordinate Coordinates { get; set; }

			public Address(IEnumerable<string> addressLines, string postalCode, string locality, string region, string country, GeoCoordinate coordinates, Affiliation affiliation)
			{
				AddressLines = addressLines.ToList();
				PostalCode = postalCode;
				Locality = locality;
				Region = region;
				Country = country;
				Coordinates = coordinates;

				Affiliation = affiliation;
			}

			public override string ToString()
			{
				return string.Join(", ",
					string.Join(" ", AddressLines),
					$"{PostalCode} {Locality}",
					Country);
			}
		}

		/*
		 * Enums
		 */
		public enum Affiliation
		{
			Home,
			Work
		}

		public enum PhoneNumberType
		{
			Landline,
			Mobile,
			Fax
		}

		public enum UrlType
		{
			Website,
			Social
		}
	}
}
