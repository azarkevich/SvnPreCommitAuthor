using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SharpSvn;

namespace SvnUpdateNames
{
	class Program
	{
		static void Main()
		{
			var map = File
				.ReadAllLines(@"mapping.txt")
				.Select(l => l.Split('\t'))
				.ToDictionary(arr => arr[0].ToLowerInvariant(), arr => arr[1])
			;

			var rmap = File
				.ReadAllLines(@"mapping.txt")
				.Select(l => l.Split('\t')[1].ToLowerInvariant())
				.Distinct()
				.ToDictionary(a => a, a => a)
			;

			var ignore = File
				.ReadAllLines(@"ignore.txt")
				.Distinct()
				.ToDictionary(a => a, a => a)
			;

			var uri = new Uri("...");

			var startRevision = new SvnRevision(SvnRevisionType.Head);

			using(var client = new SvnClient())
			{
				while(true)
				{
					var args = new SvnLogArgs {
						Start = startRevision,
						Limit = 500
					};

					Collection<SvnLogEventArgs> log;

					client.GetLog(uri, args, out log);

					Console.WriteLine("Rev: {0}...", startRevision);

					if (log.Count == 0)
						break;

					for (var i = 0; i < log.Count; i++)
					{
						if (log[i].Revision == 0)
							break;

						var author = log[i].Author.ToLowerInvariant();
						if (map.ContainsKey(author))
						{
							var replacee = map[author];

							client.SetRevisionProperty(uri, new SvnRevision(log[i].Revision), "svn:author", replacee);

							Console.WriteLine("	{0} -> {1}", author, replacee);
						}
						else if (!rmap.ContainsKey(author))
						{
							if (!ignore.ContainsKey(author))
							{
								ignore[author] = author;
								Console.WriteLine("NOT MATCHED: {0}", author);
							}
						}
					}

					if (log[log.Count - 1].Revision == 0)
						break;

					startRevision = new SvnRevision(log[log.Count - 1].Revision - 1);
				}
			}
		}
	}
}
