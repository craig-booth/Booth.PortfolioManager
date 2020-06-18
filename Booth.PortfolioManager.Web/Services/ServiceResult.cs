using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.Services
{
    enum ServiceStatus { Ok, NotFound, Error }

    class ServiceResult
    {
        public ServiceStatus Status { get; set; }

        private List<string> _Errors = new List<string>();
        public IEnumerable<string> Errors => _Errors;

        public static ServiceResult Ok()
        {
            return new ServiceResult() { Status = ServiceStatus.Ok };
        }

        public static ServiceResult NotFound()
        {
            return new ServiceResult() { Status = ServiceStatus.NotFound };
        }


        public static ServiceResult Error(string error)
        {
            var result = new ServiceResult() { Status = ServiceStatus.Error };
            result._Errors.Add(error);

            return result;
        }
        public static ServiceResult Error(string error, object arg0)
        {
            var msg = String.Format(error, arg0);

            return ServiceResult.Error(msg);
        }

        public static ServiceResult Error(string error, object arg0, object arg1)
        {
            var msg = String.Format(error, arg0, arg1);

            return ServiceResult.Error(msg);
        }

        public static ServiceResult Error(string error, object arg0, object arg1, object arg2)
        {
            var msg = String.Format(error, arg0, arg1, arg2);

            return ServiceResult.Error(msg);
        }
    }
}
