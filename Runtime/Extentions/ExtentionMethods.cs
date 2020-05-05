﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace SwiftFramework.Core
{
    public delegate void FileDownloadHandler(long downloadedBytes, long totalBytes);

    public static class ExtentionMethods
    {
        public static Bounds GetIsometricBounds(this Camera cam)
        {
            return new Bounds()
            {
                center = UnityEngine.Vector2.zero,
                extents = new UnityEngine.Vector3(cam.aspect * cam.orthographicSize, cam.orthographicSize, 0)
            };
        }

        public static bool IsValid(this float num)
        {
            if (float.IsNaN(num) || float.IsInfinity(num))
            {
                return false;
            }
            return true;
        }

        public static string RemoveExtention(this string path)
        {
            string ext = System.IO.Path.GetExtension(path);

            if (string.IsNullOrEmpty(ext) == false)
            {
                return path.Remove(path.Length - ext.Length, ext.Length);
            }

            return path;
        }

        public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle = false)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
                Visit(item, visited, sorted, dependencies, throwOnCycle);

            return sorted;
        }

        private static void Visit<T>(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle)
        {
            if (!visited.Contains(item))
            {
                visited.Add(item);

                foreach (var dep in dependencies(item))
                {
                    if (dep == null)
                    {
                        continue;
                    }
                    Visit(dep, visited, sorted, dependencies, throwOnCycle);
                }

                sorted.Add(item);
            }
            else
            {
                if (throwOnCycle && !sorted.Contains(item))
                    throw new Exception("Cyclic dependency found");
            }
        }

        public static List<T> DeepCopy<T>(this List<T> list) where T : class, IDeepCopy<T>
        {
            List<T> copy = new List<T>(list.Count);
            foreach (var item in list)
            {
                copy.Add(item.DeepCopy());
            }
            return copy;
        }

        public static T Random<T>(this IEnumerable<T> collection)
        {
            int size = collection.CountFast();
            int rand = random.Next(0, size);
            int i = 0;
            foreach (var item in collection)
            {
                if (i == rand)
                {
                    return item;
                }
                i++;
            }
            return collection.FirstOrDefaultFast();
        }

        public static int CountFast<T>(this IEnumerable<T> collection)
        {
            int i = 0;
            foreach (var item in collection)
            {
                i++;
            }
            return i;
        }

        public static T LastFast<T>(this IEnumerable<T> collection)
        {
            var count = CountFast(collection);
            int i = 0;
            foreach (var item in collection)
            {
                i++;
                if (i == count)
                {
                    return item;
                }
            }
            return default;
        }

        public static T FirstOrDefaultFast<T>(this IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                return item;
            }
            return default;
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static float Remap(this int value, float from1, float to1, float from2, float to2)
        {
            return ((float)value).Remap(from1, to1, from2, to2);
        }

        public static void CopyToClipboard(this string str)
        {
            var textEditor = new TextEditor();
            textEditor.text = str;
            textEditor.SelectAll();
            textEditor.Copy();
        }

        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static readonly System.Random random = new System.Random(Environment.TickCount);

        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(this T[] list)
        {
            int n = list.Length;
            while (n > 1)
            {
                n--;
                int k = random.Next(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static float DivideFloat(this BigInteger a, BigInteger b, int precision = 1000)
        {
            if (b == 0)
            {
                return 0;
            }
            var div = BigInteger.Divide(a * precision, b);
            var divInt = (float)div;
            return divInt / precision;
        }

        public static float Sigmoid(this float value)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-value));
        }

        public static BigInteger BigPow(this float coefficient, BigInteger baseValue, int exponent, int precision = 1000000)
        {
            return baseValue * (BigInteger.Pow((BigInteger)(coefficient * precision), exponent) / BigInteger.Pow(precision, exponent));
        }

        public static IEnumerable<T> GetValues<T>(this IEnumerable<LinkTo<T>> links) where T : UnityEngine.Object
        {
            foreach (var l in links)
            {
                yield return l.Value;
            }
        }

        public static IEnumerable<T> GetValues<T>(this IEnumerable<LinkToScriptable<T>> links) where T : class
        {
            foreach (var l in links)
            {
                yield return l.Value;
            }
        }

        public static IEnumerable<T> GetValues<T>(this IEnumerable<LinkToPrefab<T>> links) where T : class
        {
            foreach (var l in links)
            {
                yield return l.Value;
            }
        }

        public static IPromise GetPromise(this AsyncOperationHandle handle)
        {
            Promise promise = Promise.Create();

            if (handle.IsDone)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    promise.Resolve();
                }
                else
                {
                    promise.Reject(handle.OperationException);
                }
            }
            else
            {
                handle.Completed += o =>
                {
                    if (o.Status == AsyncOperationStatus.Succeeded)
                    {
                        promise.Resolve();
                    }
                    else
                    {
                        promise.Reject(handle.OperationException);
                    }
                };
            }

            return promise;
        }

        public static IPromise<T> GetPromise<T>(this AsyncOperationHandle<T> handle)
        {
            Promise<T> promise = Promise<T>.Create();

            if (handle.IsDone)
            {

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    promise.Resolve(handle.Result);
                }
                else
                {
                    promise.Reject(handle.OperationException);
                }
            }
            else
            {
                handle.Completed += o =>
                {
                    if (o.Status == AsyncOperationStatus.Succeeded)
                    {
                        promise.Resolve(o.Result);
                    }
                    else
                    {
                        promise.Reject(handle.OperationException);
                    }
                };
            }

            return promise;
        }

        public static IPromise GetPromiseWithoutResult<T>(this AsyncOperationHandle<T> handle)
        {
            Promise promise = Promise.Create();

            if (handle.IsDone)
            {

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    promise.Resolve();
                }
                else
                {
                    promise.Reject(handle.OperationException);
                }
            }
            else
            {
                handle.Completed += o =>
                {
                    if (o.Status == AsyncOperationStatus.Succeeded)
                    {
                        promise.Resolve();
                    }
                    else
                    {
                        promise.Reject(handle.OperationException);
                    }
                };
            }

            return promise;
        }

        public static IPromise LogException(this IPromise promise)
        {
            promise.Catch(e => Debug.LogException(e));
            return promise;
        }


        public static IPromise<T> LogException<T>(this IPromise<T> promise)
        {
            promise.Catch(e => Debug.LogException(e));
            return promise;
        }

        public static IPromise LogSuccess(this IPromise promise)
        {
            string origin = Environment.StackTrace.Replace(" in ", " at ");
            promise.Done(() => Debug.Log("Success" + "\n\n\n" + origin));
            return promise;
        }

        public static IPromise<T> LogResult<T>(this IPromise<T> promise)
        {
            string origin = Environment.StackTrace.Replace(" in ", " at ");
            promise.Done(r => Debug.Log(r + "\n\n\n" + origin));
            return promise;
        }

        public static IPromise<T> Instantiate<T>(this IPromise<T> promise) where T : UnityEngine.Object
        {
            Promise<T> result = Promise<T>.Create();
            promise.Then(source =>
            {
                result.Resolve(UnityEngine.Object.Instantiate(source));
            })
            .Catch(e => result.Reject(e));
            return result;
        }

        public static IPromise<T> LogAll<T>(this IPromise<T> promise)
        {
            string origin = Environment.StackTrace.Replace(" in ", " at ");
            promise.Done(r => Debug.Log(r + "\n\n\n" + origin));
            promise.Catch(e => Debug.LogException(e));
            return promise;
        }

        public static IPromise LogAll(this IPromise promise)
        {
            string origin = Environment.StackTrace.Replace(" in ", " at ");
            promise.Done(() => Debug.Log("Success" + "\n\n\n" + origin));
            promise.Catch(e => Debug.LogException(e));
            return promise;
        }

        public static string ToAddress(this Type type)
        {
            return type.FullName.Replace('.', '-');
        }

        public static string ToJson(this object obj)
        {
            return Json.Serialize(obj, Formatting.Indented);
        }

        private static readonly string[] sizes = { "B", "KB", "MB", "GB", "TB" };

        public static string ToFileSize(this byte[] file)
        {
            return ToFileSize(file.LongLength);
        }

        public static string ToFileSize(this int bytes)
        {
            return ((long)bytes).ToFileSize();
        }

        public static string ToFileSize(this long byteCount)
        {
            if (byteCount == 0)
                return "0" + sizes[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + sizes[place];
        }

        public static Type GetChildValueType(this FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                return null;
            }
            if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldInfo.FieldType.GetGenericArguments()[0];
            }
            if (fieldInfo.FieldType.IsArray)
            {
                return fieldInfo.FieldType.GetElementType();
            }
            return fieldInfo.FieldType;
        }
        public static string ToTimerString(this float seconds)
        {
            TimeSpan timeLeft = TimeSpan.FromSeconds(seconds);
            return timeLeft.ToTimerString();
        }

        public static string ToTimerString(this long seconds)
        {
            TimeSpan timeLeft = TimeSpan.FromSeconds(seconds);
            return timeLeft.ToTimerString();
        }

        public static string ToTimerString(this int seconds)
        {
            TimeSpan timeLeft = TimeSpan.FromSeconds(seconds);
            return timeLeft.ToTimerString();
        }

        public static string ToDurationString(this float seconds, ILocalizationManager localization)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToDurationString(localization);
        }

        public static string ToDurationString(this TimeSpan time, ILocalizationManager localization)
        {
            if (time.TotalDays >= 1)
            {
                return localization.GetText("#days", time.Days);
            }
            else if (time.TotalHours >= 1)
            {
                return localization.GetText("#hours", time.Hours);
            }
            else if (time.TotalMinutes >= 1)
            {
                return localization.GetText("#minutes", time.Minutes);
            }
            else
            {
                return localization.GetText("#seconds", time.Seconds);
            }
        }

        public static string ToTimeAgoString(this TimeSpan time, ILocalizationManager localization)
        {
            return $"{time.ToDurationString(localization)} {localization.GetText("#ago")}";
        }

        public static string ToDurationString(this long seconds, ILocalizationManager localization)
        {
            return ((float)seconds).ToDurationString(localization);
        }

        public static string ToMultiplierString(this long multiplier)
        {
            return ((float)multiplier).ToMultiplierString();
        }

        public static string ToMultiplierString(this int multiplier)
        {
            return ((float)multiplier).ToMultiplierString();
        }

        public static string ToMultiplierString(this float multiplier)
        {
            if (multiplier == 0)
            {
                return string.Empty;
            }
            return multiplier % 1 != 0 ? $"x {multiplier.ToString("0.0")}" : $"x {multiplier.ToString("0")}";
        }


        public static string ToTimerString(this TimeSpan timeLeft)
        {
            if (timeLeft.TotalDays > 1)
            {
                return $"{timeLeft.Days.ToString("00") + "d"}{timeLeft.Hours.ToString("00") + "h"}";
            }
            else if (timeLeft.TotalHours >= 1)
            {
                return $"{timeLeft.Hours.ToString("00")}:{timeLeft.Minutes.ToString("00")}:{timeLeft.Seconds.ToString("00")}";
            }
            else
            {
                return $"{timeLeft.Minutes.ToString("00")}:{timeLeft.Seconds.ToString("00")}";
            }
        }

        public static void SetSpriteAsync(this Image img, SpriteLink spriteLink)
        {
            if (spriteLink == null || spriteLink.HasValue == false || img == null)
            {
                return;
            }
            if (spriteLink.Loaded)
            {
                img.sprite = spriteLink.Value;
            }
            else
            {
                img.enabled = false;
                spriteLink.Load().Done(s =>
                {
                    img.sprite = s;
                    img.enabled = true;
                });
            }
        }

        public static string GetDescription(this ITimeLimit timeLimit)
        {
            if (timeLimit.TimeTillStart > 0)
            {
                return App.Core.Local.GetText("starts_in", timeLimit.TimeTillStart.ToDurationString(App.Core.Local));
            }
            else if (timeLimit.TimeTillEnd > 0)
            {
                return App.Core.Local.GetText("ends_in", timeLimit.TimeTillEnd.ToDurationString(App.Core.Local));
            }

            return App.Core.Local.GetText("coming_soon");
        }

        public static bool IsComingSoon(this ITimeLimit timeLimit)
        {
            if (timeLimit.TimeTillStart > 0)
            {
                return false;
            }
            else if (timeLimit.TimeTillEnd > 0)
            {
                return false;
            }
            return true;
        }


        public static Coroutine Show(this IAppearAnimationHandler handler, float appearTime, float duration = -1)
        {
            return App.Core.Coroutine.Begin(ShowRoutine(handler, appearTime, duration, false, null));
        }

        public static Coroutine ShowUntilTap(this IAppearAnimationHandler handler, float appearTime, Action onHide = null)
        {
            return App.Core.Coroutine.Begin(ShowRoutine(handler, appearTime, -1, true, onHide));
        }

        private static IEnumerator ShowRoutine(IAppearAnimationHandler handler, float appearTime, float duration = -1, bool hideOnTap = false, Action onHide = null)
        {
            float time = 0f;
            while (time < 1)
            {
                handler.ProcessShowing(time);
                time += Time.unscaledDeltaTime / appearTime;
                yield return null;
            }
            handler.ProcessShowing(1);

            if (hideOnTap)
            {
                while (true)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        handler.Hide(appearTime, onHide);
                        yield break;
                    }
                    yield return null;
                }
            }

            if (duration == -1)
            {
                yield break;
            }

            yield return new WaitForSecondsRealtime(duration);

            handler.Hide(appearTime, onHide);
        }

        public static Coroutine Hide(this IAppearAnimationHandler handler, float disappearTime, Action onHide = null)
        {
            return App.Core.Coroutine.Begin(HideRoutine(handler, disappearTime, onHide));
        }

        private static IEnumerator HideRoutine(IAppearAnimationHandler handler, float duration, Action onHide = null)
        {
            float time = 0f;
            while (time < 1)
            {
                handler.ProcessHiding(time);
                time += Time.unscaledDeltaTime / duration;
                yield return null;
            }
            handler.ProcessHiding(1);
            onHide?.Invoke();
        }
    }
}