using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class TimerManager : LazySingleton<TimerManager>
{
    private readonly int ONE_SECOND = 1000;

    private Action<float> updateTimerUI = null;

    public Action<float> SetUpdateTimerUI { set { updateTimerUI = value; } }

    private float timeRemaining = 15f;
    private CancellationTokenSource cancellationTokenSource = null;

    public void StartTimer()
    {
        ResetTime();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        StartCountdown(cancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid StartCountdown(CancellationToken _cancellationToken)
    {
        try
        {
            while (timeRemaining > 0)
            {
                updateTimerUI?.Invoke(timeRemaining);

                await UniTask.Delay(ONE_SECOND, cancellationToken: _cancellationToken); // Wait for 1 second or until cancelled

                timeRemaining--;
            }
        }
        catch (OperationCanceledException)
        {
            timeRemaining = 0;
        }
        finally
        {
            updateTimerUI?.Invoke(timeRemaining);
        }
    }

    private void ResetTime()
    {
        timeRemaining = 15f;
        updateTimerUI?.Invoke(timeRemaining);
    }

    public void StopTimer()
    {
        timeRemaining = 0;
        cancellationTokenSource?.Cancel();
    }
}
