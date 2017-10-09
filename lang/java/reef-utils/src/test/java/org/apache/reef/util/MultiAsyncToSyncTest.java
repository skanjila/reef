/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
package org.apache.reef.util;

import org.apache.reef.util.exception.InvalidIdentifierException;
import org.junit.Assert;
import org.junit.Test;

import java.util.concurrent.*;
import java.util.concurrent.atomic.AtomicLong;
import java.util.logging.Level;
import java.util.logging.Logger;

import static java.util.concurrent.TimeUnit.SECONDS;

/**
 * Performs an asynchronous increment of an Integer.
 */
final class AsynchronousIncrementer implements Callable<Integer> {
  private static final Logger LOG = Logger.getLogger(AsynchronousIncrementer.class.getName());
  private final int sleepTimeMillis;
  private final int input;
  private final long identifier;
  private final MultiAsyncToSync blocker;

  /**
   * Instantiate an incrementer with specific job parameters.
   * @param input The input parameter for the work.
   * @param identifier The identifier of the caller to wake on completion.
   * @param sleepTimeMillis How long to work.
   * @param blocker The MultiAsyncToSync object which is holding the blocked client.
   */
  AsynchronousIncrementer(final int input, final long identifier,
        final int sleepTimeMillis, final MultiAsyncToSync blocker) {
    this.sleepTimeMillis = sleepTimeMillis;
    this.input = input;
    this.identifier = identifier;
    this.blocker = blocker;
  }

  /**
   * Sleep and then increment the input value by one.
   * @return The input value of the operation incremented by one.
   * @throws Exception
   */
  public Integer call() throws Exception {
    LOG.log(Level.INFO, "Sleeping...");
    Thread.sleep(sleepTimeMillis);
    LOG.log(Level.INFO, "Releasing caller on identifier [{0}]...", identifier);
    blocker.release(identifier);
    return input + 1;
  }
}

/**
 * Use the MultiAsyncToSync class to implement a synchronous API
 * that uses asynchronous processing internally.
 */
final class SynchronousApi implements AutoCloseable {
  private static final Logger LOG = Logger.getLogger(SynchronousApi.class.getName());
  private final int incrementerSleepTimeMillis;
  private final MultiAsyncToSync blocker;
  private final ExecutorService executor;
  private final ConcurrentLinkedQueue<FutureTask<Integer>> taskQueue = new ConcurrentLinkedQueue<>();
  private final AtomicLong idCounter = new AtomicLong(0);

  /**
   * Parameterize the object as to length of processing time and call timeout.
   * @param incrementerSleepTimeSeconds Length of time the incrementer sleeps before
   *                                    performing the increment and returning.
   * @param timeoutPeriodSeconds The length of time before the call will timeout.
   */
  SynchronousApi(final int incrementerSleepTimeSeconds,
                 final long timeoutPeriodSeconds, final int numberOfThreads) {
    this.incrementerSleepTimeMillis = 1000 * incrementerSleepTimeSeconds;
    this.blocker = new MultiAsyncToSync(timeoutPeriodSeconds, SECONDS);
    this.executor = Executors.newFixedThreadPool(numberOfThreads);
  }

  /**
   * Initiates asynchronous processing inside the condition lock.
   */
  private class AsyncInitiator implements Callable<Boolean> {
    private final FutureTask<Integer> task;
    private final ExecutorService executor;

    AsyncInitiator(final FutureTask<Integer> task, final ExecutorService executor) {
      this.task = task;
      this.executor = executor;
    }

    public Boolean call() {
      executor.execute(task);
      return true;
    }
  }

  /**
   * Asynchronously increment the input parameter.
   * @param input An integer object whose value is to be incremented by one.
   * @return The input parameter incremented by one or zero for a timeout.
   * @throws InterruptedException Thread was interrupted by another thread.
   * @throws ExecutionException An exception was thrown an internal processing function.
   * @throws InvalidIdentifierException The call identifier is invalid.
   * @throws Exception The asynchronous processing generated an exception.
   */
  public int apiCall(final Integer input) throws Exception {
    // Create a future to run the asynchronous processing.
    final long identifier = idCounter.getAndIncrement();
    final FutureTask<Integer> task =
        new FutureTask<>(new AsynchronousIncrementer(input, identifier, incrementerSleepTimeMillis, blocker));
    taskQueue.add(task);

    LOG.log(Level.INFO, "Running the incrementer on identifier [{0}]...", identifier);
    if (blocker.block(identifier, new FutureTask<>(new AsyncInitiator(task, executor)))) {
      LOG.log(Level.INFO, "Call timed out...");
      // Timeout occurred before the asynchronous processing completed.
      return 0;
    }
    LOG.log(Level.INFO, "Call getting task result...");
    return task.get();
  }

  /**
   * Ensure all test tasks have completed.
   * @throws ExecutionException Asynchronous processing generated an exception.
   */
  public void close() throws ExecutionException, InterruptedException {
    for (final FutureTask<Integer> task : taskQueue) {
      try {
        task.get();
      } catch (final Exception e) {
        LOG.log(Level.INFO, "Caught exception waiting for completion...", e);
      }
    }
    executor.shutdownNow();
  }
}

