using System;
using NUnit.Framework;
using dyvoid.Utility;

namespace dyvoid.Utility.Tests
{
	[TestFixture]
	public class ServiceLocatorTests
	{
		[TearDown]
		public void TearDown()
		{
			ServiceLocator.Clear();
		}

		// ── Register / Get ─────────────────────────────────────────────────────

		[Test]
		public void Register_Get_RoundTrip()
		{
			var service = new ConcreteService();
			ServiceLocator.Register(service);
			Assert.AreSame(service, ServiceLocator.Get<ConcreteService>());
		}

		[Test]
		public void Get_Throws_WhenNotRegistered()
		{
			Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<ConcreteService>());
		}

		[Test]
		public void TryGet_ReturnsFalse_WhenNotRegistered()
		{
			bool found = ServiceLocator.TryGet<ConcreteService>(out _);
			Assert.IsFalse(found);
		}

		[Test]
		public void TryGet_ReturnsTrue_WhenRegistered()
		{
			ServiceLocator.Register(new ConcreteService());
			Assert.IsTrue(ServiceLocator.TryGet<ConcreteService>(out _));
		}

		[Test]
		public void TryGet_OutParam_MatchesRegistered()
		{
			var service = new ConcreteService();
			ServiceLocator.Register(service);
			ServiceLocator.TryGet<ConcreteService>(out var result);
			Assert.AreSame(service, result);
		}

		// ── Has ────────────────────────────────────────────────────────────────

		[Test]
		public void Has_ReturnsFalse_WhenNotRegistered()
		{
			Assert.IsFalse(ServiceLocator.Has<ConcreteService>());
		}

		[Test]
		public void Has_ReturnsTrue_WhenRegistered()
		{
			ServiceLocator.Register(new ConcreteService());
			Assert.IsTrue(ServiceLocator.Has<ConcreteService>());
		}

		// ── Named registrations ────────────────────────────────────────────────

		[Test]
		public void NamedRegistration_IsIsolatedFromUnnamed()
		{
			var named = new ConcreteService();
			var unnamed = new ConcreteService();
			ServiceLocator.Register(named, "myService");
			ServiceLocator.Register(unnamed);
			Assert.AreSame(named, ServiceLocator.Get<ConcreteService>("myService"));
			Assert.AreSame(unnamed, ServiceLocator.Get<ConcreteService>());
		}

		[Test]
		public void NamedRegistration_Get_Throws_WhenWrongName()
		{
			ServiceLocator.Register(new ConcreteService(), "a");
			Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<ConcreteService>("b"));
		}

		// ── Interface registration ─────────────────────────────────────────────

		[Test]
		public void Register_InterfaceImpl_ResolvesAsInterface()
		{
			var impl = new ConcreteService();
			ServiceLocator.Register<IService, ConcreteService>(impl);
			Assert.AreSame(impl, ServiceLocator.Get<IService>());
		}

		// ── Unregister / stacking ──────────────────────────────────────────────

		[Test]
		public void Unregister_RemovesInstance()
		{
			var service = new ConcreteService();
			ServiceLocator.Register(service);
			ServiceLocator.Unregister(service);
			Assert.IsFalse(ServiceLocator.Has<ConcreteService>());
		}

		[Test]
		public void Stacking_SecondRegistration_BecomesActive()
		{
			var first = new ConcreteService();
			var second = new ConcreteService();
			ServiceLocator.Register(first);
			ServiceLocator.Register(second);
			Assert.AreSame(second, ServiceLocator.Get<ConcreteService>());
		}

		[Test]
		public void Stacking_Unregister_Top_RevealsPrevious()
		{
			var first = new ConcreteService();
			var second = new ConcreteService();
			ServiceLocator.Register(first);
			ServiceLocator.Register(second);
			ServiceLocator.Unregister(second);
			Assert.AreSame(first, ServiceLocator.Get<ConcreteService>());
		}

		[Test]
		public void Register_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
				ServiceLocator.Register<ConcreteService>(null));
		}

		// ── Clear ──────────────────────────────────────────────────────────────

		[Test]
		public void Clear_RemovesAllServices()
		{
			ServiceLocator.Register(new ConcreteService());
			ServiceLocator.Clear();
			Assert.IsFalse(ServiceLocator.Has<ConcreteService>());
		}

		[Test]
		public void Clear_CallsDispose_OnDisposableService()
		{
			var disposable = new DisposableService();
			ServiceLocator.Register(disposable);
			ServiceLocator.Clear();
			Assert.IsTrue(disposable.Disposed);
		}

		[Test]
		public void Clear_DisposesEachInstanceOnce_EvenIfRegisteredUnderMultipleKeys()
		{
			var disposable = new DisposableService();
			ServiceLocator.Register<IService, DisposableService>(disposable);
			ServiceLocator.Register(disposable);
			ServiceLocator.Clear();
			Assert.AreEqual(1, disposable.DisposeCount);
		}

		// ── Helpers ────────────────────────────────────────────────────────────

		private interface IService { }

		private class ConcreteService : IService { }

		private class DisposableService : IService, IDisposable
		{
			public bool Disposed => DisposeCount > 0;
			public int DisposeCount { get; private set; }
			public void Dispose() => DisposeCount++;
		}
	}
}
