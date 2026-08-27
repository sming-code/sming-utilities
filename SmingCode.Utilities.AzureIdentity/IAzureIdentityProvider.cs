using Azure.Core;

namespace SmingCode.Utilities.AzureIdentity;

public interface IAzureIdentityProvider
{
    TokenCredential GetTokenCredential();
}
