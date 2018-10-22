﻿using Deltatre.Utils.Extensions.Enumerable;
using Deltatre.Utils.Timers;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Deltatre.Utils.Tests.Timers
{
  public partial class TimerAsyncTest
  {
    [Test]
    public async Task Start_Schedules_Execution_Of_Background_Workload()
    {
      // ARRANGE
      var values = new ConcurrentBag<int>();
      Func<CancellationToken, Task> action = _ =>
      {
        values.Add(1);
        return Task.CompletedTask;
      };

      var target = new TimerAsync(
        action,
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500));

      // ACT
      target.Start();
      await Task.Delay(1200).ConfigureAwait(false); // wait for the execution of background workload at least two times

      // ASSERT
      Assert.GreaterOrEqual(2, values.Count);
      Assert.IsTrue(values.All(value => value == 1));
    }

    [Test]
    public void Start_Can_Be_Called_More_Than_Once_Without_Throwing_Exceptions()
    {
      // ARRANGE
      var target = new TimerAsync(
        _ => Task.CompletedTask,
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500));

      // ASSERT
      Assert.DoesNotThrow(() =>
      {
        target.Start();
        target.Start();
      });
    }

    [Test]
    public async Task Start_Can_Be_Called_More_Than_Once_Without_Affecting_The_Execution_Of_Background_Workload()
    {
      // ARRANGE
      var values = new ConcurrentBag<int>();
      Func<CancellationToken, Task> action = _ =>
      {
        values.Add(1);
        return Task.CompletedTask;
      };

      var target = new TimerAsync(
        action,
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500));

      // ACT
      target.Start();
      target.Start();
      await Task.Delay(1200).ConfigureAwait(false);

      // ASSERT
      Assert.GreaterOrEqual(2, values.Count);
      Assert.IsTrue(values.All(value => value == 1));
    }

    [Test]
    public async Task Start_Is_Thread_Safe()
    {
      // ARRANGE
      var values = new ConcurrentBag<int>();
      Func<CancellationToken, Task> action = _ =>
      {
        values.Add(1);
        return Task.CompletedTask;
      };

      var target = new TimerAsync(
        action,
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500));

      bool run = true;

      var thread1 = new Thread(() =>
      {
        while (run)
        {
          target.Start();
        }
      });

      var thread2 = new Thread(() =>
      {
        while (run)
        {
          target.Start();
        }
      });

      var threads = new[] { thread1, thread2 };

      // ACT
      threads.ForEach(t => t.Start());
      await Task.Delay(1200).ConfigureAwait(false);
      run = false;
      threads.ForEach(t => t.Join());

      // ASSERT
      Assert.GreaterOrEqual(2, values.Count);
      Assert.IsTrue(values.All(value => value == 1));
    }






    
  }
}
