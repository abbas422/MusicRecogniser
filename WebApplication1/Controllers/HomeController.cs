using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;
using WebApplication1.Utilities;
using WebApplication1.Utilities.Google.Apis.YouTube.Samples;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public IConfiguration _configuration;
        private readonly IQueue _queue;
        private readonly IHubContext<JobProgressHub> _hubContext;

        public HomeController(IConfiguration configuration, IQueue queue, IHubContext<JobProgressHub> hubContext) {
            _configuration = configuration;
            _queue = queue;
            _hubContext = hubContext;
        }


        [HttpGet]
        public IActionResult Index()
        {
            @ViewBag.username = "abbas";
            string jobId = Guid.NewGuid().ToString("N");
            ViewBag.JobId = jobId;
            return View("Index", new List<VideoTempleteModel>());
        }


        [HttpPost]
        //[ActionName("YoutubeSearch")]
        //[Route("youtubeSearch")]
        public async Task<IActionResult> Index(string YoutubeUri, string JobId)
        {
            ViewBag.JobId = JobId;

            try
            {
                VideoDownloader videoDownloader = new VideoDownloader();
                await _hubContext.Clients.Group(JobId).SendAsync("Index", "Start downloading video ...");
                string VideoFileName = videoDownloader.DownloadAsync(YoutubeUri, _configuration, _hubContext, JobId);
                

                if (VideoFileName != string.Empty)
                {
                    await _hubContext.Clients.Group(JobId).SendAsync("Index", "video downloaded successfully");
                    await Task.Delay(500);
                    VideoConverter videoConverter = new VideoConverter();
                    await _hubContext.Clients.Group(JobId).SendAsync("Index", "start converting video into mp3 format ...");

                    int errorCode = videoConverter.ConvertAsync(VideoFileName, _configuration, _hubContext, JobId);
                    if (errorCode == 0)
                    {
                        await _hubContext.Clients.Group(JobId).SendAsync("Index", "video converted successfully");
                        await Task.Delay(500);

                       
                        AudioRecongnition audioRecongnition = new AudioRecongnition();
                        await _hubContext.Clients.Group(JobId).SendAsync("Index", "start recognition process of mp3 music ...");

                        RecognitionResultModel recognitionResultModel = await audioRecongnition.RecognizeAsync(VideoFileName, _configuration);
                        if (recognitionResultModel.errorCode == 0)
                        {
                            await _hubContext.Clients.Group(JobId).SendAsync("Index", "mp3 music recognized successfully successfully");
                            await Task.Delay(500);
                            YoutubeSearcher youtubeSearcher = new YoutubeSearcher();

                            await _hubContext.Clients.Group(JobId).SendAsync("Index", "Searching for videos related to the artist...");

                            List<VideoTempleteModel> listVideo = await youtubeSearcher.Run(recognitionResultModel.artist);

                            await _hubContext.Clients.Group(JobId).SendAsync("Index", "Getting youtube result...");
                            await Task.Delay(500);

                            ViewBag.error = "Video list is empty!";
                            return View("Index", listVideo);;
                        }
                        else
                        {
                            ViewBag.error = recognitionResultModel.errorDescription;
                            return View("Index", new List<VideoTempleteModel>());
                        }

                    }
                    else
                    {
                        ViewBag.error = "Sorry! An Error occure in converting video into mp3 music";
                        return View("Index", new List<VideoTempleteModel>());
                    }
                    }
                else
                {
                    ViewBag.error = "sorry! An error occure in downloading video";
                    return View("Index", new List<VideoTempleteModel>());
                }

            }
            catch (Exception ex)
            {
                ViewBag.error = "Unknown error ocured - "+ex.Message;

                return View("Index",new List<VideoTempleteModel>());

            }


        }

       

       
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
