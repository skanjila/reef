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

using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Org.Apache.REEF.Client.API;
using Org.Apache.REEF.Client.Yarn;
using Org.Apache.REEF.Client.YARN;
using Org.Apache.REEF.Driver;
using Org.Apache.REEF.Tang.Implementations.Tang;
using Org.Apache.REEF.Tang.Util;
using Xunit;

namespace Org.Apache.REEF.Client.Tests
{
    public sealed class YarnREEFParamSerializerTests
    {
        private const int AnyInt = 1000;
        private const string AnyString = "Any";

        [Fact]
        public void TestYarnREEFDotNetAppSerialization()
        {
            const string formatStr = "{{" +
                                     "\"sharedAppSubmissionParameters\":" +
                                     "{{" +
                                     "\"tcpBeginPort\":{0}," +
                                     "\"tcpRangeCount\":{0}," +
                                     "\"tcpTryCount\":{0}" +
                                     "}}," +
                                     "\"driverRecoveryTimeout\":{0}" +
                                     "}}";

            var expectedJson = string.Format(formatStr, AnyInt);

            var tcpConf = TcpPortConfigurationModule.ConfigurationModule
                .Set(TcpPortConfigurationModule.PortRangeCount, AnyInt.ToString())
                .Set(TcpPortConfigurationModule.PortRangeStart, AnyInt.ToString())
                .Set(TcpPortConfigurationModule.PortRangeTryCount, AnyInt.ToString())
                .Build();

            var driverConf = DriverConfiguration.ConfigurationModule
                .Set(DriverConfiguration.OnDriverStarted, GenericType<DriverStartHandler>.Class)
                .Set(DriverConfiguration.DriverRestartEvaluatorRecoverySeconds, AnyInt.ToString())
                .Build();

            var injector = TangFactory.GetTang().NewInjector(tcpConf, driverConf);

            var serializer = injector.GetInstance<YarnREEFDotNetParamSerializer>();
            var jobRequest = injector.GetInstance<JobRequestBuilder>().Build();

            var serializedBytes = serializer.SerializeAppArgsToBytes(
                jobRequest.AppParameters, injector, AnyString);
            var expectedString = Encoding.UTF8.GetString(serializedBytes);
            var jsonObject = JObject.Parse(expectedString);
            var expectedJsonObject = JObject.Parse(expectedJson);
            Assert.True(JToken.DeepEquals(jsonObject, expectedJsonObject));
        }

        [Fact]
        public void TestYarnREEFDotNetJobSerialization()
        {
            const string formatString =
                "{{" +
                "\"sharedJobSubmissionParameters\":" +
                "{{" +
                "\"jobId\":\"{0}\"," +
                "\"jobSubmissionFolder\":\"{0}\"" +
                "}}," +
                "\"dfsJobSubmissionFolder\":\"{0}\"," +
                "\"fileSystemUrl\":\"{1}\"," +
                "\"jobSubmissionDirectoryPrefix\":\"{0}\"" +
                "}}";

            var expectedJson = string.Format(formatString, AnyString, "NULL");
            var injector = TangFactory.GetTang().NewInjector();

            var serializer = injector.GetInstance<YarnREEFDotNetParamSerializer>();
            var jobRequest = injector.GetInstance<JobRequestBuilder>().SetJobIdentifier(AnyString).Build();

            var serializedBytes = serializer.SerializeJobArgsToBytes(jobRequest.JobParameters, AnyString, AnyString);
            var expectedString = Encoding.UTF8.GetString(serializedBytes);
            var jsonObject = JObject.Parse(expectedString);
            var expectedJsonObject = JObject.Parse(expectedJson);
            Assert.True(JToken.DeepEquals(jsonObject, expectedJsonObject));
        }

