using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrimeFamilyGenerator
{
    public static class Extensions
    {
        /// <summary>
        /// Picks a random item from a list of object
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="list">List of objects</param>
        /// <returns>A random object from the list</returns>
        public static T PickRandom<T>(this IList<T> list) where T : class
        {
            return list.Count < 1 ? null : list[new Random(Guid.NewGuid().GetHashCode()-1).Next(0, list.Count)];
        }

        /// <summary>
        /// Creates an object represented by the provided XML 
        /// </summary>
        /// <typeparam name="T">{Object} to convert to</typeparam>
        /// <param name="xmlSerializedObject">XML deserialized into an Object</param>
        /// <returns></returns>
        public static T DeserializeTo<T>(this string xmlSerializedObject)
        {
            var xmlReader = new XmlTextReader(new StringReader(xmlSerializedObject.TrimStart()));
            var ser = new XmlSerializer(typeof(T));
            var obj = (T)ser.Deserialize(xmlReader);
            xmlReader.Close();
            return obj;
        }

        /// <summary>
        /// Creates XML of an object
        /// </summary>
        /// <param name="obj">this object</param>
        /// <param name="indent"></param>
        /// <returns>An XML representation of the object</returns>
        public static string Serialize(this object obj, bool indent)
        {
            var ser = new XmlSerializer(obj.GetType());
            var memStream = new MemoryStream();
            var xmlWriter = new XmlTextWriter(memStream, Encoding.UTF8) { Namespaces = true };
            if (indent)
                xmlWriter.Formatting = Formatting.Indented;
            ser.Serialize(xmlWriter, obj);

            var xml = Encoding.UTF8.GetString(memStream.GetBuffer(), 0, (int)memStream.Length);
            xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
            xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
            xmlWriter.Close();
            memStream.Close();
            return xml;
        }
    }
}