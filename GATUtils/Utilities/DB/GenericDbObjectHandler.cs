using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GATUtils.Utilities.DB
{
    public static class GenericDbObjectHandler
    {
        /// <summary>
        /// Gets an enumerable object from a Data reader.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">DB command reader.</param>
        /// <param name="buildObject">The expected ouptput object command</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnumerableObjectFromReader<T>(IDataReader reader, Func<IDataRecord, T> buildObject)
        {
            try
            {
                while (reader.Read())
                {
                    yield return buildObject(reader);
                }
            }
            finally
            {
                reader.Dispose();
            }
        }
    }
}
