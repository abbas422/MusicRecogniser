using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Utilities
{
    public class VideoDownloader
    {
        //public Process process = null;
        public string DownloadAsync(string url, IConfiguration configuration, IHubContext<JobProgressHub> _hubContext, string JobId)
        {
            ProcessStartInfo processInfo = null; Process process = null;
            string contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
            
            string FileName = Guid.NewGuid().ToString();
            try
            {
                processInfo = new ProcessStartInfo("youtube-dl", @"--recode-video mp4 --output "+ contentRoot+ @"\Downloads\Videos\" + FileName + " " + url);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;


                process = Process.Start(processInfo);


                process.OutputDataReceived += async (object sender, DataReceivedEventArgs e) => /*flg += e.Data;*/
                 await _hubContext.Clients.Group(JobId).SendAsync("Index", e.Data);

                //Console.WriteLine("output>>" + e.Data);

                process.BeginOutputReadLine();



                process.ErrorDataReceived += async (object sender, DataReceivedEventArgs e) =>
                 await _hubContext.Clients.Group(JobId).SendAsync("Index", e.Data);

                //Console.WriteLine("error>>" + e.Data);
                process.BeginErrorReadLine();



                process.WaitForExit();


                //Console.WriteLine("ExitCode: {0}", process.ExitCode);
                process.Close();
                if (!File.Exists(contentRoot + @"\Downloads\Videos\" + FileName+".mp4")){
                    FileName = string.Empty;
                }

            }
            catch (Exception ex)
            {
                FileName = string.Empty;
            }
            //Console.WriteLine(processInfo.FileName.ToString());
            return FileName;

        }

    }
}
