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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudService
{
    using System;
    using System.IO;
    using System.Linq;
    using Common;
    using Commands.Common.Properties;

    public class PublishContext
    {
        public ServiceSettings ServiceSettings { get; private set; }
        
        public string PackagePath { get; set; }
        
        public string CloudConfigPath { get; private set; }

        public string RootPath { get; private set; }
        
        public string ServiceName { get; private set; }
        
        public string DeploymentName { get; private set; }
        
        public string SubscriptionId { get; private set; }

        public CloudServiceProject ServiceProject { get; set; }

        public bool PackageIsFromStorageAccount { get; set; }

        public PublishContext(
            ServiceSettings settings,
            string packagePath,
            string cloudConfigPath,
            string serviceName,
            string deploymentName,
            string rootPath)
        {
            Validate.ValidateNullArgument(settings, Resources.InvalidServiceSettingMessage);
            Validate.ValidateStringIsNullOrEmpty(packagePath, "packagePath");
            Validate.ValidateFileFull(cloudConfigPath, Resources.ServiceConfiguration);
            Validate.ValidateStringIsNullOrEmpty(serviceName, "serviceName");
            
            this.ServiceSettings = settings;
            this.PackagePath = packagePath;
            this.CloudConfigPath = cloudConfigPath;
            this.RootPath = rootPath;
            this.ServiceName = serviceName;
            this.DeploymentName = string.IsNullOrEmpty(deploymentName) ? 
                char.ToLower(ServiceSettings.Slot[0]) + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff") 
                : deploymentName;

            if (!string.IsNullOrEmpty(settings.Subscription))
            {
                try
                {
                    SubscriptionId =
                        WindowsAzureProfile.Instance.Subscriptions.Where(s => s.SubscriptionName == settings.Subscription)
                            .Select(s => s.SubscriptionId)
                            .First();
                }
                catch (Exception)
                {
                    throw new ArgumentException(Resources.SubscriptionIdNotFoundMessage, settings.Subscription);
                }
            }
            else
            {
                throw new ArgumentNullException("settings.Subscription", Resources.InvalidSubscriptionNameMessage);
            }
        }

        public void ConfigPackageSettings(string package, string workingDirectory)
        {
            PackagePath = package;
            PackageIsFromStorageAccount = IsStorageAccountUrl(package);
            if (!PackageIsFromStorageAccount)
            {
                if (!Path.IsPathRooted(package))
                {
                    PackagePath = Path.Combine(workingDirectory, package);
                }
            }
        }

        private bool IsStorageAccountUrl(string packagePath)
        {
            bool result = false;
            try
            {
                Uri uri = new Uri(packagePath);
                return uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && 
                    uri.Host.EndsWith("blob.core.windows.net", StringComparison.OrdinalIgnoreCase);
            }
            catch { }
            return result;
        }
    }
}