using System;
using System.Collections.Generic;
using System.Linq;

namespace Thon.Hotels.PactVerifier
{
    public enum Errors { Unknown, Validation, Http };

    public abstract class Result<T>
    {
    }

    public class Ok<T> : Result<T>
    {
        public Ok(T value)
        {
            Value = value;
        }
        public T Value { get; private set; }
    }

    public class Error<T> : Result<T>
    {

        public Error(Enum type, params string[] messages)
        {
            ErrorType = type;
            Messages = messages;
        }
        public Enum ErrorType { get; }

        public IEnumerable<string> Messages { get; private set; } = Enumerable.Empty<string>();
    }    
}
