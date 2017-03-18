﻿using Reactive4.NET.operators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reactive.Streams;
using Reactive4.NET.subscribers;
using System.Threading;

namespace Reactive4.NET
{
    /// <summary>
    /// Extension methods for composing backpressure-enabled operators in a fluent manner.
    /// </summary>
    public static class Flowable
    {
        /// <summary>
        /// The default buffer size, prefetch amount, capacity hint.
        /// </summary>
        /// <returns>The default buffer size, positive.</returns>
        public static int BufferSize()
        {
            return 128;
        }

        // ********************************************************************************
        // Interop methods
        // ********************************************************************************

        /// <summary>
        /// Converts an arbitrary Reactive-Streams IPublisher into an IFlowable to
        /// enable fluent API operations on it.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="publisher">The IPublisher to convert.</param>
        /// <returns>The new IFlowable instance</returns>
        public static IFlowable<T> ToFlowable<T>(this IPublisher<T> publisher)
        {
            if (publisher is IFlowable<T> f)
            {
                return f;
            }
            return new FlowableFromPublisher<T>(publisher);
        }

        /// <summary>
        /// Converts an arbitrary Reactive-Streams IPublisher into an IFlowable to
        /// enable fluent API operations on it.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="publisher">The IPublisher to convert.</param>
        /// <returns>The new IFlowable instance</returns>
        public static IFlowable<T> FromPublisher<T>(IPublisher<T> publisher)
        {
            return publisher.ToFlowable();
        }

        /// <summary>
        /// Convert a standard IObservable into an IFlowable that is run with the
        /// specified backpressure strategy.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The IObservable to convert.</param>
        /// <param name="strategy">The backpressure strategy to apply.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ToFlowable<T>(this IObservable<T> source, BackpressureStrategy strategy)
        {
            return new FlowableFromObservable<T>(source, strategy);
        }

        /// <summary>
        /// Convert a standard IObservable into an IFlowable that is run with the
        /// specified backpressure strategy.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The IObservable to convert.</param>
        /// <param name="strategy">The backpressure strategy to apply.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> FromObservable<T>(IObservable<T> source, BackpressureStrategy strategy)
        {
            return source.ToFlowable(strategy);
        }

        /// <summary>
        /// Signals the success or failure of the given (ongoing) Task instance.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="task">The task whose outcome to signal.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ToFlowable<T>(this Task<T> task)
        {
            return new FlowableFromTask<T>(task);
        }

        /// <summary>
        /// Signals the success or failure of the given (ongoing) Task instance.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="task">The task whose outcome to signal.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> FromTask<T>(Task<T> task)
        {
            return task.ToFlowable();
        }

        /// <summary>
        /// Signals the success (via an empty IFlowable) or failure of the given (ongoing) Task instance.
        /// </summary>
        /// <param name="task">The task whose outcome to signal.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<object> ToFlowable(this Task task)
        {
            return new FlowableFromTaskVoid(task);
        }

        /// <summary>
        /// Signals the success (via an empty IFlowable) or failure of the given (ongoing) Task instance.
        /// </summary>
        /// <param name="task">The task whose outcome to signal.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<object> FromTask(this Task task)
        {
            return task.ToFlowable();
        }

        /// <summary>
        /// Convert the IFlowable instance into a standard IObservable.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable to convert.</param>
        /// <returns>The new IObservable instance.</returns>
        public static IObservable<T> ToObservable<T>(this IFlowable<T> source)
        {
            return new FlowableToObservable<T>(source);
        }

        // ********************************************************************************
        // Factory methods
        // ********************************************************************************

        /// <summary>
        /// Signals a single item.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="item">The item to emit.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Just<T>(T item)
        {
            return new FlowableJust<T>(item);
        }

        /// <summary>
        /// Repeatedly signals the given item indefinitely.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="item">The item to emit.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> RepeatItem<T>(T item)
        {
            return new FlowableRepeatItem<T>(item);
        }

        /// <summary>
        /// Signals the given exception instance.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="exception">The exception to signal.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Error<T>(Exception exception)
        {
            return new FlowableError<T>(exception);
        }

        /// <summary>
        /// Signals the Exception returned by the errorSupplier for each
        /// individual ISubscriber.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="errorSupplier">The Func that returns the Exception to signal.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Error<T>(Func<Exception> errorSupplier)
        {
            return new FlowableErrorSupplier<T>(errorSupplier);
        }

        /// <summary>
        /// Signals a normal completion.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Empty<T>()
        {
            return FlowableEmpty<T>.Instance;
        }

        /// <summary>
        /// Doesn't signal anything.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Never<T>()
        {
            return FlowableNever<T>.Instance;
        }

        /// <summary>
        /// Signals the only item returned by the function call.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="function">The function to call for an item.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> FromFunction<T>(Func<T> function)
        {
            return new FlowableFromFunction<T>(function);
        }

        /// <summary>
        /// Repeatedly calls a function and signals the value it returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="function">The function called repeatedly.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> RepeatFunction<T>(Func<T> function)
        {
            return new FlowableRepeatFunction<T>(function);
        }

        /// <summary>
        /// Creates an IFlowable and allows signalling events in an imperative
        /// push manner.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="emitter">The action called for each incoming ISubscriber to trigger emissions of signals.</param>
        /// <param name="strategy">The backpressure strategy to use when the emitter overproduces items.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Create<T>(Action<IFlowableEmitter<T>> emitter, BackpressureStrategy strategy)
        {
            return new FlowableCreate<T>(emitter, strategy);
        }

        /// <summary>
        /// Generates items in a backpressure-aware manner without any per-ISubscriber state.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="emitter">The action called to signal an event for each downstream request.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Generate<T>(Action<IGeneratorEmitter<T>> emitter)
        {
            return Generate<T, object>(() => null, (s, e) => { emitter(e); return null; }, s => { });
        }

        /// <summary>
        /// Generates items in a backpressure-aware manner with a per-ISubscriber state.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="S">The state type.</typeparam>
        /// <param name="emitter">The action called to signal an event for each downstream request.</param>
        /// <param name="stateFactory">The function called to create the initial state per ISubscriber.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Generate<T, S>(Func<S> stateFactory, Action<S, IGeneratorEmitter<T>> emitter)
        {
            return Generate<T, S>(stateFactory, (s, e) => { emitter(s, e); return s; }, s => { });
        }

        /// <summary>
        /// Generates items in a backpressure-aware manner with a per-ISubscriber state
        /// and a state cleanup callback.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="S">The state type.</typeparam>
        /// <param name="emitter">The action called to signal an event for each downstream request.</param>
        /// <param name="stateFactory">The function called to create the initial state per ISubscriber.</param>
        /// <param name="stateCleanup">The action called to cleanup the state.</param>
        /// <param name="eager">If true, the stateCleanup is called before the terminal signal is emitted;
        /// if false, the stateCleanup is called after the terminal signal is emitted.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Generate<T, S>(Func<S> stateFactory, Action<S, IGeneratorEmitter<T>> emitter, Action<S> stateCleanup, bool eager = false)
        {
            return Generate<T, S>(stateFactory, (s, e) => { emitter(s, e); return s; }, stateCleanup, eager);
        }

        /// <summary>
        /// Generates items in a backpressure-aware manner with a per-ISubscriber state
        /// that is updated each time the emitter funtion is invoked.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="S">The state type.</typeparam>
        /// <param name="emitter">The action called to signal an event for each downstream request.</param>
        /// <param name="stateFactory">The function called to create the initial state per ISubscriber.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Generate<T, S>(Func<S> stateFactory, Func<S, IGeneratorEmitter<T>, S> emitter)
        {
            return Generate<T, S>(stateFactory, emitter, s => { });
        }

        /// <summary>
        /// Generates items in a backpressure-aware manner with a per-ISubscriber state
        /// that is updated each time the emitter funtion is invoked and a state cleanup callback.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="S">The state type.</typeparam>
        /// <param name="emitter">The action called to signal an event for each downstream request.</param>
        /// <param name="stateFactory">The function called to create the initial state per ISubscriber.</param>
        /// <param name="stateCleanup">The action called to cleanup the state.</param>
        /// <param name="eager">If true, the stateCleanup is called before the terminal signal is emitted;
        /// if false, the stateCleanup is called after the terminal signal is emitted.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Generate<T, S>(Func<S> stateFactory, Func<S, IGeneratorEmitter<T>, S> emitter, Action<S> stateCleanup, bool eager = false)
        {
            return new FlowableGenerate<T, S>(stateFactory, emitter, stateCleanup, eager);
        }

