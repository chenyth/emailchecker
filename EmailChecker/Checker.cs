using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmailChecker
{
    class Checker
    {
        private string fromAddress = "yao@myhost.com";
        private int timeOut = 10000;


        public void CheckMailByList(string fileName)
        {
            try
            {
                StreamWriter goodWrite = new StreamWriter(fileName+".real", false);
                StreamWriter unkonwWrite = new StreamWriter(fileName + ".fake", false);
                StreamWriter errorWrite = new StreamWriter(fileName + ".error", false);
                goodWrite.AutoFlush = true;
                errorWrite.AutoFlush = true;
                unkonwWrite.AutoFlush = true;
                StreamReader reader = new StreamReader(fileName);
                while (!reader.EndOfStream)
                {
                    string mail = reader.ReadLine();
                    mail = mail.Trim();
                    int result = CheckMailAddress(mail);
                    if (result == 1)
                    {
                        goodWrite.WriteLine(mail);
                    }
                    else if (result == 0)
                    {
                        unkonwWrite.WriteLine(mail);
                    }
                    else if (result == -1)
                    {
                        errorWrite.WriteLine(mail);
                    }
                }
                goodWrite.Flush();
                unkonwWrite.Flush();
                errorWrite.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
               
        }





        /// <summary>
        /// Check one email address
        /// </summary>
        /// <param name="mail">Email Address</param>
        /// <returns>
        /// 0:  not exists
        /// 1:  exists
        /// -1: error
        /// </returns>
        public int CheckMailAddress(string mail)
        {
            try
            {
                string smtpServer = GetSMTPServerByMailAddress(mail);
                if (smtpServer == null)
                {
                    return -1;
                }
                TcpClient tClient = new TcpClient(smtpServer, 25);
                tClient.SendTimeout = timeOut;
                tClient.ReceiveTimeout = timeOut;

                string CRLF = "\r\n";
                byte[] dataBuffer;
                string ResponseString;
                NetworkStream netStream = tClient.GetStream();
                StreamReader reader = new StreamReader(netStream);
                ResponseString = reader.ReadLine();
                dataBuffer = BytesFromString("HELO There" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                dataBuffer = BytesFromString("MAIL FROM:<" + fromAddress + ">" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                dataBuffer = BytesFromString("RCPT TO:<" + mail + ">" + CRLF);
                netStream.Write(dataBuffer, 0, dataBuffer.Length);
                ResponseString = reader.ReadLine();
                int responseCode = GetResponseCode(ResponseString);
                Console.WriteLine(responseCode);
                if (responseCode == 550)
                {
                    System.Console.WriteLine(mail + " NO!");
                    dataBuffer = BytesFromString("QUITE" + CRLF);
                    netStream.Write(dataBuffer, 0, dataBuffer.Length);
                    tClient.Close();
                    return 0;

                }
                else
                {
                    System.Console.WriteLine(mail + " Yes!");
                    dataBuffer = BytesFromString("QUITE" + CRLF);
                    netStream.Write(dataBuffer, 0, dataBuffer.Length);
                    tClient.Close();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return -1;
            }
        }
        private byte[] BytesFromString(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
        private int GetResponseCode(string ResponseString)
        {
            if (ResponseString == null || ResponseString.Length < 3)
            {
                return 550;
            }
            return int.Parse(ResponseString.Substring(0, 3));
        }
        public string GetSMTPServerByMailAddress(string mail)
        {
            int indexOfAt = mail.IndexOf("@") + 1;

            string domain = mail.Substring(indexOfAt, mail.Length - indexOfAt);
            string[] array = GetMXRecords(domain);
            if (array != null && array.Length > 0)
            {
                return array[0];
            }
            else
                return null;
        }
        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)]ref string pszName, QueryTypes wType, QueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);

        public static string[] GetMXRecords(string domain)
        {

            IntPtr ptr1 = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            MXRecord recMx;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new NotSupportedException();
            }
            ArrayList list1 = new ArrayList();
            try
            {

                int num1 = DnsQuery(ref domain, QueryTypes.DNS_TYPE_MX, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ptr1, 0);
                if (num1 != 0)
                {
                    if (num1 == 9003)
                    {
                        list1.Add("DNS record does not exist");
                    }
                    else
                    {
                        throw new Win32Exception(num1);
                    }
                }
                for (ptr2 = ptr1; !ptr2.Equals(IntPtr.Zero); ptr2 = recMx.pNext)
                {
                    recMx = (MXRecord)Marshal.PtrToStructure(ptr2, typeof(MXRecord));
                    if (recMx.wType == 15)
                    {
                        string text1 = Marshal.PtrToStringAuto(recMx.pNameExchange);
                        list1.Add(text1);
                    }
                }
            }
            finally
            {
                DnsRecordListFree(ptr1, 0);
            }
            return (string[])list1.ToArray(typeof(string));
        }

        private enum QueryOptions
        {
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
            DNS_QUERY_BYPASS_CACHE = 8,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_NO_RECURSION = 4,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_RESERVED = -16777216,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_STANDARD = 0,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_USE_TCP_ONLY = 2,
            DNS_QUERY_WIRE_ONLY = 0x100
        }

        private enum QueryTypes
        {
            DNS_TYPE_MX = 15
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MXRecord
        {
            public IntPtr pNext;
            public string pName;
            public short wType;
            public short wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public IntPtr pNameExchange;
            public short wPreference;
            public short Pad;
        }
    }
}
