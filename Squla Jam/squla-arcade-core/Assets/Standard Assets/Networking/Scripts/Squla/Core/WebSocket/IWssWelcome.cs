using System;

namespace Squla.Core.Network
{
	public interface IWSSWelcome
	{
		void GotWelcome ();

		void GotUnauthorized ();
	}
}
