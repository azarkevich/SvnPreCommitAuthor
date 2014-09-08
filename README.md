SvnPreCommitAuthor
==================

Can be used for replace user names.

Create hook with next content(here is only idea, use real paths):

set PATH=%PATH%;C:\Program Files (x86)\VisualSVN Server\bin
"%1\hooks\name-mapper\svnprecommitauthor.exe" "%1" "%2" "%1\hooks\name-mapper\mapping.txt" >&2 || exit 1

fill name-mapper\mapping.txt with data and enjoy.
