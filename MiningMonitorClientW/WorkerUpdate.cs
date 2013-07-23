using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json;
using System.IO;

namespace MiningMonitorClientW
{
    class WorkerUpdate
    {
        public string worker_user_name { get; set; }
        public string hashrate { get; set; }
        public string accepted { get; set; }
        public string rejected { get; set; }
        public string hw_errors { get; set; }
        public string num_gpu { get; set; }
        public string[] gpus { get; set; }

        public void update(string user_worker)
        {    
          System.Timers.Timer timer = new System.Timers.Timer(45000);
          timer.Elapsed += (sender, e) =>
          {
              //query the miner for summary and gpucount information
              String SummaryQuery = QueryMiner("summary");
              String gpuNum = FindKey(QueryMiner("gpucount"), "Count");
              //String PoolQuery = QueryMiner("pools");
              int numgpus = Convert.ToInt32(gpuNum);
              //Array of strings to hold each gpu query 
              String[] gpuQueries = new String[numgpus];
              //add the GPU queries into the array
              for (int i = 0; i < numgpus; i++)
              {
                  gpuQueries[i] = QueryMiner("gpu|" + i);
              }
              //now add information specific to each gpu to a list
              List<string> gpuList = new List<string>();
              for (int i = 0; i + 1 <= gpuQueries.Length; i++)
              {
                  gpuList.Add(FindKey(gpuQueries[i], "Temperature"));
                  gpuList.Add(FindKey(gpuQueries[i], "MHS 5s"));
              }
              //set all the values that we have gotten from the queries
              this.worker_user_name = user_worker;
              this.hashrate = FindKey(SummaryQuery, "MHS av");
              this.accepted = FindKey(SummaryQuery, "Accepted");
              this.rejected = FindKey(SummaryQuery, "Rejected");
              this.hw_errors = FindKey(SummaryQuery, "Hardware Errors");
              this.num_gpu = gpuNum;
              this.gpus = gpuList.ToArray();
              //create JSON from the workerUpdate object
              string JSON = JsonConvert.SerializeObject(this);
              Console.WriteLine(JSON);
              //send to website
              string httpBack = HttpPutRequest(JSON);
          };
         timer.Start();
        }
        static string QueryMiner(string command)
        {
            byte[] bytes = new byte[1024];
            try
            {
                //code for gettting current machines IP Use this code if client is running on the miner
                //IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
                IPAddress ipAddr = IPAddress.Parse("198.244.100.254");
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4028);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(ipEndPoint);
                Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                string SummaryMessage = command;
                byte[] msg = Encoding.ASCII.GetBytes(SummaryMessage);

                sender.Send(msg);
                byte[] buffer = new byte[1024];
                int lengthOfReturnedBuffer = sender.Receive(buffer);
                char[] chars = new char[lengthOfReturnedBuffer];

                Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(buffer, 0, lengthOfReturnedBuffer, chars, 0);
                String SummaryJson = new String(chars);
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                return SummaryJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.ToString());
                return ex.ToString();
            }
        }
        static string HttpPutRequest(string Json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://miningmonitor.herokuapp.com/workers/update");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(Json);
                streamWriter.Flush(); 
                streamWriter.Close();
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpWebRequest.GetRequestStream()))
                {
                    string results = streamReader.ReadToEnd();
                    return results;
                }
            }

        }
        //Function to parse the string returns from the miner modified slightly from cgminer java api example
        static string FindKey(String result, string key)
        {
            String value;
            String name;
            String[] sections = result.Split('|');

            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i].Trim().Length > 0)
                {
                    String[] data = sections[i].Split(',');

                    for (int j = 0; j < data.Length; j++)
                    {
                        String[] nameval = data[j].Split('=');
                        if (j == 0)
                        {
                            if (nameval.Length > 1 && Char.IsDigit(nameval[1][0]))
                                name = nameval[0] + nameval[1];
                            else
                                name = nameval[0];
                        }
                        if (nameval.Length > 1)
                        {
                            name = nameval[0];
                            value = nameval[1];
                        }
                        else
                        {
                            name = "" + j;
                            value = nameval[0];
                        }
                        if (name == key)
                            return value;
                    }
                }
            }
            return "not found";
        }
    }
}
