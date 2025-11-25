using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : SingletonMono<TaskManager>
{

    public float TotalProgress =>
            tasks.Count == 0 ? 0 : currentTaskIndex / (float)tasks.Count;

    public readonly List<Func<IEnumerator>> tasks = new();
    private int currentTaskIndex = 0;

    public float CurrentTaskProgress { get; private set; }
    public float TaskCount { get => tasks.Count; }

    public void SetCurrentTaskProgress(float v)
    {
        CurrentTaskProgress = Mathf.Clamp01(v);
    }
    public void AddTask(Func<IEnumerator> task) => tasks.Add(task);
    internal void AddTask(List<Func<IEnumerator>> preTasks)
    {
        for (int i = 0; i < preTasks.Count; i++) tasks.Add(preTasks[i]);
    }
    public IEnumerator RunTasks(Action callback = null)
    {
        currentTaskIndex = 0;

        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            float start = Time.realtimeSinceStartup;

            // Run task
            yield return StartCoroutine(task());

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

    
}
