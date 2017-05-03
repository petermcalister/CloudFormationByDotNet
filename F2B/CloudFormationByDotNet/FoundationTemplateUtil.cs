using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.CloudFormation;
using System.Configuration;
using Amazon.Runtime.CredentialManagement;
using Amazon.CloudFormation.Model;

namespace Tandem.Tdw.F2B.Demo
{
    class FoundationTemplateUtil
    {
        private AWSCredentials awsCredentials;
        
        public FoundationTemplateUtil(Amazon.Runtime.AWSCredentials credentials)
        {
            //IAmazonCloudFormation stackbuilder = new AmazonCloudFormationClient(credentials);
            
            IAmazonCloudFormation stackbuilder = new AmazonCloudFormationClient();

            AmazonCloudFormationRequest request = new DescribeStacksRequest();

            var response = stackbuilder.DescribeStacks(new DescribeStacksRequest());

            foreach (var stack in response.Stacks)
            {
                Console.WriteLine("Stack: {0}", stack.StackName);
                Console.WriteLine("  Status: {0}", stack.StackStatus);
                Console.WriteLine("  Created: {0}", stack.CreationTime);

                var ps = stack.Parameters;

                if (ps.Any())
                {
                    Console.WriteLine("  Parameters:");

                    foreach (var p in ps)
                    {
                        Console.WriteLine("    {0} = {1}",
                          p.ParameterKey, p.ParameterValue);
                    }

                }
            }

            //AWSRegion euWest1 = AWSRegion.SetRegionFromName(RegionEndpoint.EUWest1);

            Console.WriteLine("===========================================");
            Console.WriteLine("Getting Started with AWS CloudFormation");
            Console.WriteLine("===========================================\n");

            String stackName = "CloudFormationSampleStack";
            String logicalResourceName = "SampleNotificationTopic";
            //C:\Bitbucket\Analytics\MetaDataStore\F2B\main-bastion-json.template\main-bastion-json.template
            try
            {
                // Create a stack
                CreateStackRequest createRequest = new CreateStackRequest();
                createRequest.StackName = stackName;
                //createRequest.StackPolicyBody = ;
                //createRequest.
                //Clou
                //createRequest.StackPolicyBody = convertStreamToString(CloudFormationSample.class.getResourceAsStream("CloudFormationSample.template"));
                //Console.WriteLine("Creating a stack called " + createRequest.getStackName() + ".");
                //stackbuilder.createStack(createRequest);

                //// Wait for stack to be created
                //// Note that you could use SNS notifications on the CreateStack call to track the progress of the stack creation
                //Console.WriteLine("Stack creation completed, the stack " + stackName + " completed with " + waitForCompletion(stackbuilder, stackName);

            }
            catch
            {
            }
        }

        // Convert a stream into a single, newline separated string
        static String convertStreamToString(System.IO.Stream InputStream)
        {
            StreamReader reader = new StreamReader(InputStream); // new BufferedReader(new InputStreamReader(in));
            StringBuilder stringbuilder = new StringBuilder();
            String line = null;
            while ((line = reader.ReadLine()) != null)
            {
                stringbuilder.Append(line + "\n");
            }
            InputStream.Close();
            return stringbuilder.ToString();
        }

        //static String waitForCompletion(Amazon.CloudFormation. stackbuilder, String stackName) 
        //{
        //    DescribeStacksRequest wait = new DescribeStacksRequest();
        //}
            //{

            //    
            //wait.setStackName(stackName);
            //        Boolean completed = false;
            //String stackStatus = "Unknown";
            //String stackReason = "";

            //System.out.print("Waiting");

            //        while (!completed) {
            //            List<Stack> stacks = stackbuilder.describeStacks(wait).getStacks();
            //            if (stacks.isEmpty())
            //            {
            //                completed   = true;
            //                stackStatus = "NO_SUCH_STACK";
            //                stackReason = "Stack has been deleted";
            //            } else {
            //                for (Stack stack : stacks) {
            //                    if (stack.getStackStatus().equals(StackStatus.CREATE_COMPLETE.toString()) ||
            //                            stack.getStackStatus().equals(StackStatus.CREATE_FAILED.toString()) ||
            //                            stack.getStackStatus().equals(StackStatus.ROLLBACK_FAILED.toString()) ||
            //                            stack.getStackStatus().equals(StackStatus.DELETE_FAILED.toString())) {
            //                        completed = true;
            //                        stackStatus = stack.getStackStatus();
            //                        stackReason = stack.getStackStatusReason();
            //                    }
            //                }
            //            }

