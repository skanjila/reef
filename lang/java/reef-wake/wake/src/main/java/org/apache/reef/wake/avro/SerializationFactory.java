/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package org.apache.reef.wake.avro;

import org.apache.reef.wake.avro.impl.MessageSerializerImpl;
import org.apache.reef.wake.avro.impl.MessageDeserializerImpl;

/**
 * Provides Avro message specific message serializers and deserializers.
 */
public final class SerializationFactory {

  private SerializationFactory() {}

  /**
   * Instantiate an Avro message serializer for the message type in the generic parameter.
   * @param msgMetaClass The reflection class for the message.
   * @param <TMessage> The type of the Avro message to be serialized.
   * @return A reference to an IMessageSerializer interface.
   */
  public static <TMessage> IMessageSerializer createSerializer(final Class<TMessage> msgMetaClass) {
    return new MessageSerializerImpl<>(msgMetaClass);
  }

  /**
   * Instantiate an Avro message deserializer for the message type in the generic parameter.
   * @param msgMetaClass The reflection class for the message.
   * @param <TMessage> The type of the Avro message to be deserialized.
   * @return A reference to an IMessageDeserializer interface.
   */
  public static <TMessage> IMessageDeserializer createDeserializer(final Class<TMessage> msgMetaClass) {
    return new MessageDeserializerImpl<>(msgMetaClass);
  }
}
