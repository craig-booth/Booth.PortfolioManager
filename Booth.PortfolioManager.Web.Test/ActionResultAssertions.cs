using Booth.PortfolioManager.Web.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Booth.PortfolioManager.Web.Test
{
    static class ActionResultExtensions
    {
        public static ActionResultAssertions Should(this ActionResult result)
        {
            return new ActionResultAssertions(result, AssertionChain.GetOrCreate());
        }
    }

    class ActionResultAssertions : ReferenceTypeAssertions<ActionResult, ActionResultAssertions>
    {
        public ActionResultAssertions(ActionResult result, AssertionChain chain) 
             : base(result, chain)
        {
        }

        protected override string Identifier => "action status";

        [CustomAssertion]
        public AndConstraint<ActionResultAssertions> BeForbidResult(string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x is ForbidResult)
                .FailWith("Expected {context:action result} to be Forbiden{reason}, but found {0}.", Subject.GetType());

            return new AndConstraint<ActionResultAssertions>(this);
        }

        [CustomAssertion]
        public AndConstraint<ActionResultAssertions> BeNotFoundResult(string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x is NotFoundResult)
                .FailWith("Expected {context:action result} to be NotFound{reason}, but found {0}.", Subject.GetType());

            return new AndConstraint<ActionResultAssertions>(this);
        }

        [CustomAssertion]
        public AndConstraint<ActionResultAssertions> BeOkResult(string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x is OkResult)
                .FailWith("Expected {context:action result} to be OK{reason}, but found {0}.", Subject.GetType());

            return new AndConstraint<ActionResultAssertions>(this);
        }

        [CustomAssertion]
        public AndWhichConstraint<ActionResultAssertions, ObjectResult> BeOkObjectResult(string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x is OkObjectResult)
                .FailWith("Expected {context:action result} to be OK{reason}, but found {0}.", Subject.GetType());

            return new AndWhichConstraint<ActionResultAssertions, ObjectResult>(this, (Subject as ObjectResult));
        }

        [CustomAssertion]
        public AndConstraint<ActionResultAssertions> BeBadRequestResult(string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x is BadRequestResult)
                .FailWith("Expected {context:action result} to be BadRequest{reason}, but found {0}.", Subject.GetType());

            return new AndConstraint<ActionResultAssertions>(this);
        }

        [CustomAssertion]
        public AndWhichConstraint<ActionResultAssertions, ObjectResult> BeBadRequestObjectResult(string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(x => x is BadRequestObjectResult)
                .FailWith("Expected {context:action result} to be BadRequest{reason}, but found {0}.", Subject.GetType());

            return new AndWhichConstraint<ActionResultAssertions, ObjectResult>(this, (Subject as ObjectResult));
        }


    }

}
