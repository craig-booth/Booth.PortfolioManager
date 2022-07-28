using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

using Booth.PortfolioManager.Web.Services;
using System.Linq;

namespace Booth.PortfolioManager.Web.Test
{
    static class ServiceResultExtensions
    {
        public static ServiceResultAssertions Should(this ServiceResult result)
        {
            return new ServiceResultAssertions(result);
        }
    }

    class ServiceResultAssertions : ReferenceTypeAssertions<ServiceResult, ServiceResultAssertions>
    {
        public ServiceResultAssertions(ServiceResult result) 
             : base(result)
        {
        }

        protected override string Identifier => "service status";

        public AndConstraint<ServiceResultAssertions> HaveOkStatus(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x.Status == ServiceStatus.Ok)
                .FailWith("Expected {context:service result} to be OK{reason}, but found {0}.", Subject.Status);

            return new AndConstraint<ServiceResultAssertions>(this);
        }

        public AndConstraint<ServiceResultAssertions> HaveNotFoundStatus(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x.Status == ServiceStatus.NotFound)
                .FailWith("Expected {context:service result} to be NotFound{reason}, but found {0}.", Subject.Status);

            return new AndConstraint<ServiceResultAssertions>(this);
        }

        public ServiceErrorAssertions HaveErrorStatus(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x.Status == ServiceStatus.Error)
                .FailWith("Expected {context:service result} to be Error{reason}, but found {0}.", Subject.Status);

            return new ServiceErrorAssertions(Subject);
        }
    }

    class ServiceErrorAssertions : ReferenceTypeAssertions<ServiceResult, ServiceErrorAssertions>
    {
        public ServiceErrorAssertions(ServiceResult result)
             : base(result)
        {

        }

        protected override string Identifier => "service errors";

        public AndConstraint<ServiceErrorAssertions> WithError(string error, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!string.IsNullOrEmpty(error))
                .FailWith("Error message cannot be null or empty")
                .Then
                .Given(() => Subject.Errors)
                .ForCondition(x => x.Contains(error))
                .FailWith("Expected {context:service errors} {0} to contain {1}{reason}.", Subject.Errors, error);

            return new AndConstraint<ServiceErrorAssertions>(this);
        }

    }
}
