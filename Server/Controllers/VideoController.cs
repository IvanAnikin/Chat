using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        [HttpGet]
        public ContentResult Get()
        {

            string text;
            var fileStream = new FileStream(@"VideoPage.html", FileMode.Open, FileAccess.Read);
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

        [HttpGet("images")]
        public ContentResult Images()
        {

            string text;
            var fileStream = new FileStream(@"VideoImagesPage.html", FileMode.Open, FileAccess.Read);
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


        [HttpGet("photo")]
        public ContentResult Photo()
        {

            string text;
            var fileStream = new FileStream(@"VideoPhotoPage.html", FileMode.Open, FileAccess.Read);
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

        [HttpGet("upload")]
        public ContentResult UploadPhoto()
        {
            try
            {
                string text;
                var fileStream = new FileStream(@"uploadPhoto.html", FileMode.Open, FileAccess.Read);
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
            catch (Exception e)
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Content = e.Message 
                };
            }
        }

        [HttpGet("takePhoto")]
        public ContentResult TakePhoto()
        {
            string text;
            var fileStream = new FileStream(@"photoTake.html", FileMode.Open, FileAccess.Read);
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

        [HttpGet("string")]
        public string String()
        {

            return "STRING";
        }


    }
}