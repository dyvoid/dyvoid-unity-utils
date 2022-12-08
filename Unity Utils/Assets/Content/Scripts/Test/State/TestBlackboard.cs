using MediaMonks.State;
using MediaMonks.Utility;
using UnityEngine;

namespace Test
{
	public class TestBlackboard : MonoBehaviour, IStateBlackboard
	{
		public ObservableFloat Dus = new();
	}
}