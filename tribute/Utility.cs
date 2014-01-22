using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace tribute
{
    static internal class Utility
    {
        public static byte[] Serialize<TRequest>(TRequest request)
        {
            var jsonSerializer = new JsonSerializer();
            byte[] bytes;

            using (var ms = new MemoryStream())
            using (var bson = new BsonWriter(ms))
            {
                jsonSerializer.Serialize(bson, request);
                bytes = ms.ToArray();
            }

            return bytes;
        }

        public static TResponse Deserialize<TResponse>(byte[] responseData)
        {
            var jsonSerializer = new JsonSerializer();
            TResponse response = default(TResponse);

            using (var ms = new MemoryStream(responseData))
            using (var bson = new BsonReader(ms))
            {
                response = jsonSerializer.Deserialize<TResponse>(bson);
            }

            return response;
        }
    }
}