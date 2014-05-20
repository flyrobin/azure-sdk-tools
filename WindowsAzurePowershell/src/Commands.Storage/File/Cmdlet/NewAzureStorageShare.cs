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
namespace Microsoft.WindowsAzure.Commands.Storage.File.Cmdlet
{
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    [Cmdlet(VerbsCommon.New, Constants.ShareCmdletName, DefaultParameterSetName = Constants.ShareNameParameterSetName)]
    public class NewAzureStorageShare : AzureStorageFileCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = "Name of the file share to be created.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        protected override void ExecuteCmdletInternal()
        {
            NamingUtil.ValidateShareName(this.Name, false);

            var client = this.GetCloudFileClient();
            var share = client.GetShareReference(this.Name);
            share.Create(this.RequestOptions, this.OperationContext);
            this.WriteObject(share);
        }
    }
}
