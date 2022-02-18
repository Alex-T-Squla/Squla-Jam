using System;

namespace Squla.Core.Modelize
{
	public abstract class TypeMetaClass
	{
		protected Type targetType;

		protected TypeMetaClass (Type targetType)
		{
			this.targetType = targetType;
		}

		public abstract System.Object Modelize (System.Object target, System.Object source);

		public virtual void Flush ()
		{
			
		}
	}
}

