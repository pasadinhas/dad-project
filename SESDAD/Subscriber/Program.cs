﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 5) {
                Console.Error.WriteLine("Wrong usage.");
                return;                                
            }
            string nl = Environment.NewLine;
            Console.WriteLine("Port: {0}" + nl + "Name: {1}" + nl + "OrderingPolicy: {2}"
                + nl + "Routing policy: {3}" + nl + "LoggingPolicy: {4}" + nl
                + "PuppetMasterLogService: {5}",
                args[0], args[1], args[2], args[3], args[4], args[5]);
            Console.WriteLine("Brokers:");
            string[] brokers = new string[args.Length - 6];
            for (int i = 6; i < args.Length; i++)
            {
                brokers[i - 6] = args[i];
                Console.WriteLine(args[i]);
            }

            TcpChannel channel = new TcpChannel(int.Parse(args[0]));
            ChannelServices.RegisterChannel(channel, false);
            SubscriberServer subscriber = new SubscriberServer(args[5],args[4],brokers);
            RemotingServices.Marshal(subscriber, args[1], typeof(SubscriberServer));
            Console.WriteLine("Subscriber up and running....");
            Console.ReadLine();
        }
    }
}
