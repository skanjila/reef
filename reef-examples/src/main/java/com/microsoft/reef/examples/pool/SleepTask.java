/**
 * Copyright (C) 2014 Microsoft Corporation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *         http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.microsoft.reef.examples.pool;

import com.microsoft.reef.task.Task;
import com.microsoft.tang.annotations.Parameter;

import javax.inject.Inject;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Sleep for delay seconds and quit.
 */
public class SleepTask implements Task {

  /** Standard java logger. */
  private static final Logger LOG = Logger.getLogger(SleepTask.class.getName());

  /** Number of milliseconds to sleep. */
  private final int delay;

  /**
   * Task constructor. Parameters are injected automatically by TANG.
   * @param delay number of seconds to sleep.
   */
  @Inject
  private SleepTask(final @Parameter(Launch.Delay.class) Integer delay) {
    this.delay = delay * 1000;
  }

  /**
   * Sleep for delay milliseconds and return.
   * @param memento ignored.
   * @return null.
   */
  @Override
  public byte[] call(final byte[] memento) {
    LOG.log(Level.INFO, "Task started: sleep for: {0} msec.", this.delay);
    final long ts = System.currentTimeMillis();
    for (long period = this.delay; period > 0; period -= System.currentTimeMillis() - ts) {
      try {
        Thread.sleep(period);
      } catch (final InterruptedException ex) {
        LOG.log(Level.FINEST, "Interrupted: {0}", ex);
      }
    }
    return null;
  }
}
