using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SwissSdr.Datamodel
{
	public abstract class Multilingual
	{
		public static Multilingual<T> Create<T>(T value)
		{
			return new Multilingual<T>(new LocalizedValue<T>(value));
		}
		public static Multilingual<T> Create<T>(T value, string languageCode)
		{
			return new Multilingual<T>(new LocalizedValue<T>(value, languageCode));
		}
		public static Multilingual<T> Create<T>(params LocalizedValue<T>[] items)
		{
			return new Multilingual<T>(items);
		}
		public static Multilingual<T> Create<T>(IDictionary<string, T> items)
		{
			return new Multilingual<T>(items.Select(kv => new LocalizedValue<T>(kv.Value, kv.Key)));
		}
	}

	public class Multilingual<T> : KeyedCollection<string, LocalizedValue<T>>
	{
		private static IEnumerable<string> _languageCascade = new [] {"en", "de", "fr", "it", "iv"};

		// serialization constructor
		public Multilingual() { }

		public Multilingual(params LocalizedValue<T>[] items) : this(items.AsEnumerable()) { }

		public Multilingual(IEnumerable<LocalizedValue<T>> items)
		{
			foreach (var item in items)
			{
				Add(item);
			}
		}

		protected override string GetKeyForItem(LocalizedValue<T> item) => item.LanguageCode;

		public bool ContainsLanguage(CultureInfo culture) => Contains(culture.TwoLetterISOLanguageName);
		public bool ContainsLanguage(string languageCode) => Contains(languageCode);

		public void Add(T value, CultureInfo culture) => Add(value, culture.TwoLetterISOLanguageName);
		public void Add(T value, string languageCode) => Add(new LocalizedValue<T>(value, languageCode));

		public T Value() => Value(CultureInfo.CurrentUICulture);
		public T Value(CultureInfo culture) => Value(culture.TwoLetterISOLanguageName);
		public T Value(string languageCode) => this[languageCode].Value;

		public T ValueOrDefault() => ValueOrDefault(CultureInfo.CurrentUICulture);
		public T ValueOrDefault(CultureInfo culture) => ValueOrDefault(culture.TwoLetterISOLanguageName);
		public T ValueOrDefault(string languageCode)
		{
			if (!Contains(languageCode)) return default(T);
			return this[languageCode].Value;
		}

		public T ValueOrFallback(Func<T> fallbackCreator) => ValueOrFallback(CultureInfo.CurrentUICulture, fallbackCreator);
		public T ValueOrFallback(CultureInfo culture, Func<T> fallbackCreator) => ValueOrFallback(culture.TwoLetterISOLanguageName, fallbackCreator);
		public T ValueOrFallback(string languageCode, Func<T> fallbackCreator)
		{
			if (!Contains(languageCode)) return fallbackCreator.Invoke();
			return this[languageCode].Value;
		}

		public T ValueByBestMatch() => ValueByBestMatch(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
		public T ValueByBestMatch(CultureInfo culture) => ValueByBestMatch(culture.TwoLetterISOLanguageName);
		public T ValueByBestMatch(string languageCode)
		{
			return new[] { languageCode }.Concat(_languageCascade)
				.Select(ValueOrDefault)
				.FirstOrDefault(x => x != null);
		}

		public T ValueByCascade()
		{
			return _languageCascade
				.Select(ValueOrDefault)
				.FirstOrDefault(x => x != null);
		}

		public void Remove(CultureInfo culture) => Remove(culture.TwoLetterISOLanguageName);

		public override string ToString()
		{
			switch (Items.Count)
			{
				case 0:
					return string.Empty;
				case 1:
					return Items.Single().Value?.ToString();
				default:
					if (ContainsLanguage(CultureInfo.CurrentUICulture))
					{
						return Value(CultureInfo.CurrentUICulture)?.ToString();
					}
					else if (ContainsLanguage(CultureInfo.InvariantCulture))
					{
						return Value(CultureInfo.InvariantCulture).ToString();
					}
					else
					{
						return string.Empty;
					}
			}
		}
	}

	public abstract class LocalizedValue
	{
		public static LocalizedValue<T> Create<T>(T value, string languageCode)
		{
			return new LocalizedValue<T>(value, languageCode);
		}
	}

	public class LocalizedValue<T>
	{
		public string LanguageCode { get; protected set; }
		public T Value { get; protected set; }

		protected LocalizedValue() { }
		public LocalizedValue(T value)
		{
			Value = value;
			LanguageCode = CultureInfo.InvariantCulture.TwoLetterISOLanguageName;
		}
		public LocalizedValue(T value, string languageCode)
		{
			// try to construct a culture to validate languageCode,
			// but only if it is not "iv" (invariant culture) - that will throw an error
			if (languageCode != CultureInfo.InvariantCulture.TwoLetterISOLanguageName)
			{
				var culture = new CultureInfo(languageCode);
			}

			Value = value;
			LanguageCode = languageCode;
		}
		public LocalizedValue(T value, CultureInfo culture)
		{
			Value = value;
			LanguageCode = culture.TwoLetterISOLanguageName;
		}

		public override string ToString()
		{
			return Value?.ToString();
		}
	}
}