#define APR_DECLARE_STATIC

#include <svn_fs.h>
#include <apr_pools.h>

void apr_error(apr_status_t apr_st, const char *source)
{
	char buff[512];
	char *err = apr_strerror(apr_st, buff, sizeof(buff));

	fprintf(stderr, "APR error in %s: %d (0x%08X): %s\n", source, apr_st, apr_st, err);
}

void svn_error(svn_error_t *svn_err, const char *source)
{
	fprintf(stderr, "ERROR: %s: %s\n", source, svn_err->message);
}

int map_authors(const char *reposPath, const char *txnName, const char *mappingFile, apr_pool_t *pool)
{
	svn_fs_txn_t *txn;
	svn_fs_t *fs;
	svn_error_t *svn_err;
	svn_string_t *author;
	apr_status_t apr_st;
	apr_file_t *file;
	char lineBuff[128];

	svn_err = svn_fs_open(&fs, svn_string_createf(pool, "%s\\db", reposPath)->data, NULL, pool);
	if(svn_err != NULL)
	{
		svn_error(svn_err, "svn_fs_open");
		return 1;
	}

	svn_err = svn_fs_open_txn(&txn, fs, txnName, pool);
	if(svn_err != NULL)
	{
		svn_error(svn_err, "svn_fs_open_txn");
		return 1;
	}

	svn_err = svn_fs_txn_prop(&author, txn, "svn:author", pool);
	if(svn_err != NULL)
	{
		svn_error(svn_err, "svn_fs_txn_prop");
		return 1;
	}

	/* read mapping and try find name */
	apr_st = apr_file_open(&file, mappingFile, APR_READ, APR_OS_DEFAULT, pool);
	if(apr_st != APR_SUCCESS)
	{
		apr_error(apr_st, "apr_file_open");
		return 1;
	}

	while(1)
	{
		svn_string_t *map_from;

		apr_st = apr_file_gets(lineBuff, sizeof(lineBuff) - 1, file);

		/* break on EOF */
		if(APR_STATUS_IS_EOF(apr_st))
			break;

		/* or error */
		if(apr_st != APR_SUCCESS)
		{
			apr_error(apr_st, "apr_file_gets");
			return 1;
		}

		map_from = svn_string_create(strtok(lineBuff, "\t\r\n"), pool);

		if(stricmp(map_from->data, author->data) == 0)
		{
			svn_string_t *map_to;

			const char* map_to_s = strtok(NULL, "\t\r\n");
			if(map_to_s == NULL)
			{
				fprintf(stderr, "Incorrect mapping file.\n");
				return 1;
			}

			map_to = svn_string_create(map_to_s, pool);

			svn_err = svn_fs_change_txn_prop(txn, "svn:author", map_to, pool);
			if(svn_err != NULL)
			{
				svn_error(svn_err, "svn_fs_change_txn_prop");
				return 1;
			}

			/* indicate successfull replace by output old + new name */
			fprintf(stdout, "%s	%s\n", author->data, map_to->data);
			break;
		}
	}

	return 0;
}

int main(int argc, char* argv[])
{
	apr_status_t apr_st;
	apr_pool_t *pool;
	int ret;

	if(argc < 4)
	{
		fprintf(stderr, "Usage: SvnPreCommitAuthor.exe <repose path> <transaction name> <names mappig file>\n\
where\n\
	<repose path> <transaction name> - argumats passed to hook\n\
	<names mapping file> - path to ASCII file in format <original name><tab><new name>\n\
");
		return 1;
	}

	apr_st = apr_pool_initialize();
	if(apr_st != APR_SUCCESS)
	{
		apr_error(apr_st, "apr_pool_initialize");
		return 1;
	}

	apr_st = apr_pool_create_ex(&pool, NULL, NULL, NULL);
	if(apr_st != APR_SUCCESS)
	{
		apr_error(apr_st, "apr_pool_create_ex");
		return 1;
	}

	ret = map_authors(argv[1], argv[2], argv[3], pool);

	apr_pool_destroy(pool);

	return ret;
}

