using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Checker ck = new Checker();

            if (args.Length > 0) //There are parameters
            {
                if (args.Length == 1) //Single Email Address check
                {
                    ck.CheckMailAddress(args[0]);
                }
                else if (args.Length == 2)
                {
                    if (args[0].Equals("-f"))
                    {
                        ck.CheckMailByList(args[1]);
                    }
                    else
                    {
                        PrintHelpInfo();
                    }
                }
            }
            else
            {
                PrintHelpInfo();
            }
        }
        public static void PrintHelpInfo()
        {
            Console.WriteLine(@"
Useage:  
    1. Check one Email address:
        EmailChecker mailNeedCheck@xxxx.com     
    2. Check Email address file:
        EmailChecker -f mailListFileName
   The result file will be named as following:
        mailListFileName.real for the real email address.
        mailListFileName.fake for the not existed email address.
        mailListFileName.error for the email address could not be checked.
Enjoy!                
                ");
        }
    }
}
