using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Utilities
{
    public class AudioRecongnition
    {
        public async Task<RecognitionResultModel> RecognizeAsync(string audioFileName, IConfiguration configuration)
        {
            JObject jsonResult = null;
            string responseJsonString, status, artist = string.Empty, kind, errorDescription, contentRoot;
            int errorCode = 0;



            try
            {
                contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);

                var mp3FilebytesArray = File.ReadAllBytes(contentRoot + @"\Downloads\Music\" + audioFileName+".mp3");
                
                responseJsonString = await Upload(mp3FilebytesArray);

                if (responseJsonString.Equals(string.Empty))
                {

                    errorDescription = "Sorry! An error occure in recognition of the song";
                    errorCode = -1;
                }

                jsonResult = (JObject)JsonConvert.DeserializeObject(responseJsonString);

                status = jsonResult["status"].Value<string>();
                if (!status.Equals("success") || !responseJsonString.Contains("artist") || !responseJsonString.Contains("itunes") || !responseJsonString.Contains("kind"))
                {
                    errorDescription = "Sorry! we cannot recognize the artist of the music";
                    errorCode = -1;
                }
                else
                {
                    artist = jsonResult["result"]["artist"].Value<string>();
                    kind = jsonResult["result"]["itunes"]["kind"].Value<string>();

                    if (!kind.Equals("song"))
                    {
                        errorDescription = "music is not a song";
                        errorCode = -1;
                    }
                    else
                    {
                        errorDescription = "";
                        errorCode = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                errorDescription = ex.Message;
                errorCode = -1;
            }

            RecognitionResultModel recognitionModel = new RecognitionResultModel() { artist = artist, errorCode = errorCode, errorDescription = errorDescription };

            return recognitionModel;

        }

        private async Task<string> Upload(byte[] paramFileBytes)
        {
            string responseString = string.Empty;

                HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
                using (var client = new HttpClient())
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(bytesContent, "file", "file");

                    var response = await client.PostAsync("https://api.audd.io/ ?method=recognize&return=itunes,deezer&itunes_country=us", formData);
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    var stream = await response.Content.ReadAsStreamAsync();
                    StreamReader reader = new StreamReader(stream);
                    responseString = await reader.ReadToEndAsync();

                }


            return responseString;
        }
    }
}
