// svnprecommitauthor.cpp : Defines the entry point for the console application.
//

#define APR_DECLARE_STATIC

#include <svn_fs.h>
#include <apr_pools.h>

int main(int argc, char* argv[])
{
	svn_fs_txn_t *txn;
	svn_fs_t *fs;
	apr_pool_t *pool = NULL;
	svn_error_t *svn_err = NULL;
	svn_string_t *val = NULL;

	apr_status_t apr_st = apr_pool_initialize();

	apr_st = apr_pool_create_ex(&pool, NULL, NULL, NULL);

	svn_err = svn_fs_open(&fs, "D:\\store\\svn-repositories\\docs\\db", NULL, pool);

	svn_err = svn_fs_open_txn(&txn, fs, "242-6r", pool);

	svn_err = svn_fs_txn_prop(&val, txn, "svn:author", pool);

	val = svn_string_create("sazarkevich", pool);
	svn_err = svn_fs_change_txn_prop(txn, "svn:author", val, pool);

	return 0;
}

