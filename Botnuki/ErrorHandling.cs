using System;

namespace Botnuki
{
    public class ErrorHandling
    {
        public static void ThrowGenException(string file, string eventMethod, string exception)
        {
            Console.Write("An error has occurred!");
            Console.Write("File: " + file);
            Console.Write("Method/Event: " + eventMethod);
            Console.Write("Description: " + exception);
        }
    }
}
