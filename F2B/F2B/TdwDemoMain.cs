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
using Amazon.CloudFormation.Model;
using System.Security.Cryptography;
using System.Net;

namespace Tandem.Tdw.F2B.Demo
{
    class Program
    {
        private static Random random = new Random();
        private static string QSS3BucketName = "tdw-cft-dev";
        private static string QSS3KeyPrefix = "tdw-cft-dev";

        //private static AmazonS3EncryptionClient s3EncryptionClientFileMode;

        static void Main(string[] args)
        {
            EncryptionMaterials encryptionMaterials = new EncryptionMaterials(TdwUtils.CreateAsymmetricProvider());
            //S3demo testS3 = new S3demo(encryptionMaterials);
            CloudFormationDemo cfDemo = new CloudFormationDemo(args);
            //KinesisDemo KinesisDemo = new KinesisDemo(args);
        }
    }
}
