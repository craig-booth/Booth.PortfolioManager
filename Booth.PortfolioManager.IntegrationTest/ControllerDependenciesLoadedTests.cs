using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.IntegrationTest.TestFixture;

namespace Booth.PortfolioManager.IntegrationTest
{
    public class ControllerDependenciesLoadedTests : IClassFixture<AppTestFixture>
    {
        private AppTestFixture _Fixture;
        public ControllerDependenciesLoadedTests(AppTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(ConstructorParameters))]
        public void ConstructorDependenciesShouldBeResolved(ControllerMethodParameter parameter)
        {
            using (var scope = _Fixture.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService(parameter.Type);
            }
        }

        [Theory]
        [MemberData(nameof(Parameters))]
        public void ParameterDependenciesShouldBeResolved(ControllerMethodParameter parameter)
        {
            using (var scope = _Fixture.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService(parameter.Type);
            }
        }

        public static IEnumerable<object[]> ConstructorParameters()
        {
            var controllers = TypeUtils.GetSubclassesOf(typeof(ControllerBase), true)
                .Where(x => x.Assembly.GetName().Name.StartsWith("Booth.PortfolioManager.Web"));

            foreach (var controller in controllers)
            {
                foreach (var constructor in controller.GetConstructors())
                {
                    foreach (var parameter in constructor.GetParameters())
                    {
                         yield return new object[] { new ControllerMethodParameter(controller.Name, constructor.Name, parameter) };
                    }
                }
            } 
        }

        public static IEnumerable<object[]> Parameters()
        {
            var controllers = TypeUtils.GetSubclassesOf(typeof(ControllerBase), true)
                .Where(x => x.Assembly.GetName().Name.StartsWith("Booth.PortfolioManager.Web"));

            foreach (var controller in controllers)
            {
                foreach (var method in controller.Methods().ThatAreNotDecoratedWith<HttpPostAttribute>())
                {
                    foreach (var parameter in method.GetParameters())
                    {
                        if (parameter.GetCustomAttribute<FromServicesAttribute>() != null)
                            yield return new object[] { new ControllerMethodParameter(controller.Name, method.Name, parameter) };
                    }
                }
            }
        }

    }

    public class Controller : IXunitSerializable
    {
        public string Name { get { return Type.Name; } }
        public Type Type { get; private set; }

        public Controller()
        {

        }

        public Controller(Type type)
        {
            Type = type;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            var typeName = info.GetValue<string>("Controller");

            Type = System.Type.GetType(typeName);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Controller", Type.AssemblyQualifiedName, typeof(string));
        }

        public override string ToString()
        {
            return Type.Name;
        }
    }
    public class ControllerMethodParameter : IXunitSerializable
    {
        public Type Type { get; private set; }
        public string Name { get; private set; }
        public string Controller { get; private set; }
        public string Method { get; private set; }

        public ControllerMethodParameter()
        {

        }

        public ControllerMethodParameter(string controller, string method, ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            Type = parameterInfo.ParameterType;
            Controller = controller;
            Method = method;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {           
            Controller = info.GetValue<string>("Controller");
            Method = info.GetValue<string>("Method");
            Name = info.GetValue<string>("Parameter");

            var typeName = info.GetValue<string>("ParameterType");
            Type = System.Type.GetType(typeName);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Controller", Controller, typeof(string));
            info.AddValue("Method", Method, typeof(string));
            info.AddValue("Parameter", Name, typeof(string));
            info.AddValue("ParameterType", Type.AssemblyQualifiedName, typeof(string));
        }


        public override string ToString()
        {
            return String.Format("controller {0}, method {1}, parameter {2} of type {3}", Controller, Method, Name, Type.Name);
        }
    }

}