            //            // Show we are waiting
            //            System.out.print(".");

            //            // Not done yet so sleep for 10 seconds.
            //            if (!completed) Thread.sleep(10000);
            //        }

            //        // Show we are done
            //        System.out.print("done\n");

            //        return stackStatus + " (" + stackReason + ")";

    }
}


            //            // Show all the stacks for this account along with the resources for each stack
            //            for (Stack stack : stackbuilder.describeStacks(new DescribeStacksRequest()).getStacks()) {
            //                System.out.println("Stack : " + stack.getStackName() + " [" + stack.getStackStatus().toString() + "]");

            //                DescribeStackResourcesRequest stackResourceRequest = new DescribeStackResourcesRequest();
            //    stackResourceRequest.setStackName(stack.getStackName());
            //                for (StackResource resource : stackbuilder.describeStackResources(stackResourceRequest).getStackResources()) {
            //                    System.out.format("    %1$-40s %2$-25s %3$s\n", resource.getResourceType(), resource.getLogicalResourceId(), resource.getPhysicalResourceId());
            //                }
            //            }

            //            // Lookup a resource by its logical name
            //            DescribeStackResourcesRequest logicalNameResourceRequest = new DescribeStackResourcesRequest();
            //logicalNameResourceRequest.setStackName(stackName);
            //            logicalNameResourceRequest.setLogicalResourceId(logicalResourceName);
            //            System.out.format("Looking up resource name %1$s from stack %2$s\n", logicalNameResourceRequest.getLogicalResourceId(), logicalNameResourceRequest.getStackName());
            //            for (StackResource resource : stackbuilder.describeStackResources(logicalNameResourceRequest).getStackResources()) {
            //                System.out.format("    %1$-40s %2$-25s %3$s\n", resource.getResourceType(), resource.getLogicalResourceId(), resource.getPhysicalResourceId());
            //            }

            //            // Delete the stack
            //            DeleteStackRequest deleteRequest = new DeleteStackRequest();
            //deleteRequest.setStackName(stackName);
            //            System.out.println("Deleting the stack called " + deleteRequest.getStackName() + ".");
            //            stackbuilder.deleteStack(deleteRequest);

            //            // Wait for stack to be deleted
            //            // Note that you could used SNS notifications on the original CreateStack call to track the progress of the stack deletion
            //            System.out.println("Stack creation completed, the stack " + stackName + " completed with " + waitForCompletion(stackbuilder, stackName));

            //        } catch (AmazonServiceException ase) {
            //            System.out.println("Caught an AmazonServiceException, which means your request made it "
            //                    + "to AWS CloudFormation, but was rejected with an error response for some reason.");
            //System.out.println("Error Message:    " + ase.getMessage());
            //            System.out.println("HTTP Status Code: " + ase.getStatusCode());
            //            System.out.println("AWS Error Code:   " + ase.getErrorCode());
            //            System.out.println("Error Type:       " + ase.getErrorType());
            //            System.out.println("Request ID:       " + ase.getRequestId());
            //        } catch (AmazonClientException ace) {
            //            System.out.println("Caught an AmazonClientException, which means the client encountered "
            //                    + "a serious internal problem while trying to communicate with AWS CloudFormation, "
            //                    + "such as not being able to access the network.");
            //System.out.println("Error Message: " + ace.getMessage());
            //        }
            //    }


            //    // Wait for a stack to complete transitioning
            //    // End stack states are:
            //    //    CREATE_COMPLETE
            //    //    CREATE_FAILED
            //    //    DELETE_FAILED
            //    //    ROLLBACK_FAILED
            //    // OR the stack no longer exists