        /// <summary>
        /// Emits the items of the given params array.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="items">The items to emit.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> FromArray<T>(params T[] items)
        {
            return new FlowableArray<T>(items);
        }

        /// <summary>
        /// Emits the items of the given IEnumerable.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="items">The items to emit.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> FromEnumerable<T>(IEnumerable<T> items)
        {
            return new FlowableEnumerable<T>(items);
        }

        /// <summary>
        /// Emits a range of ints from start (inclusive) to start + count (exclusive).
        /// </summary>
        /// <param name="start">The starting value.</param>
        /// <param name="count">The number of ints to emit.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<int> Range(int start, int count)
        {
            return new FlowableRange(start, start + count);
        }

        /// <summary>
        /// Defers the creation of the actual IPublisher, allowing a per-ISubscriber
        /// state and/or individual IPublishers.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="supplier">The function that returns an IPublisher for each ISubscriber.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Defer<T>(Func<IPublisher<T>> supplier)
        {
            return new FlowableDefer<T>(supplier);
        }

        /// <summary>
        /// Emits 0L after the specified delay running on the Executors.Computation executor.
        /// </summary>
        /// <param name="delay">The delay timespan.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<long> Timer(TimeSpan delay)
        {
            return Timer(delay, Executors.Computation);
        }

        /// <summary>
        /// Emits 0L after the specified delay running on the given executor.
        /// </summary>
        /// <param name="delay">The delay timespan.</param>
        /// <param name="executor">The executor to use for delaying the emission.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<long> Timer(TimeSpan delay, IExecutorService executor)
        {
            return new FlowableTimer(delay, executor);
        }

        /// <summary>
        /// Emits an ever increasing long values periodically on the Executors.Computation
        /// executor.
        /// </summary>
        /// <param name="period">The initial and periodic delay.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<long> Interval(TimeSpan period)
        {
            return Interval(period, period, Executors.Computation);
        }

        /// <summary>
        /// Emits an ever increasing long values periodically on the Executors.Computation
        /// executor.
        /// </summary>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="period">The periodic delay.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<long> Interval(TimeSpan initialDelay, TimeSpan period)
        {
            return Interval(initialDelay, period, Executors.Computation);
        }

        /// <summary>
        /// Emits an ever increasing long values periodically on the given
        /// executor.
        /// </summary>
        /// <param name="period">The initial and periodic delay.</param>
        /// <param name="executor">The executor to use for delaying the emission.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<long> Interval(TimeSpan period, IExecutorService executor)
        {
            return Interval(period, period, executor);
        }

        /// <summary>
        /// Emits an ever increasing long values periodically on the given
        /// executor.
        /// </summary>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="period">The periodic delay.</param>
        /// <param name="executor">The executor to use for delaying the emission.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<long> Interval(TimeSpan initialDelay, TimeSpan period, IExecutorService executor)
        {
            return new FlowableInterval(initialDelay, period, executor);
        }

        /// <summary>
        /// Creates a per-ISubscriber resource, generates an IPublisher from that resource
        /// and relays its signals until completion when the resource is cleaned up via a callback.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="D">The resource type.</typeparam>
        /// <param name="resourceFactory">The function that creates a per-ISubscriber resource.</param>
        /// <param name="resourceMapper">The function that given the resource, it returns an IPublisher to relay events of.</param>
        /// <param name="resourceCleanup">The function that cleans up the resource once the IPublisher has terminated or the
        /// sequence is cancelled</param>
        /// <param name="eager">If true, the stateCleanup is called before the terminal signal is emitted;
        /// if false, the stateCleanup is called after the terminal signal is emitted.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Using<T, D>(Func<D> resourceFactory, Func<D, IPublisher<T>> resourceMapper, Action<D> resourceCleanup = null, bool eager = true)
        {
            return new FlowableUsing<T, D>(resourceFactory, resourceMapper, resourceCleanup, eager);
        }

        // ********************************************************************************
        // Multi-source factory methods
        // ********************************************************************************

        /// <summary>
        /// Relays events of the IPublisher that signals first.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The params array of IPublisher instances.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Amb<T>(params IPublisher<T>[] sources) {
            var n = sources.Length;
            if (n == 0)
            {
                return Empty<T>();
            }
            if (n == 1)
            {
                return FromPublisher(sources[0]);
            }
            return new FlowableAmbArray<T>(sources);
        }

        /// <summary>
        /// Relays events of the IPublisher that signals first.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher instances.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Amb<T>(IEnumerable<IPublisher<T>> sources)
        {
            return new FlowableAmbEnumerable<T>(sources);
        }

        /// <summary>
        /// Concatenates elements of IPublishers one after the other.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The params array of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Concat<T>(params IPublisher<T>[] sources)
        {
            return new FlowableConcatArray<T>(sources);
        }

        /// <summary>
        /// Concatenates elements of IPublishers one after the other.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Concat<T>(IEnumerable<IPublisher<T>> sources)
        {
            return new FlowableConcatEnumerable<T>(sources);
        }

        /// <summary>
        /// Concatenates elements of IPublishers one after the other.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Concat<T>(this IPublisher<IPublisher<T>> sources)
        {
            return Concat(sources, BufferSize());
        }

        /// <summary>
        /// Concatenates elements of IPublishers one after the other.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher of IPublisher sources.</param>
        /// <param name="prefetch">Number of inner IPublishers to prefetch from the outer IPublisher.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Concat<T>(this IPublisher<IPublisher<T>> sources, int prefetch)
        {
            return new FlowableConcatMapPublisher<IPublisher<T>, T>(sources, v => v, prefetch);
        }

        /// <summary>
        /// Concatenates the IPublishers one after the other while pre-running them.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The params array of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ConcatEager<T>(params IPublisher<T>[] sources)
        {
            return FromArray(sources).ConcatMapEager(v => v);
        }

        /// <summary>
        /// Concatenates the IPublishers one after the other while pre-running them.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ConcatEager<T>(IEnumerable<IPublisher<T>> sources)
        {
            return ConcatEager(sources, BufferSize(), BufferSize());
        }

        /// <summary>
        /// Concatenates the IPublishers one after the other while pre-running them.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="maxConcurrency">The maximum number of IPublishers to run at once.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ConcatEager<T>(IEnumerable<IPublisher<T>> sources, int maxConcurrency)
        {
            return ConcatEager(sources, maxConcurrency, BufferSize());
        }

        /// <summary>
        /// Concatenates the IPublishers one after the other while pre-running them.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="maxConcurrency">The maximum number of IPublishers to run at once.</param>
        /// <param name="prefetch">Number of items to prefetch from the inner IPublishers.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ConcatEager<T>(IEnumerable<IPublisher<T>> sources, int maxConcurrency, int prefetch)
        {
            return FromEnumerable(sources).ConcatMapEager(v => v, maxConcurrency, prefetch);
        }

        /// <summary>
        /// Concatenates elements of IPublishers one after the other.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ConcatEager<T>(this IPublisher<IPublisher<T>> sources)
        {
            return ConcatEager(sources, BufferSize(), BufferSize());
        }

        /// <summary>
        /// Concatenates the IPublishers one after the other while pre-running them.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="maxConcurrency">The maximum number of IPublishers to run at once.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ConcatEager<T>(this IPublisher<IPublisher<T>> sources, int maxConcurrency)
        {
            return ConcatEager(sources, maxConcurrency, BufferSize());
        }

        /// <summary>
        /// Concatenates the IPublishers one after the other while pre-running them.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="maxConcurrency">The maximum number of IPublishers to run at once.</param>
        /// <param name="prefetch">Number of items to prefetch from the inner IPublishers.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ConcatEager<T>(this IPublisher<IPublisher<T>> sources, int maxConcurrency, int prefetch)
        {
            return new FlowableConcatMapEagerPublisher<IPublisher<T>, T>(sources, v => v, maxConcurrency, prefetch);
        }

        /// <summary>
        /// Merges an array of IPublishers.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The params array of IPublisher instances.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Merge<T>(params IPublisher<T>[] sources)
        {
            return FromArray(sources).FlatMap(v => v);
        }

        /// <summary>
        /// Merges an array of IPublishers.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher instances.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Merge<T>(IEnumerable<IPublisher<T>> sources)
        {
            return FromEnumerable(sources).FlatMap(v => v);
        }

        /// <summary>
        /// Merges an array of IPublishers up to a maximum at a time.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The params array of IPublisher source.</param>
        /// <param name="maxConcurrency">The maximum number of inner IPublishers to merge at once.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Merge<T>(IEnumerable<IPublisher<T>> sources, int maxConcurrency)
        {
            return FromEnumerable(sources).FlatMap(v => v, maxConcurrency);
        }

