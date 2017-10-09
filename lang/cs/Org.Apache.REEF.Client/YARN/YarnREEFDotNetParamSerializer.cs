﻿// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System.IO;
using Org.Apache.REEF.Client.API;
using Org.Apache.REEF.Client.Avro;
using Org.Apache.REEF.Client.Avro.YARN;
using Org.Apache.REEF.Client.YARN.Parameters;
using Org.Apache.REEF.Common.Avro;
using Org.Apache.REEF.Common.Files;
using Org.Apache.REEF.Driver.Bridge;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Tang.Interface;
using Org.Apache.REEF.Wake.Remote.Parameters;

namespace Org.Apache.REEF.Client.YARN
{
    /// <summary>
    /// Job/application parameter file serializer for <see cref="YarnREEFDotNetClient"/>.
    /// </summary>
    internal sealed class YarnREEFDotNetParamSerializer
    {
        private readonly REEFFileNames _fileNames;
        private readonly string _fileSystemUrl;

        [Inject]
        private YarnREEFDotNetParamSerializer(REEFFileNames fileNames,
            [Parameter(typeof(FileSystemUrl))] string fileSystemUrl)
        {
            _fileNames = fileNames;
            _fileSystemUrl = fileSystemUrl;
        }

        /// <summary>
        /// Serializes the application parameters to reef/local/app-submission-params.json.
        /// </summary>
        internal void SerializeAppFile(AppParameters appParameters, IInjector paramInjector, string localDriverFolderPath)
        {
            var serializedArgs = SerializeAppArgsToBytes(appParameters, paramInjector, localDriverFolderPath);

            var submissionAppArgsFilePath = Path.Combine(
                localDriverFolderPath, _fileNames.GetLocalFolderPath(), _fileNames.GetAppSubmissionParametersFile());

            using (var jobArgsFileStream = new FileStream(submissionAppArgsFilePath, FileMode.CreateNew))
            {
                jobArgsFileStream.Write(serializedArgs, 0, serializedArgs.Length);
            }
        }

        internal byte[] SerializeAppArgsToBytes(AppParameters appParameters, IInjector paramInjector, string localDriverFolderPath)
        {
            var avroAppSubmissionParameters = new AvroAppSubmissionParameters
            {
                tcpBeginPort = paramInjector.GetNamedInstance<TcpPortRangeStart, int>(),
                tcpRangeCount = paramInjector.GetNamedInstance<TcpPortRangeCount, int>(),
                tcpTryCount = paramInjector.GetNamedInstance<TcpPortRangeTryCount, int>()
            };

            var avroYarnAppSubmissionParameters = new AvroYarnAppSubmissionParameters
            {
                sharedAppSubmissionParameters = avroAppSubmissionParameters,
                driverRecoveryTimeout =
                    paramInjector.GetNamedInstance<DriverBridgeConfigurationOptions.DriverRestartEvaluatorRecoverySeconds, int>()
            };

            return AvroJsonSerializer<AvroYarnAppSubmissionParameters>.ToBytes(avroYarnAppSubmissionParameters);
        }

        /// <summary>
        /// Serializes the job parameters to job-submission-params.json.
        /// </summary>
        internal void SerializeJobFile(JobParameters jobParameters, string localDriverFolderPath, string jobSubmissionDirectory)
        {
            var serializedArgs = SerializeJobArgsToBytes(jobParameters, localDriverFolderPath, jobSubmissionDirectory);

            var submissionJobArgsFilePath = Path.Combine(localDriverFolderPath,
                _fileNames.GetJobSubmissionParametersFile());

            using (var jobArgsFileStream = new FileStream(submissionJobArgsFilePath, FileMode.CreateNew))
            {
                jobArgsFileStream.Write(serializedArgs, 0, serializedArgs.Length);
            }
        }

        internal byte[] SerializeJobArgsToBytes(JobParameters jobParameters, string localDriverFolderPath, string jobSubmissionDirectory)
        {
            var avroJobSubmissionParameters = new AvroJobSubmissionParameters
            {
                jobId = jobParameters.JobIdentifier,
                jobSubmissionFolder = localDriverFolderPath
            };

            var avroYarnJobSubmissionParameters = new AvroYarnJobSubmissionParameters
            {
                jobSubmissionDirectoryPrefix = jobSubmissionDirectory,
                dfsJobSubmissionFolder = jobSubmissionDirectory,
                fileSystemUrl = _fileSystemUrl,
                sharedJobSubmissionParameters = avroJobSubmissionParameters
            };

            return AvroJsonSerializer<AvroYarnJobSubmissionParameters>.ToBytes(avroYarnJobSubmissionParameters);
        }
    }
}