/**
 * Verify proper operation of the MultiAsyncToSync class.
 */
public final class MultiAsyncToSyncTest {
  private static final Logger LOG = Logger.getLogger(MultiAsyncToSyncTest.class.getName());

  /**
   * Verify calculations successfully complete when no timeout occurs.
   */
  @Test
  public void testNoTimeout() throws Exception {
    LOG.log(Level.INFO, "Starting...");

    // Parameters that do not force a timeout.
    final int incrementerSleepTimeSeconds = 2;
    final long timeoutPeriodSeconds = 4;
    final int input = 1;

    try (final SynchronousApi apiObject =
           new SynchronousApi(incrementerSleepTimeSeconds, timeoutPeriodSeconds, 2)) {
      final int result = apiObject.apiCall(input);
      Assert.assertEquals("Value incremented by one", input + 1, result);
    }
  }

  /**
   * Verify an error is returned when a timeout occurs.
   */
  @Test
  public void testTimeout() throws Exception {
    LOG.log(Level.INFO, "Starting...");

    // Parameters that force a timeout.
    final int incrementerSleepTimeSeconds = 4;
    final long timeoutPeriodSeconds = 2;
    final int input = 1;

    try (final SynchronousApi apiObject =
           new SynchronousApi(incrementerSleepTimeSeconds, timeoutPeriodSeconds, 2)) {
      final int result = apiObject.apiCall(input);
      Assert.assertEquals("Timeout occurred", result, 0);
    }
  }

  /**
   * Verify no interaction occurs when multiple calls are in flight.
   */
  @Test
  public void testMultipleCalls() throws Exception {

    LOG.log(Level.INFO, "Starting...");

    // Parameters that do not force a timeout.
    final int incrementerSleepTimeSeconds = 2;
    final long timeoutPeriodSeconds = 4;

    try (final SynchronousApi apiObject =
           new SynchronousApi(incrementerSleepTimeSeconds, timeoutPeriodSeconds, 2)) {
      final String function = "apiCall";
      final int input = 1;
      final FutureTask<Integer> task1 =
          new FutureTask<>(new MethodCallable<Integer>(apiObject, function, input));
      final FutureTask<Integer> task2
          = new FutureTask<>(new MethodCallable<Integer>(apiObject, function, input + 1));

      // Execute API calls concurrently.
      final ExecutorService executor = Executors.newFixedThreadPool(2);
      executor.execute(task1);
      executor.execute(task2);

      final int result1 = task1.get();
      final int result2 = task2.get();

      Assert.assertEquals("Input must be incremented by one", input + 1, result1);
      Assert.assertEquals("Input must be incremented by one", input + 2, result2);

      executor.shutdownNow();
    }
  }

  /**
   * Verify no race conditions occurs when multiple calls are in flight.
   */
  @Test
  public void testRaceConditions() throws Exception {

    LOG.log(Level.INFO, "Starting...");

    // Parameters that do not force a timeout.
    final int incrementerSleepTimeSeconds = 1;
    final long timeoutPeriodSeconds = 10;
    final String function = "apiCall";

    final int nTasks = 100;
    final FutureTask[] tasks = new FutureTask[nTasks];
    final ExecutorService executor = Executors.newFixedThreadPool(10);

    try (final SynchronousApi apiObject =
           new SynchronousApi(incrementerSleepTimeSeconds, timeoutPeriodSeconds, 10)) {

      for (int idx = 0; idx < nTasks; ++idx) {
        tasks[idx] = new FutureTask<>(new MethodCallable<Integer>(apiObject, function, idx));
        executor.execute(tasks[idx]);
      }

      for (int idx = 0; idx < nTasks; ++idx) {
        final int result = (int)tasks[idx].get();
        Assert.assertEquals("Input must be incremented by one", idx + 1, result);
      }
    }
    executor.shutdownNow();
  }

  /**
   * Verify calling block and release on same thread generates an exception.
   */
  @Test
  public void testCallOnSameThread() throws Exception {
    LOG.log(Level.INFO, "Starting...");

    final long timeoutPeriodSeconds = 2;
    final long identifier = 78;
    boolean result = false;

    try {
      final MultiAsyncToSync asyncToSync = new MultiAsyncToSync(timeoutPeriodSeconds, TimeUnit.SECONDS);
      FutureTask<Object> syncProc = new FutureTask<>(new Callable<Object>() {
        public Object call() throws InterruptedException, InvalidIdentifierException {
          asyncToSync.release(identifier);
          return null;
        }
      });
      asyncToSync.block(identifier, syncProc);
      syncProc.get();
    } catch (ExecutionException ee) {
      if (ee.getCause() instanceof RuntimeException) {
        LOG.log(Level.INFO, "Caught expected runtime exception...", ee);
        result = true;
      }
    }
    Assert.assertTrue("Expected runtime exception", result);
  }
}

