﻿using JetBrains.Annotations;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StringDB.Databases
{
	/// <inheritdoc />
	/// <summary>
	/// An implementor of IDatabase that requires the inheriting class
	/// to only implement two functions.
	/// </summary>
	/// <typeparam name="TKey">The type of key of the database.</typeparam>
	/// <typeparam name="TValue">The type of value of the database.</typeparam>
	[PublicAPI]
	public abstract class BaseDatabase<TKey, TValue> : IDatabase<TKey, TValue>
	{
		private readonly IEqualityComparer<TKey> _keyComparer;
		private readonly IEqualityComparer<TValue> _valueComparer;

		protected BaseDatabase()
			: this(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
		{
		}

		protected BaseDatabase([NotNull] IEqualityComparer<TKey> keyComparer)
			: this(keyComparer, EqualityComparer<TValue>.Default)
		{
		}

		protected BaseDatabase([NotNull] IEqualityComparer<TValue> valueComparer)
			: this(EqualityComparer<TKey>.Default, valueComparer)
		{
		}

		protected BaseDatabase
		(
			[NotNull] IEqualityComparer<TKey> keyComparer,
			[NotNull] IEqualityComparer<TValue> valueComparer
		)
		{
			_keyComparer = keyComparer;
			_valueComparer = valueComparer;
		}

		/// <inheritdoc />
		public abstract void InsertRange(params KeyValuePair<TKey, TValue>[] items);

		/// <summary>
		/// Enumerates over all the items in the database.
		/// </summary>
		/// <returns>An IEnumerable of KeyValuePairs of keys and their lazy-loading values.</returns>
		[NotNull]
		protected abstract IEnumerable<KeyValuePair<TKey, ILazyLoader<TValue>>> Evaluate();

		/// <inheritdoc />
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">When the key is unable to be found.</exception>
		public virtual TValue Get(TKey key)
		{
			var success = TryGet(key, out var value);

			if (success && !_valueComparer.Equals(value, default))
			{
				return value;
			}

			throw new KeyNotFoundException($"Unable to find {key} in the database.");
		}

		public virtual bool TryGet(TKey key, out TValue value)
		{
			foreach (var result in GetAll(key))
			{
				value = result.Load();
				return true;
			}

			value = default;
			return false;
		}

		public virtual void Insert(TKey key, TValue value)
			=> InsertRange(new KeyValuePair<TKey, TValue>(key, value));

		public virtual IEnumerable<ILazyLoader<TValue>> GetAll(TKey key)
			=> Evaluate()
			.Where(item => _keyComparer.Equals(key, item.Key))
			.Select(item => item.Value);

		public virtual IEnumerator<KeyValuePair<TKey, ILazyLoader<TValue>>> GetEnumerator() => Evaluate().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Cleans up any resources the database is using.
		/// </summary>
		public abstract void Dispose();
	}
}