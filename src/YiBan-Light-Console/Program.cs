using System;
using System.Text;
using System.Threading;
using YiBan_Light_Lib;

namespace YiBan_Light_Console
{
    public class Program
    {

        private static string GetHiddenString()
        {
            string pass = null;
            StringBuilder sb = null;
            char ch;

            sb = new StringBuilder();
            ch = Console.ReadKey(true).KeyChar;
            while (ch != '\n' && ch != '\r')
            {
                sb.Append(ch);
                ch = Console.ReadKey(true).KeyChar;
            }
            pass = sb.ToString();
            return pass;
        }

        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (args.Length == 1)
            {
                var control = new SignController();
                string user = null;
                string pass = null;

                switch (args[0])
                {
                    case "-s":
                        control.Init();
                        control.Start();
                        Thread.Sleep(-1);
                        break;
                    case "-n":
                        Console.WriteLine("Input your username:");
                        user = Console.ReadLine();
                        
                        Console.WriteLine("Input your password:");
                        pass = GetHiddenString();

                        control.Set(user, pass);
                        control.Start();
                        Thread.Sleep(-1);
                        break;
                    case "-a":
                        control.Init();

                        Console.WriteLine("Input your username:");
                        user = Console.ReadLine();

                        Console.WriteLine("Input your password:");
                        pass = GetHiddenString();

                        control.Set(user, pass);
                        control.Start();
                        Thread.Sleep(-1);
                        break;
                }
            }
            else
            {
                Console.WriteLine("Welcome to use Yiban-Light-Console");
                Console.WriteLine("Usage:");
                Console.WriteLine("-s   Sign using the data.xml");
                Console.WriteLine("-n   Add users using the interactive console overwriting data.xml");
                Console.WriteLine("-a   Append users using the interactive console into data.xml");
            }
        }
    }
}
