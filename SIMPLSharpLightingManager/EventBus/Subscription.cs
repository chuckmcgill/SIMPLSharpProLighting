namespace Easy.MessageHub
{
    using System;
    using SSharpDateTimeLibrary;

    internal sealed class Subscription
    {
        private const long TicksMultiplier = 1000 * TimeSpan.TicksPerMillisecond;
        private readonly long _throttleByTicks;
        private double? _lastHandleTimestamp;

        private readonly Guid _token;
        private readonly Type _type;
        private readonly object _handler;
        private readonly object _filter;
        private object _handle;

        internal Subscription(Type type, Guid token, TimeSpan throttleBy, object handler)
        {
            _type = type;
            _token = token;
            _handler = handler;
            _throttleByTicks = throttleBy.Ticks;
        }

        internal Subscription(Type type, Guid token, TimeSpan throttleBy, object handler, object filter)
        {
            _type = type;
            _token = token;
            _handler = handler;
            _filter = filter;
            _throttleByTicks = throttleBy.Ticks;
        }

        internal void Handle<T>(T message)
        {

            if (!CanHandle()) { return; }

            var handler = Handler as Action<T>;
            var filter = Filter as Predicate<T>;


            // ReSharper disable once PossibleNullReferenceException
            if (filter(message))
            {
                handler(message);
            }

        }


        internal bool CanHandle()
        {


            if (_throttleByTicks == 0) { return true; }



            if (_lastHandleTimestamp == null)
            {
                _lastHandleTimestamp = DateTimePrecise.TickCountLong;
                return true;
            }


            // DateTimePrecise.Now;

            //var now = Stopwatch.GetTimestamp();
            var now = DateTimePrecise.TickCountLong;


            //Was Stopwatch.Frequency that changed to TimeSpan.TicksPerSecond

            var durationInTicks = (now - _lastHandleTimestamp) / TimeSpan.TicksPerSecond * TicksMultiplier;

            if (durationInTicks >= _throttleByTicks)
            {
                _lastHandleTimestamp = now;
                return true;
            }

            return false;

        }


        internal Guid Token
        {
            get
            {
                return _token;
            }
        }

        internal Type Type
        {
            get
            {
                return _type;
            }
        }
        private object Handler
        {
            get
            {
                return _handler;
            }
        }

        internal object Filter
        {
            get
            {
                return _filter;
            }
        }
    }
}