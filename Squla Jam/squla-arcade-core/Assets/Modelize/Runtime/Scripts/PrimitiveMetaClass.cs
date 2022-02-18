using System;

namespace Squla.Core.Modelize
{
	public class PrimitiveMetaClass: TypeMetaClass
	{
		delegate System.Object ValueConverter (System.Object source);

		private ValueConverter converter;

		public PrimitiveMetaClass (Type targetType) : base (targetType)
		{
			this.targetType = targetType;
			if (targetType == typeof(int))
				converter = ValueAsInt;
			else if (targetType == typeof(float))
				converter = ValueAsFloat;
			else if (targetType == typeof(string))
				converter = ValueAsString;
			else if (targetType == typeof(bool))
				converter = ValueAsBool;
			else if (targetType == typeof(DateTime))
				converter = ValueAsDateTime;
			else
				converter = Unsupported;
		}

		public override System.Object Modelize (System.Object target, System.Object source)
		{
			return converter (source);
		}

		System.Object ValueAsInt (System.Object source)
		{
			if (source == null)
				return 0;

			return Convert.ToInt32 (source);

		}

		System.Object ValueAsFloat (System.Object source)
		{
			if (source == null)
				return 0.0f;


			return Convert.ToSingle (source);

		}

		System.Object ValueAsString (System.Object source)
		{
			return source == null ? string.Empty : source.ToString ();
		}

		System.Object ValueAsBool (System.Object source)
		{
			if (source == null)
				return false;

			if (source is bool) {
				return source;
			}

			if (source is long) {
				long value = (long)source;
				return value == 0 ? false : true;
			}

			// might be other types.
			throw new Exception (string.Format ("value not convertable to bool: {0} {1}", source.GetType (), source));
		}

		System.Object ValueAsDateTime (System.Object source)
		{
			if (source == null)
				return new DateTime (0, 0, 0);

			if (source is string) {
				return (DateTime)(Convert.ToDateTime ((string)source));
			}

			throw new Exception (string.Format ("value not convertable to DateTime: {0} {1}", source.GetType (), source));
		}

		System.Object Unsupported (System.Object source)
		{
			throw new Exception (string.Format ("Unsupported type: {0}", targetType));
		}
	}
}

