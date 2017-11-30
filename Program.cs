//
// Program.cs is the entry point for the trace route program
//
// Author: Dan Brunwasser (drb8650)
//

using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace traceroute
{
	class Program
	{
		// Specify program constants
		const int timeout = 1000;
		const int maxHops = 30;
		const int numTests = 3;

		//
		// Main method for running the trace route program
		//
		// @param args The given command line arguments
		static void Main(string[] args)
		{
			// Print usage if no host is given
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: traceroute [host]");
				return;
			}

			// Store the IPv4 and IPv6 addresses for the host
			IPAddress ipv4 = null;
			IPAddress ipv6 = null;

			// Try getting the addresses using the hostname
			try
			{
				// Query DNS for IPs
				var ips = Dns.GetHostAddresses(args[0]);

				// Iterate over results for IPs
				foreach (var ip in ips)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						// Store IPv4 address
						ipv4 = ip;
					}
					else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
					{
						// Store IPv6 address
						ipv6 = ip;
					}
				}
			}
			catch
			{
				// Quit if no addresses were found
				Console.WriteLine("Eror: could not get addresses for host {0}", args[0]);
				return;
			}

			// Run the trace on the IPv4 address if found
			if (ipv4 != null)
			{
				Console.WriteLine("IPv4 Trace:");
				Trace(ipv4);
				Console.WriteLine();
			}

			// Run the trace on the IPv6 address if found
			if (ipv6 != null)
			{
				Console.WriteLine("IPv6 Trace:");
				Trace(ipv6);
				Console.WriteLine();
			}
		}

		//
		// Runs a traceroute to the specified IP address
		// Prints the results to the console
		//
		// @param ip The IP address to trace the route to
		static void Trace(IPAddress ip)
		{
			// Run ping tests for each number of hops from 1 to maxHops
			for (int count = 1; count <= maxHops; count++)
			{
				// Write the current hop count
				Console.Write("{0,-4}", count);

				// Get ping sender, options, and reply
				var ping = new Ping();
				var opts = new PingOptions(count, false);
				var reply = default(PingReply);

				// Ping once for each number of tests
				for (int test = 0; test < numTests; test++)
				{
					// Send the ping and time it using a stopwatch
					var timer = Stopwatch.StartNew();
					reply = ping.Send(ip, timeout, new byte[32], opts);
					timer.Stop();

					if (reply.Status == IPStatus.TimedOut)
					{
						// If we timed out, don't display a time
						Console.Write("{0,-8}", "*");
					}
					else
					{
						// Otherwise, print the timer results
						Console.Write("{0,-8}", timer.ElapsedMilliseconds + " ms");
					}
				}

				// Initialize the host as the IP address
				var host = reply.Address.ToString();

				if (reply.Status == IPStatus.TimedOut)
				{
					// If we timed out, indicate that in the host
					host = "Reqeust timed out.";
				}
				else
				{
					// Otherwise, try looking up the host
					try
					{
						host = Dns.GetHostEntry(reply.Address).HostName;
					}
					catch { }
				}

				// Write out the host
				Console.WriteLine(host);

				// We've reached the destination, method is done
				if (reply.Status == IPStatus.Success)
				{
					return;
				}
			}
		}
	}
}
