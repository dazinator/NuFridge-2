using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace NuFridge.Shared.Exceptions
{
    public class OptionException : Exception
    {
        private readonly string _option;

        public string OptionName
        {
            get
            {
                return _option;
            }
        }

        public OptionException()
        {
        }

        public OptionException(string message, string optionName)
            : base(message)
        {
            _option = optionName;
        }

        public OptionException(string message, string optionName, Exception innerException)
            : base(message, innerException)
        {
            _option = optionName;
        }

        protected OptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _option = info.GetString("OptionName");
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("OptionName", _option);
        }
    }
}
