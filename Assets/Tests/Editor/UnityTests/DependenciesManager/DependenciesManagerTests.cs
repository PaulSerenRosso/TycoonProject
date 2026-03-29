using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

namespace Tests
{
    [TestFixture]
    public class DependenciesManagerTests
    {
        private DependenciesManager _dependenciesManager;
        private GameObject _gameObject;

        // Test interfaces and classes
        public interface ITestService
        {
            string GetMessage();
        }

        public class TestService : ITestService
        {
            public string GetMessage() => "Test Service";
        }

        public class AnotherTestService : ITestService
        {
            public string GetMessage() => "Another Test Service";
        }

        public class ServiceWithDependency
        {
            public ServiceWithDependency()
            {
                
            }
            public ITestService TestService { get; }

            public ServiceWithDependency(ITestService testService)
            {
                TestService = testService;
            }
        }

        public class ServiceWithMultipleDependencies
        {
            public ITestService TestService { get; }
            public string StringValue { get; }

            public ServiceWithMultipleDependencies()
            {
                
            }
            
            public ServiceWithMultipleDependencies(ITestService testService, string stringValue = "default")
            {
                TestService = testService;
                StringValue = stringValue;
            }
        }

        public class ServiceWithParameterlessConstructor
        {
            public string Value { get; set; } = "Default";
        }

        public class TestMonoBehaviour : MonoBehaviour
        {
            
            [Inject]
            public ITestService InjectedService { get; set; }

            [Inject]
            private ITestService _injectedField;

            [Inject(Optional = true)]
            public string OptionalService { get; set; }

            public ITestService GetInjectedField() => _injectedField;
        }

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("TestDependenciesManager");
            _dependenciesManager = _gameObject.AddComponent<DependenciesManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        #region Singleton Instance Tests

        [Test]
        public void Instance_WhenCalledMultipleTimes_ReturnsSameInstance()
        {
            // Act
            var instance1 = DependenciesManager.Instance;
            var instance2 = DependenciesManager.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void Instance_WhenNoInstanceExists_CreatesNewInstance()
        {
            // Act
            var instance = DependenciesManager.Instance;

            // Assert
            Assert.IsNotNull(instance);
            Assert.IsInstanceOf<DependenciesManager>(instance);
        }

        #endregion

        #region Registration Tests

        [Test]
        public void Register_WithValidService_AllowsResolution()
        {
            // Arrange
            var testService = new TestService();

            // Act
            _dependenciesManager.Register<ITestService>(testService);
            var resolved = _dependenciesManager.Resolve<ITestService>();

            // Assert
            Assert.AreSame(testService, resolved);
        }

        [Test]
        public void RegisterFactory_WithValidFactory_CreatesNewInstanceOnEachResolve()
        {
            // Arrange
            _dependenciesManager.RegisterFactory<ITestService>(() => new TestService());

            // Act
            var resolved1 = _dependenciesManager.Resolve<ITestService>();
            var resolved2 = _dependenciesManager.Resolve<ITestService>();

            // Assert
            Assert.IsNotNull(resolved1);
            Assert.IsNotNull(resolved2);
            Assert.AreNotSame(resolved1, resolved2);
        }

        [Test]
        public void RegisterTransient_WithParameterlessConstructor_AllowsResolution()
        {
            // Act
            _dependenciesManager.RegisterTransient<ServiceWithParameterlessConstructor>();
            var resolved = _dependenciesManager.Resolve<ServiceWithParameterlessConstructor>();

            // Assert
            Assert.IsNotNull(resolved);
            Assert.AreEqual("Default", resolved.Value);
        }

        [Test]
        public void RegisterTransient_WithInterfaceAndImplementation_AllowsResolution()
        {
            // Act
            _dependenciesManager.RegisterTransient<ITestService, TestService>();
            var resolved = _dependenciesManager.Resolve<ITestService>();

            // Assert
            Assert.IsNotNull(resolved);
            Assert.IsInstanceOf<TestService>(resolved);
            Assert.AreEqual("Test Service", resolved.GetMessage());
        }

        #endregion

        #region Resolution Tests

        [Test]
        public void Resolve_WithUnregisteredService_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => 
                _dependenciesManager.Resolve<ITestService>());
        }

        [Test]
        public void TryResolve_WithRegisteredService_ReturnsTrue()
        {
            // Arrange
            _dependenciesManager.Register<ITestService>(new TestService());

            // Act
            bool result = _dependenciesManager.TryResolve<ITestService>(out var service);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(service);
        }

        [Test]
        public void TryResolve_WithUnregisteredService_ReturnsFalse()
        {
            // Act
            bool result = _dependenciesManager.TryResolve<ITestService>(out var service);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(service);
        }

        #endregion

        #region Constructor Injection Tests

        [Test]
        public void CreateWithConstructorInjection_WithDependencies_InjectsDependencies()
        {
            // Arrange
            _dependenciesManager.Register<ITestService>(new TestService());
            _dependenciesManager.RegisterTransient<ServiceWithDependency>();

            // Act
            var resolved = _dependenciesManager.Resolve<ServiceWithDependency>();

            // Assert
            Assert.IsNotNull(resolved);
            Assert.IsNotNull(resolved.TestService);
            Assert.AreEqual("Test Service", resolved.TestService.GetMessage());
        }

