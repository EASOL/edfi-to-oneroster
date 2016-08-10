using System;

namespace EF2OR.Models
{
    public class EF2ORCustomException : Exception
    {
        public EF2ORCustomException()
        {
        }

        public EF2ORCustomException(string message)
        : base(message)
        {
        }

        public EF2ORCustomException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}