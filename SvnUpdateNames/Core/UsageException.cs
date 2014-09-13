using System;

namespace SvnUpdateNames.Core
{
	class UsageException : ApplicationException
	{
		public UsageException(string message)
			: base(message)
		{
		}
	}
}
