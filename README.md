SvnPreCommitAuthor
==================

Tool intended for replace SVN user names in pre-commit hook.

Create hook with next content(use real paths):

```
set PATH=%PATH%;C:\Program Files (x86)\VisualSVN Server\bin
"%1\hooks\name-mapper\svnprecommitauthor.exe" "%1" "%2" "%1\hooks\name-mapper\mapping.txt" >&2 || exit 1
```

fill name-mapper\mapping.txt with data and enjoy.


SvnUpdateNames
=================

Tool for update existing names in repository. Accept mapping efile in same formatas for SvnPreCommitAuthor
