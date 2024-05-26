using System;
using System.Collections.Generic;
using System.Text;

namespace Rolfin.QueryBuilder
{
    internal static class Guards
    {
        public static void NullOrWhiteSpace(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
        }
    }
}
