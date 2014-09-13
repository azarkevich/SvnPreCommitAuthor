using System;
using System.Collections.Generic;
using System.Linq;

namespace SvnUpdateNames.Core
{
	static class OptionsParser
	{
		public static string GetCommand(List<string> args, string defValue = null)
		{
			for (var i = 0; i < args.Count; i++)
			{
				if (args[i] == "--")
					break;

				if (args[i].StartsWith("-"))
					continue;

				var value = args[i];
				args.RemoveAt(i);
				return value;
			}

			return defValue;
		}

		public static string GetValue(List<string> args, string defValue = null)
		{
			var wasSwitchStop = false;
			for (var i = 0; i < args.Count; i++)
			{
				if (args[i] == "--")
				{
					wasSwitchStop = true;
					continue;
				}

				if (args[i].StartsWith("-") && !wasSwitchStop)
					continue;

				var value = args[i];
				args.RemoveAt(i);
				return value;
			}

			return defValue;
		}

		static string[] GetSwitchValues(List<string> args, string switchName, int count)
		{
			var switchArg = args.TakeWhile(a => a != "--").FirstOrDefault(a => a == switchName);
			if (switchArg == null)
				return null;

			var index = args.IndexOf(switchArg);
			if (args.Count <= (index + count))
				throw new UsageException("No value(s) for switch " + switchName);

			var ret = args.Skip(index + 1).Take(count).ToArray();
			args.RemoveRange(index, count + 1);

			return ret;
		}

		public static bool GetSwitch(List<string> args, string switchName)
		{
			return GetSwitchValues(args, switchName, 0) != null;
		}

		public static int GetInt(List<string> args, string switchName, int defValue = 0)
		{
			var values = GetSwitchValues(args, switchName, 1);

			if (values == null)
				return defValue;

			int ret;
			if (!Int32.TryParse(values[0], out ret))
				throw new UsageException("Value for switch " + switchName + " is not integer");

			return ret;
		}

		public static string GetString(List<string> args, string switchName, string defValue = null)
		{
			var values = GetSwitchValues(args, switchName, 1);

			if (values == null)
				return defValue;

			return values[0];
		}

		public static Tuple<string, string> Get2String(List<string> args, string switchName, string defValue1 = null, string defValue2 = null)
		{
			var values = GetSwitchValues(args, switchName, 2);

			if (values == null)
				return Tuple.Create(defValue1, defValue2);

			return Tuple.Create(values[0], values[1]);
		}
	}
}
