using System;

namespace InvenageAPI.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AccessRightAttribute : Attribute
    {
        public string Scope { get; set; }

        public AccessRightAttribute(string scope)
        {
            Scope = scope;
        }
    }
}
