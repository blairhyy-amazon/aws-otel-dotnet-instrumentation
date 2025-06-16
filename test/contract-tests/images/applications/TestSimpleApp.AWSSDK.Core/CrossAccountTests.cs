using Amazon.S3;
using Amazon.S3.Model;

namespace TestSimpleApp.AWSSDK.Core;

public class CrossAccountTests(
    [FromKeyedServices("cross-account")] IAmazonS3 crossAccountClient,
    ILogger<CrossAccountTests> logger) :
    ContractTest(logger)
{
    public Task<PutBucketResponse> CreateBucketCrossAccount()
    {
        return crossAccountClient.PutBucketAsync(new PutBucketRequest 
        { 
            BucketName = "cross-account-bucket",
            BucketRegion = "eu-central-1"
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
