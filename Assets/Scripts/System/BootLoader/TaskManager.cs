using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
enum TaskPhase
{
    None,
    DataInit,
    SceneInit
}
public class TaskManager : SingletonMono<TaskManager>
{

    // include current task progress in overall progress
    public float TotalProgress =>
            tasks.Count == 0 ? 0 : (currentTaskIndex + CurrentTaskProgress) / (float)tasks.Count;

    public readonly List<Func<IEnumerator>> tasks = new();
    public readonly List<Func<IEnumerator>> dataInitTasks = new();
    private int currentTaskIndex = 0;

    public float CurrentTaskProgress { get; private set; }
    public float TaskCount { get => tasks.Count; }

    public void SetCurrentTaskProgress(float v)
    {
        CurrentTaskProgress = Mathf.Clamp01(v);
    }
    public void AddDataInitTask(Func<IEnumerator> task) => dataInitTasks.Add(task);
    internal void AddDataInitTask(List<Func<IEnumerator>> preTasks)
    {
        for (int i = 0; i < preTasks.Count; i++) dataInitTasks.Add(preTasks[i]);
    }
    public void AddTask(Func<IEnumerator> task) => tasks.Add(task);
    internal void AddTask(List<Func<IEnumerator>> preTasks)
    {
        for (int i = 0; i < preTasks.Count; i++) tasks.Add(preTasks[i]);
    }
    public IEnumerator RunTasks(Action callback = null)
    {
        currentTaskIndex = 0;

        Debug.Log("task count " + tasks.Count);
        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            float start = Time.realtimeSinceStartup;
            Debug.Log($"[BOOT] Starting task {i + 1}/{tasks.Count}");
            // reset current-task progress before starting
            SetCurrentTaskProgress(0f);

            // Run task (task coroutine should call SetCurrentTaskProgress as it progresses)
            yield return StartCoroutine(task());

            // ensure task shows completed
            SetCurrentTaskProgress(1f);

            float duration = Time.realtimeSinceStartup - start;
            Debug.Log($"[BOOT] Task {i + 1}/{tasks.Count} finished in {duration:0.00} sec");

            currentTaskIndex++;
        }

        // All tasks done
        Debug.Log($"[BOOT] All tasks finished in {Time.realtimeSinceStartup:0.00} sec");

        // Clear task list
        tasks.Clear();
        // Invoke callback once
        callback?.Invoke();
    }

    internal IEnumerator RunDataInitTask(Action callback)
    {
        Debug.Log("[TaskManager] RunDataInitTask START");

        int count = dataInitTasks.Count;
        if (count == 0)
        {
            callback?.Invoke();
            yield break;
        }

        for (int i = 0; i < count; i++)
        {
            var task = dataInitTasks[i];
            if (task == null) continue;

            Debug.Log($"[DATA] Starting data task {i + 1}/{count}");

            SetCurrentTaskProgress(0f);
            yield return StartCoroutine(task.Invoke());
            SetCurrentTaskProgress(1f);

            currentTaskIndex++;
        }

        dataInitTasks.Clear();

        Debug.Log("[TaskManager] RunDataInitTask DONE");
        callback?.Invoke();
    }


}