        [Fact]
        public void TestYarnREEFAppSerialization()
        {
            const string formatString = "{{" +
                                        "\"sharedAppSubmissionParameters\":" +
                                        "{{\"tcpBeginPort\":{0}," +
                                        "\"tcpRangeCount\":{0}," +
                                        "\"tcpTryCount\":{0}" +
                                        "}}," +
                                        "\"driverRecoveryTimeout\":{0}" +
                                        "}}";

            var expectedJson = string.Format(formatString, AnyInt);
            var tcpConf = TcpPortConfigurationModule.ConfigurationModule
                .Set(TcpPortConfigurationModule.PortRangeCount, AnyInt.ToString())
                .Set(TcpPortConfigurationModule.PortRangeStart, AnyInt.ToString())
                .Set(TcpPortConfigurationModule.PortRangeTryCount, AnyInt.ToString())
                .Build();

            var driverConf = DriverConfiguration.ConfigurationModule
                .Set(DriverConfiguration.OnDriverStarted, GenericType<DriverStartHandler>.Class)
                .Set(DriverConfiguration.DriverRestartEvaluatorRecoverySeconds, AnyInt.ToString())
                .Build();

            var injector = TangFactory.GetTang().NewInjector(tcpConf, driverConf);

            var serializer = injector.GetInstance<YarnREEFParamSerializer>();
            var jobRequest = injector.GetInstance<JobRequestBuilder>()
                .SetDriverMemory(AnyInt)
                .SetMaxApplicationSubmissions(AnyInt)
                .Build();

            var serializedBytes = serializer.SerializeAppArgsToBytes(jobRequest.AppParameters, injector);
            var expectedString = Encoding.UTF8.GetString(serializedBytes);
            var jsonObject = JObject.Parse(expectedString);
            var expectedJsonObject = JObject.Parse(expectedJson);
            Assert.True(JToken.DeepEquals(jsonObject, expectedJsonObject));
        }

        [Fact]
        public void TestYarnREEFJobSerialization()
        {
            const string formatString =
                "{{" +
                "\"yarnJobSubmissionParameters\":" +
                "{{" +
                "\"sharedJobSubmissionParameters\":" +
                "{{" +
                "\"jobId\":\"{0}\"," +
                "\"jobSubmissionFolder\":\"{0}\"" +
                "}}," +
                "\"dfsJobSubmissionFolder\":\"NULL\"," +
                "\"fileSystemUrl\":\"{0}\"," +
                "\"jobSubmissionDirectoryPrefix\":\"{0}\"" +
                "}}," +
                "\"securityTokenKind\":\"{0}\"," +
                "\"securityTokenService\":\"{0}\"," +
                "\"maxApplicationSubmissions\":{1}," +
                "\"driverMemory\":{1}," +
                "\"driverStdoutFilePath\":\"{0}\"," +
                "\"driverStderrFilePath\":\"{0}\"" +
                "}}";

            var conf = YARNClientConfiguration.ConfigurationModule
                .Set(YARNClientConfiguration.SecurityTokenKind, AnyString)
                .Set(YARNClientConfiguration.SecurityTokenService, AnyString)
                .Set(YARNClientConfiguration.FileSystemUrl, AnyString)
                .Set(YARNClientConfiguration.JobSubmissionFolderPrefix, AnyString)
                .Build();

            var expectedJson = string.Format(formatString, AnyString, AnyInt);
            var injector = TangFactory.GetTang().NewInjector(conf);

            var serializer = injector.GetInstance<YarnREEFParamSerializer>();
            var jobRequest = injector.GetInstance<JobRequestBuilder>()
                .SetJobIdentifier(AnyString)
                .SetMaxApplicationSubmissions(AnyInt)
                .SetDriverMemory(AnyInt)
                .SetDriverStderrFilePath(AnyString)
                .SetDriverStdoutFilePath(AnyString)
                .Build();

            var serializedBytes = serializer.SerializeJobArgsToBytes(jobRequest.JobParameters, AnyString);
            var expectedString = Encoding.UTF8.GetString(serializedBytes);
            var jsonObject = JObject.Parse(expectedString);
            var expectedJsonObject = JObject.Parse(expectedJson);
            Assert.True(JToken.DeepEquals(jsonObject, expectedJsonObject));
        }

        private sealed class DriverStartHandler : IObserver<IDriverStarted>
        {
            public void OnNext(IDriverStarted value)
            {
                // Intentionally empty.
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }
        }
    }
}
