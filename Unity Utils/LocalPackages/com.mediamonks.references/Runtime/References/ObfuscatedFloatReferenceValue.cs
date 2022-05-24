using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MediaMonks.References
{
	/// <summary>
	/// Stores a float obfuscated in memory.
	/// </summary>
	[CreateAssetMenu(menuName = "References/Obfuscated Float value")]
	public class ObfuscatedFloatReferenceValue : FloatReferenceValue
	{
		protected override void Initialize()
		{
			base.Initialize();

			// Internally obfuscate the value by XOR-ing it with a random value
			var obf = 0;
			// Actually store a scrambled version of the value here i.o. in FloatReferenceValue
			var memValue = 0;

			InputFilter = value =>
			{
				memValue = FloatPacker.ToInt(value);
				// Set new obfuscation XOR every time the score is set
				obf = Random.Range(int.MinValue, int.MaxValue);
				memValue ^= obf;
				// Return 0 as we're not actually using the value stored in FloatReferenceValue
				return 0;
			};
			OutputFilter = value => FloatPacker.ToFloat(memValue ^ obf);
		}

		/// <summary>
		/// Maps a float and int value to the same memory position.
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatPacker
		{
			[FieldOffset(0)] private float _floatValue;
			[FieldOffset(0)] private int _intValue;

			public static float ToFloat(int intValue) => new FloatPacker { _intValue = intValue }._floatValue;

			public static int ToInt(float floatValue) => new FloatPacker { _floatValue = floatValue }._intValue;
		}
	}
}