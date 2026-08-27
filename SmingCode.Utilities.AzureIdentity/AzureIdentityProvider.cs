using Azure.Core;

namespace SmingCode.Utilities.AzureIdentity;

internal class AzureIdentityProvider(
    AzureIdentityProviderHelper _azureIdentityProviderHelper
) : IAzureIdentityProvider
{
    public TokenCredential GetTokenCredential() =>
        _azureIdentityProviderHelper.GetTokenCredential();
}