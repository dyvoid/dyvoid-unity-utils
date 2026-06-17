using NUnit.Framework;
using dyvoid.Utility;

namespace dyvoid.Utility.Tests
{
	[TestFixture]
	public class ObservableValueTests
	{
		[Test]
		public void Constructor_SetsInitialValue()
		{
			var obs = new ObservableValue<int>(42);
			Assert.AreEqual(42, obs.Value);
		}

		[Test]
		public void SetValue_DifferentValue_FiresCallback()
		{
			var obs = new ObservableValue<int>(0);
			int callCount = 0;
			obs.Subscribe((newVal, oldVal) => callCount++);
			obs.Value = 1;
			Assert.AreEqual(1, callCount);
		}

		[Test]
		public void SetValue_SameValue_DoesNotFireCallback()
		{
			var obs = new ObservableValue<int>(5);
			int callCount = 0;
			obs.Subscribe((newVal, oldVal) => callCount++);
			obs.Value = 5;
			Assert.AreEqual(0, callCount);
		}

		[Test]
		public void Callback_ReceivesCorrectNewAndOldValues()
		{
			var obs = new ObservableValue<int>(10);
			int receivedNew = -1, receivedOld = -1;
			obs.Subscribe((newVal, oldVal) => { receivedNew = newVal; receivedOld = oldVal; });
			obs.Value = 20;
			Assert.AreEqual(20, receivedNew);
			Assert.AreEqual(10, receivedOld);
		}

		[Test]
		public void Subscribe_WithGetInitialValue_FiresImmediately()
		{
			var obs = new ObservableValue<int>(7);
			int callCount = 0;
			obs.Subscribe((n, o) => callCount++, getInitialValue: true);
			Assert.AreEqual(1, callCount);
		}

		[Test]
		public void Subscribe_Duplicate_DoesNotDoublefire()
		{
			var obs = new ObservableValue<int>(0);
			int callCount = 0;
			void Callback(int n, int o) => callCount++;
			obs.Subscribe(Callback);
			obs.Subscribe(Callback);
			obs.Value = 1;
			Assert.AreEqual(1, callCount);
		}

		[Test]
		public void Unsubscribe_StopsNotifications()
		{
			var obs = new ObservableValue<int>(0);
			int callCount = 0;
			void Callback(int n, int o) => callCount++;
			obs.Subscribe(Callback);
			obs.Unsubscribe(Callback);
			obs.Value = 1;
			Assert.AreEqual(0, callCount);
		}

		[Test]
		public void Dispose_ClearsAllSubscribers()
		{
			var obs = new ObservableValue<int>(0);
			int callCount = 0;
			obs.Subscribe((n, o) => callCount++);
			obs.Dispose();
			obs.Value = 1;
			Assert.AreEqual(0, callCount);
		}

		[Test]
		public void Map_DerivedObservable_UpdatesWhenSourceChanges()
		{
			var source = new ObservableValue<int>(2);
			var mapped = source.Map((newVal, _) => newVal * 10);
			source.Value = 5;
			Assert.AreEqual(50, mapped.Value);
		}

		[Test]
		public void Map_InitialValue_IsComputedFromSource()
		{
			var source = new ObservableValue<int>(3);
			var mapped = source.Map((newVal, _) => newVal * 2);
			Assert.AreEqual(6, mapped.Value);
		}

		[Test]
		public void Map_Dispose_UnsubscribesFromSource()
		{
			var source = new ObservableValue<int>(1);
			var mapped = source.Map((n, _) => n * 2);
			mapped.Dispose();
			source.Value = 5;
			Assert.AreEqual(2, mapped.Value);
		}

		[Test]
		public void Merge_Static_UpdatesWhenEitherSourceChanges()
		{
			var a = new ObservableValue<int>(1);
			var b = new ObservableValue<int>(2);
			var merged = ObservableValue<int>.Merge(a, b, (x, y) => x + y);
			Assert.AreEqual(3, merged.Value);
			a.Value = 10;
			Assert.AreEqual(12, merged.Value);
			b.Value = 20;
			Assert.AreEqual(30, merged.Value);
		}

		[Test]
		public void MergeWith_Instance_UpdatesWhenEitherSourceChanges()
		{
			var a = new ObservableValue<int>(3);
			var b = new ObservableValue<int>(4);
			var merged = a.MergeWith(b, (x, y) => x * y);
			Assert.AreEqual(12, merged.Value);
			a.Value = 5;
			Assert.AreEqual(20, merged.Value);
		}

		[Test]
		public void Merge_Dispose_UnsubscribesFromBothSources()
		{
			var a = new ObservableValue<int>(1);
			var b = new ObservableValue<int>(2);
			var merged = ObservableValue<int>.Merge(a, b, (x, y) => x + y);
			int initialValue = merged.Value;
			merged.Dispose();
			a.Value = 100;
			b.Value = 200;
			Assert.AreEqual(initialValue, merged.Value);
		}

		[Test]
		public void Subscribe_Returns_SameInstance_ForChaining()
		{
			var obs = new ObservableValue<int>(0);
			var returned = obs.Subscribe((n, o) => { });
			Assert.AreSame(obs, returned);
		}

		[Test]
		public void Unsubscribe_Returns_SameInstance_ForChaining()
		{
			var obs = new ObservableValue<int>(0);
			void Cb(int n, int o) { }
			obs.Subscribe(Cb);
			var returned = obs.Unsubscribe(Cb);
			Assert.AreSame(obs, returned);
		}
	}
}
