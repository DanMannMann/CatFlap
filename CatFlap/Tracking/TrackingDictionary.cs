using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using FelineSoft.CatFlap.Extensions;

namespace FelineSoft.CatFlap
{
    /// <summary>
    /// The keys contain property names. The values are lists of foreign key names and their values, corresponding
    /// to the contents of the specified reference collection at the time it was retrieved from the database.
    /// </summary>
    public class TrackingDictionary : Dictionary<string,TrackedIdentities>
    {
        public static TrackingDictionary FromArray(TrackingEntry[] array)
        {
            TrackingDictionary result = new TrackingDictionary();
            foreach (var entry in array)
            {
                result.Add(entry.ReferencePropertyName, TrackedIdentities.FromArray(entry.TrackedEntities));
            }
            return result;
        }

        public TrackingEntry[] ToArray()
        {
            var result = new TrackingEntry[this.Count];
            int i = 0;
            foreach (var item in this)
            {
                result[i] = new TrackingEntry() { ReferencePropertyName = item.Key, TrackedEntities = item.Value.ToArray() };
                i++;
            }
            return result;
        }

        public string ToBase64()
        {
            var target = ToArray();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream mr = new MemoryStream();
            bf.Serialize(mr, target);
            var base64 = System.Convert.ToBase64String(mr.ToArray());
            var zipped = base64.ZipString();
            base64 = System.Convert.ToBase64String(zipped);
            return base64;
        }

        public static TrackingDictionary FromBase64(string base64)
        {
            BinaryFormatter bf = new BinaryFormatter();
            var unzipped = System.Convert.FromBase64String(base64).UnzipString();
            MemoryStream ms = new MemoryStream(System.Convert.FromBase64String(unzipped));
            var built = (TrackingEntry[])bf.Deserialize(ms);
            return FromArray(built);
        }
    }
}
