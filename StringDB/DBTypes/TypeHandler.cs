﻿using System.IO;

namespace StringDB {

	/// <summary>A generic interface for a given TypeHandler</summary>
	internal interface ITypeHandler {

		/// <summary>Unique byte identifier. Set it above 0x2F to avoid colliding with the predefined types.</summary>
		byte Id { get; }

		/// <summary>Returns whatever type it is, typeof(T)</summary>
		System.Type Type { get; }
	}

	/// <summary>Allows StringDB to handle a new type, without much effort.</summary>
	/// <typeparam name="T">The type</typeparam>
	public abstract class TypeHandler<T> : ITypeHandler {

		/// <inheritdoc/>
		public abstract byte Id { get; }

		/// <inheritdoc/>
		public System.Type Type => typeof(T);

		/// <summary>Gets the length of an item, or how long it would be when attempting to store it.</summary>
		/// <param name="item">The item to calculate the length for</param>
		public abstract long GetLength(T item);

		/// <summary>Write an object to a BinaryWriter. The BinaryWriter should only be used to write the necessary data, and no seeking should be done. All you need to do is write the data, writing the length of the data will be taken care of for you assuming that the GetLength method is implemented properly.</summary>
		/// <param name="bw">The BinaryWriter to use</param>
		/// <param name="item">The item to write</param>
		public abstract void Write(BinaryWriter bw, T item);

		/// <summary>Read back the item from a stream, given the length of it. If you're not using the length of it, there's a good chance you're doing something wrong.</summary>
		/// <param name="br">The BinaryReader</param>
		/// <param name="len">The length of the data</param>
		public abstract T Read(BinaryReader br, long len);

		/// <summary>Automatically reads the length of a given item to pass through to the T Read(br, len) function</summary>
		/// <param name="br">The BinaryReader</param>
		public T Read(BinaryReader br)
			=> this.Read(br, TypeHandlerLengthManager.ReadLength(br));

		/// <summary>Compare if two items are the same.</summary>
		/// <param name="item1">The first item to compare</param>
		/// <param name="item2">The second item to compare</param>
		public abstract bool Compare(T item1, T item2);
	}

	internal static class TypeHandlerLengthManager {

		public static void WriteLength(BinaryWriter bw, long len) { // write out the length
			if (len <= byte.MaxValue) { // depending on the length
				bw.Write(Consts.IsByteValue); // write the identifier for it
				bw.Write((byte)len); // and the length
			} else if (len <= ushort.MaxValue) {
				bw.Write(Consts.IsUShortValue);
				bw.Write((ushort)len);
			} else if (len <= uint.MaxValue) {
				bw.Write(Consts.IsUIntValue);
				bw.Write((uint)len);
			} else {
				bw.Write(Consts.IsLongValue);
				bw.Write(len);
			}
		}

		public static long ReadLength(BinaryReader br) { // read the length depending on the identifier
			switch (br.ReadByte()) {
				case Consts.IsByteValue:
				return br.ReadByte();

				case Consts.IsUShortValue:
				return br.ReadUInt16();

				case Consts.IsUIntValue:
				return br.ReadUInt32();

				case Consts.IsLongValue:
				return br.ReadInt64();

				default: return 0;
			}
		}

		internal static long EstimateWriteLengthSize(long len) { // estimate how big in bytes a WriteLength would be with a given length
			const int length = sizeof(byte) + sizeof(byte);

			return len <= byte.MaxValue ?
				length + sizeof(byte)
				: len <= ushort.MaxValue ?
					length + sizeof(ushort)
					: len <= uint.MaxValue ?
						length + sizeof(uint)
						: length + sizeof(long);
		}
	}
}