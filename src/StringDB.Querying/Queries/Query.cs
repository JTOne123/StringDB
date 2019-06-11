﻿using JetBrains.Annotations;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace StringDB.Querying.Queries
{
	/// <summary>
	/// A simple implementation of the <see cref="IQuery{TKey, TValue}"/> interface.
	/// </summary>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <typeparam name="TValue">The type of value.</typeparam>
	[PublicAPI]
	public class Query<TKey, TValue> : IQuery<TKey, TValue>
	{
		private readonly Func<TKey, IRequest<TValue>, Task<QueryAcceptance>> _accept;
		private readonly Func<TKey, IRequest<TValue>, Task> _process;
		private readonly CancellationToken _cancellationToken;

		/// <summary>
		/// Create a new query.
		/// </summary>
		/// <param name="accept">The delegate for accepting a query.</param>
		/// <param name="process">The delegate for processing a query.</param>
		public Query
		(
			[NotNull] Func<TKey, IRequest<TValue>, Task<QueryAcceptance>> accept,
			[NotNull] Func<TKey, IRequest<TValue>, Task> process,
			[CanBeNull] CancellationToken cancellationToken = default
		)
		{
			_accept = accept;
			_process = process;
			_cancellationToken = cancellationToken;
		}

		public bool IsCancellationRequested => _cancellationToken.IsCancellationRequested;

		public Task<QueryAcceptance> Accept(TKey key, IRequest<TValue> value)
			=> _accept(key, value);

		public Task Process(TKey key, IRequest<TValue> value)
			=> _process(key, value);

		public void Dispose()
		{
		}
	}
}