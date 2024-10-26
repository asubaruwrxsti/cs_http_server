using ModularHttpServer.Middlewares;

namespace ModularHttpServer.Utilities
{
    public static class ErrorCodes
    {
        public const string LoggerServiceNotFound = "ERR001";
        public const string RequestHandlerServiceNotFound = "ERR002";
        public const string ServerUrlNotConfigured = "ERR003";
        public const string NoHandlerForPath = "ERR004";
        public const string URIPrefixNotEndingInSlash = "ERR006";
        public const string MiddlewarePipelineNotFound = "ERR005";
        public const string ServerError = "ERR007";
        public const string StaticFilesRootNotConfigured = "ERR008";

        public static string GetErrorMessage(string errorCode)
        {
            return errorCode switch
            {
                LoggerServiceNotFound => "Logger service not found.",
                RequestHandlerServiceNotFound => "RequestHandler service not found.",
                ServerUrlNotConfigured => "Server URL is not configured.",
                NoHandlerForPath => "No handler found for path.",
                URIPrefixNotEndingInSlash => "Only URI prefixes ending in '/' are allowed.",
                MiddlewarePipelineNotFound => "Middleware pipeline not found.",
                ServerError => "An error occurred while processing the request.",
                StaticFilesRootNotConfigured => "Static files root is not configured.",
                _ => "Unknown error."
            };
        }
    }
}