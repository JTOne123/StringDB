﻿using JetBrains.Annotations;

using StringDB.IO;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace StringDB
{
	/// <summary>
	/// Handy extensions for a database.
	/// </summary>
	[PublicAPI]
	public static class DatabaseExtensions
	{
		/// <summary>
		/// Returns every key of the database.
		/// </summary>
		/// <typeparam name="TKey">The type of key.</typeparam>
		/// <typeparam name="TValue">The type of value.</typeparam>
		/// <param name="db">The database to fetch all the keys from.</param>
		/// <returns>A <see cref="IEnumerable{T}"/> of keys.</returns>
		[NotNull]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TKey> Keys<TKey, TValue>
		(
			[NotNull] this IDatabase<TKey, TValue> db
		)
		{
			foreach (var entry in db)
			{
				yield return entry.Key;
			}
		}

		/// <summary>
		/// Returns every value of the database.
		/// </summary>
		/// <typeparam name="TKey">The type of key.</typeparam>
		/// <typeparam name="TValue">The type of value.</typeparam>
		/// <param name="db">The database to fetch all the values from.</param>
		/// <returns>A <see cref="IEnumerable{T}"/> of values.</returns>
		[NotNull]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<ILazyLoader<TValue>> Values<TKey, TValue>
		(
			[NotNull] this IDatabase<TKey, TValue> db
		)
		{
			foreach (var entry in db)
			{
				yield return entry.Value;
			}
		}

		/// <summary>
		/// Loads every value, and returns the loaded value of the database.
		/// </summary>
		/// <typeparam name="TKey">The type of key.</typeparam>
		/// <typeparam name="TValue">The type of value.</typeparam>
		/// <param name="db">The database to fetch all the values from.</param>
		/// <returns>A <see cref="IEnumerable{T}"/> of loaded values.</returns>
		[NotNull]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TValue> ValuesAggressive<TKey, TValue>
		(
			[NotNull] this IDatabase<TKey, TValue> db
		)
		{
			foreach (var entry in db)
			{
				yield return entry.Value.Load();
			}
		}

		/// <summary>
		/// Enumerates over the database and loads values as soon as it's optimal to do so.
		/// </summary>
		/// <typeparam name="TKey">The type of key.</typeparam>
		/// <typeparam name="TValue">The type of value.</typeparam>
		/// <param name="db">The database to fetch all the values from.</param>
		/// <param name="optimalToken">The token to use to determine when it is optimal to read values.</param>
		/// <returns>An <see cref="IEnumerator{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s with the data.</returns>
		[NotNull]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateOptimally<TKey, TValue>
		(
			[NotNull] this IDatabase<TKey, TValue> db,
			IOptimalToken optimalToken
		)
		{
			var lazyList = new List<KeyValuePair<TKey, ILazyLoader<TValue>>>();

			foreach (var entry in db)
			{
				lazyList.Add(entry);

				if (!optimalToken.OptimalReadingTime)
				{
					continue;
				}

				foreach (var lazyEntry in lazyList)
				{
					yield return new KeyValuePair<TKey, TValue>(lazyEntry.Key, lazyEntry.Value.Load());
				}

				lazyList.Clear();
			}

			if (lazyList.Count == 0)
			{
				yield break;
			}

			// i really hate how this is duplicated, but we can't use local functions
			// since those are delegates and that'd be a bit of a performance hit
			// so copying and pasting is the ugliest, but fastest solution.
			// i'm very open to PRs for this :p
			foreach (var lazyEntry in lazyList)
			{
				yield return new KeyValuePair<TKey, TValue>(lazyEntry.Key, lazyEntry.Value.Load());
			}
		}

		/// <summary>
		/// Enumerates over the database and loads <param name="valueLoadAmount"></param> values at a time.
		/// </summary>
		/// <typeparam name="TKey">The type of key.</typeparam>
		/// <typeparam name="TValue">The type of value.</typeparam>
		/// <param name="db">The database to fetch all the values from.</param>
		/// <param name="valueLoadAmount">The amount of values to load at a time.</param>
		/// <returns>An <see cref="IEnumerator{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s with the data.</returns>
		[NotNull]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateAggressively<TKey, TValue>
		(
			[NotNull] this IDatabase<TKey, TValue> db,
			int valueLoadAmount
		)
		{
			var lazyList = new List<KeyValuePair<TKey, ILazyLoader<TValue>>>(valueLoadAmount);
			var loadedList = new List<KeyValuePair<TKey, TValue>>(valueLoadAmount);

			using (var enumerator = db.GetEnumerator())
			{
				int result;

				do
				{
					result = Pool(valueLoadAmount, enumerator, ref lazyList);

					foreach (var item in lazyList)
					{
						loadedList.Add(new KeyValuePair<TKey, TValue>(item.Key, item.Value.Load()));
					}

					foreach (var item in loadedList)
					{
						yield return item;
					}

					loadedList.Clear();
					lazyList.Clear();
				}
				while (result == valueLoadAmount);
			}
		}

		private static int Pool<TKey, TValue>
		(
			int amount,
			[NotNull] IEnumerator<KeyValuePair<TKey, ILazyLoader<TValue>>> enumerator,
			[NotNull] ref List<KeyValuePair<TKey, ILazyLoader<TValue>>> lazyList
		)
		{
			var fillAmount = 0;

			for (; fillAmount < amount && enumerator.MoveNext(); fillAmount++)
			{
				lazyList.Add(enumerator.Current);
			}

			return fillAmount;
		}
	}
}