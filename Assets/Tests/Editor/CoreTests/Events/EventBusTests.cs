using System;
using NUnit.Framework;
using Core.Events;

namespace Core.Events.Tests
{
    [TestFixture]
    public class EventBusTests
    {
        private EventBus _eventBus;
        
        // Test event classes
        public class TestEvent
        {
            public string Message { get; set; }
            public int Value { get; set; }
        }
        
        public class AnotherTestEvent
        {
            public bool Flag { get; set; }
        }

        [SetUp]
        public void Setup()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void Subscribe_WithValidHandler_AddsHandlerToEventBus()
        {
            // Arrange
            bool handlerCalled = false;
            Action<TestEvent> handler = (e) => handlerCalled = true;

            // Act
            _eventBus.Subscribe(handler);
            _eventBus.Publish(new TestEvent { Message = "Test" });

            // Assert
            Assert.IsTrue(handlerCalled);
        }

        [Test]
        public void Subscribe_WithNullHandler_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _eventBus.Subscribe<TestEvent>(null));
        }

        [Test]
        public void Publish_WithSubscribedHandler_InvokesHandler()
        {
            // Arrange
            TestEvent receivedEvent = null;
            Action<TestEvent> handler = (e) => receivedEvent = e;
            var testEvent = new TestEvent { Message = "Hello", Value = 42 };

            _eventBus.Subscribe(handler);

            // Act
            _eventBus.Publish(testEvent);

            // Assert
            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual("Hello", receivedEvent.Message);
            Assert.AreEqual(42, receivedEvent.Value);
        }

        [Test]
        public void Publish_WithNullEvent_DoesNotInvokeHandler()
        {
            // Arrange
            bool handlerCalled = false;
            Action<TestEvent> handler = (e) => handlerCalled = true;
            _eventBus.Subscribe(handler);

            // Act
            _eventBus.Publish<TestEvent>(null);

            // Assert
            Assert.IsFalse(handlerCalled);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            // Arrange
            var testEvent = new TestEvent { Message = "Test" };

            // Act & Assert
            Assert.DoesNotThrow(() => _eventBus.Publish(testEvent));
        }

        [Test]
        public void Publish_WithMultipleHandlers_InvokesAllHandlers()
        {
            // Arrange
            int handlerCallCount = 0;
            Action<TestEvent> handler1 = (e) => handlerCallCount++;
            Action<TestEvent> handler2 = (e) => handlerCallCount++;
            Action<TestEvent> handler3 = (e) => handlerCallCount++;

            _eventBus.Subscribe(handler1);
            _eventBus.Subscribe(handler2);
            _eventBus.Subscribe(handler3);

            // Act
            _eventBus.Publish(new TestEvent { Message = "Test" });

            // Assert
            Assert.AreEqual(3, handlerCallCount);
        }

        [Test]
        public void Unsubscribe_WithValidHandler_RemovesHandler()
        {
            // Arrange
            bool handlerCalled = false;
            Action<TestEvent> handler = (e) => handlerCalled = true;

            _eventBus.Subscribe(handler);
            
            // Act
            _eventBus.Unsubscribe(handler);
            _eventBus.Publish(new TestEvent { Message = "Test" });

            // Assert
            Assert.IsFalse(handlerCalled);
        }

        [Test]
        public void Unsubscribe_WithNullHandler_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _eventBus.Unsubscribe<TestEvent>(null));
        }

        [Test]
        public void Unsubscribe_WithNonExistentHandler_DoesNotThrow()
        {
            // Arrange
            Action<TestEvent> handler = (e) => { };

            // Act & Assert
            Assert.DoesNotThrow(() => _eventBus.Unsubscribe(handler));
        }

        [Test]
        public void EventBus_WithDifferentEventTypes_HandlesIndependently()
        {
            // Arrange
            bool testEventHandlerCalled = false;
            bool anotherEventHandlerCalled = false;

            Action<TestEvent> testHandler = (e) => testEventHandlerCalled = true;
            Action<AnotherTestEvent> anotherHandler = (e) => anotherEventHandlerCalled = true;

            _eventBus.Subscribe(testHandler);
            _eventBus.Subscribe(anotherHandler);

            // Act
            _eventBus.Publish(new TestEvent { Message = "Test" });

            // Assert
            Assert.IsTrue(testEventHandlerCalled);
            Assert.IsFalse(anotherEventHandlerCalled);
        }

        [Test]
        public void Unsubscribe_WithOneOfMultipleHandlers_OnlyRemovesSpecificHandler()
        {
            // Arrange
            int handler1CallCount = 0;
            int handler2CallCount = 0;

            Action<TestEvent> handler1 = (e) => handler1CallCount++;
            Action<TestEvent> handler2 = (e) => handler2CallCount++;

            _eventBus.Subscribe(handler1);
            _eventBus.Subscribe(handler2);

            // Act
            _eventBus.Unsubscribe(handler1);
            _eventBus.Publish(new TestEvent { Message = "Test" });

            // Assert
            Assert.AreEqual(0, handler1CallCount);
            Assert.AreEqual(1, handler2CallCount);
        }

        [Test]
        public void Publish_WithHandlerThatThrowsException_StopWithOtherHandlers()
        {
            // Arrange
            bool handler1Called = false;
            bool handler2Called = false;

            Action<TestEvent> throwingHandler = (e) => throw new Exception("Test exception");
            Action<TestEvent> handler1 = (e) => handler1Called = true;
            Action<TestEvent> handler2 = (e) => handler2Called = true;

            _eventBus.Subscribe(handler1);
            _eventBus.Subscribe(throwingHandler);
            _eventBus.Subscribe(handler2);
            
            Assert.Throws<Exception>(()=>_eventBus.Publish(new TestEvent { Message = "Test" }));
            Assert.IsTrue(handler1Called);
            Assert.IsFalse(handler2Called);
        }
    }
}
