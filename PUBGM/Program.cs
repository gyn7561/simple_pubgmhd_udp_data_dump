using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PUBGM
{
    class Program
    {
        static HashSet<long> whiteList = new HashSet<long>();
        static void Main(string[] args)
        {
            for (int i = 0; i < 100; i++)
            {
                //微信区IP
                var host = "wx" + i + ".pg.qq.com";
                try
                {
                    var ipList = Dns.GetHostAddresses(host);
                    foreach (var ip in ipList)
                    {
                        whiteList.Add(ip.Address);
                    }
                }
                catch (Exception)
                {

                }
            }
            var devices = CaptureDeviceList.Instance.ToList();
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine(i + ":\r\n" + devices[i]);
            }
            //var device = devices[3];
            Console.WriteLine("选择监听网卡");
            var device = devices[int.Parse(Console.ReadLine())];
            device.Open();
            device.OnPacketArrival += Device_OnPacketArrival;
            device.StartCapture();
        }

        private static void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var udpPacket = (PacketDotNet.UdpPacket)packet.Extract(typeof(PacketDotNet.UdpPacket));

            if (udpPacket != null)
            {
                var ipv4Packet = udpPacket.ParentPacket as IPv4Packet;
                if (ipv4Packet != null && (
                    whiteList.Contains(ipv4Packet.DestinationAddress.Address)
                    ||
                    whiteList.Contains(ipv4Packet.SourceAddress.Address)
                    ))
                {
                    Console.Write(udpPacket.PayloadData.Length + "  " + DateTime.Now + " ");
                    Console.WriteLine(BitConverter.ToString(udpPacket.PayloadData, 0));
                }
            }

        }
    }
}