        [Test]
        public void CreateWithConstructorInjection_WithDefaultParameters_UsesDefaultValues()
        {
            // Arrange
            _dependenciesManager.Register<ITestService>(new TestService());
            _dependenciesManager.RegisterTransient<ServiceWithMultipleDependencies>();

            // Act
            var resolved = _dependenciesManager.Resolve<ServiceWithMultipleDependencies>();

            // Assert
            Assert.IsNotNull(resolved);
            Assert.IsNotNull(resolved.TestService);
            Assert.AreEqual("default", resolved.StringValue);
        }

        [Test]
        public void CreateWithConstructorInjection_WithParameterlessConstructor_UsesParameterlessConstructor()
        {
            // Arrange
            _dependenciesManager.RegisterTransient<ServiceWithParameterlessConstructor>();

            // Act
            var resolved = _dependenciesManager.Resolve<ServiceWithParameterlessConstructor>();

            // Assert
            Assert.IsNotNull(resolved);
            Assert.AreEqual("Default", resolved.Value);
        }

        [Test]
        public void CreateWithConstructorInjection_WithMissingDependency_ThrowsException()
        {
            // Arrange
            _dependenciesManager.RegisterTransient<ServiceWithDependency>();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _dependenciesManager.Resolve<ServiceWithDependency>());
            
            Assert.That(exception.Message, Contains.Substring("Cannot resolve parameter"));
        }

        #endregion

        #region Property Injection Tests

        [Test]
        public void InjectProperties_WithRegisteredDependency_InjectsProperty()
        {
            // Arrange
            var testService = new TestService();
            _dependenciesManager.Register<ITestService>(testService);
            
            var target = _gameObject.AddComponent<TestMonoBehaviour>();

            // Act
            _dependenciesManager.InjectProperties(target);

            // Assert
            Assert.AreSame(testService, target.InjectedService);
        }

        [Test]
        public void InjectProperties_WithUnregisteredOptionalDependency_DoesNotThrow()
        {
            // Arrange
            var target = _gameObject.AddComponent<TestMonoBehaviour>();

            // Act & Assert
            Assert.DoesNotThrow(() => _dependenciesManager.InjectProperties(target));
            Assert.IsNull(target.OptionalService);
        }

        [Test]
        public void InjectProperties_WithSameTypeMultipleTimes_UsesCache()
        {
            // Arrange
            var testService = new TestService();
            _dependenciesManager.Register<ITestService>(testService);
            
            var target1 = _gameObject.AddComponent<TestMonoBehaviour>();
            var target2 = _gameObject.AddComponent<TestMonoBehaviour>();

            // Act
            _dependenciesManager.InjectProperties(target1);
            _dependenciesManager.InjectProperties(target2);

            // Assert
            Assert.AreSame(testService, target1.InjectedService);
            Assert.AreSame(testService, target2.InjectedService);
        }

        #endregion

        #region Field Injection Tests

        [Test]
        public void InjectFields_WithRegisteredDependency_InjectsField()
        {
            // Arrange
            var testService = new TestService();
            _dependenciesManager.Register<ITestService>(testService);
            
            var target = _gameObject.AddComponent<TestMonoBehaviour>();

            // Act
            _dependenciesManager.InjectFields(target);

            // Assert
            Assert.AreSame(testService, target.GetInjectedField());
        }

        [Test]
        public void InjectFields_WithSameTypeMultipleTimes_UsesCache()
        {
            // Arrange
            var testService = new TestService();
            _dependenciesManager.Register<ITestService>(testService);
            
            var target1 = _gameObject.AddComponent<TestMonoBehaviour>();
            var target2 = _gameObject.AddComponent<TestMonoBehaviour>();

            // Act
            _dependenciesManager.InjectFields(target1);
            _dependenciesManager.InjectFields(target2);

            // Assert
            Assert.AreSame(testService, target1.GetInjectedField());
            Assert.AreSame(testService, target2.GetInjectedField());
        }

        #endregion

        #region Integration Tests

        [Test]
        public void FullWorkflow_RegisterResolveAndInject_WorksCorrectly()
        {
            // Arrange
            var testService = new TestService();
            _dependenciesManager.Register<ITestService>(testService);
            _dependenciesManager.RegisterTransient<ServiceWithDependency>();
            
            var target = _gameObject.AddComponent<TestMonoBehaviour>();

            // Act
            var resolvedService = _dependenciesManager.Resolve<ServiceWithDependency>();
            _dependenciesManager.InjectProperties(target);
            _dependenciesManager.InjectFields(target);

            // Assert
            Assert.IsNotNull(resolvedService);
            Assert.AreSame(testService, resolvedService.TestService);
            Assert.AreSame(testService, target.InjectedService);
            Assert.AreSame(testService, target.GetInjectedField());
        }

        #endregion
    }
}
