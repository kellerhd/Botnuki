using System;

namespace Botnuki
{
    public class ErrorHandling
    {
        public static string ThrowGenException(string file, string eventMethod, string exception)
        {
            return $"An error has occurred!\r\nFile: { file }\r\nMethod/Event: { eventMethod }\r\nDescription: { exception}";
        }
    }
}
