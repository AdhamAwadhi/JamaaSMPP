using System;
using System.Collections.Concurrent;

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// Returns the next segment ID (0-65535) for a given source/destination pair.
    /// </summary>
    public interface ISegmentIdGenerator
    {
        int NextSegmentId(string sourceAddress, string destinationAddress);
    }

    /// <summary>
    /// Abstraction for per (source,destination) cyclic counters (0-65535).
    /// Implement to plug in alternative backends (Redis, distributed cache, etc).
    /// </summary>
    public interface ISegmentIdCounterStore
    {
        int Next(string sourceAddress, string destinationAddress);
    }

    /// <summary>
    /// In-memory implementation using ConcurrentDictionary (original behavior).
    /// </summary>
    public sealed class InMemorySegmentIdCounterStore : ISegmentIdCounterStore
    {
        private struct AddressKey : IEquatable<AddressKey>
        {
            public readonly string Source;
            public readonly string Destination;
            public AddressKey(string s, string d)
            {
                Source = s ?? string.Empty;
                Destination = d ?? string.Empty;
            }
            public bool Equals(AddressKey other)
            {
                return string.Equals(Source, other.Source, StringComparison.Ordinal) &&
                       string.Equals(Destination, other.Destination, StringComparison.Ordinal);
            }
            public override bool Equals(object obj) { return obj is AddressKey k && Equals(k); }
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Source != null ? Source.GetHashCode() : 0) * 397) ^
                           (Destination != null ? Destination.GetHashCode() : 0);
                }
            }
        }

        private readonly ConcurrentDictionary<AddressKey, ushort> _counters =
            new ConcurrentDictionary<AddressKey, ushort>();

        public int Next(string sourceAddress, string destinationAddress)
        {
            var key = new AddressKey(sourceAddress, destinationAddress);
            ushort next = _counters.AddOrUpdate(
                key,
                k => (ushort)0,
                (k, prev) => (ushort)((prev + 1) & 0xFFFF));
            return next;
        }
    }

    /// <summary>
    /// Default segment ID generator delegating storage to an ISegmentIdCounterStore.
    /// </summary>
    public sealed class DefaultSegmentIdGenerator : ISegmentIdGenerator
    {
        private readonly ISegmentIdCounterStore _store;

        public DefaultSegmentIdGenerator(ISegmentIdCounterStore store)
        {
            if (store == null) throw new ArgumentNullException("store");
            _store = store;
        }

        public int NextSegmentId(string sourceAddress, string destinationAddress)
        {
            return _store.Next(sourceAddress, destinationAddress);
        }
    }

    /// <summary>
    /// Factory ensuring a single (immutable) global segment id generator instance.
    /// </summary>
    public static class SegmentIdGeneratorFactory
    {
        private static ISegmentIdGenerator _generator;
        private static readonly object _sync = new object();

        public static ISegmentIdGenerator Generator
        {
            get
            {
                var g = _generator;
                if (g == null)
                {
                    lock (_sync)
                    {
                        if (_generator == null)
                            _generator = new DefaultSegmentIdGenerator(new InMemorySegmentIdCounterStore()); // uses in-memory store
                        g = _generator;
                    }
                }
                return g;
            }
        }

        /// <summary>
        /// Configure a custom generator (e.g. new DefaultSegmentIdGenerator(customStore)). First call wins.
        /// </summary>
        public static void Configure(ISegmentIdGenerator generator, bool throwIfAlreadyConfigured = true)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            lock (_sync)
            {
                if (_generator != null)
                {
                    if (throwIfAlreadyConfigured)
                        throw new InvalidOperationException("SegmentIdGenerator already configured.");
                    return;
                }
                _generator = generator;
            }
        }
    }
}
