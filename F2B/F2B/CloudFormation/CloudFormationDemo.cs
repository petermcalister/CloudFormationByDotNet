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
    class CloudFormationDemo
    {
        private static string QSS3BucketName = "tdw-cft-dev";
        private static string QSS3KeyPrefix = "tdw-cft-dev";

        //private static AmazonS3EncryptionClient s3EncryptionClientFileMode;

        public CloudFormationDemo(string[] args)
        {
            EncryptionMaterials encryptionMaterials = new EncryptionMaterials(TdwUtils.CreateAsymmetricProvider());
            
            //CopyTemplatesToS3(encryptionMaterials);
            //TestParentChildTemplates();
            //TestCfStack(encryptionMaterials);
            ApplyCloudFormationChangeSetExample();
        }

        static void TestParentChildTemplates()
        {
            string bucket_Name = QSS3BucketName;
            string templateName = QSS3KeyPrefix + TdwUtils.cfClassPathParentSubnet.Replace("tdw_cf_template\\", "");
            string stack_name = templateName.Replace("-", "");
            stack_name = stack_name.Replace(".template", "");
            string dataPath = null;
            byte[] dataBytes = null;
            PutObjectRequest request = null;
            PutObjectResponse response = null;
            AmazonS3Client s3Client = new AmazonS3Client();

            bucket_Name = TdwUtils.CreateBucket(s3Client, QSS3BucketName);

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = bucket_Name,
                Key = templateName,
            };

            string data = null;
            using (GetObjectResponse getObjectResponse = s3Client.GetObject(getObjectRequest))
            {
                using (var stream = getObjectResponse.ResponseStream)
                using (var reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
            }

            Amazon.CloudFormation.AmazonCloudFormationClient cfClient = new AmazonCloudFormationClient();

            try
            {
                DeleteStackRequest deleteRequest = new DeleteStackRequest() { StackName = stack_name };
                cfClient.DeleteStack(deleteRequest);
            }
            catch (Exception ex)
            {
                ex = null;
            }

            List<string> CfCapabilities = new List<string>();
            CfCapabilities.Add("CAPABILITY_IAM");
            CreateStackRequest stackRequest = new CreateStackRequest() { StackName = stack_name, TemplateBody = data, Capabilities = CfCapabilities };
            CreateStackResponse stackResponse = cfClient.CreateStack(stackRequest);
        }

        static void ApplyCloudFormationChangeSetExample()
        {
            string bucket_Name = QSS3BucketName;
            string templateName = QSS3KeyPrefix + TdwUtils.cfClassPathBastionChangeSet.Replace("tdw_cf_template\\", "");
            string stack_name = QSS3KeyPrefix + TdwUtils.cfClassPathBastion.Replace("tdw_cf_template\\", "");
            stack_name = stack_name.Replace("-", "");
            stack_name = stack_name.Replace(".template", ""); 
            AmazonS3Client s3Client = new AmazonS3Client();
            Amazon.CloudFormation.AmazonCloudFormationClient cfClient = new AmazonCloudFormationClient();

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = bucket_Name,
                Key = templateName,
            };

            string data = null;
            using (GetObjectResponse getObjectResponse = s3Client.GetObject(getObjectRequest))
            {
                using (var stream = getObjectResponse.ResponseStream)
                using (var reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
            }

            List<string> CfCapabilities = new List<string>();
            CfCapabilities.Add("CAPABILITY_IAM");

            List<Amazon.CloudFormation.Model.Parameter> parameters = new List<Amazon.CloudFormation.Model.Parameter>();
            parameters.Add(new Parameter
            {
                ParameterKey = "pEnvTag",
                ParameterValue = "development"
            });

            List<string> notificationArns = new List<string>();
            notificationArns.Add("aws:sns:eu-west-1:009837347446:tdwcftdevmainbastion-LoggingTemplate-1E3KD8XDHOSTY-rSecurityAlarmTopic-1TNN0GI7819UM");

            List<string> resourceTypes = new List<string>();
            resourceTypes.Add("AWS::*");

            List<Amazon.CloudFormation.Model.Tag> tagList = new List<Amazon.CloudFormation.Model.Tag>();
            tagList.Add(new Amazon.CloudFormation.Model.Tag() { Key = "environment", Value = "development" });

            CreateChangeSetRequest cfReq = new CreateChangeSetRequest()
            {
                Capabilities = CfCapabilities,
                ChangeSetName = "tdwv010001",
                ChangeSetType = ChangeSetType.UPDATE,
                ClientToken = "fromappsettingsv010001",
                Description = "Adding kinesis template to tdw stack and parameterizing env parameter",
                //NotificationARNs = notificationArns,
                Parameters = parameters,
                //ResourceTypes = resourceTypes,
                //RoleARN
                StackName = stack_name,
                Tags = tagList,
                TemplateBody = data
                //UsePreviousTemplate = true
            };

            CreateChangeSetResponse cfResp = cfClient.CreateChangeSet(cfReq);
        }

        static void TestCfStack(EncryptionMaterials encryptionMaterials)
        {
            string bucket_Name = QSS3BucketName;
            string templateName = QSS3KeyPrefix + TdwUtils.cfClassPathBastion.Replace("tdw_cf_template\\", "");
            string stack_name = templateName.Replace("-", "");
            stack_name = stack_name.Replace(".template", "");
            //AmazonS3EncryptionClient s3Client = new AmazonS3EncryptionClient(encryptionMaterials);
            AmazonS3Client s3Client = new AmazonS3Client();

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = bucket_Name,
                Key = templateName,
            };

            string data = null;
            using (GetObjectResponse getObjectResponse = s3Client.GetObject(getObjectRequest))
            {
                using (var stream = getObjectResponse.ResponseStream)
                using (var reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
            }

            Amazon.CloudFormation.AmazonCloudFormationClient cfClient = new AmazonCloudFormationClient();
            ValidateTemplateResponse templateResponse = cfClient.ValidateTemplate(new ValidateTemplateRequest() { TemplateBody = data });

            List<string> capabilities = templateResponse.Capabilities;
            string capabilitiesReason = templateResponse.CapabilitiesReason;
            string description = templateResponse.Description;
            List<TemplateParameter> parameters = templateResponse.Parameters;

            if (parameters.Any())
            {
                Console.WriteLine("  Parameters:");

                foreach (var p in parameters)
                {
                    Console.WriteLine("    {0} = {1}",
                      p.ParameterKey, p.Description);
                }
            }

            //try
            //{
            //    DeleteStackRequest deleteRequest = new DeleteStackRequest() { StackName = stack_name };
            //    cfClient.DeleteStack(deleteRequest);
            //}
            //catch (Exception ex)
            //{
            //    ex = null;
            //}

            DescribeStacksResponse testForStackDescResp = new DescribeStacksResponse();
            try
            {
                testForStackDescResp = cfClient.DescribeStacks(new DescribeStacksRequest() { StackName = stack_name });
            }
            catch (Exception ex)
            {
                testForStackDescResp = null;
            }

            if (testForStackDescResp == null)
            {
                List<string> CfCapabilities = new List<string>();
                CfCapabilities.Add("CAPABILITY_IAM");
                CreateStackRequest stackRequest = new CreateStackRequest() { StackName = stack_name, TemplateBody = data, Capabilities = CfCapabilities };

                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pDBPassword", ParameterValue = "LiverpoolFC" } );
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pNotifyEmail", ParameterValue = "peter.mcalister@tandem.co.uk" });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pEC2KeyPairBastion", ParameterValue = "BastionSshKvp" });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pEC2KeyPair", ParameterValue = "Ec2SshKvp" });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pSupportsConfig", ParameterValue = "Yes" });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pAvailabilityZoneA", ParameterValue = "eu-west-1a" });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pAvailabilityZoneB", ParameterValue = "eu-west-1b" });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "pVPCTenancy", ParameterValue = "default" });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "QSS3BucketName", ParameterValue = QSS3BucketName });
                //stackRequest.Parameters.Add(new Parameter() { ParameterKey = "QSS3KeyPrefix", ParameterValue = QSS3KeyPrefix });

                templateResponse = cfClient.ValidateTemplate(new ValidateTemplateRequest() { TemplateBody = data });
                CreateStackResponse stackResponse = cfClient.CreateStack(stackRequest);
            }

            testForStackDescResp = cfClient.DescribeStacks(new DescribeStacksRequest());

            foreach (var stack in testForStackDescResp.Stacks)
            {
                Console.WriteLine("stack: {0}", stack.StackName);
                Console.WriteLine("  status: {0}", stack.StackStatus);
                Console.WriteLine("  created: {0}", stack.CreationTime);

                var ps = stack.Parameters;

                if (ps.Any())
                {
                    Console.WriteLine("  parameters:");

                    foreach (var p in ps)
                    {
                        Console.WriteLine("    {0} = {1}",
                          p.ParameterKey, p.ParameterValue);
                    }
                }
            }
        }

        static void CopyTemplatesToS3(EncryptionMaterials encryptionMaterials)
        {
            string bucket_Name = null;
            string dataPath = null;
            byte[] dataBytes = null;
            PutObjectRequest request = null;
            PutObjectResponse response = null;
            //AmazonS3EncryptionClient s3Client = new AmazonS3EncryptionClient(encryptionMaterials);
            AmazonS3Client s3Client = new AmazonS3Client();

            try
            {
                TdwUtils.TearDownS3BucketByPrefix(s3Client, "tdwcftdev");
                AmazonS3Util.DeleteS3BucketWithObjects(s3Client, QSS3BucketName);
            }
            catch (Exception ex)
            {
                ex = null;
            }
            bucket_Name = TdwUtils.CreateBucket(s3Client, QSS3BucketName);

            ///Cross stack communication, Parent
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathParentSubnet);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathParentSubnet.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Cross stack communication, first child
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathChildSubnetProducer);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathChildSubnetProducer.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Cross stack communication, second child
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathChildSubnet);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathChildSubnet.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Application Template
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathApplication);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathApplication.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            //Config Rules
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathConfigRules);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathConfigRules.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Iam Template
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathIam);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathIam.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            //Kinesis
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathKinesis);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathKinesis.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Logging Template
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathLogging);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathLogging.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Main Bastion Template
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathBastion);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathBastion.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Management Vpc Template
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathManagementVpc);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathManagementVpc.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///Prod Vpc Template
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathProductionVpc);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathProductionVpc.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);

            ///ChangeSet Template for main bastion
            dataPath = TdwUtils.bingPathToAppDir(TdwUtils.cfClassPathBastionChangeSet);
            dataBytes = TdwUtils.FileToArray(dataPath);
            request = new PutObjectRequest()
            {
                BucketName = QSS3BucketName,
                Key = QSS3KeyPrefix + TdwUtils.cfClassPathBastionChangeSet.Replace("tdw_cf_template\\", ""),
                InputStream = new MemoryStream(dataBytes)
            };
            response = s3Client.PutObject(request);
        }
    }
}
