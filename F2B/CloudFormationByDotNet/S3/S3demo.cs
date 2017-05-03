using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using Amazon;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using Amazon.S3.Encryption;
using Amazon.S3.Transfer;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Kinesis.Model;
using Amazon.Kinesis;
using Amazon.KinesisFirehose;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using Amazon.Runtime.CredentialManagement;
using Amazon.CloudFormation;

namespace Tandem.Tdw.F2B.Demo
{
    class S3demo
    {
        private static Random random = new Random();

        public S3demo(EncryptionMaterials encryptionMaterials)
        {
            TestS3();
            TestEncryptedS3(encryptionMaterials);
            TestDecryptedS3(encryptionMaterials);
        }

        static void TestEncryptedS3(EncryptionMaterials encryptionMaterials)
        {
            string bucket_Name = null;
            AmazonS3EncryptionClient s3Client = new AmazonS3EncryptionClient(encryptionMaterials);

            bucket_Name = TdwUtils.rootBucketName + "encrypted-content";
            try
            {
                AmazonS3Util.DeleteS3BucketWithObjects(s3Client, bucket_Name);
            }
            catch (Exception ex)
            {
                ex = null;
            }
            bucket_Name = TdwUtils.CreateBucket(s3Client, bucket_Name);

            string dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPath);
            byte[] dataBytes = TdwUtils.FileToArray(dataPath);
            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucket_Name,
                Key = TdwUtils.keyName,
                InputStream = new MemoryStream(dataBytes)
            };

            PutObjectResponse response = s3Client.PutObject(request);

            Console.WriteLine("===============>TestEncryptedS3 START<===============");
            Console.WriteLine("Encryption method was:");
            Console.WriteLine(response.ServerSideEncryptionMethod);
            Console.WriteLine("===============>TestEncryptedS3 END<===============");
        }

        static void TestDecryptedS3(EncryptionMaterials encryptionMaterials)
        {
            string bucket_Name = null;
            AmazonS3EncryptionClient s3Client = new AmazonS3EncryptionClient(encryptionMaterials);

            bucket_Name = TdwUtils.rootBucketName + "encrypted-content";
            bucket_Name = TdwUtils.CreateBucket(s3Client, bucket_Name);

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = bucket_Name,
                Key = TdwUtils.keyName
            };

            string data = null;
            using (GetObjectResponse getObjectResponse = s3Client.GetObject(getObjectRequest))
            {
                using (var stream = getObjectResponse.ResponseStream)
                using (var reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
                Console.WriteLine("===============>TestDecryptedS3 START<===============");
                Console.WriteLine("Encryption method was:");
                Console.WriteLine(getObjectResponse.ServerSideEncryptionMethod);
                Console.WriteLine("===============> <===============");
            }
            Console.WriteLine(data);
            Console.WriteLine("===============>TestDecryptedS3 END<===============");
        }

        static void TestS3()
        {
            string bucket_Name = null;
            var client = new AmazonS3Client();
            bucket_Name = TdwUtils.rootBucketName + "simple-content";
            try
            {
                AmazonS3Util.DeleteS3BucketWithObjects(client, bucket_Name);
            }
            catch (Exception ex)
            {
                ex = null;
            }
            bucket_Name = TdwUtils.CreateBucket(client, bucket_Name);


            string dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPath);
            byte[] dataBytes = TdwUtils.FileToArray(dataPath);

            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucket_Name,
                Key = "simple-content",
                InputStream = new MemoryStream(dataBytes)
            };
            PutObjectResponse response = client.PutObject(request);

            Console.WriteLine("===============>TestS3 START<===============");
            Console.WriteLine("Encryption method was:");
            Console.WriteLine(response.ServerSideEncryptionMethod);
            Console.WriteLine("===============>TestS3 END<===============");
        }
        
        static void SetProfileOnDeploy()
        {
            String profileName = ConfigurationManager.AppSettings["AWSProfileName"];
            String accessKeyId = ConfigurationManager.AppSettings["OctopusAccessIdToken"];
            String accessKey = ConfigurationManager.AppSettings["OctopusAccessKeyToken"];

            CredentialProfileOptions options = new CredentialProfileOptions
            {
                AccessKey = accessKeyId,
                SecretKey = accessKey
            };
            CredentialProfile profile = new CredentialProfile(profileName, options);
            profile.Region = RegionEndpoint.EUWest1;
            var sharedFile = new SharedCredentialsFile();
            sharedFile.RegisterProfile(profile);
        }
    }
}
