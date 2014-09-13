using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SharpSvn;
using SvnUpdateNames.Core;

namespace SvnUpdateNames
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				var argsl = args.ToList();

				if(OptionsParser.GetSwitch(argsl, "--help"))
				{
					ShowHelp();
					return 0;
				}

				var beQuiet = OptionsParser.GetSwitch(argsl, "--quiet");

				var reposUrl = OptionsParser.GetString(argsl, "--url");
				if(reposUrl == null)
					throw new UsageException("--url not specified.");

				var mappingPath = OptionsParser.GetString(argsl, "--mapping");

				var old2NewNameMap = File
					.ReadAllLines(mappingPath)
					.Select(l => l.Split('\t'))
					.ToDictionary(arr => arr[0].ToLowerInvariant(), arr => arr[1])
				;

				var ignoreNamesSet = File
					.ReadAllLines(mappingPath)
					.Select(l => l.Split('\t')[1].ToLowerInvariant())
					.Distinct()
					.ToDictionary(a => a, a => a)
				;

				var ignorePath = OptionsParser.GetString(argsl, "--ignore");

				if (ignorePath != null)
				{
					File
						.ReadAllLines(ignorePath)
						.Distinct()
						.ToList()
						.ForEach(n => {
							ignoreNamesSet[n.ToLowerInvariant()] = n;
						})
					;
				}

				var uri = new Uri(reposUrl);

				var startRevision = new SvnRevision(SvnRevisionType.Head);

				using (var client = new SvnClient())
				{
					while (true)
					{
						var svnArgs = new SvnLogArgs
						{
							Start = startRevision,
							Limit = 500
						};

						Collection<SvnLogEventArgs> log;

						client.GetLog(uri, svnArgs, out log);

						if (!beQuiet)
							Console.WriteLine("Rev: {0}...", startRevision);

						if (log.Count == 0)
							break;

						for (var i = 0; i < log.Count; i++)
						{
							if (log[i].Revision == 0)
								break;

							var author = log[i].Author.ToLowerInvariant();
							if (old2NewNameMap.ContainsKey(author))
							{
								var replacee = old2NewNameMap[author];

								client.SetRevisionProperty(uri, new SvnRevision(log[i].Revision), "svn:author", replacee);

								if(!beQuiet)
									Console.WriteLine("	{0} -> {1}", author, replacee);
							}
							else if (!ignoreNamesSet.ContainsKey(author))
							{
								ignoreNamesSet[author] = author;
								Console.Error.WriteLine("NOT MATCHED: {0}", author);
							}
						}

						if (log[log.Count - 1].Revision == 0)
							break;

						startRevision = new SvnRevision(log[log.Count - 1].Revision - 1);
					}
				}

				return 0;
			}
			catch(UsageException ex)
			{
				Console.WriteLine(ex.Message);
				ShowHelp();
				return 1;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 1;
			}
		}

		static void ShowHelp()
		{
			Console.Error.WriteLine("Usage: SvnUpdateNames --url <repos url> --mapping <mapping> [--ignore <ignores>] [--quiet]");
			Console.Error.WriteLine("where");
			Console.Error.WriteLine("	<repos url> - url to SVN repository");
			Console.Error.WriteLine("	<mapping> - path to mapping file, used by svnprecommitauthor hook");
			Console.Error.WriteLine("	<ignore> - optional path list of names (one per line) which should be ignored");
		}
	}
}
