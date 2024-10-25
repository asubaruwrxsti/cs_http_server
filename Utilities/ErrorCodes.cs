namespace ModularHttpServer.Utilities
{
    public static class ErrorCodes
    {
        public const string LoggerServiceNotFound = "ERR001";
        public const string RequestHandlerServiceNotFound = "ERR002";
        public const string ServerUrlNotConfigured = "ERR003";
        public const string NoHandlerForPath = "ERR004";

        public static string GetErrorMessage(string errorCode)
        {
            return errorCode switch
            {
                LoggerServiceNotFound => "Logger service not found.",
                RequestHandlerServiceNotFound => "RequestHandler service not found.",
                ServerUrlNotConfigured => "Server URL is not configured.",
                NoHandlerForPath => "No handler found for path.",
                _ => "Unknown error."
            };
        }
    }
}