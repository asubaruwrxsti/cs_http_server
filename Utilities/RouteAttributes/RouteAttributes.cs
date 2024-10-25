using System;

namespace ModularHttpServer.Utilities.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RouteAttribute : Attribute
    {
        public string Path { get; }

        public RouteAttribute(string path)
        {
            Path = path;
        }
    }
}