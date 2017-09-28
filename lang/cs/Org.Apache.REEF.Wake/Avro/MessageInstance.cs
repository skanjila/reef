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
// under the License

namespace Org.Apache.REEF.Wake.Avro
{
    /// <summary>
    /// Wrapper class to bind a specific instance of a message with the associated sequence number.
    /// </summary>
    /// <typeparam name="T">Message payload type.</typeparam>
    public sealed class MessageInstance<T> : IMessageInstance<T>
    {
        /// <summary>
        /// Get the sequence number of a message.
        /// </summary>
        public long Sequence { get; private set; }

        /// <summary>
        /// Return the data payload of message instance.
        /// </summary>
        public T Message { get; private set; }

        /// <summary>
        /// Create a new instance of the (sequence number, message payload) pair.
        /// </summary>
        /// <param name="sequence">The message sequence number.</param>
        /// <param name="message">The message payload.</param>
        public MessageInstance(long sequence, T message)
        {
            Sequence = sequence;
            Message = message;
        }
    }
}
