﻿// ----------------------------------------------------------------------------------
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
namespace Microsoft.WindowsAzure.Commands.Storage.File
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Commands.Storage.Model.ResourceModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.File;

    public abstract class AzureStorageFileCmdletBase : StorageCmdletBase
    {
        private FileRequestOptions requestOptions;

        private OperationContext opertaionContext;

        [Parameter(
            ValueFromPipeline = true,
            ParameterSetName = Constants.ShareNameParameterSetName,
            HelpMessage = "Azure Storage Context Object")]
        [Parameter(
            ValueFromPipeline = true,
            ParameterSetName = Constants.MatchingPrefixParameterSetName,
            HelpMessage = "Azure Storage Context Object")]
        [Parameter(
            ValueFromPipeline = true,
            ParameterSetName = Constants.SpecificParameterSetName,
            HelpMessage = "Azure Storage Context Object")]
        public AzureStorageContext Context { get; set; }

        [Parameter(HelpMessage = "The server time out for each request in seconds.")]
        public int? ServerTimeoutPerRequest { get; set; }

        [Parameter(HelpMessage = "The client side maximum execution time for each request in seconds.")]
        public int? ClientTimeoutPerRequest { get; set; }

        [Parameter(HelpMessage = "The client request id used for tracking storage REST API.")]
        public string ClientRequestId { get; set; }

        public AzureStorageFileCmdletBase()
        {
            this.ClientRequestId = Guid.NewGuid().ToString();
        }

        protected virtual TimeSpan DefaultServerTimeoutPerRequest
        {
            get
            {
                return Constants.DefaultServerTimeoutPerRequest;
            }
        }

        protected virtual TimeSpan DefaultClientTimeoutPerRequest
        {
            get
            {
                return Constants.DefaultClientTimeoutPerRequests;
            }
        }

        protected AccessCondition AccessCondition
        {
            get
            {
                return AccessCondition.GenerateEmptyCondition();
            }
        }

        protected FileRequestOptions RequestOptions
        {
            get
            {
                if (this.requestOptions == null)
                {
                    this.requestOptions = new FileRequestOptions();
                    if (this.ServerTimeoutPerRequest.HasValue)
                    {
                        this.requestOptions.ServerTimeout = ConvertToTimeSpan(this.ServerTimeoutPerRequest.Value);
                    }
                    else
                    {
                        this.requestOptions.ServerTimeout = this.DefaultServerTimeoutPerRequest;
                    }

                    if (this.ClientTimeoutPerRequest.HasValue)
                    {
                        this.requestOptions.MaximumExecutionTime = ConvertToTimeSpan(this.ClientTimeoutPerRequest.Value);
                    }
                    else
                    {
                        this.requestOptions.MaximumExecutionTime = this.DefaultClientTimeoutPerRequest;
                    }
                }

                return this.requestOptions;
            }
        }

        protected OperationContext OperationContext
        {
            get
            {
                if (this.opertaionContext == null)
                {
                    this.opertaionContext = new OperationContext()
                    {
                        ClientRequestID = this.ClientRequestId,
                        StartTime = DateTime.Now
                    };
                }

                return this.opertaionContext;
            }
        }

        protected CloudFileClient GetCloudFileClient()
        {
            if (this.Context != null)
            {
                return this.Context.StorageAccount.CreateCloudFileClient();
            }

            return this.GetStorageAccountFromEnvironmentVariable().CreateCloudFileClient();
        }

        protected CloudFileShare BuildFileShareObjectFromName(string name)
        {
            NamingUtil.ValidateShareName(name, false);
            var client = this.GetCloudFileClient();
            return client.GetShareReference(name);
        }

        private static TimeSpan? ConvertToTimeSpan(int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                return TimeSpan.FromSeconds(timeoutInSeconds);
            }
            else if (timeoutInSeconds == Timeout.Infinite)
            {
                return null;
            }
            else
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidTimeoutValue, timeoutInSeconds));
            }
        }

        /// <summary>
        /// Get storage account from environment variable "AZURE_STORAGE_CONNECTION_STRING"
        /// </summary>
        /// <returns>Cloud storage account</returns>
        private CloudStorageAccount GetStorageAccountFromEnvironmentVariable()
        {
            string connectionString = Environment.GetEnvironmentVariable(Constants.ConnectionStringEnvironmentName);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(Resources.DefaultStorageCredentialsNotFound);
            }
            else
            {
                this.WriteDebugWithTimestamp(Resources.GetStorageAccountFromEnvironmentVariable);

                try
                {
                    return CloudStorageAccount.Parse(connectionString);
                }
                catch
                {
                    this.WriteVerboseWithTimestamp(Resources.CannotGetStorageAccountFromEnvironmentVariable);
                    throw;
                }
            }
        }
    }
}
