
/*

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ImageClassification.ModelScorer;
using Microsoft.AspNetCore.Http;
using ImageClassification.ImageDataStructures;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Mlimage : ControllerBase
    {
        private IChatManager _chatManager;

        public Mlimage(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        [HttpGet]
        public ContentResult Get()
        {
            string text;
            var fileStream = new FileStream(@"MLimage.html", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = text
            };
        }

        [HttpGet ("prediction")]
        public async Task<string> Prediction(IFormFile image)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            var tagsTsv = Path.Combine(assetsPath, "inputs", "images", "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", "images");
            var inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb");
            var labelsTxt = Path.Combine(assetsPath, "inputs", "inception", "imagenet_comp_graph_label_strings.txt");

            try
            {
                var filePath = Path.GetTempFileName();
                //Save image
                using (var ms = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(ms);
                }
                var modelScorer = new TFModelScorer(tagsTsv, imagesFolder, inceptionPb, labelsTxt);

                var prediction = (ImageNetDataProbability)modelScorer.Score(new ImageNetData() { ImagePath = filePath, Label = "" });

                return prediction.ToString();
            }
            catch
            {
                return "Error";
            }
            //return View();
        }
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;
            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }
    }
}


*/