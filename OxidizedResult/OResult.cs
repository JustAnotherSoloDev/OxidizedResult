using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Oxidized.Result
{
    /// <summary>
    /// Oresult is a container which wraps the data and it processing into a single struct
    /// Or in Functional progamming terms it is a MONAD
    /// </summary>
    /// <typeparam name="TData">Type of data to wrap</typeparam>
    public struct OResult<TData>
    {
        public readonly TData Value { get; }
        public readonly bool HasError { get; }
        public readonly Exception? Exception { get; }

        public OResult(TData Data)
        {
            Value = Data;
            HasError = false;
            Exception = null;
        }
        /// <summary>
        /// Creates an OResult from an Exception
        /// </summary>
        /// <param name="ex">Exception that will be used to create the OResult Type</param>
        public OResult(Exception ex)
        {
            Value = default(TData);
            HasError = true;
            Exception = ex;
        }
        /// <summary>
        /// Create an Oresult from the return value of a function or lambda.
        /// </summary>
        /// <param name="fn">Lambda or function to be executed</param>
        public OResult(Func<TData> fn)
        {
            try
            {
                Value = fn();
                HasError = false;
                Exception = null;
            }
            catch (Exception ex)
            {
                Value = default(TData);
                HasError = true;
                Exception = ex;
            }
        }
        /// <summary>
        /// creates an oResult from a Async function or Lambda
        /// </summary>
        /// <param name="fn">Async func or lambda to be executed</param>
        public OResult(Func<Task<TData>> fn)
        {
            try
            {
                var task = fn();
                task.Wait();
                Value = task.Result;
                HasError = false;
                Exception = null;
            }
            catch (Exception ex)
            {
                Value = default(TData);
                HasError = true;
                Exception = ex.InnerException;
            }
        }
        /// <summary>
        /// Prorcess the Value in OResult
        /// </summary>
        /// <param name="processor">Function to proces the value</param>
        /// <returns></returns>
        public OResult<TData> Process(Func<TData, TData> processor)
        {
            if (HasError)
            {
                return this;
            }
            TData value = Value;
            return new OResult<TData>(() => processor(value));
        }
        /// <summary>
        /// Prorcess the Value in OResult
        /// </summary>
        /// <typeparam name="TResult">Result Type</typeparam>
        /// <param name="processor">Function to proces the value</param>
        /// <returns></returns>
        public OResult<TResult> Process<TResult>(Func<TData, TResult> processor)
        {
            if (HasError)
            {
                return new OResult<TResult>(Exception);
            }
            try
            {
                var value = processor(Value);
                return new OResult<TResult>(value);

            }
            catch (Exception ex)
            {
                return new OResult<TResult>(ex);
            }
        }
        /// <summary>
        /// Match an Oresult type to a function or an exception and return a specific result type
        /// </summary>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="onSuccess">Function that will be executed if there are no errors</param>
        /// <param name="onError">function that will be executed when there are errors</param>
        /// <returns></returns>
        public TResult MatchResult<TResult>(Func<TData, TResult> onSuccess, Func<Exception, TResult> onError)
        {
            if (this.HasError)
            {
                return onError(this.Exception);
            }
            return onSuccess(this.Value);
        }
        /// <summary>
        /// Match an Oresult type to a function or an exception
        /// </summary>
        /// <param name="onSuccess">Function that will be executed if there are no errors</param>
        /// <param name="onError">function that will be executed when there are errors</param>
        /// <returns></returns>
        public void MatchResult(Action<TData> onSuccess, Action<Exception> onError)
        {
            if (this.HasError)
            {
                onError(this.Exception);
                return;
            }
            onSuccess(this.Value);
        }
        /// <summary>
        /// Get the value in Oresult or throw the exception 
        /// </summary>
        /// <returns></returns>
        public TData UnWrap()
        {
            if (this.HasError)
            {
                throw this.Exception;
            }
            return this.Value;
        }
        /// <summary>
        /// Process the value or use the default Value provided as argument
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="fn"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public OResult<TReturn> ProcessOrElse<TReturn>(Func<TData, TReturn> fn, TReturn defaultValue)
        {
            if (this.HasError)
            {
                return new OResult<TReturn>(defaultValue);
            }
            return this.Process(fn);
        }

        /// <summary>
        /// Process the value or use the result from the function provided as an argument
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="fn"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public OResult<TReturn> ProcessOrElse<TReturn>(Func<TData, TReturn> fn, Func<Exception,TReturn> onError)
        {
            if (this.HasError)
            {
                var exception = this.Exception;
                return new OResult<TReturn>(()=>onError(exception));
            }
            return this.Process(fn);
        }
    }




    public static class AsyncExtensions
    {
        #region Function to Map OResult to Async Task
        public static async Task<OResult<U>> ProcessAsync<T, U>(this OResult<T> prevResult, Func<T, Task<U>> processor)
        {
            if (prevResult.HasError)
            {
                return new OResult<U>(prevResult.Exception);
            }
            return await processor(prevResult.Value).ContinueWith((result) =>
            {
                if (result.IsFaulted)
                {
                    return new OResult<U>(result.Exception!.InnerException!);
                }
                return new OResult<U>(result.Result);
            });
        }
        #endregion


        #region Functions to map task to another task
        /// <summary>
        /// Chain previous task with another task and return a new OResult with new type
        /// </summary>
        /// <typeparam name="TData">Previous Result Type</typeparam>
        /// <typeparam name="TResult">New result Type</typeparam>
        /// <param name="prevResult"></param>
        /// <param name="processor">Function to process data</param>
        /// <returns>Oreult of type U</returns>
        public static async Task<OResult<TResult>> ProcessAsync<TData, TResult>(this Task<OResult<TData>> prevResult, Func<TData, Task<TResult>> processor)
        {
            var data = await prevResult;
            if (data.HasError)
            {
                return new OResult<TResult>(data.Exception);
            }
            try
            {
                var result = await processor(data.Value);
                return new OResult<TResult>(result);
            }
            catch (Exception e)
            {
                return new OResult<TResult>(e);
            }
        }
        /// <summary>
        /// Chain previous task with another task of type Oresult type
        /// </summary>
        /// <typeparam name="TData">Type of Input</typeparam>
        /// <typeparam name="TResult">Type of Result</typeparam>
        /// <param name="prevResult"></param>
        /// <param name="processor">Fnction that takes Tdata as input and returns TResult as output</param>
        /// <returns></returns>
        public static async Task<OResult<TResult>> ProcessAsync<TData, TResult>(this Task<OResult<TData>> prevResult, Func<TData, TResult> processor)
        {
            var data = await prevResult;
            if (data.HasError)
            {
                return new OResult<TResult>(data.Exception);
            }
            return new OResult<TResult>(() => processor(data.Value));
        }

        #endregion

        public static async Task<TResult> MatchResultAsync<TData, TResult>(this Task<OResult<TData>> prevResult, Func<TData, TResult> OnSuccess, Func<Exception, TResult> onError)
        {
            await prevResult;
            var result = prevResult.Result;
            if (result.HasError)
            {
                return onError(result.Exception);
            }
            return OnSuccess(result.Value);
        }

        public static async Task MatchResultAsync<TData>(this Task<OResult<TData>> prevResult, Action<TData> OnSuccess, Action<Exception> onError)
        {
            await prevResult;
            var result = prevResult.Result;
            if (result.HasError)
            {
                onError(result.Exception);
                return;
            }
            OnSuccess(result.Value);
        }

        public static async Task<TData> UnWrapAsync<TData>(this Task<OResult<TData>> task)
        {
            await task;
            var result = task.Result;
            if (result.HasError)
            {
                throw result.Exception;
            }
            return result.Value;
        }


        public static async Task<OResult<TReturn>> ProcessAsyncOrElse<TData, TReturn>(
            this Task<OResult<TData>> task,
            Func<TData, Task<TReturn>> fn, TReturn defaultValue)
        {
            var result = await task;
            if (result.HasError)
            {
                return new OResult<TReturn>(defaultValue);
            }
            return await result.ProcessAsync(fn);
        }
        public static async Task<OResult<TReturn>> ProcessAsyncOrElse<TData, TReturn>(
            this Task<OResult<TData>> task,
            Func<TData, Task<TReturn>> fn, Func<Exception?, TReturn> onError)
        {
            var result = await task;
            if (result.HasError)
            {
                try
                {
                    var value = onError(result.Exception);
                    return new OResult<TReturn>(value);
                }
                catch (Exception ex)
                {
                    return new OResult<TReturn>(ex);

                }
            }
            return await result.ProcessAsync(fn);
        }
    }


}
