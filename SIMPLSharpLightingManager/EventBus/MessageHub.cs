namespace Easy.MessageHub
{
    using System;
    using System.Diagnostics;
    using Crestron.SimplSharp;


    /// <summary>
    /// An implementation of the <c>Event Aggregator</c> pattern.
    /// </summary>
    public sealed class MessageHub : IMessageHub
    {
        private Action<Type, object> _globalHandler;
        private Action<Guid, Exception> _globalErrorHandler;
        private static readonly MessageHub instance = new MessageHub();

        static MessageHub() { }

        private MessageHub() { }

        /// <summary>
        /// Returns a single instance of the <see cref="MessageHub"/>
        /// </summary>
        public static MessageHub Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Registers a callback which is invoked on every message published by the <see cref="MessageHub"/>.
        /// <remarks>Invoking this method with a new <paramref name="onMessage"/>overwrites the previous one.</remarks>
        /// </summary>
        /// <param name="onMessage">
        /// The callback to invoke on every message
        /// <remarks>The callback receives the type of the message and the message as arguments</remarks>
        /// </param>
        public void RegisterGlobalHandler(Action<Type, object> onMessage)
        {
            EnsureNotNull(onMessage);
            _globalHandler = onMessage;
        }

        /// <summary>
        /// Invoked if an error occurs when publishing a message to a subscriber.
        /// <remarks>Invoking this method with a new <paramref name="onError"/>overwrites the previous one.</remarks>
        /// </summary>
        public void RegisterGlobalErrorHandler(Action<Guid, Exception> onError)
        {
            EnsureNotNull(onError);
            _globalErrorHandler = onError;
        }

        /// <summary>
        /// Publishes the <paramref name="message"/> on the <see cref="MessageHub"/>.
        /// </summary>
        /// <param name="message">The message to published</param>
        public void Publish<T>(T message)
        {

            var localSubscriptions = Subscriptions.GetTheLatestSubscriptions();

            var msgType = typeof(T);


#if NET_STANDARD
            var msgTypeInfo = msgType.GetTypeInfo();
#endif
            if (_globalHandler != null)
                _globalHandler.Invoke(msgType, message);

            // ReSharper disable once ForCanBeConvertedToForeach | Performance Critical
            for (var idx = 0; idx < localSubscriptions.Length; idx++)
            {
                var subscription = localSubscriptions[idx];

#if NET_STANDARD
                if (!subscription.Type.GetTypeInfo().IsAssignableFrom(msgTypeInfo)) { continue; }
#else
                if (!subscription.Type.IsAssignableFrom(msgType)) { continue; }
#endif
                try
                {
                    subscription.Handle(message);
                }
                catch (Exception e)
                {
                    if (_globalErrorHandler != null)
                        _globalErrorHandler.Invoke(subscription.Token, e);
                }
            }

        }

        /// <summary>
        /// Subscribes a callback against the <see cref="MessageHub"/> for a specific type of message.
        /// </summary>
        /// <typeparam name="T">The type of message to subscribe to</typeparam>
        /// <param name="action">The callback to be invoked once the message is published on the <see cref="MessageHub"/></param>
        /// <returns>The token representing the subscription</returns>
        public Guid Subscribe<T>(Action<T> action, Predicate<T> filter)
        {
            return Subscribe(action, filter, TimeSpan.Zero);
        }

        public Guid Subscribe<T>(Action<T> action)
        {
            return Subscribe(action, TimeSpan.Zero);
        }

        /// <summary>
        /// Subscribes a callback against the <see cref="MessageHub"/> for a specific type of message.
        /// </summary>
        /// <typeparam name="T">The type of message to subscribe to</typeparam>
        /// <param name="action">The callback to be invoked once the message is published on the <see cref="MessageHub"/></param>
        /// <param name="throttleBy">The <see cref="TimeSpan"/> specifying the rate at which subscription is throttled</param>
        /// <returns>The token representing the subscription</returns>

        public Guid Subscribe<T>(Action<T> action, TimeSpan throttleBy)
        {
            EnsureNotNull(action);
            return Subscriptions.Register(throttleBy, action, null);
        }


        public Guid Subscribe<T>(Action<T> action, Predicate<T> filter, TimeSpan throttleBy)
        {
            EnsureNotNull(action);
            EnsureNotNull(filter);

            return Subscriptions.Register(throttleBy, action, filter);
        }

        /// <summary>
        /// Unsubscribes a subscription from the <see cref="MessageHub"/>.
        /// </summary>
        /// <param name="token">The token representing the subscription</param>
        public void Unsubscribe(Guid token)
        {
            Subscriptions.UnRegister(token);
        }

        /// <summary>
        /// Checks if a specific subscription is active on the <see cref="MessageHub"/>.
        /// </summary>
        /// <param name="token">The token representing the subscription</param>
        /// <returns><c>True</c> if the subscription is active otherwise <c>False</c></returns>
        public bool IsSubscribed(Guid token)
        {
            return Subscriptions.IsRegistered(token);
        }
        /// <summary>
        /// Clears all the subscriptions from the <see cref="MessageHub"/>.
        /// <remarks>The global handler and the global error handler are not affected</remarks>
        /// </summary>
        public void ClearSubscriptions()
        {
            Subscriptions.Clear();
        }

        /// <summary>
        /// Disposes the <see cref="MessageHub"/>.
        /// </summary>
        public void Dispose()
        {
            _globalHandler = null;
            ClearSubscriptions();
        }

        [DebuggerStepThrough]
        private void EnsureNotNull(object obj)
        {
            //This was nameof(obj), but changing it to move forward.
            if (obj == null) { throw new NullReferenceException(obj.ToString()); }
        }
    }
}
