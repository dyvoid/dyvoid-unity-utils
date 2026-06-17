using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using dyvoid.Utility;

namespace dyvoid.Utility.Tests
{
	[TestFixture]
	public class FSMTests
	{
		private GameObject _go;

		[SetUp]
		public void SetUp()
		{
			_go = new GameObject("FSMTest");
		}

		[TearDown]
		public void TearDown()
		{
			if (_go != null)
			{
				UnityEngine.Object.DestroyImmediate(_go);
			}
		}

		// ── Helpers ────────────────────────────────────────────────────────────

		private enum TestState { Idle, Walk, Jump }

		private class TestFSM : FSM<TestState> { }

		private class IdleBehaviour : FSMStateBehaviour<TestState>
		{
			public override TestState State => TestState.Idle;
			public int EnterCount;
			public int ExitCount;
			public TestState LastEnterFrom;
			public TestState LastExitTo;
			public override void OnEnter(TestState prevState) { EnterCount++; LastEnterFrom = prevState; }
			public override void OnExit(TestState nextState) { ExitCount++; LastExitTo = nextState; }
		}

		private class WalkBehaviour : FSMStateBehaviour<TestState>
		{
			public override TestState State => TestState.Walk;
			public int EnterCount;
			public override void OnEnter(TestState prevState) => EnterCount++;
		}

		private class DuplicateIdleBehaviour : FSMStateBehaviour<TestState>
		{
			public override TestState State => TestState.Idle;
		}

		private static void CallStart(MonoBehaviour mb)
		{
			try
			{
				mb.GetType()
					.GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					?.Invoke(mb, null);
			}
			catch (System.Reflection.TargetInvocationException e)
			{
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
			}
		}

		// ── Initialization ─────────────────────────────────────────────────────

		[Test]
		public void Start_BuildsBehaviourMap_FromSiblingComponents()
		{
			_go.AddComponent<TestFSM>();
			_go.AddComponent<IdleBehaviour>();
			_go.AddComponent<WalkBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			Assert.IsTrue(fsm.TryGetBehavior(TestState.Idle, out _));
			Assert.IsTrue(fsm.TryGetBehavior(TestState.Walk, out _));
		}

		[Test]
		public void Start_DuplicateBehaviour_ThrowsInvalidOperationException()
		{
			_go.AddComponent<TestFSM>();
			_go.AddComponent<IdleBehaviour>();
			_go.AddComponent<DuplicateIdleBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			Assert.Throws<InvalidOperationException>(() => CallStart(fsm));
		}

		[Test]
		public void Start_SetsInitialState_ViaObservable()
		{
			_go.AddComponent<TestFSM>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			Assert.AreEqual(TestState.Idle, fsm.CurrentState);
		}

		// ── State transitions ──────────────────────────────────────────────────

		[Test]
		public void ChangeState_UpdatesCurrentState()
		{
			_go.AddComponent<TestFSM>();
			_go.AddComponent<IdleBehaviour>();
			_go.AddComponent<WalkBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			fsm.ChangeState(TestState.Walk);
			Assert.AreEqual(TestState.Walk, fsm.CurrentState);
		}

		[Test]
		public void ChangeState_CallsOnExit_OnPreviousBehaviour()
		{
			_go.AddComponent<TestFSM>();
			var idle = _go.AddComponent<IdleBehaviour>();
			_go.AddComponent<WalkBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			fsm.ChangeState(TestState.Walk);
			Assert.AreEqual(1, idle.ExitCount);
			Assert.AreEqual(TestState.Walk, idle.LastExitTo);
		}

		[Test]
		public void ChangeState_CallsOnEnter_OnNewBehaviour()
		{
			_go.AddComponent<TestFSM>();
			_go.AddComponent<IdleBehaviour>();
			var walk = _go.AddComponent<WalkBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			fsm.ChangeState(TestState.Walk);
			Assert.AreEqual(1, walk.EnterCount);
		}

		[Test]
		public void ChangeState_SetsPreviousBehaviour()
		{
			_go.AddComponent<TestFSM>();
			_go.AddComponent<IdleBehaviour>();
			_go.AddComponent<WalkBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			fsm.ChangeState(TestState.Walk);
			Assert.AreEqual(TestState.Idle, fsm.PreviousState);
		}

		[Test]
		public void ChangeState_ToStateWithNoBehaviour_SetsCurrentBehaviourNull()
		{
			_go.AddComponent<TestFSM>();
			_go.AddComponent<IdleBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			fsm.ChangeState(TestState.Jump);
			Assert.IsNull(fsm.CurrentBehaviour);
		}

		[Test]
		public void ChangeState_ToStateWithNoBehaviour_StateObservableStillUpdates()
		{
			_go.AddComponent<TestFSM>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			fsm.ChangeState(TestState.Jump);
			Assert.AreEqual(TestState.Jump, fsm.CurrentState);
		}

		// ── GetBehavior / TryGetBehavior ───────────────────────────────────────

		[Test]
		public void GetBehavior_ReturnsCorrectBehaviour()
		{
			_go.AddComponent<TestFSM>();
			var idle = _go.AddComponent<IdleBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			Assert.AreSame(idle, fsm.GetBehavior(TestState.Idle));
		}

		[Test]
		public void TryGetBehavior_False_ForUnregisteredState()
		{
			_go.AddComponent<TestFSM>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			Assert.IsFalse(fsm.TryGetBehavior(TestState.Jump, out _));
		}

		// ── StateObservable ────────────────────────────────────────────────────

		[Test]
		public void StateObservable_FiresOnChangeState()
		{
			_go.AddComponent<TestFSM>();
			_go.AddComponent<IdleBehaviour>();
			_go.AddComponent<WalkBehaviour>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			int fired = 0;
			fsm.StateObservable.Subscribe((n, o) => fired++);
			fsm.ChangeState(TestState.Walk);
			Assert.AreEqual(1, fired);
		}

		// ── OnDestroy ─────────────────────────────────────────────────────────

		[Test]
		public void OnDestroy_DisposesStateObservable()
		{
			_go.AddComponent<TestFSM>();
			var fsm = _go.GetComponent<TestFSM>();
			CallStart(fsm);
			int fired = 0;
			fsm.StateObservable.Subscribe((n, o) => fired++);
			UnityEngine.Object.DestroyImmediate(_go);
			_go = null;
			Assert.AreEqual(0, fired);
		}
	}
}
