using System;
using System.Globalization;
using MediaMonks.References;
using TMPro;
using UnityEngine;

namespace References.Components
{
	/// <summary>
	/// A label that displays the value of a ScriptableObjectReference.
	/// If the ScriptableObjectReference implements IValueAsObjectHasChangedSender, the label is updated when the value changes.
	/// </summary>
	[RequireComponent(typeof(TMP_Text))]
	public class ReferenceValueLabel : MonoBehaviour
	{
		[SerializeField] private ScriptableObjectReferenceBase _valueReference;
		[SerializeField] private string _format = "{0}";

		[Tooltip("Which format style to use.")]
		[SerializeField] private TextFormatting _formatStyle = TextFormatting.StringFormat;

#if UNITY_EDITOR
		[SerializeField] private bool _showTypeInEditor = true;
#endif

		private TMP_Text _label;

		private IValueAsObjectHasChangedSender _objectValueHasChangedInstance;

		public enum TextFormatting
		{
			TMPro,
			StringFormat,
		}

		public string Format
		{
			get => _format;
			set
			{
				_format = value;

				if (_valueReference.TryGetValue(out object outValue))
				{
					UpdateLabelText(outValue);
				}
			}
		}

		public TMP_Text Label => _label ? _label : _label = GetComponent<TMP_Text>();

		public TextFormatting FormatStyle => _formatStyle;

		private void Start()
		{
			if (!_valueReference) return;

			SetValueReference(_valueReference);
		}

		public void SetValueReference(ScriptableObjectReferenceBase value)
		{
			if (_objectValueHasChangedInstance != null)
			{
				_objectValueHasChangedInstance.ValueAsObjectHasChanged -= OnValueChanged;
				_objectValueHasChangedInstance = null;
			}

			_valueReference = value;

			if (_valueReference.TryGetValue(out object outValue))
			{
				UpdateLabelText(outValue);
			}

			_objectValueHasChangedInstance = value as IValueAsObjectHasChangedSender;
			if (_objectValueHasChangedInstance != null)
			{
				_objectValueHasChangedInstance.ValueAsObjectHasChanged += OnValueChanged;
			}
		}

		public virtual void UpdateLabelText(object value)
		{
			switch (_formatStyle)
			{
				case TextFormatting.TMPro:
					switch (value)
					{
						case float val:
							Label.SetText(_format, val);
							break;
						case int val:
							Label.SetText(_format, val);
							break;
						default:
							Label.text = string.Format(CultureInfo.InvariantCulture, _format, value);
							break;
					}

					break;
				case TextFormatting.StringFormat:
					Label.text = string.Format(CultureInfo.InvariantCulture, _format, value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnValueChanged(object newValue, object oldValue)
		{
			UpdateLabelText(newValue);
		}

		private void OnDestroy()
		{
			if (_objectValueHasChangedInstance == null) return;

			_objectValueHasChangedInstance.ValueAsObjectHasChanged -= OnValueChanged;
			_objectValueHasChangedInstance = null;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (Application.isPlaying) return;

			var text = GetComponent<TMP_Text>();

			if (text != null)
			{
				text.text = _valueReference == null || !_showTypeInEditor
					? _format
					: string.Format(CultureInfo.InvariantCulture, _format, $"{{{_valueReference.GetTypeOfValue().Name}}}");
			}
		}
#endif
	}
}