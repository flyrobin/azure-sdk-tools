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

namespace Microsoft.WindowsAzure.Commands.Storage.Model.Contract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.File;

    /// <summary>
    /// File management interface
    /// </summary>
    public interface IStorageFileManagement : IStorageManagement
    {
        /// <summary>
        ///  Returns a reference to a Microsoft.WindowsAzure.Storage.File.CloudFileShare
        ///  object with the specified name.
        /// </summary>
        /// <param name="shareName">A string containing the name of the share.</param>
        /// <returns>A reference to a share.</returns>
        public CloudFileShare GetShareReference(string shareName);

        /// <summary>
        ///  Returns an enumerable collection of the files in the share, which are retrieved
        ///  lazily.
        /// </summary>
        /// <param name="directory">Indicating the directory to be listed.</param>
        /// <param name="options">
        ///  An Microsoft.WindowsAzure.Storage.File.FileRequestOptions object that specifies
        ///  additional options for the request.
        /// </param>
        /// <param name="operationContext">
        ///  An Microsoft.WindowsAzure.Storage.OperationContext object that represents
        ///  the context for the current operation.
        /// </param>
        /// <returns>
        ///  An enumerable collection of objects that implement Microsoft.WindowsAzure.Storage.File.IListFileItem
        ///  and are retrieved lazily.
        /// </returns>
        public IEnumerable<IListFileItem> ListFilesAndDirectories(CloudFileDirectory directory, FileRequestOptions options = null, OperationContext operationContext = null);
    }
}
