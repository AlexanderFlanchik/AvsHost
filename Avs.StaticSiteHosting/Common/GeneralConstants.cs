namespace Avs.StaticSiteHosting.Web
{
    public class GeneralConstants
    {
        #region MongoDb collections

        public const string USERS_COLLECTION = "Users";
        public const string ROLES_COLLECTION = "Roles";
        public const string SITES_COLLECTION = "Sites";
        public const string HELPSECTION_COLLECTION = "HelpSections";
        public const string HELPTOPIC_COLLECTION = "HelpTopics";
        public const string TOPICPARAGRAPH_COLLECTION = "TopicParagraphs";
        public const string CONTENT_ITEMS_COLLECTION = "ContentItems";
        public const string HELPRESOURCE_COLLECTION = "HelpResources";
        public const string CONVERSATION_COLLECTION = "Conversations";
        public const string CONVERSATION_MESSAGE_COLLECTION = "ConversationMessages";
        public const string SITE_EVENTS_COLLECTION = "SiteEvents";
        public const string SITE_VIEWED_INFO_COLLECTION = "ViewedSiteInfos";
        public const string APP_SETTINGS_COLLECTION = "AppSettings";
        public const string TAGS_COLLECTION = "Tags";

        #endregion

        #region Roles

        public const string DEFAULT_USER_ROLE = "User";
        public const string ADMIN_ROLE = "Administrator";

        #endregion

        public const string TOTAL_ROWS_AMOUNT = "total-rows-amount";
        public const string ACTIVE_SITES_AMOUNT = "active-sites-amount";
        public const string UPLOAD_SESSION_ID = "upload-session-id";

        public const string ERROR_ROUTE = "_error";
        public const string GET_ACCESS_TOKEN_NAME = "__accessToken";
        public const string GET_ACCESS_TOKEN_NAME_SIGNALR = "access_token";
        public const string SITE_CONTEXT_KEY = "Site_Context_Data";

        public const string NEW_RESOURCE_PATTERN = "%25NEW_RESOURCE%25";
        public const string EXIST_RESOURCE_PATTERN = "%25EXIST_RESOURCE%25";
    }
}