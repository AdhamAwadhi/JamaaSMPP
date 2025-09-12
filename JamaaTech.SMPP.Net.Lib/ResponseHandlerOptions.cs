/************************************************************************
 * Options for configuring an IResponseHandler implementation.
 ************************************************************************/
namespace JamaaTech.Smpp.Net.Lib
{
    /// <summary>
    /// Options to configure a response handler implementation.
    /// </summary>
    public class ResponseHandlerOptions
    {
        /// <summary>
        /// Default timeout in milliseconds (minimum enforced = 5000).
        /// </summary>
        public int DefaultResponseTimeout { get; set; } = 5000;

        /// <summary>
        /// Name of implementation to load: "default" (legacy) or "concurrent".
        /// </summary>
        public string Implementation { get; set; } = "concurrent";
    }
}