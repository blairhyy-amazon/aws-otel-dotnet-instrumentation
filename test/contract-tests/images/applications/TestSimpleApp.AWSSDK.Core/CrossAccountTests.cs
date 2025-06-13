using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace TestSimpleApp.AWSSDK.Core;

public class CrossAccountTests(
    ILogger<CrossAccountTests> logger) :
    ContractTest(logger)
{
    private const string CrossAccountEndpoint = "http://localstack:4566";
    private const string CrossAccountRegion = "eu-central-1";

    public Task<PutBucketResponse> CreateBucketCrossAccount()
    {
        var crossAccountClient = CreateCrossAccountS3Client();
        
        return crossAccountClient.PutBucketAsync(new PutBucketRequest 
        { 
            BucketName = "cross-account-bucket",
            BucketRegion = CrossAccountRegion
        });
    }
    
    private IAmazonS3 CreateCrossAccountS3Client()
    {
        var credentials = new SessionAWSCredentials(
            "account_b_access_key_id",
            "account_b_secret_access_key",
            "account_b_token");
        
        return new AmazonS3Client(
            credentials,
            new AmazonS3Config
            {
                ServiceURL = CrossAccountEndpoint,
                ForcePathStyle = true,
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(CrossAccountRegion)
            });
    }

    protected override Task CreateFault(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override Task CreateError(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
