// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    public static class WindowsAzureSubscriptionExtensions
    {
        public static CloudStorageAccount GetCloudStorageAccount(this WindowsAzureSubscription subscription)
        {
            if (subscription == null || subscription.SubscriptionId == null)
            {
                return null;
            }

            if (subscription.currentCloudStorageAccount != null)
            {
                return subscription.currentCloudStorageAccount as CloudStorageAccount;
            }
            else
            {
                using (var storageClient = subscription.CreateClient<StorageManagementClient>())
                {
                    var storageServiceResponse = storageClient.StorageAccounts.Get(subscription.currentStorageAccountName);
                    var storageKeysResponse = storageClient.StorageAccounts.GetKeys(subscription.currentStorageAccountName);

                    subscription.currentCloudStorageAccount = new CloudStorageAccount(
                        new StorageCredentials(storageServiceResponse.StorageAccount.Name, storageKeysResponse.PrimaryKey),
                        GeneralUtilities.CreateHttpsEndpoint(storageServiceResponse.StorageAccount.Properties.Endpoints[0].ToString()),
                        GeneralUtilities.CreateHttpsEndpoint(storageServiceResponse.StorageAccount.Properties.Endpoints[1].ToString()),
                        GeneralUtilities.CreateHttpsEndpoint(storageServiceResponse.StorageAccount.Properties.Endpoints[2].ToString()),
                        GeneralUtilities.CreateHttpsEndpoint(storageServiceResponse.StorageAccount.Properties.Endpoints[3].ToString()));

                    return subscription.currentCloudStorageAccount as CloudStorageAccount;
                }
            }
        }
    }
}
