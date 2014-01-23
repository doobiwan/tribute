using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using ErrorEventArgs = BookSleeve.ErrorEventArgs;

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

        public static void OnErrorHandler(object sender, ErrorEventArgs args)
        {
            Debugger.Break();
            throw (args.Exception);
        }

        public static string GetObjectHash(byte[] requestData)
        {
            string hash;
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                hash = Convert.ToBase64String(sha1.ComputeHash(requestData));
            }
            return hash;
        }
    }
}