using Aliyun.OSS;
using Aliyun.OSS.Common;
using Infrastructure;
using System;
using System.IO;

namespace ZR.Common
{
    public class AliyunOssHelper
    { 
        static string accessKeyId = AppSettings.GetConfig("ALIYUN_OSS:KEY");
        static string accessKeySecret = AppSettings.GetConfig("ALIYUN_OSS:SECRET");
        static string endpoint = AppSettings.GetConfig("ALIYUN_OSS:REGIONID");
        static string bucketName1 = AppSettings.GetConfig("ALIYUN_OSS:bucketName");
        
        /// <summary>
        /// Upload to Aliyun
        /// </summary>
        /// <param name="filestreams"></param>
        /// <param name="dirPath">Storage path eg: upload/2020/01/01/xxx.png</param>
        /// <param name="bucketName">Storage bucket, if empty defaults to config file</param>
        public static System.Net.HttpStatusCode PutObjectFromFile(Stream filestreams, string dirPath, string bucketName = "")
        {
            OssClient client = new(endpoint, accessKeyId, accessKeySecret);
            if (string.IsNullOrEmpty(bucketName)) { bucketName = bucketName1; }
            try
            {
                dirPath = dirPath.Replace("\\", "/");
                PutObjectResult putObjectResult = client.PutObject(bucketName, dirPath, filestreams);
                // Console.WriteLine("Put object:{0} succeeded", directory);

                return putObjectResult.HttpStatusCode;
            }
            catch (OssException ex)
            {
                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                  ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
            }
            return System.Net.HttpStatusCode.BadRequest;
        }

        /// <summary>
        /// Delete resource
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public static System.Net.HttpStatusCode DeleteFile(string dirPath, string bucketName = "")
        {
            if (string.IsNullOrEmpty(bucketName)) { bucketName = bucketName1; }
            try
            {
                OssClient client = new(endpoint, accessKeyId, accessKeySecret);
                DeleteObjectResult putObjectResult = client.DeleteObject(bucketName, dirPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return System.Net.HttpStatusCode.BadRequest;
        }
    }
}