        /// <summary>
        /// Merges an array of IPublishers up to a maximum at a time.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The params array of IPublisher source.</param>
        /// <param name="maxConcurrency">The maximum number of inner IPublishers to merge at once.</param>
        /// <param name="prefetch">The number of items to prefetch from the inner IPublishers.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Merge<T>(IEnumerable<IPublisher<T>> sources, int maxConcurrency, int prefetch)
        {
            return FromEnumerable(sources).FlatMap(v => v, maxConcurrency, prefetch);
        }

        /// <summary>
        /// Merges an array of IPublishers.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher of inner IPublisher instances.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Merge<T>(this IPublisher<IPublisher<T>> sources)
        {
            return Merge(sources, BufferSize(), BufferSize());
        }

        /// <summary>
        /// Merges an array of IPublishers up to a maximum at a time.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher of inner IPublisher instances.</param>
        /// <param name="maxConcurrency">The maximum number of inner IPublishers to merge at once.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Merge<T>(this IPublisher<IPublisher<T>> sources, int maxConcurrency)
        {
            return Merge(sources, maxConcurrency, BufferSize());
        }

        /// <summary>
        /// Merges an array of IPublishers up to a maximum at a time.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher of inner IPublisher instances.</param>
        /// <param name="maxConcurrency">The maximum number of inner IPublishers to merge at once.</param>
        /// <param name="bufferSize">The number of items to prefetch from the inner IPublishers.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Merge<T>(this IPublisher<IPublisher<T>> sources, int maxConcurrency, int bufferSize)
        {
            return new FlowableFlatMapPublisher<IPublisher<T>, T>(sources, v => v, maxConcurrency, bufferSize);
        }

        /// <summary>
        /// Combines the latest elements from all the IPublishers via a combiner function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="combiner">The function that receives the array of the latest items and
        /// returns the value to be emitted to downstream.</param>
        /// <param name="sources">The params array of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> CombineLatest<T, R>(Func<T[], R> combiner, params IPublisher<T>[] sources)
        {
            return CombineLatest(combiner, BufferSize(), sources);
        }

        /// <summary>
        /// Combines the latest elements from all the IPublishers via a combiner function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="combiner">The function that receives the array of the latest items and
        /// returns the value to be emitted to downstream.</param>
        /// <param name="prefetch">The number of items to prefetch from each IPublisher.</param>
        /// <param name="sources">The params array of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> CombineLatest<T, R>(Func<T[], R> combiner, int prefetch, params IPublisher<T>[] sources)
        {
            var n = sources.Length;
            if (n == 0)
            {
                return Empty<R>();
            }
            if (n == 1)
            {
                return FromPublisher(sources[0]).Map(v => combiner(new T[] { v }));
            }
            return new FlowableCombineLatest<T, R>(sources, combiner, prefetch);
        }

        /// <summary>
        /// Combines the latest elements from all the IPublishers via a combiner function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="combiner">The function that receives the array of the latest items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> CombineLatest<T, R>(IEnumerable<IPublisher<T>> sources, Func<T[], R> combiner)
        {
            return CombineLatest(sources, combiner, BufferSize());
        }

        /// <summary>
        /// Combines the latest elements from all the IPublishers via a combiner function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="combiner">The function that receives the array of the latest items and
        /// returns the value to be emitted to downstream.</param>
        /// <param name="prefetch">The number of items to prefetch from each IPublisher.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> CombineLatest<T, R>(IEnumerable<IPublisher<T>> sources, Func<T[], R> combiner, int prefetch)
        {
            return new FlowableCombineLatestEnumerable<T, R>(sources, combiner, prefetch);
        }

        /// <summary>
        /// Converts a sequence of type T into a sequence of (boxed) objects.
        /// </summary>
        /// <typeparam name="T">The input value type</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<object> Boxed<T>(this IPublisher<T> source)
        {
            return new FlowableBoxed<T>(source);
        }

        /// <summary>
        /// Combines the latest elements from all the IPublishers via a combiner function.
        /// </summary>
        /// <typeparam name="T1">The first input value type.</typeparam>
        /// <typeparam name="T2">The first input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="source1">The first input IPublisher source.</param>
        /// <param name="source2">The second input IPublisher source.</param>
        /// <param name="combiner">The function that receives the array of the latest items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> CombineLatest<T1, T2, R>(IPublisher<T1> source1, IPublisher<T2> source2, Func<T1, T2, R> combiner)
        {
            return CombineLatest<object, R>(
                a => combiner((T1)a[0], (T2)a[1]), 
                source1.Boxed(), source2.Boxed());
        }

