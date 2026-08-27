using Azure.Core;

namespace SmingCode.Utilities.AzureIdentity;

internal class AzureIdentityProviderHelper(
    Func<TokenCredential> tokenCredentialFactory
)
{
    internal TokenCredential GetTokenCredential() => tokenCredentialFactory();
}
