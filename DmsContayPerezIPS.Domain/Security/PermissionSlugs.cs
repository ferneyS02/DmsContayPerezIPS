namespace DmsContayPerezIPS.Domain.Security;


public static class PermissionSlugs
{
    public const string DocumentsCreate = "documents:create";
    public const string DocumentsRead = "documents:read";
    public const string DocumentsReadPhi = "documents:read.phi";
    public const string DocumentsReadRedacted = "documents:read.redacted";
    public const string DocumentsUpdateMetadata = "documents:update.metadata";
    public const string DocumentsVersionCreate = "documents:version.create";
    public const string DocumentsApprove = "documents:approve";
    public const string DocumentsPublish = "documents:publish";
    public const string DocumentsObsolete = "documents:obsolete";
    public const string DocumentsArchive = "documents:archive";
    public const string DocumentsDispose = "documents:dispose";
    public const string DocumentsMove = "documents:move";
    public const string DocumentsAclManage = "documents:acl.manage";
    public const string DocumentsSharePresign = "documents:share.presign";
    public const string DocumentsExport = "documents:export";
    public const string TagsManage = "tags:manage";
    public const string SeriesManage = "series:manage";
    public const string RetentionHold = "retention:hold";
    public const string RetentionLegalHold = "retention:legalhold";
    public const string AuditRead = "audit:read";
    public const string UsersManage = "users:manage";
    public const string RolesManage = "roles:manage";
    public const string SystemConfigure = "system:configure";
    public const string MinioBucketsManage = "minio:buckets.manage";
    public const string SearchAdvanced = "search:advanced";
}