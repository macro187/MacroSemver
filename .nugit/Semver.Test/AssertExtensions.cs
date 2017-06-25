using System;

namespace Semver.Test
{
    public static class AssertExtensions
    {

        public static void Throws<TException>(Action action)
            where TException : Exception
        {
            Throws(typeof(TException), action);
        }

        public static void Throws(Type exceptionType, Action action)
        {
            if (exceptionType == null) throw new ArgumentNullException(nameof(exceptionType));
            if (action == null) throw new ArgumentNullException(nameof(action));
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (exceptionType.IsAssignableFrom(e.GetType())) return;
                throw;
            }
            throw new Exception("Expected exception was not thrown");
        }

    }
}
