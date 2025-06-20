// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace integration_test_app.Controllers;

[ApiController]
[Route("[controller]")]
public class AppController : ControllerBase
{
    private readonly AmazonS3Client s3Client = new AmazonS3Client();
    private readonly HttpClient httpClient = new HttpClient();
    private readonly AmazonKinesisClient kinesisClient = new AmazonKinesisClient();
    private readonly AmazonSecurityTokenServiceClient stsClient = new AmazonSecurityTokenServiceClient();
    private readonly AmazonSQSClient sqsClient = new AmazonSQSClient();
    
    [HttpGet]
    [Route("/get-bucket")]
    public async Task<string> GetBucket(string roleArn)
    {
        var sessionName = "CrossAccountAccess";
        var bucketName = "cross-account-test-blairhyy";
        
        var assumeRoleResponse = await stsClient.AssumeRoleAsync(new AssumeRoleRequest
        {
            RoleArn = roleArn, RoleSessionName = sessionName
        });

        var credentials = assumeRoleResponse.Credentials;

        var s3Client = new AmazonS3Client(
            credentials.AccessKeyId,
            credentials.SecretAccessKey,
            credentials.SessionToken,
            RegionEndpoint.USEast2 // Adjust region as needed
        );
        
        await s3Client.GetBucketLocationAsync(new GetBucketLocationRequest
        {
            BucketName = bucketName
        });

        return this.GetTraceId();
    }

    [HttpPost]
    [Route("/send-message")]
    public async Task<string> SendMessageToQueue(string queueUrl)
    {
        var sendRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = "dotnet sqs"
        };

        await sqsClient.SendMessageAsync(sendRequest);
        return this.GetTraceId();
    }
    
    [HttpGet]
    [Route("/get-table")]
    public async Task<string> GetTable(string tableName, string roleArn)
    {
        var sessionName = "CrossAccountAccess";
        
        var assumeRoleResponse = await stsClient.AssumeRoleAsync(new AssumeRoleRequest
        {
            RoleArn = roleArn, RoleSessionName = sessionName
        });

        var credentials = assumeRoleResponse.Credentials;

        var dynamoDbClient = new AmazonDynamoDBClient(
            credentials.AccessKeyId,
            credentials.SecretAccessKey,
            credentials.SessionToken,
            RegionEndpoint.USEast2 // Use appropriate region
        );
        
        // Describe the table
        var tableResponse = await dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
        {
            TableName = tableName
        });

        string tableArn = tableResponse.Table.TableArn;
        return this.GetTraceId() + tableArn;
    }

    [HttpGet]
    [Route("/kinesis-stream")]
    public async Task<string> GetKinesisStream(string streamArn)
    {
        var request = new DescribeStreamSummaryRequest { StreamARN = streamArn };

        // var request = new CreateStreamRequest { StreamName = "test_stream" };
        // var response = await this.kinesisClient.CreateStreamAsync(request);
        var response = await this.kinesisClient.DescribeStreamSummaryAsync(request);
        var summary = response.StreamDescriptionSummary;
        return this.GetTraceId() + summary.StreamARN.ToString();
    }

    [HttpGet]
    [Route("/outgoing-http-call")]
    public string OutgoingHttp()
    {
        _ = this.httpClient.GetAsync("https://aws.amazon.com").Result;

        return this.GetTraceId();
    }

    [HttpGet]
    [Route("/aws-sdk-call")]
    public string AWSSDKCall()
    {
        _ = this.s3Client.ListBucketsAsync().Result;

        return this.GetTraceId();
    }

    [HttpGet]
    [Route("/")]
    public string Default()
    {
        return "Application started!";
    }

    private string GetTraceId()
    {
        var traceId = Activity.Current.TraceId.ToHexString();
        var version = "1";
        var epoch = traceId.Substring(0, 8);
        var random = traceId.Substring(8);
        return "{" + "\"traceId\"" + ": " + "\"" + version + "-" + epoch + "-" + random + "\"" + "}";
    }
}
