using UnityEngine;

namespace State
{
	/// <summary>
	/// A shared data container for state behaviors that need access to common scene elements or data.
	/// Must be attached to the same GameObject as the FSM component.
	/// </summary>
	/// <remarks>
	/// Inherit from this class to create a strongly-typed blackboard for your FSM. This allows you to:
	/// - Centralize references to scene objects and components
	/// - Share data between different state behaviors
	/// 
	/// Example:
	/// <code>
	/// public class PlayerBlackboard : FSMBlackboard {
	///     // Shared component references, exposed in editor
	///     public Rigidbody Rigidbody;
	///     public Animator Animator;
	///     
	///     // Shared values between behaviors, not exposed in editor
	///     public bool IsGrounded { get; set; }
	///     public int ComboCount { get; set; }
	/// }
	/// 
	/// // Option 1: Using the base FSMStateBehaviour with casting
	/// public class JumpState : FSMStateBehaviour<PlayerState> {
	///     public override void OnUpdateState() {
	///         var blackboard = stateMachine.Blackboard as PlayerBlackboard;
	///         blackboard.Animator.SetFloat("JumpVelocity", blackboard.Rigidbody.velocity.y);
	///         blackboard.IsGrounded = blackboard.Rigidbody.position.y < 0.1f;
	///     }
	/// }
	/// 
	/// // Option 2: Using the generic FSMStateBehaviour with type safe Blackboard as second 
	/// // generic parameter
	/// public class AttackState : FSMStateBehaviour<PlayerState, PlayerBlackboard> {
	///     public override void OnUpdateState() {
	///         blackboard.Animator.SetFloat("JumpVelocity", blackboard.Rigidbody.velocity.y);
	///         blackboard.IsGrounded = blackboard.Rigidbody.position.y < 0.1f;
	///     }
	/// }
	/// </code>
	/// </remarks>
	public class FSMBlackboard : MonoBehaviour
	{
		// Inherit from this class to add your blackboard fields
	}
}
