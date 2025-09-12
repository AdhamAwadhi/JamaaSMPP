/************************************************************************
 * Factory for creating IResponseHandler based on ResponseHandlerOptions.
 ************************************************************************/
using System;

namespace JamaaTech.Smpp.Net.Lib
{
    public static class ResponseHandlerFactory
    {
        private static ResponseHandlerOptions _options;
        private static readonly object _sync = new object();

        public static IResponseHandler Create()
        {
            return Create(_options);
        }

        public static IResponseHandler Create(ResponseHandlerOptions options)
        {
            if (options == null) options = new ResponseHandlerOptions();

            switch (options.Implementation.Trim().ToLowerInvariant())
            {
                case "v1":
                case "legacy":
                    // You may optionally modify the legacy ResponseHandler to implement IResponseHandler.
                    return new ResponseHandler() { DefaultResponseTimeout = options.DefaultResponseTimeout };
                case "concurrent": 
                    return new ConcurrentResponseHandler(options);
               
                case "v2":
                case "default":
                default:
                    return new ResponseHandlerV2() { DefaultResponseTimeout = options.DefaultResponseTimeout };
            }
        }

        public static void Configure(ResponseHandlerOptions options, bool throwIfAlreadyConfigured = true)
        {
            if (options == null) throw new ArgumentNullException("options");
            lock (_sync)
            {
                if (_options != null)
                {
                    if (throwIfAlreadyConfigured)
                        throw new InvalidOperationException("ResponseHandlerOptions already configured.");
                    return;
                }
                _options = options;
            }
        }
    }
}