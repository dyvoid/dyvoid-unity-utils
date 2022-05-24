using MediaMonks.References;
using UnityEngine;

namespace References.Components
{
	public class TransformReferenceInstaller : SceneInstaller
	{
		[Header("Assigns a Transform to the reference")]
		[SerializeField] private TransformReferenceObject _transformReference;
		[SerializeField] private Transform _transform;

		public override void Install()
		{
			_transformReference.SetValue(_transform);
		}

		public override void Uninstall()
		{
			_transformReference.NullSafeClearValue();
		}
	}
}