using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Unity.Networking.Transport;

namespace Incode.Networking
{
    public static class NetworkUtils
    {

        public static long PingServer(string serverAddr)
        {

            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            int timeout = 120;

            IPAddress serverIP;
            int port;
            EndpointParse(serverAddr, out serverIP, out port, 0);
            PingReply reply = pingSender.Send(serverIP, timeout, buffer, options);

            if (reply.Status == IPStatus.Success)
            {
                return reply.RoundtripTime;
            }
            else
            {
                Debug.LogError($"Ping Server Failed: {reply.Status.ToString()}");
            }

            return -1;
        }

        public static bool EndpointParse(string endpoint, out IPAddress address, out int port, int defaultPort)
        {
            string address_part;
            address = null;
            port = 0;

            if (endpoint.Contains(":"))
            {
                int.TryParse(endpoint.AfterLast(":"), out port);
                address_part = endpoint.BeforeFirst(":");
            }
            else
                address_part = endpoint;

            if (port == 0) { port = defaultPort; }

            // Resolve in case we got a hostname
            var resolvedAddress = System.Net.Dns.GetHostAddresses(address_part);
            foreach (var r in resolvedAddress)
            {
                if (r.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    // Pick first ipv4
                    address = r;
                    return true;
                }
            }
            return false;
        }

        public static bool WritePackedVector3(ref DataStreamWriter writer, Vector3 vec3, NetworkCompressionModel compressionModel)
        {
            bool succeeded = writer.WritePackedFloat(vec3.x, compressionModel) &&
                                writer.WritePackedFloat(vec3.y, compressionModel) &&
                                writer.WritePackedFloat(vec3.z, compressionModel);
            return succeeded;
        }

        public static bool WritePackedQuaternion(ref DataStreamWriter writer, Quaternion quat, NetworkCompressionModel compressionModel)
        {
            bool succeeded = writer.WritePackedFloat(quat.x, compressionModel) &&
                                writer.WritePackedFloat(quat.y, compressionModel) &&
                                writer.WritePackedFloat(quat.z, compressionModel) &&
                                writer.WritePackedFloat(quat.w, compressionModel);
            return succeeded;
        }

        public static bool WritePackedFloats(ref DataStreamWriter writer, float[] values, NetworkCompressionModel compressionModel)
        {
            bool succeeded = true;
            for (int i = 0; i < values.Length; ++i)
            {
                bool packed = writer.WritePackedFloat(values[i], compressionModel);
                if (packed == false)
                {
                    succeeded = false;
                }
            }
            return succeeded;
        }

        public static Vector3 ReadPackedVector3(ref DataStreamReader reader, NetworkCompressionModel compressionModel)
        {
            return new Vector3(reader.ReadPackedFloat(compressionModel),
                                reader.ReadPackedFloat(compressionModel),
                                reader.ReadPackedFloat(compressionModel));
        }

        public static Quaternion ReadPackedQuaternion(ref DataStreamReader reader, NetworkCompressionModel compressionModel)
        {
            return new Quaternion(reader.ReadPackedFloat(compressionModel),
                                    reader.ReadPackedFloat(compressionModel),
                                    reader.ReadPackedFloat(compressionModel),
                                    reader.ReadPackedFloat(compressionModel));
        }
    }
}
