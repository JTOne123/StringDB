﻿using StringDB.Reader;
using StringDB.Writer;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace StringDB {

	/// <summary>A StringDB database, used to encapsulate an IReader and an IWriter together for easy usage.</summary>
	public interface IDatabase : IReader, IWriter, IDisposable {

		/// <summary>Cleans out the current database, and copies all of the contents of this database into the other one. You may be able to experience a smaller DB file if you've used StringDB to not to perfectionist values.</summary>
		/// <param name="dbCleanTo">The database that will be used to insert the other database's values into</param>
		void CleanTo(IDatabase dbCleanTo);

		/// <summary>Cleans out the database specified, and copies all of the contents of the other database into this one. You may be able to experience a smaller DB file if you've used StringDB to not to perfectionist values.</summary>
		/// <param name="dbCleanFrom">The database to clean up</param>
		void CleanFrom(IDatabase dbCleanFrom);
	}

	/// <inheritdoc/>
	public class Database : IDatabase {

		internal Database(Stream s, bool disposeStream) {
			this._lock = new object();

			this._disposeStream = disposeStream;

			this._stream = s;
			this._reader = new Reader.Reader(s, this._lock);
			this._writer = new Writer.Writer(s, this._lock);
		}

		/// <summary>Create a new Database from a stream</summary><param name="s">The stream to be using</param><param name="disposeStream">If the stream should be disposed after we're done using it</param>
		public static Database FromStream(Stream s, bool disposeStream = false) => new Database(s, disposeStream);

		/// <summary>Create a new Database from a string name to open a file</summary><param name="name">The name of the file</param>
		public static Database FromFile(string name) => new Database(File.Open(name, FileMode.OpenOrCreate), true);

		private object _lock;

		private bool _disposeStream;
		private Stream _stream;
		private IReader _reader;
		private IWriter _writer; /// <inheritdoc/>

		public void CleanTo(IDatabase dbCleanTo) =>
			dbCleanTo.InsertRange(FromDatabase(this)); /// <inheritdoc/>

		public void CleanFrom(IDatabase dbCleanFrom) =>
			this.InsertRange(FromDatabase(dbCleanFrom)); /// <inheritdoc/>

		public void DrainBuffer() =>
			this._reader.DrainBuffer(); /// <inheritdoc/>

		public IEnumerator<ReaderPair> GetEnumerator() =>
			this._reader.GetEnumerator(); /// <inheritdoc/>

		IEnumerator IEnumerable.GetEnumerator() =>
			this._reader.GetEnumerator(); /// <inheritdoc/>

		public ReaderPair First() =>
			this._reader.First(); /// <inheritdoc/>

		public ReaderPair GetByIndex(string index) =>
			this._reader.GetByIndex(index ?? throw new ArgumentNullException(nameof(index))); /// <inheritdoc/>

		public IEnumerable<ReaderPair> GetMultipleByIndex(string index) =>
			this._reader.GetMultipleByIndex(index ?? throw new ArgumentNullException(nameof(index))); /// <inheritdoc/>

		public void Dispose() {
			this._writer.Dispose();
			this._writer = null;

			this._reader = null;

			this._stream.Flush();

			if (this._disposeStream)
				this._stream.Dispose();
		}

		private IEnumerable<KeyValuePair<string, string>> FromDatabase(IDatabase other) {
			foreach (var i in other)
				yield return new KeyValuePair<string, string>(i.Index, i.Value);
		}

		/// <inheritdoc/>
		public void Insert(byte[] index, byte[] value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(byte[] index, string value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(byte[] index, Stream value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(string index, byte[] value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(string index, string value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(string index, Stream value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(Stream index, byte[] value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(Stream index, string value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(Stream index, Stream value) => this._writer.Insert(index, value); /// <inheritdoc/>
		public void Insert(KeyValuePair<byte[], byte[]> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<byte[], string> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<byte[], Stream> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<string, byte[]> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<string, string> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<string, Stream> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<Stream, byte[]> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<Stream, string> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void Insert(KeyValuePair<Stream, Stream> kvp) => this._writer.Insert(kvp); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<byte[], byte[]>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<byte[], string>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<byte[], Stream>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<string, byte[]>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<string, string>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<string, Stream>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<Stream, byte[]>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<Stream, string>> items) => this._writer.InsertRange(items); /// <inheritdoc/>
		public void InsertRange(IEnumerable<KeyValuePair<Stream, Stream>> items) => this._writer.InsertRange(items); /// <inheritdoc/>

		public void OverwriteValue(ReaderPair replacePair, byte[] newValue) {
			this._writer.OverwriteValue(replacePair, newValue);
			this._reader.DrainBuffer();
		} /// <inheritdoc/>

		public void OverwriteValue(ReaderPair replacePair, string newValue) {
			this._writer.OverwriteValue(replacePair, newValue);
			this._reader.DrainBuffer();
		} /// <inheritdoc/>

		public void OverwriteValue(ReaderPair replacePair, Stream newValue) {
			this._writer.OverwriteValue(replacePair, newValue);
			this._reader.DrainBuffer();
		}
	}
}