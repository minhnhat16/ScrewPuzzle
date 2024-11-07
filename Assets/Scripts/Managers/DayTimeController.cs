using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DayTimeController : MonoBehaviour
{
    public static DayTimeController instance;
    public bool isNewDay;
    public string lastCheckedTimeCard;
    public UnityEvent<bool> newDateEvent = new UnityEvent<bool>();
    [SerializeField] private DateTime targetDateTime;

    private void Awake()
    {
        instance = this;
        targetDateTime = DateTime.Now.AddDays(2);
    }
    // Update is called once per frsame
    public  IEnumerator InitCouroutine()
    {
        yield return new WaitUntil(()=> DataAPIController.instance.isInitDone);
        CheckNewDay();
    }
    public void CheckNewDay()
    {
        DateTime now = DateTime.Now;
        DateTime last = DataAPIController.instance.GetTimeClaimItem();

        // Check if 'last' is a valid DateTime
        if (last != DateTime.MinValue)
        {
            // Calculate the time difference
            TimeSpan timeDifference = now - last;

            // Check if the difference is greater than 24 hours
            if (timeDifference.TotalHours > 24)
            {
                //Debug.Log("More than 24 hours have passed since the last claim.");
                isNewDay = true;
                DataAPIController.instance.SetIsClaimTodayData(!isNewDay);
                //DataAPIController.instance.SetDayTimeData(now.ToString());
                NewDay(true);
            }
            else
            {
                //Debug.Log("Less than 24 hours have passed since the last claim.");
                isNewDay = false;
            }
        }
        else
        {
            //Debug.Log("Invalid last claim time.");
        }
    }
    public TimeSpan GetRemainingTime(DateTime lastSpinData)
    {
        DateTime nextAllowedSpinTime = lastSpinData.AddHours(24);
        TimeSpan remainingTime = nextAllowedSpinTime - DateTime.Now;

        if (remainingTime < TimeSpan.Zero)
        {
            // If the remaining time is negative, it means 24 hours have already passed
            remainingTime = TimeSpan.Zero;
        }

        return remainingTime;
    }

    public string GetCountdownString(TimeSpan remainingTime)
    {
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
                             remainingTime.Hours,
                             remainingTime.Minutes,
                             remainingTime.Seconds);
    }

    public void NewDay(bool isNew)
    {
        isNewDay = isNew;
        if(isNew)
        {
            newDateEvent?.Invoke(true);
        }
        
    }
    public void StartCountingTime(string targetDateTimeString, Text countdownText)
    {
        StartCoroutine(CountingTime(targetDateTimeString, countdownText));

    }
    IEnumerator CountingTime(string targetDateTimeString, Text countdownText)
    {
        DateTime targetDateTime;

        // Parse the target date and time
        if (DateTime.TryParse(targetDateTimeString, out targetDateTime))
        {
            while (true)
            {
                // Calculate the time remaining
                TimeSpan countdown = targetDateTime - DateTime.Now;

                if (countdown.TotalSeconds > 0)
                {
                    // Display the countdown in "dd:hh:mm:ss" format
                    countdownText.text = "Time remaining: " + countdown.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    countdownText.text = "The countdown has ended.";
                    yield break; // Exit the coroutine when countdown ends
                }

                // Wait for a second before updating again
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            countdownText.text = "Invalid date and time format.";
        }
    }

    //public void DayTimeCounter()
    //{
    //    // Parse the target date and time
    //    if (DateTime.TryParse(targetDateTimeString, out targetDateTime))
    //    {
    //        // Check if the target time is in the future
    //        if (targetDateTime <= DateTime.Now)
    //        {
    //            countdownText.text = "The target date and time has passed.";
    //        }
    //        else
    //        {
    //            StartCoroutine(CountingTime());
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("Invalid date and time format.");
    //    }
    ////}
}
