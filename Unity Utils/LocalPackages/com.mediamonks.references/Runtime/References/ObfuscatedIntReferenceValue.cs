using UnityEngine;

namespace MediaMonks.References
{
	/// <summary>
	/// Stores an int obfuscated in memory.
	/// </summary>
	[CreateAssetMenu(menuName = "References/Obfuscated Int value")]
	public class ObfuscatedIntReferenceValue : IntReferenceValue
	{
		protected override void Initialize()
		{
			base.Initialize();

			// Internally obfuscate the value by XOR-ing it with a random value
			// TODO: clamp to realistic values
			var obf = 0;
			InputFilter = value =>
			{
				// Set new obfuscation XOR every time the score is set
				obf = Random.Range(int.MinValue, int.MaxValue);
				return value ^ obf;
			};
			OutputFilter = value => value ^ obf;
		}
	}
}