        /// <summary>
        /// Combines the latest elements from all the IPublishers via a combiner function.
        /// </summary>
        /// <typeparam name="T1">The first input value type.</typeparam>
        /// <typeparam name="T2">The first input value type.</typeparam>
        /// <typeparam name="T3">The third input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="source1">The first input IPublisher source.</param>
        /// <param name="source2">The second input IPublisher source.</param>
        /// <param name="source3">The third input IPublisher source.</param>
        /// <param name="combiner">The function that receives the array of the latest items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> CombineLatest<T1, T2, T3, R>(IPublisher<T1> source1, IPublisher<T2> source2,
            IPublisher<T3> source3, Func<T1, T2, T3, R> combiner)
        {
            return CombineLatest<object, R>(
                a => combiner((T1)a[0], (T2)a[1], (T3)a[2]),
                source1.Boxed(), source2.Boxed(), source3.Boxed());
        }

        /// <summary>
        /// Combines the latest elements from all the IPublishers via a combiner function.
        /// </summary>
        /// <typeparam name="T1">The first input value type.</typeparam>
        /// <typeparam name="T2">The first input value type.</typeparam>
        /// <typeparam name="T3">The third input value type.</typeparam>
        /// <typeparam name="T4">The fourth input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="source1">The first input IPublisher source.</param>
        /// <param name="source2">The second input IPublisher source.</param>
        /// <param name="source3">The third input IPublisher source.</param>
        /// <param name="source4">The fourth input IPublisher source.</param>
        /// <param name="combiner">The function that receives the array of the latest items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> CombineLatest<T1, T2, T3, T4, R>(IPublisher<T1> source1, IPublisher<T2> source2,
            IPublisher<T3> source3, IPublisher<T4> source4, Func<T1, T2, T3, T4, R> combiner)
        {
            return CombineLatest<object, R>(
                a => combiner((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3]),
                source1.Boxed(), source2.Boxed(), source3.Boxed(), source4.Boxed());
        }

        /// <summary>
        /// Combines the next elements from all the IPublishers via a zipper function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="zipper">The function that receives the array of the next items and
        /// returns the value to be emitted to downstream.</param>
        /// <param name="sources">The params array of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Zip<T, R>(Func<T[], R> zipper, params IPublisher<T>[] sources)
        {
            return Zip(zipper, BufferSize(), sources);
        }

        /// <summary>
        /// Combines the next elements from all the IPublishers via a zipper function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="zipper">The function that receives the array of the next items and
        /// returns the value to be emitted to downstream.</param>
        /// <param name="prefetch">The number of items to prefetch from each IPublisher.</param>
        /// <param name="sources">The params array of IPublisher sources.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Zip<T, R>(Func<T[], R> zipper, int prefetch, params IPublisher<T>[] sources)
        {
            return new FlowableZipArray<T, R>(sources, zipper, prefetch);
        }

        /// <summary>
        /// Combines the next elements from all the IPublishers via a zipper function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="zipper">The function that receives the array of the next items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Zip<T, R>(IEnumerable<IPublisher<T>> sources, Func<T[], R> zipper)
        {
            return Zip(sources, zipper, BufferSize());
        }

        /// <summary>
        /// Combines the next elements from all the IPublishers via a zipper function.
        /// </summary>
        /// <typeparam name="T">The common input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="sources">The enumerable of IPublisher sources.</param>
        /// <param name="zipper">The function that receives the array of the next items and
        /// returns the value to be emitted to downstream.</param>
        /// <param name="prefetch">The number of items to prefetch from each IPublisher.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Zip<T, R>(IEnumerable<IPublisher<T>> sources, Func<T[], R> zipper, int prefetch)
        {
            return new FlowableZipEnumerable<T, R>(sources, zipper, prefetch);
        }

        /// <summary>
        /// Combines the next elements from all the IPublishers via a zipper function.
        /// </summary>
        /// <typeparam name="T1">The first input value type.</typeparam>
        /// <typeparam name="T2">The first input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="source1">The first input IPublisher source.</param>
        /// <param name="source2">The second input IPublisher source.</param>
        /// <param name="zipper">The function that receives the array of the next items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Zip<T1, T2, R>(IPublisher<T1> source1, IPublisher<T2> source2, Func<T1, T2, R> zipper)
        {
            return Zip<object, R>(
                a => zipper((T1)a[0], (T2)a[1]),
                source1.Boxed(), source2.Boxed());
        }

        /// <summary>
        /// Combines the next elements from all the IPublishers via a zipper function.
        /// </summary>
        /// <typeparam name="T1">The first input value type.</typeparam>
        /// <typeparam name="T2">The first input value type.</typeparam>
        /// <typeparam name="T3">The third input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="source1">The first input IPublisher source.</param>
        /// <param name="source2">The second input IPublisher source.</param>
        /// <param name="source3">The third input IPublisher source.</param>
        /// <param name="zipper">The function that receives the array of the next items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Zip<T1, T2, T3, R>(IPublisher<T1> source1, IPublisher<T2> source2, 
            IPublisher<T3> source3, Func<T1, T2, T3, R> zipper)
        {
            return Zip<object, R>(
                a => zipper((T1)a[0], (T2)a[1], (T3)a[2]),
                source1.Boxed(), source2.Boxed(), source3.Boxed());
        }

        /// <summary>
        /// Combines the next elements from all the IPublishers via a zipper function.
        /// </summary>
        /// <typeparam name="T1">The first input value type.</typeparam>
        /// <typeparam name="T2">The first input value type.</typeparam>
        /// <typeparam name="T3">The third input value type.</typeparam>
        /// <typeparam name="T4">The fourth input value type.</typeparam>
        /// <typeparam name="R">The output value type.</typeparam>
        /// <param name="source1">The first input IPublisher source.</param>
        /// <param name="source2">The second input IPublisher source.</param>
        /// <param name="source3">The third input IPublisher source.</param>
        /// <param name="source4">The fourth input IPublisher source.</param>
        /// <param name="zipper">The function that receives the array of the next items and
        /// returns the value to be emitted to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Zip<T1, T2, T3, T4, R>(IPublisher<T1> source1, IPublisher<T2> source2,
            IPublisher<T3> source3, IPublisher<T4> source4, Func<T1, T2, T3, T4, R> zipper)
        {
            return Zip<object, R>(
                a => zipper((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3]),
                source1.Boxed(), source2.Boxed(), source3.Boxed(), source4.Boxed());
        }

        /// <summary>
        /// Switches to emitting the items of the inner IPublisher when the outer IPublisher
        /// produces that IPublisher.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher producing the inner IPublisher instances</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> SwitchOnNext<T>(IPublisher<IPublisher<T>> sources)
        {
            return SwitchOnNext(sources, BufferSize());
        }

        /// <summary>
        /// Switches to emitting the items of the inner IPublisher when the outer IPublisher
        /// produces that IPublisher.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="sources">The IPublisher producing the inner IPublisher instances</param>
        /// <param name="prefetch">The number of items to prefetch from each iner IPublisher source.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> SwitchOnNext<T>(IPublisher<IPublisher<T>> sources, int prefetch)
        {
            return new FlowableSwitchMapPublisher<IPublisher<T>, T>(sources, v => v, prefetch);
        }

        // ********************************************************************************
        // Instance operators
        // ********************************************************************************

        public static IParallelFlowable<T> Parallel<T>(this IFlowable<T> source)
        {
            return Parallel(source, Environment.ProcessorCount, BufferSize());
        }

        public static IParallelFlowable<T> Parallel<T>(this IFlowable<T> source, int parallelism)
        {
            return Parallel(source, parallelism, BufferSize());
        }

        public static IParallelFlowable<T> Parallel<T>(this IFlowable<T> source, int parallelism, int bufferSize)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the upstream values into values to be emitted to downstream via
        /// a mapper function.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The downstream value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that maps each upstream value
        /// into a downstream value.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> Map<T, R>(this IFlowable<T> source, Func<T, R> mapper)
        {
            return new FlowableMap<T, R>(source, mapper);
        }

        public static IFlowable<R> MapAsync<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<R> MapAsync<T, U, R>(this IFlowable<T> source, Func<T, IPublisher<U>> mapper, Func<T, U, R> resultMapper)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Filters the upstream values based on a predicate function and
        /// allows those passing through for which the predicate returns true.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="predicate">The function that receives each item and
        /// should return true for those that should be passed onto the 
        /// downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Filter<T>(this IFlowable<T> source, Func<T, bool> predicate)
        {
            return new FlowableFilter<T>(source, predicate);
        }

        public static IFlowable<T> FilterAsync<T>(this IFlowable<T> source, Func<T, IPublisher<bool>> predicate)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Consumes and relays up to the specified number of items from upstream,
        /// cancels it and emits a completion signal to the downstream.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="n">The number of items to let pass.</param>
        /// <param name="limitRequest">If true, the operator doesn't request
        /// more than <paramref name="n"/>; if false and if the downstream
        /// requests more than <paramref name="n"/>, the operator requests
        /// in an unbounded manner.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Take<T>(this IFlowable<T> source, long n, bool limitRequest = false)
        {
            return new FlowableTake<T>(source, n, limitRequest);
        }

        /// <summary>
        /// Skips the given number of items from the source IFlowable and
        /// relays the rest.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="n">The number of items to skip.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Skip<T>(this IFlowable<T> source, long n)
        {
            return new FlowableSkip<T>(source, n);
        }

        /// <summary>
        /// Takes the last number of items from the source IFlowable and relays them 
        /// to downstream.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="n">The number of last items to keep and relay.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> TakeLast<T>(this IFlowable<T> source, int n)
        {
            if (n <= 0)
            {
                return Empty<T>();
            }
            if (n == 1)
            {
                return new FlowableTakeLastOne<T>(source);
            }
            return new FlowableTakeLast<T>(source, n);
        }

        /// <summary>
        /// Skips the last given number of items from the source IFlowable.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="n">The number of items to skip from the end of the stream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> SkipLast<T>(this IFlowable<T> source, int n)
        {
            if (n <= 0)
            {
                return source;
            }
            return new FlowableSkipLast<T>(source, n);
        }

        /// <summary>
        /// Collects the items of the upstream into a collection provided via 
        /// a function for each subscriber and via collector function.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="C">The collection type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="collectionSupplier">The function called for each subscriber to generate
        /// a per-subscriber collection which will be emitted at the end.</param>
        /// <param name="collector">The function that "adds" the upstream item into the collection.</param>
        /// <returns></returns>
        public static IFlowable<C> Collect<T, C>(this IFlowable<T> source, Func<C> collectionSupplier, Action<C, T> collector)
        {
            return new FlowableCollect<T, C>(source, collectionSupplier, collector);
        }

        public static IFlowable<T> Reduce<T>(this IFlowable<T> source, Func<T, T, T> reducer)
        {
            return new FlowableReducePlain<T>(source, reducer);
        }

        public static IFlowable<R> Reduce<T, R>(this IFlowable<T> source, Func<R> initialSupplier, Func<R, T, R> reducer)
        {
            return new FlowableReduce<T, R>(source, initialSupplier, reducer);
        }

        /// <summary>
        /// Collects all upstream values into a List and emits it to the downstream.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="capacityHint">The number of items expected from downstream, positive.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<IList<T>> ToList<T>(this IFlowable<T> source, int capacityHint = 10)
        {
            return Collect(source, () => new List<T>(capacityHint), (a, b) => a.Add(b));
        }

        /// <summary>
        /// Sums up a source of ints into a single int value.
        /// </summary>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<int> SumInt(this IFlowable<int> source)
        {
            return Reduce(source, (a, b) => a + b);
        }

        /// <summary>
        /// Sums up a source of longs into a single long value.
        /// </summary>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<long> SumLong(this IFlowable<long> source)
        {
            return Reduce(source, (a, b) => a + b);
        }

        public static IFlowable<int> MaxInt(this IFlowable<int> source)
        {
            return Reduce(source, (a, b) => Math.Max(a, b));
        }

        public static IFlowable<T> Max<T>(this IFlowable<T> source, IComparer<T> comparer)
        {
            return Reduce(source, (a, b) => comparer.Compare(a, b) < 0 ? b : a);
        }

        public static IFlowable<long> MaxLong(this IFlowable<long> source)
        {
            return Reduce(source, (a, b) => Math.Max(a, b));
        }

        public static IFlowable<int> MinInt(this IFlowable<int> source)
        {
            return Reduce(source, (a, b) => Math.Min(a, b));
        }

        public static IFlowable<long> MinLong(this IFlowable<long> source)
        {
            return Reduce(source, (a, b) => Math.Min(a, b));
        }

        public static IFlowable<T> Min<T>(this IFlowable<T> source, IComparer<T> comparer)
        {
            return Reduce(source, (a, b) => comparer.Compare(a, b) < 0 ? a : b);
        }

        /// <summary>
        /// Ignores all upstream values and only relay the terminal event.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns></returns>
        public static IFlowable<T> IgnoreElements<T>(this IFlowable<T> source)
        {
            return new FlowableIgnoreElements<T>(source);
        }

        /// <summary>
        /// Maps each upstream value into an IPublisher and merges them
        /// together in a potentially interleaved fashion.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that takes each upstream item
        /// and turns them into an IPublisher source to be merged.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> FlatMap<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper)
        {
            return FlatMap(source, mapper, BufferSize(), BufferSize());
        }

        /// <summary>
        /// Maps each upstream value into an IPublisher and merges them
        /// together in a potentially interleaved fashion, running
        /// up to the specified number of IPublisher's at a time.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that takes each upstream item
        /// and turns them into an IPublisher source to be merged.</param>
        /// <param name="maxConcurrency">The maximum number of IPublishers to run at a time.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> FlatMap<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper, int maxConcurrency)
        {
            return FlatMap(source, mapper, maxConcurrency, BufferSize());
        }

        /// <summary>
        /// Maps each upstream value into an IPublisher and merges them
        /// together in a potentially interleaved fashion, running
        /// up to the specified number of IPublisher's at a time
        /// and prefetches the given number of items from each IPublishers.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that takes each upstream item
        /// and turns them into an IPublisher source to be merged.</param>
        /// <param name="maxConcurrency">The maximum number of IPublishers to run at a time.</param>
        /// <param name="bufferSize">The number of items to prefetch and buffer from each inner IPublisher.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> FlatMap<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper, int maxConcurrency, int bufferSize)
        {
            return new FlowableFlatMap<T, R>(source, mapper, maxConcurrency, bufferSize);
        }

        /// <summary>
        /// Subscribes to and optionally requests on the specified executor's thread.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="executor">The IExecutorService whose IExecutorWorker to use for
        /// subscribing and requesting.</param>
        /// <param name="requestOn">If true, requests towards the upstream will be executed on
        /// the same worker thread; if false, requests are forwarded on the same thread it
        /// was issued.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> SubscribeOn<T>(this IFlowable<T> source, IExecutorService executor, bool requestOn = true)
        {
            return new FlowableSubscribeOn<T>(source, executor, requestOn);
        }

        /// <summary>
        /// Makes sure signals from upstream are delivered to the downstream from
        /// a thread specified by the given IExecutorService's worker.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="executor">The IExecutorService whose worker to use for delivering
        /// signals to downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ObserveOn<T>(this IFlowable<T> source, IExecutorService executor)
        {
            return ObserveOn(source, executor, BufferSize());
        }

        /// <summary>
        /// Makes sure signals from upstream are delivered to the downstream from
        /// a thread specified by the given IExecutorService's worker and
        /// using the given buffer size for prefetching and buffering items
        /// until the downstream can process them.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="executor">The IExecutorService whose worker to use for delivering
        /// signals to downstream.</param>
        /// <param name="bufferSize">The number of items to prefetch and hold in the internal buffer.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> ObserveOn<T>(this IFlowable<T> source, IExecutorService executor, int bufferSize)
        {
            return new FlowableObserveOn<T>(source, executor, bufferSize);
        }

        /// <summary>
        /// Changes the request patter to request the given amount upfront and
        /// after 75% has been delivered, it request that 75% amount from upstream
        /// again.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="batchSize">The number of items to prefetch and hold
        /// in the buffer.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> RebatchRequests<T>(this IFlowable<T> source, int batchSize)
        {
            return ObserveOn(source, Executors.Immediate, batchSize);
        }

        public static IFlowable<T> Delay<T>(this IFlowable<T> source, TimeSpan delay)
        {
            return Delay(source, delay, Executors.Computation);
        }

        public static IFlowable<T> Delay<T>(this IFlowable<T> source, TimeSpan delay, IExecutorService executor)
        {
            return new FlowableDelay<T>(source, delay, executor);
        }

        public static IFlowable<T> DelaySubscription<T, U>(this IFlowable<T> source, IPublisher<U> other)
        {
            return new FlowableDelaySubscription<T, U>(source, other);
        }

        public static IFlowable<T> DelaySubscription<T>(this IFlowable<T> source, TimeSpan delay)
        {
            return DelaySubscription(source, Timer(delay, Executors.Computation));
        }

        public static IFlowable<T> DelaySubscription<T, U>(this IFlowable<T> source, TimeSpan delay, IExecutorService executor)
        {
            return DelaySubscription(source, Timer(delay, executor));
        }

        public static IFlowable<R> ConcatMap<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper)
        {
            return ConcatMap(source, mapper, 2);
        }

        public static IFlowable<R> ConcatMap<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper, int prefetch)
        {
            return new FlowableConcatMap<T, R>(source, mapper, prefetch);
        }

        /// <summary>
        /// Maps the upstream values into IPublishers, pre-runs them and
        /// concatenates their items in order.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The inner IPublisher and result value type.</typeparam>
        /// <param name="source">The source IFlowable instance</param>
        /// <param name="mapper">The mapper that turns an upstream value into an IPublisher.</param>
        /// <returns>The new IFlowable instance</returns>
        public static IFlowable<R> ConcatMapEager<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper)
        {
            return ConcatMapEager(source, mapper, BufferSize(), BufferSize());
        }

        /// <summary>
        /// Maps the upstream values into IPublishers, pre-runs them and
        /// concatenates their items in order.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The inner IPublisher and result value type.</typeparam>
        /// <param name="source">The source IFlowable instance</param>
        /// <param name="mapper">The mapper that turns an upstream value into an IPublisher.</param>
        /// <param name="maxConcurrency">The maximum number of concurrently running inner IPublishers, positive</param>
        /// <returns>The new IFlowable instance</returns>
        public static IFlowable<R> ConcatMapEager<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper, int maxConcurrency)
        {
            return ConcatMapEager(source, mapper, maxConcurrency, BufferSize());
        }

        /// <summary>
        /// Maps the upstream values into IPublishers, pre-runs them and
        /// concatenates their items in order.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The inner IPublisher and result value type.</typeparam>
        /// <param name="source">The source IFlowable instance</param>
        /// <param name="mapper">The mapper that turns an upstream value into an IPublisher.</param>
        /// <param name="maxConcurrency">The maximum number of concurrently running inner IPublishers, positive</param>
        /// <param name="prefetch">The number of items to prefetch from each inner IPublisher</param>
        /// <returns>The new IFlowable instance</returns>
        public static IFlowable<R> ConcatMapEager<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper, int maxConcurrency, int prefetch)
        {
            return new FlowableConcatMapEager<T, R>(source, mapper, maxConcurrency, prefetch);
        }

        /// <summary>
        /// Hides the identity of the source IFlowable and prevents
        /// identity-based optimizations over the flow.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance</param>
        /// <returns>The new IFlowable instance</returns>
        public static IFlowable<T> Hide<T>(this IFlowable<T> source)
        {
            return new FlowableHide<T>(source);
        }

        /// <summary>
        /// Relays items that haven't been seen before (based on the default
        /// IEqualityComparer for the type T).
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source Flowable instance</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Distinct<T>(this IFlowable<T> source)
        {
            return Distinct(source, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Relays items that haven't been seen before (based on the 
        /// provided IEqualityComparer instance).
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source Flowable instance</param>
        /// <param name="comparer">The comparer to compare items.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> Distinct<T>(this IFlowable<T> source, IEqualityComparer<T> comparer)
        {
            return new FlowableDistinct<T>(source, comparer);
        }

        /// <summary>
        /// Relays items if one item is different from the previous one
        /// (by comparing with the default IEqualityComparer for type T).
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> DistinctUntilChanged<T>(this IFlowable<T> source)
        {
            return DistinctUntilChanged(source, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Relays items if one item is different from the previous one
        /// (by comparing with the given IEqualityComparer).
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="comparer">The comparer to use for comparing subsequent items.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> DistinctUntilChanged<T>(this IFlowable<T> source, IEqualityComparer<T> comparer)
        {
            return new FlowableDistinctUntilChanged<T>(source, comparer);
        }

        public static IFlowable<T> TakeUntil<T, U>(this IFlowable<T> source, IPublisher<U> other)
        {
            return new FlowableTakeUntil<T, U>(source, other);
        }

        public static IFlowable<T> SkipUntil<T, U>(this IFlowable<T> source, IPublisher<U> other)
        {
            return new FlowableSkipUntil<T, U>(source, other);
        }

        public static IFlowable<R> Lift<T, R>(this IFlowable<T> source, Func<IFlowableSubscriber<R>, IFlowableSubscriber<T>> lifter)
        {
            return new FlowableLift<T, R>(source, lifter);
        }

        /// <summary>
        /// Invokes the composer function with the given source IFlowable
        /// in a fluent fashion.
        /// </summary>
        /// <typeparam name="T">The source value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="composer">The function called with the source IFlowable
        /// and should produce an IPublisher as a result.</param>
        /// <returns>The result of the function call.</returns>
        public static IFlowable<R> Compose<T, R>(this IFlowable<T> source, Func<IFlowable<T>, IPublisher<R>> composer)
        {
            return composer(source).ToFlowable();
        }

        public static R To<T, R>(this IFlowable<T> source, Func<IFlowable<T>, R> converter)
        {
            return converter(source);
        }

        /// <summary>
        /// Defers the call to the composer function to subscription time,
        /// allowing a per-Subscriber state to be created.
        /// </summary>
        /// <typeparam name="T">The input value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="composer">The function that receives the upstream source and
        /// should return the IPublisher to be subscribed to by the downstream.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> DeferCompose<T, R>(this IFlowable<T> source, Func<IFlowable<T>, IPublisher<R>> composer)
        {
            return Defer(() => composer(source));
        }

        /// <summary>
        /// Switches to emitting the values of the latest IPublisher mapped from
        /// the upstream value, cancelling the previous IPublisher in the process.
        /// </summary>
        /// <typeparam name="T">The input value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that receives an item from upstream
        /// and turns it into an IPublisher whose elements should be relayed then on.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> SwitchMap<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper)
        { 
            return SwitchMap(source, mapper, BufferSize());
        }

        /// <summary>
        /// Switches to emitting the values of the latest IPublisher mapped from
        /// the upstream value, cancelling the previous IPublisher in the process.
        /// </summary>
        /// <typeparam name="T">The input value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that receives an item from upstream
        /// and turns it into an IPublisher whose elements should be relayed then on.</param>
        /// <param name="prefetch">The number of items to prefetch from each inner IPublisher.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<R> SwitchMap<T, R>(this IFlowable<T> source, Func<T, IPublisher<R>> mapper, int prefetch)
        {
            return new FlowableSwitchMap<T, R>(source, mapper, prefetch);
        }

        public static IFlowable<T> DefaultIfEmpty<T>(this IFlowable<T> source, T defaultItem)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> SwitchIfEmpty<T>(this IFlowable<T> source, IPublisher<T> fallback)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> SwitchIfEmpty<T>(this IFlowable<T> source, params IPublisher<T>[] fallbacks)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> SwitchIfEmpty<T>(this IFlowable<T> source, IEnumerable<IPublisher<T>> fallbacks)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Repeat<T>(this IFlowable<T> source, long times = long.MaxValue)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Repeat<T>(this IFlowable<T> source, Func<bool> stop, long times = long.MaxValue)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> RepeatWhen<T, U>(this IFlowable<T> source, Func<IFlowable<object>, IPublisher<U>> handler)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Retry<T>(this IFlowable<T> source, long times = long.MaxValue)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Retry<T>(this IFlowable<T> source, Func<Exception, bool> predicate, long times = long.MaxValue)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> RetryWhen<T, U>(this IFlowable<T> source, Func<IFlowable<Exception>, IPublisher<U>> handler)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> OnErrorReturn<T>(this IFlowable<T> source, T fallbackItem)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> OnErrorComplete<T>(this IFlowable<T> source)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> OnErrorResumeNext<T>(this IFlowable<T> source, IPublisher<T> fallback)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> OnErrorResumeNext<T>(this IFlowable<T> source, Func<Exception, IPublisher<T>> handler)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Timeout<T>(this IFlowable<T> source, TimeSpan itemTimeout, IPublisher<T> fallback = null)
        {
            return Timeout<T>(source, itemTimeout, Executors.Computation, fallback);
        }

        public static IFlowable<T> Timeout<T>(this IFlowable<T> source, TimeSpan itemTimeout, IExecutorService executor, IPublisher<T> fallback = null)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Timeout<T>(this IFlowable<T> source, TimeSpan firstTimeout, TimeSpan itemTimeout, IPublisher<T> fallback = null)
        {
            return Timeout<T>(source, firstTimeout, itemTimeout, Executors.Computation, fallback);
        }

        public static IFlowable<T> Timeout<T>(this IFlowable<T> source, TimeSpan firstTimeout, TimeSpan itemTimeout, IExecutorService executor, IPublisher<T> fallback = null)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Consumes the upstream source in an unbounded manner and
        /// signals InvalidOperationException if the downstream can't
        /// keep up.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> OnBackpressureError<T>(this IFlowable<T> source)
        {
            return new FlowableOnBackpressureError<T>(source);
        }

        /// <summary>
        /// Consumes the upstream source in an unbounded manner and
        /// drops items (calling the onDrop action if available)
        /// if the downstream can't keep up.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onDrop">The optional action called when an item is dropped.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> OnBackpressureDrop<T>(this IFlowable<T> source, Action<T> onDrop = null)
        {
            return new FlowableOnBackpressureDrop<T>(source, onDrop);
        }

        /// <summary>
        /// Consumes the upstream source in an unbounded fashion and and the
        /// downstream picks up the latest available item on its own pace.
        /// The provided Action will be called with the unclaimed items.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onDrop">The action called with the unclaimed items.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> OnBackpressureLatest<T>(this IFlowable<T> source, Action<T> onDrop = null)
        {
            return new FlowableOnBackpressureLatest<T>(source, onDrop);
        }

        /// <summary>
        /// Consumes the upsteram source in an unbounded manner and
        /// buffers all items if the downstream can't keep up.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> OnBackpressureBuffer<T>(this IFlowable<T> source)
        {
            return OnBackpressureBuffer(source, BufferSize(), BufferStrategy.ALL, null);
        }

        public static IFlowable<T> OnBackpressureBuffer<T>(this IFlowable<T> source, int capacityHint, BufferStrategy strategy = BufferStrategy.ALL, Action<T> onDrop = null)
        {
            if (strategy == BufferStrategy.ALL)
            {
                return new FlowableOnBackpressureBufferAll<T>(source, capacityHint);
            }
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<IGroupedFlowable<K, T>> GroupBy<T, K>(this IFlowable<T> source, Func<T, K> keyMapper)
        {
            return GroupBy<T, K, T>(source, keyMapper, v => v);
        }

        public static IFlowable<IGroupedFlowable<K, V>> GroupBy<T, K, V>(this IFlowable<T> source, Func<T, K> keyMapper, Func<T, V> valueMapper)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<R> WithLatestFrom<T, U, R>(this IFlowable<T> source, IPublisher<U> other, Func<T, U, R> combiner)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<R> WithLatestFrom<T, R>(this IFlowable<T> source, Func<T[], R> combiner, params IPublisher<T>[] others)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<R> WithLatestFrom<T, R>(this IFlowable<T> source, Func<T[], R> combiner, IEnumerable<IPublisher<T>> others)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Sample<T>(this IFlowable<T> source, TimeSpan period)
        {
            return Sample(source, period, Executors.Computation);
        }

        public static IFlowable<T> Sample<T>(this IFlowable<T> source, TimeSpan period, IExecutorService executor)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Sample<T, U>(this IFlowable<T> source, IPublisher<U> sampler)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Debounce<T>(this IFlowable<T> source, TimeSpan delay)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> ThrottleFirst<T>(this IFlowable<T> source, TimeSpan delay)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> ThrottleLast<T>(this IFlowable<T> source, TimeSpan delay)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> ThrottleWithTimeout<T>(this IFlowable<T> source, TimeSpan delay)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<IList<T>> Buffer<T>(this IFlowable<T> source, int size)
        {
            return Buffer(source, size, size, () => new List<T>());
        }

        public static IFlowable<C> Buffer<T, C>(this IFlowable<T> source, int size, Func<C> collectionSupplier) where C : ICollection<T>
        {
            return Buffer(source, size, size, collectionSupplier);
        }

        public static IFlowable<IList<T>> Buffer<T>(this IFlowable<T> source, int size, int skip)
        {
            return Buffer(source, size, skip, () => new List<T>());
        }

        public static IFlowable<C> Buffer<T, C>(this IFlowable<T> source, int size, int skip, Func<C> collectionSupplier) where C : ICollection<T>
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<IList<T>> Buffer<T, U>(this IFlowable<T> source, IPublisher<U> boundary)
        {
            return Buffer(source, boundary, () => new List<T>());
        }

        public static IFlowable<C> Buffer<T, U, C>(this IFlowable<T> source, IPublisher<U> boundary, Func<C> collectionSupplier) where C : ICollection<T>
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<IFlowable<T>> Window<T>(this IFlowable<T> source, int size)
        {
            return Window(source, size, size);
        }

        public static IFlowable<IFlowable<T>> Window<T>(this IFlowable<T> source, int size, int skip)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<IFlowable<T>> Window<T, U>(this IFlowable<T> source, IPublisher<U> boundary)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> Scan<T, R>(this IFlowable<T> source, Func<T, T, T> scanner)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<R> Scan<T, R>(this IFlowable<T> source, Func<R> initialSupplier, Func<R, T, R> scanner)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> AmbWith<T>(this IFlowable<T> source, IPublisher<T> other)
        {
            return Amb(source, other);
        }

        public static IFlowable<T> ConcatWith<T>(this IFlowable<T> source, IPublisher<T> other)
        {
            return Concat(source, other);
        }

        public static IFlowable<T> MergeWith<T>(this IFlowable<T> source, IPublisher<T> other)
        {
            return Merge(source, other);
        }

        public static IFlowable<R> ZipWith<T, U, R>(this IFlowable<T> source, IPublisher<U> other, Func<T, U, R> zipper)
        {
            return Zip(source, other, zipper);
        }

        /// <summary>
        /// Maps the upstream values onto IEnumerables and relays the items in them in a sequential manner.
        /// </summary>
        /// <typeparam name="T">The source value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that maps items to IEnumerables.</param>
        /// <returns>The new Flowable instance.</returns>
        public static IFlowable<R> FlatMapEnumerable<T, R>(this IFlowable<T> source, Func<T, IEnumerable<R>> mapper)
        {
            return FlatMapEnumerable(source, mapper, BufferSize());
        }

        /// <summary>
        /// Maps the upstream values onto IEnumerables and relays the items in them in a sequential manner.
        /// </summary>
        /// <typeparam name="T">The source value type.</typeparam>
        /// <typeparam name="R">The result value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="mapper">The function that maps items to IEnumerables.</param>
        /// <param name="prefetch">The number of items to prefetch from the upstream.</param>
        /// <returns>The new Flowable instance.</returns>
        public static IFlowable<R> FlatMapEnumerable<T, R>(this IFlowable<T> source, Func<T, IEnumerable<R>> mapper, int prefetch)
        {
            return new FlowableFlatMapEnumerable<T, R>(source, mapper, prefetch);
        }

        /// <summary>
        /// Takes elements from upstream while the predicate returns true for the
        /// current item.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="predicate">The function called with the current item and should
        /// return true for emitting and continuing or false for ending the sequence.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> TakeWhile<T>(this IFlowable<T> source, Func<T, bool> predicate)
        {
            return new FlowableTakeWhile<T>(source, predicate);
        }

        /// <summary>
        /// Takes items from upstream until the predicate returns true after the
        /// current item.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="predicate">The predicate that receives the current item
        /// and should return true to stop the sequence.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> TakeUntil<T>(this IFlowable<T> source, Func<T, bool> predicate)
        {
            return new FlowableTakeUntilPredicate<T>(source, predicate);
        }

        /// <summary>
        /// Skips the upstream items while the predicate returns true
        /// then relays the remaining items.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="predicate">The function that takes the current item and returns
        /// true to skip it or false to relay it and all subsequent items.</param>
        /// <returns>The new IFlowable instance.</returns>
        public static IFlowable<T> SkipWhile<T>(this IFlowable<T> source, Func<T, bool> predicate)
        {
            return new FlowableSkipWhile<T>(source, predicate);
        }

        // ********************************************************************************
        // IConnectableFlowable related
        // ********************************************************************************

        public static IConnectableFlowable<T> Publish<T>(this IFlowable<T> source)
        {
            return Publish(source, BufferSize());
        }

        public static IConnectableFlowable<T> Publish<T>(this IFlowable<T> source, int bufferSize)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<R> Publish<T, R>(this IFlowable<T> source, Func<IFlowable<T>, IPublisher<R>> handler)
        {
            return Publish(source, handler, BufferSize());
        }

        public static IFlowable<R> Publish<T, R>(this IFlowable<T> source, Func<IFlowable<T>, IPublisher<R>> handler, int bufferSize)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IConnectableFlowable<T> Replay<T>(this IFlowable<T> source)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IConnectableFlowable<R> Replay<T, R>(this IFlowable<T> source, Func<IFlowable<T>, IPublisher<R>> handler)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowable<T> AutoConnect<T>(this IConnectableFlowable<T> source, int count = 1, Action<IDisposable> onConnect = null)
        {
            if (count == 0)
            {
                source.Connect(onConnect);
                return source;
            }
            return new FlowableAutoConnect<T>(source, count, onConnect);
        }

        public static IFlowable<T> RefCount<T>(this IConnectableFlowable<T> source, int count = 1)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        public static IFlowableProcessor<T> RefCount<T>(this IFlowableProcessor<T> source)
        {
            if (source is FlowableProcessorRefCount<T>)
            {
                return source;
            }
            return new FlowableProcessorRefCount<T>(source);
        }

        public static IFlowableProcessor<T> Serialize<T>(this IFlowableProcessor<T> source)
        {
            if (source is FlowableProcessorSerialize<T> s)
            {
                return s;
            }
            return new FlowableProcessorSerialize<T>(source);
        }

        // ********************************************************************************
        // State peeking methods
        // ********************************************************************************

        public static IFlowable<T> DoOnNext<T>(this IFlowable<T> source, Action<T> onNext)
        {
            return FlowablePeek<T>.Create(source, onNext: onNext);
        }

        public static IFlowable<T> DoAfterNext<T>(this IFlowable<T> source, Action<T> onAfterNext)
        {
            return FlowablePeek<T>.Create(source, onAfterNext: onAfterNext);
        }

        public static IFlowable<T> DoOnError<T>(this IFlowable<T> source, Action<Exception> onError)
        {
            return FlowablePeek<T>.Create(source, onError: onError);
        }

        public static IFlowable<T> DoOnComplete<T>(this IFlowable<T> source, Action onComplete)
        {
            return FlowablePeek<T>.Create(source, onComplete: onComplete);
        }

        public static IFlowable<T> DoOnTerminated<T>(this IFlowable<T> source, Action onTerminated)
        {
            return FlowablePeek<T>.Create(source, onTerminated: onTerminated);
        }

        public static IFlowable<T> DoAfterTerminated<T>(this IFlowable<T> source, Action onAfterTerminated)
        {
            return FlowablePeek<T>.Create(source, onAfterTerminated: onAfterTerminated);
        }

        public static IFlowable<T> DoFinally<T>(this IFlowable<T> source, Action onFinally)
        {
            return new FlowableDoFinally<T>(source, onFinally);
        }

        public static IFlowable<T> DoOnSubscribe<T>(this IFlowable<T> source, Action<ISubscription> onSubscribe)
        {
            return FlowablePeek<T>.Create(source, onSubscribe: onSubscribe);
        }

        public static IFlowable<T> DoOnRequest<T>(this IFlowable<T> source, Action<long> onRequest)
        {
            return FlowablePeek<T>.Create(source, onRequest: onRequest);
        }

        public static IFlowable<T> DoOnCancel<T>(this IFlowable<T> source, Action onCancel)
        {
            return FlowablePeek<T>.Create(source, onCancel: onCancel);
        }

        // ********************************************************************************
        // Consumer methods
        // ********************************************************************************

        /// <summary>
        /// Consume the source IFlowable.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The IDisposable that allows cancelling the sequence.</returns>
        public static IDisposable Subscribe<T>(this IFlowable<T> source)
        {
            return Subscribe(source, v => { }, e => { }, () => { });
        }

        /// <summary>
        /// Consume the source IFlowable and call the given action
        /// with each item received.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onNext">The Action called with each item received.</param>
        /// <returns>The IDisposable that allows cancelling the sequence.</returns>
        public static IDisposable Subscribe<T>(this IFlowable<T> source, Action<T> onNext)
        {
            return Subscribe(source, onNext, e => { }, () => { });
        }

        /// <summary>
        /// Consume the source IFlowable and call the given actions
        /// with each item received or the error.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onNext">The Action called with each item received.</param>
        /// <param name="onError">The Action called with the error.</param>
        /// <returns>The IDisposable that allows cancelling the sequence.</returns>
        public static IDisposable Subscribe<T>(this IFlowable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            return Subscribe(source, onNext, onError, () => { });
        }

        /// <summary>
        /// Consume the source IFlowable and call the given actions
        /// with each item received, the error or the completion event.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onNext">The Action called with each item received.</param>
        /// <param name="onError">The Action called with the error.</param>
        /// <param name="onComplete">The Action called when the upstream completes.</param>
        /// <returns>The IDisposable that allows cancelling the sequence.</returns>
        public static IDisposable Subscribe<T>(this IFlowable<T> source, Action<T> onNext, Action<Exception> onError, Action onComplete)
        {
            var s = new ActionSubscriber<T>(onNext, onError, onComplete);
            source.Subscribe(s);
            return s;
        }

        /// <summary>
        /// Subscribes the given IFlowableSubscriber (descendant) to the
        /// source IFlowable and return the same IFlowableSubscriber instance.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="S">The IFlowableSubscriber (subclass) type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="subscriber">The IFlowableSubsriber (subclass) instance.</param>
        /// <returns>The <paramref name="subscriber"/>.</returns>
        public static S SubscribeWith<T, S>(this IFlowable<T> source, S subscriber) where S : IFlowableSubscriber<T>
        {
            source.Subscribe(subscriber);
            return subscriber;
        }

        /// <summary>
        /// Returns a Task that returns the first element from the source
        /// IFlowable.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="cts">The cancellation token source to allow cancelling the sequence.</param>
        /// <returns>The new Task instance.</returns>
        public static Task<T> FirstTask<T>(this IFlowable<T> source, CancellationTokenSource cts)
        {
            var s = new TaskFirstSubscriber<T>(cts);
            source.Subscribe(s);
            return s.Task;
        }

        /// <summary>
        /// Returns a Task that returns the last element from the source
        /// IFlowable.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="cts">The cancellation token source to allow cancelling the sequence.</param>
        /// <returns>The new Task instance.</returns>
        public static Task<T> LastTask<T>(this IFlowable<T> source, CancellationTokenSource cts)
        {
            var s = new TaskLastSubscriber<T>(cts);
            source.Subscribe(s);
            return s.Task;
        }

        /// <summary>
        /// Returns a Task that succeeds or fails when the source
        /// IFlowable terminates.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="cts">The cancellation token source to allow cancelling the sequence.</param>
        /// <returns>The new Task instance.</returns>
        public static Task IgnoreElementsTask<T>(this IFlowable<T> source, CancellationTokenSource cts)
        {
            var s = new TaskIgnoreElementsSubscriber<T>(cts);
            source.Subscribe(s);
            return s.Task;
        }

        // ********************************************************************************
        // Blocking operators
        // ********************************************************************************

        /// <summary>
        /// Blocks until the source produces its first item or completes.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance</param>
        /// <param name="result">the first item</param>
        /// <returns>True if the soruce produced an item, false if the source was empty.</returns>
        public static bool BlockingFirst<T>(this IFlowable<T> source, out T result)
        {
            var s = new BlockingFirstSubscriber<T>();
            source.Subscribe(s);
            return s.BlockingGet(out result);
        }

        /// <summary>
        /// Blocks until the source produces its last item or completes.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance</param>
        /// <param name="result">the first item</param>
        /// <returns>True if the soruce produced an item, false if the source was empty.</returns>
        public static bool BlockingLast<T>(this IFlowable<T> source, out T result)
        {
            var s = new BlockingLastSubscriber<T>();
            source.Subscribe(s);
            return s.BlockingGet(out result);
        }

        /// <summary>
        /// Returns an IEnumerable that when enumerated, subscribes to
        /// the source and blocks for each item and terminal event
        /// before relaying it through the IEnumerator.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <returns>The new IEnumerable instance.</returns>
        public static IEnumerable<T> BlockingEnumerable<T>(this IFlowable<T> source)
        {
            return BlockingEnumerable(source, BufferSize());
        }

        /// <summary>
        /// Returns an IEnumerable that when enumerated, subscribes to
        /// the source and blocks for each item and terminal event
        /// before relaying it through the IEnumerator.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="prefetch">The number of items to prefetch.</param>
        /// <returns>The new IEnumerable instance.</returns>
        public static IEnumerable<T> BlockingEnumerable<T>(this IFlowable<T> source, int prefetch)
        {
            var parent = new BlockingEnumeratorSubscriber<T>(prefetch);
            source.Subscribe(parent);

            while (parent.MoveNext())
            {
                yield return parent.Current;
            }
            yield break;
        }

        /// <summary>
        /// Blocks until the source terminates.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        public static void BlockingSubscribe<T>(this IFlowable<T> source)
        {
            BlockingSubscribe(source, v => { }, e => { }, () => { });
        }

        /// <summary>
        /// Consumes the IFlowable source in a blocking
        /// fashion and calls the action on the current thread.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onNext">The action to call with the items from the source.</param>
        public static void BlockingSubscribe<T>(this IFlowable<T> source, Action<T> onNext)
        {
            BlockingSubscribe(source, onNext, e => { }, () => { });
        }

        /// <summary>
        /// Consumes the IFlowable source in a blocking
        /// fashion and calls the actions on the current thread.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onNext">The action to call with the items from the source.</param>
        /// <param name="onError">The action to call with the error from the source.</param>
        public static void BlockingSubscribe<T>(this IFlowable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            BlockingSubscribe(source, onNext, onError, () => { });
        }

        /// <summary>
        /// Consumes the IFlowable source in a blocking
        /// fashion and calls the actions on the current thread.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onNext">The action to call with the items from the source.</param>
        /// <param name="onError">The action to call with the error from the source.</param>
        /// <param name="onComplete">The action to to call when the source terminates.</param>
        /// <param name="onConnect">The optional action called with the IDisposable representing the
        /// active connection that allows cancelling the flow.</param>
        public static void BlockingSubscribe<T>(this IFlowable<T> source, Action<T> onNext, Action<Exception> onError, Action onComplete, Action<IDisposable> onConnect = null)
        {
            BlockingSubscribe(source, onNext, onError, onComplete, BufferSize(), onConnect);
        }

        /// <summary>
        /// Consumes the IFlowable source in a blocking
        /// fashion and calls the actions on the current thread.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="onNext">The action to call with the items from the source.</param>
        /// <param name="onError">The action to call with the error from the source.</param>
        /// <param name="onComplete">The action to to call when the source terminates.</param>
        /// <param name="prefetch">The number of items to prefetch.</param>
        /// <param name="onConnect">The optional action called with the IDisposable representing the
        /// active connection that allows cancelling the flow.</param>
        public static void BlockingSubscribe<T>(this IFlowable<T> source, Action<T> onNext, Action<Exception> onError, Action onComplete, int prefetch, Action<IDisposable> onConnect = null)
        {
            var s = new BlockingLambdaSubscriber<T>(prefetch, onNext, onError, onComplete);
            onConnect?.Invoke(s);
            source.Subscribe(s);
            s.Run();
        }

        /// <summary>
        /// Consumes the IFlowable source and relays events to the
        /// given IFlowableSubscriber instance.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="subscriber">The IFlowableSubscriber that consumes the signals.</param>
        public static void BlockingSubscribe<T>(this IFlowable<T> source, IFlowableSubscriber<T> subscriber)
        {
            BlockingSubscribe(source, subscriber, BufferSize());
        }

        /// <summary>
        /// Consumes the IFlowable source and relays events to the
        /// given IFlowableSubscriber instance.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="subscriber">The IFlowableSubscriber that consumes the signals.</param>
        /// <param name="prefetch">The number of items to prefetch from the source.</param>
        public static void BlockingSubscribe<T>(this IFlowable<T> source, IFlowableSubscriber<T> subscriber, int prefetch)
        {
            var s = new BlockingSubscriber<T>(prefetch, subscriber);
            source.Subscribe(s);
            s.Run();
        }

        // ********************************************************************************
        // Test methods
        // ********************************************************************************

        /// <summary>
        /// Creates a TestSubscriber with the given initial settings
        /// and subscribes it to the source IFlowable.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="source">The source IFlowable instance.</param>
        /// <param name="initialRequest">The initial request amount, non-negative, default long.MaxValue.</param>
        /// <param name="cancel">If true, the TestSubscriber will be cancelled before subscribing to the source.</param>
        /// <returns>The new TestSubscriber instance.</returns>
        public static TestSubscriber<T> Test<T>(this IFlowable<T> source, long initialRequest = long.MaxValue, bool cancel = false)
        {
            var ts = new TestSubscriber<T>(initialRequest);
            if (cancel)
            {
                ts.Cancel();
            }
            source.Subscribe(ts);
            return ts;
        }
    }
}
