using System;

namespace IEBroker.net46
{
    class Constant
    {
        public const String MODE_INSTALL = "install";
        public const String MODE_REMOVE = "remove";
        public const String MODE_EXECUTE = "execute";
        public const String MODE_EXECUTE_HIDDEN = "execute-without-console";
        public const String MODE_USAGE = "usage";

        public const String DEFAULT_PROTOCOL_GENERAL = "iepp";
        public const String DEFAULT_PROTOCOL_SECURE = "iepps";
        public const String DEFAULT_EXECUTABLE_PATH = @"C:\Program Files\Internet Explorer\iexplore.exe";

        public const int ERROR_NO_ERROR = 0;
        public const int ERROR_ARGS_NOT_EXIST = 101;
        public const int ERROR_ARG0_NO_OPTION = 102;
    }
}
