using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Utilities
{
    namespace Google.Apis.YouTube.Samples
    {
   
        public class YoutubeSearcher
        {
           

            public async Task<List<VideoTempleteModel>> Run(string keyword)
            {
                List<VideoTempleteModel> videos = new List<VideoTempleteModel>();

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "Your key",//Your google API Key
                    ApplicationName = this.GetType().ToString()
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = keyword;
                searchListRequest.MaxResults = 50;
                
                var searchListResponse = await searchListRequest.ExecuteAsync();

                int i = 0;
                foreach (var searchResult in searchListResponse.Items)
                {
                    i++;
                 
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            videos.Add(new VideoTempleteModel { VideoId = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId,
                            Title = searchResult.Snippet.Title,
                            Kind = "video",
                            Thumbnails = searchResult.Snippet.Thumbnails.Medium.Url
                            });
                            break;
                    }
                }

                return videos;
            }
        }
    }
    }
