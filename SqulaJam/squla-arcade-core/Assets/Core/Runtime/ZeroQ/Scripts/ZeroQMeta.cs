using System;
using System.Reflection;
using System.Collections.Generic;
using Squla.Core.Logging;
using System.Linq;


namespace Squla.Core.ZeroQ
{
	internal class ZeroQMeta
	{
		static SmartLogger logger = SmartLogger.GetLogger<ZeroQMeta> ();

		internal Dictionary<string, SubscriberMeta> subscribersMeta;

		public ZeroQMeta (Type targetType)
		{
			var methodInfos = targetType.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			for (int i = 0; i < methodInfos.Length; i++) {
				var mInfo = methodInfos [i];

				var subscribeAttrs = (Subscribe[])Attribute.GetCustomAttributes (mInfo, typeof(Subscribe));
				for (var j = 0; j < subscribeAttrs.Length; j++) {
					var subscribeAttr = subscribeAttrs [j];
					Handle_Subscribe (targetType, mInfo, subscribeAttr);
				}
			}
		}

		private void Handle_Subscribe (Type targetType, MethodInfo mInfo, Subscribe attr)
		{
			if (attr == null)
				return;

			if (string.IsNullOrEmpty (attr.Name)) {
				logger.Error ("[Subscribe] method with no command won't work {0}.{1}", targetType.FullName, mInfo.Name);
				return;
			}

			if (mInfo.GetParameters ().Length > 1) {
				logger.Error ("[Subscribe] method with more than 1 won't work {0}.{1}", targetType.FullName, mInfo.Name);
				return;
			}

			if (subscribersMeta == null) {
				subscribersMeta = new Dictionary<string, SubscriberMeta> ();
			}

			var subscriptionName = attr.Name;
			if (subscribersMeta.ContainsKey (subscriptionName)) {
				logger.Error ("[Subscribe (\"{0}\")] duplicated within the same type '{1}.{2}'", subscriptionName, targetType.FullName, mInfo.Name);
				return;
			}

			subscribersMeta [subscriptionName] = new SubscriberMeta (mInfo);
		}
	}

	internal class SubscriberMeta
	{
		private static System.Object[] emptyArray = new System.Object[] { };

		private MethodInfo methodInfo;
		private bool hasArg;

		public SubscriberMeta (MethodInfo methodInfo)
		{
			this.methodInfo = methodInfo;
			this.hasArg = methodInfo.GetParameters ().Length == 1;
		}

		public void Invoke (System.Object target, System.Object source)
		{
			try {
				if (hasArg) {
					methodInfo.Invoke(target, new System.Object[] {source});
				} else {
					methodInfo.Invoke(target, emptyArray);
				}
			} catch (ArgumentException e) {
				throw new Exception($"{methodInfo.DeclaringType}.{methodInfo.Name}: {e.Message}", e);
			}
		}
	}


}

