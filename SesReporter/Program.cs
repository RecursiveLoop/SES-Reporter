using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace SesReporter
{
    class Program
    {
        static string AssumedRoleName;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please supply the assumed role name as an argument.");
                return;
            }
            AssumedRoleName = args[0];

            var strAccountsTextFile = Path.Combine(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "Accounts.txt");

            var srAccounts = File.OpenText(strAccountsTextFile);
            var tasks = new List<Task<List<SendDataPoint>>>();

            while (!srAccounts.EndOfStream)
            {
                string strAccountName = srAccounts.ReadLine();

                tasks.Add(GetSesStatsForAccount(strAccountName));

            }

            Task.WaitAll(tasks.ToArray());

            var strOutputFile = Path.Combine(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");

            using (var outputFS = File.Open(strOutputFile, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                foreach (var task in tasks)
                {
                    Console.WriteLine($"{task.Result.Count.ToString()} results found");
                    foreach (var dataPoint in task.Result)
                    {
                        string strToWrite = $"{dataPoint.Timestamp.ToString("yyyyMMdd hh:mm:ss tt")} - Bounces: " +
                            $"{dataPoint.Bounces.ToString()} Complaints: {dataPoint.Complaints.ToString()} Rejects: {dataPoint.Rejects.ToString()} Delivery Attempts: {dataPoint.DeliveryAttempts.ToString()}";

                        WriteTextToStream(outputFS, strToWrite);
                    }
                    
                }
            }

            Console.WriteLine("Process completed, file written " + strOutputFile);
        }

        static void WriteTextToStream(Stream stm, string str)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(str);

            stm.Write(data, 0, data.Length);
        }

        async static Task<List<SendDataPoint>> GetSesStatsForAccount(string Account)
        {
            string strRoleARN = "arn:aws:iam::" + Account + ":role/" + AssumedRoleName;
            Amazon.SecurityToken.AmazonSecurityTokenServiceClient stsClient = new Amazon.SecurityToken.AmazonSecurityTokenServiceClient();
            var assumeRoleResponse = await stsClient.AssumeRoleAsync(new Amazon.SecurityToken.Model.AssumeRoleRequest { RoleArn = strRoleARN, RoleSessionName = "TempSession" });


            SessionAWSCredentials sessionCredentials =
                new SessionAWSCredentials(assumeRoleResponse.Credentials.AccessKeyId,
                                          assumeRoleResponse.Credentials.SecretAccessKey,
                                          assumeRoleResponse.Credentials.SessionToken);

            var regions = new Amazon.RegionEndpoint[] { Amazon.RegionEndpoint.USEast1,Amazon.RegionEndpoint.USWest2,Amazon.RegionEndpoint.EUWest1 };

            List<SendDataPoint> lst = new List<SendDataPoint>();

            foreach (var region in regions)
            {
                Console.WriteLine($"Checking {region.ToString()} for account {Account}");

                AmazonSimpleEmailServiceClient sesClient = new AmazonSimpleEmailServiceClient(sessionCredentials, region);

                var response = await sesClient.GetSendStatisticsAsync();

                lst.AddRange(response.SendDataPoints);
            }

            return lst;
        }
    }
}
