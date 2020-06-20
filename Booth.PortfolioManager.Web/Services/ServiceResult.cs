using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.Services
{
    enum ServiceStatus { Ok, NotFound, Error }

    class ServiceResult
    {
        public ServiceStatus Status { get; }

        private List<string> _Errors = new List<string>();
        public IEnumerable<string> Errors => _Errors;

        public ServiceResult(ServiceStatus status)
        {
            Status = status;
        }

        public static ServiceResult Ok()
        {
            return new ServiceResult(ServiceStatus.Ok);
        }

        public static ServiceResult NotFound()
        {
            return new ServiceResult(ServiceStatus.NotFound);
        }

        public static ServiceResult Error(string error)
        {
            var result = new ServiceResult(ServiceStatus.Error);
            result.AddErrorMessage(error);

            return result;
        }
        public static ServiceResult Error(string error, object arg0)
        {
            var result = new ServiceResult(ServiceStatus.Error);
            result.AddErrorMessage(error, arg0);

            return result;
        }

        public static ServiceResult Error(string error, object arg0, object arg1)
        {
            var result = new ServiceResult(ServiceStatus.Error);
            result.AddErrorMessage(error, arg0, arg1);

            return result;
        }

        public static ServiceResult Error(string error, object arg0, object arg1, object arg2)
        {
            var result = new ServiceResult(ServiceStatus.Error);
            result.AddErrorMessage(error, arg0, arg1, arg2);

            return result;
        }

        public void AddErrorMessage(string error)
        {
            _Errors.Add(error);
        }

        public void AddErrorMessage(string error, object arg0)
        {
            var msg = String.Format(error, arg0);

            _Errors.Add(msg);
        }

        public void AddErrorMessage(string error, object arg0, object arg1)
        {
            var msg = String.Format(error, arg0, arg1);

            _Errors.Add(msg);
        }

        public void AddErrorMessage(string error, object arg0, object arg1, object arg2)
        {
            var msg = String.Format(error, arg0, arg1, arg2);

            _Errors.Add(msg);
        }
    }

    class ServiceResult<T> : ServiceResult
    {
        public T Result { get; }

        protected ServiceResult(ServiceStatus status, T result) : base(status) 
        {
            Result = result;
        }
        protected ServiceResult(ServiceStatus status) : this(status, default(T)) { }

        public static ServiceResult<T> Ok(T result)
        {
            return new ServiceResult<T>(ServiceStatus.Ok, result);
        }

        public new static ServiceResult<T> NotFound()  
        {
            return new ServiceResult<T>(ServiceStatus.NotFound);
        }

        public new static ServiceResult<T> Error(string error)
        {
            var result = new ServiceResult<T>(ServiceStatus.Error);
            result.AddErrorMessage(error);

            return result;
        }
        public new static ServiceResult Error(string error, object arg0)
        {
            var result = new ServiceResult<T>(ServiceStatus.Error);
            result.AddErrorMessage(error, arg0);

            return result;
        }

        public new static ServiceResult Error(string error, object arg0, object arg1)
        {
            var result = new ServiceResult<T>(ServiceStatus.Error);
            result.AddErrorMessage(error, arg0, arg1);

            return result;
        }

        public new static ServiceResult Error(string error, object arg0, object arg1, object arg2)
        {
            var result = new ServiceResult<T>(ServiceStatus.Error);
            result.AddErrorMessage(error, arg0, arg1, arg2);

            return result;
        }
    }

}
