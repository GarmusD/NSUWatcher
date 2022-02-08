using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NamedPipeWrapper;

namespace nsuwconsole
{
    class Program
    {
        static bool done = false;

        static void Main(string[] args)
        {
            NamedPipeClient<string> pc = new NamedPipeClient<string>("nsuwatcher");
            pc.ServerMessage += Pc_ServerMessage;

            Console.WriteLine("Press any key to connect to server...");
            Console.Read();
            pc.Start();

            while (!done)
            {
                string s = Console.ReadLine();
                if (s.Equals("q", StringComparison.InvariantCultureIgnoreCase))
                {
                    done = true;
                }
                else
                {
                    pc.PushMessage(s);
                }
            }
            pc.Stop();
        }

        private static void Pc_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
            Console.WriteLine(message);
        }
    }
}
