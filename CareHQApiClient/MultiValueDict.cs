using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Web;

namespace CareHQ
{
    /// <summary>
    /// Represents a collection of keys with multiple values
    /// </summary>
    public class MultiValueDict : IEnumerable
    {

        private Dictionary<string, List<string>> _dict 
            = new Dictionary<string, List<string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiValueDict"/> class
        /// that is empty
        /// </summary>
        public MultiValueDict() { }

        /// <summary>
        /// Adds a key to the <see cref="MultiValueDict"/> and inserts the 
        /// values
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>
        /// Returns a reference of the <see cref="MultiValueDict"/>, for 
        /// chaining
        /// </returns>
        public MultiValueDict Add(string key, params object[] values)
        {

            List<string> listValues;

            // Check if key exists, if not, add a new key/list
            if (!_dict.TryGetValue(key, out listValues))
            {
                listValues = new List<string>();
                _dict.Add(key, listValues);
            }

            foreach (object v in values)
            {
                if (v != null)
                    listValues?.Add($"{v}");
            }

            return this;
        }

        /// <summary>
        /// Removes a key from the <see cref="MultiValueDict"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// Returns the <see cref="MultiValueDict"/> for chaining
        /// </returns>
        public MultiValueDict Remove(string key)
        {
            _dict.Remove(key);
            return this;
        }

        /// <summary>
        /// Removes all keys and values from the <see cref="MultiValueDict"/>
        /// </summary>
        /// <returns>
        /// Return the <see cref="MultiValueDict"/> for chaining
        /// </returns>
        public MultiValueDict Clear()
        {
            _dict.Clear();
            return this;
        }

        /// <summary>
        /// Converts the <see cref="MultiValueDict"/> values to a 
        /// querystring as a <see cref="NameValueCollection"/>
        /// </summary>
        /// <returns>
        /// Returns the querystring as a <see cref="NameValueCollection"/>
        /// </returns>
        public NameValueCollection ToQueryString()
        {
            NameValueCollection query = HttpUtility.ParseQueryString("");
            foreach (KeyValuePair<string, List<string>> kvp in _dict)
            {
                foreach (var v in kvp.Value)
                {
                    query.Add(kvp.Key, v);
                }
            }
            return query;
        }

        /// <summary>
        /// Converts the dictionary values to form urlencoded name/values
        /// </summary>
        /// <returns>
        /// The form body as application/x-www-form-urlencoded MIME type
        /// </returns>
        public FormUrlEncodedContent ToFormBody()
        {
            return new FormUrlEncodedContent(ToBody());
        }

        /// <summary>
        /// Create a list of <see cref="KeyValuePair"/>s from the dictionary
        /// </summary>
        /// <returns>
        /// A list of <see cref="KeyValuePair"/> containing the 
        /// </returns>
        public List<KeyValuePair<string, string>> ToBody()
        {
            List<KeyValuePair<string, string>> kvpList 
                = new List<KeyValuePair<string, string>>();

            foreach (KeyValuePair<string, List<string>> kvp in _dict)
            {
                foreach (var v in kvp.Value)
                {
                    kvpList.Add(
                        new KeyValuePair<string, string>(kvp.Key, v)
                    );
                }
            }

            return kvpList;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("{");
            foreach (var entry in _dict)
            {
                sb.AppendLine(
                    $"  {entry.Key}: {String.Join(", ", entry.Value)}"
                );
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        public IEnumerator GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
}