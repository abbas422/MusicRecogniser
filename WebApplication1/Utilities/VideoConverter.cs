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
    public class VideoConverter
    {
        public int ConvertAsync(string sourceVideoName, IConfiguration configuration, IHubContext<JobProgressHub> _hubContext, string JobId)
        {
            int errorCode = 0;
            ProcessStartInfo processInfo = null; Process process = null;
            string FileName = Guid.NewGuid().ToString();

            string contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);

            try
            {
                processInfo = new ProcessStartInfo("ffmpeg.exe", @"-ss 0 -t 60 -i "+ contentRoot+ @"\Downloads\Videos\" + sourceVideoName + @".mp4 "+ contentRoot + @"\Downloads\Music\" + sourceVideoName + ".mp3");
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
                if (!File.Exists(contentRoot + @"\Downloads\Music\" + sourceVideoName + ".mp3"))
                {
                    errorCode = -1;
                }
            }
            catch (Exception ex)
            {
                errorCode = -1;
            }
            return errorCode;

            }

    }
}
