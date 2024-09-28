namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {

    #pragma warning disable 169
    /// <summary>
    /// UserProfile class
    /// </summary>
    public class UserProfile
    {
        public static int ConcurrentSessionCount = -1; // Maximum sessions allowed

        public static bool IsForceLogoutUser = false; // Force logout user // DN

        public static bool IsForceLogoutConcurrentUser = false; // Force logout concurrent user // DN

        public static int SessionCleanupTime = 60 * 24; // Clean up unused sessions if idle more than 1 day

        public static int SessionTimeout = -1;

        public static int MaxRetry = 3;

        public static int RetryLockout = 20;

        public static int PasswordExpire = 90;

        public static string ConcurrentSessions = "Sessions";

        public static string SessionId = "SessionID";

        public static string LastAccessedDateTime = "LastAccessedDateTime";

        public static string ForceLogout = "ForceLogout";

        public static string LoginRetryCount = "LoginRetryCount";

        public static string LastBadLoginDateTime = "LastBadLoginDateTime";

        public static string LastPasswordChangedDate = "LastPasswordChangedDate";

        public static string LanguageId = "LanguageId";

        public static string SearchFilters = "SearchFilters";

        public static string Image = "UserImage";

        public static string Secret = "Secret";

        public static string SecretCreateDateTime = "SecretCreateDateTime";

        public static string SecretVerifyDateTime = "SecretVerifyDateTime";

        public static string SecretLastVerifyCode = "SecretLastVerifyCode";

        public static string BackupCodes = "BackupCodes";

        public static string OneTimePassword = "OTP";

        public static string OTPAccount = "OTPAccount";

        public static string OTPCreateDateTime = "OTPCreateDateTime";

        public static string OTPVerifyDateTime = "OTPVerifyDateTime";

        public static string ChatEnabled = "ChatEnabled";

        private string _userName = "";

        private object? _userId;

        private object? _userPrimaryKey;

        private object? _parentUserId;

        private string _userLevel = AdvancedSecurity.AnonymousUserLevelId.ToString();

        private Dictionary<string, object?> _profile = new();

        private Dictionary<string, object?> _allfilters = new();

        private string _provider = "";

        public int TimeoutTime;

        public int MaxRetryCount;

        public int RetryLockoutTime;

        public int PasswordExpiryTime;

        // Constructor
        public UserProfile(string userName = "")
        {
            TimeoutTime = SessionTimeout;
            MaxRetryCount = MaxRetry;
            RetryLockoutTime = RetryLockout;
            PasswordExpiryTime = PasswordExpire;
            SetLoginRetryCount(0).SetLastBadLoginDateTime(""); // Max login retry
            SetLastPasswordChangedDate(""); // Password Expiry
            if (!Empty(userName))
                SetUserName(userName).LoadFromStorage();
        }

        // Get user name
        public string GetUserName() => _userName;

        // Set user name
        public UserProfile SetUserName(string value)
        {
            _userName = value;
            return this;
        }

        // Get user ID
        public object? GetUserID() => _userId;

        // Set user ID
        public UserProfile SetUserID(object? value)
        {
            _userId = value;
            return this;
        }

        // Get user primary key
        public object? GetUserPrimaryKey() => _userPrimaryKey;

        // Set user primary key
        public UserProfile SetUserPrimaryKey(object? value)
        {
            _userPrimaryKey = value;
            return this;
        }

        // Get parent user ID
        public object? GetParentUserID() => _parentUserId;

        // Set parent user ID
        public UserProfile SetParentUserID(object? value)
        {
            _parentUserId = value;
            return this;
        }

        // Get user level
        public string GetUserLevel() => _userLevel;

        // Set user level
        public UserProfile SetUserLevel(string value)
        {
            _userLevel = ConvertToString(value);
            return this;
        }

        // Get login arguments
        public dynamic GetLoginArguments() => new {
            userName = GetUserName(),
            userId = GetUserID(),
            parentUserId = GetParentUserID(),
            userLevel = GetUserLevel(),
            userPrimaryKey = GetUserPrimaryKey()
        };

        // Get profile
        public Dictionary<string, object?> GetProfile() => _profile;

        // Set profile
        public UserProfile SetProfile(Dictionary<string, object?> value)
        {
            _profile = value;
            return this;
        }

        // Get provider
        public string GetProvider() => _provider;

        // Set provider
        public UserProfile SetProvider(string value)
        {
            _provider = value;
            return this;
        }

        // Has value in profile
        public bool Has(string name) => _profile.ContainsKey(name);

        // Get value
        public object? GetValue(string name) => _profile.TryGetValue(name, out object? value) ? value : null;

        // Get value
        public T GetValue<T>(string name) => ChangeType<T>(GetValue(name));

        // Set value
        public void SetValue(string name, object? value)
        {
            if (value == null)
                Delete(name);
            else if (_profile.ContainsKey(name))
                _profile[name] = value;
            else
                _profile.Add(name, value);
        }

        // Get/Set by name
        public object? this[string name]
        {
            get => GetValue(name);
            set => SetValue(name, value);
        }

        // Delete property
        public void Delete(string name) => _profile.Remove(name);

        // Assign properties
        public void Assign(Dictionary<string, object>? input)
        {
            if (input == null)
                return;
            foreach (var (key, value) in input)
                if (key != Config.UserProfileFieldName) // Skip profile field // DN
                    SetValue(key, value);
        }

        // Check if System Admin
        protected bool IsSystemAdmin() => IsSysAdmin();

        // Get language id
        public string GetLanguageId() => GetValue<string>(LanguageId);

        // Set language id
        public UserProfile SetLanguageId(string? value)
        {
            SetValue(LanguageId, value);
            return this;
        }

        // Get search filters
        public string GetFilters() => GetValue<string>(SearchFilters);

        // Set search filters
        public UserProfile SetFilters(string? value)
        {
            SetValue(SearchFilters, value);
            return this;
        }

        // Get search filters for a page
        public string GetSearchFilters(string pageid)
        {
            try {
                string searchFilters = GetFilters();
                if (!Empty(searchFilters)) {
                    _allfilters = StringToProfile(searchFilters);
                    if (_allfilters.TryGetValue(pageid, out object? result) && result != null)
                        return ConvertToString(result);
                }
            } catch {
                if (Config.Debug)
                    throw;
            }
            return "";
        }

        // Set search filters for a page
        public async Task<bool> SetSearchFilters(string pageid, string filters)
        {
            try {
                string searchFilters = GetFilters();
                if (!Empty(searchFilters))
                    _allfilters = StringToProfile(searchFilters);
                if (!_allfilters.ContainsKey(pageid))
                    _allfilters.Add(pageid, filters);
                else
                    _allfilters[pageid] = filters;
                return await SetFilters(ConvertToJson(_allfilters)).SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
            }
            return false;
        }

        // Load profile from storage
        public bool LoadFromStorage() => LoadFromStorageAsync().GetAwaiter().GetResult();

        // Load profile from storage
        #pragma warning disable 1998

        public async Task<bool> LoadFromStorageAsync()
        {
            if (Session.TryGetValue(Config.SessionUserProfileRecord, out object? value)) {
                if (value is Dictionary<string, object> dict) { // Load from session for register and system admin
                    if (dict.TryGetValue(Config.SysAdminUserProfile, out object? profile)) {
                        if (profile != null) {
                            LoadProfile(ConvertToString(profile));
                            return true;
                        }
                    }
                }
            }

            // Load from database
            if (Empty(GetUserName())) // Empty user name
                return false;
            return false;
        }
        #pragma warning restore 1998

        // Save profile to storage
        public bool SaveToStorage() => SaveToStorageAsync().GetAwaiter().GetResult();

        // Save profile to storage
        #pragma warning disable 1998

        public async Task<bool> SaveToStorageAsync()
        {
            if (Session.TryGetValue(Config.SessionUserProfileRecord, out object? value)) {
                if (value is Dictionary<string, object> dict) { // Load from session for register and system admin
                    if (dict.ContainsKey(Config.SysAdminUserProfile))
                        dict[Config.SysAdminUserProfile] = ProfileToString();
                    else
                        dict.Add(Config.SysAdminUserProfile, ProfileToString());
                    Session.SetValue(Config.SessionUserProfileRecord, dict);
                    return true;
                }
            }

            // Save to database
            if (Empty(GetUserName())) // Empty user name
                return false;
            return true;
        }
        #pragma warning restore 1998

        // Load profile from session
        public void LoadFromSession() => LoadProfile(Session.GetString(Config.SessionUserProfile));

        // Load profile from session (comaptible with old version)
        public void Load() => LoadFromSession();

        // Save profile to session
        public void SaveToSession() => Session[Config.SessionUserProfile] = ProfileToString();

        // Save profile to session (comaptible with old version)
        public void Save() => SaveToSession();

        // Load profile
        public void LoadProfile(string str)
        {
            if (!Empty(str) && !SameString(str, "{}")) // DN
                _profile = StringToProfile(str);
        }

        // Clear profile
        public void ClearProfile() => _profile.Clear();

        // Clear profile (alias)
        public void Clear() => ClearProfile();

        // Serialize profile
        public string ProfileToString() => JsonConvert.SerializeObject(_profile);

        // Split profile
        private Dictionary<string, object?> StringToProfile(string str)
        {
            try {
                return JsonConvert.DeserializeObject<Dictionary<string, object?>>(str) ?? new();
            } catch {
                return new();
            }
        }

        // Get concurrent sessions
        public List<Dictionary<string, object>?> GetConcurrentSessions()
        {
            try {
                if (GetValue(ConcurrentSessions) is JArray ar)
                    return ar.Select(s => s.ToObject<Dictionary<string, object>?>()).Where(s => s != null).ToList();
                return new();
            } catch {
                if (Config.Debug)
                    throw;
                return new();
            }
        }

        // Set concurrent sessions
        public UserProfile SetConcurrentSessions(List<Dictionary<string, object>?>? value)
        {
            SetValue(ConcurrentSessions, value);
            return this;
        }

        // Check if valid user
        public async Task<bool> IsValidUser(string sessionId, bool addSession = true)
        {
            if (IsSystemAdmin() || IsApi()) // Ignore system admin / API
                return true;
            try {
                bool valid = false;
                int cnt = 0;
                bool logoutUser = IsForceLogoutConcurrentUser && ConcurrentSessionCount == 1;
                var sessions = GetConcurrentSessions();
                foreach (var session in sessions) {
                    if (session == null)
                        continue;
                    DateTime? dt = ConvertToDateTime(session[LastAccessedDateTime]);
                    bool forceLogout = ConvertToBool(session[ForceLogout]);
                    if (SameString(session[SessionId], sessionId)) {
                        valid = true;
                        if (!forceLogout && (TimeoutTime < 0 || dt != null && ((TimeSpan)(DateTime.Now - dt)).TotalMinutes > TimeoutTime)) // Update accessed time
                            session[LastAccessedDateTime] = StdCurrentDateTime();
                        break;
                    } else if (logoutUser) { // Logout concurrent user
                        session[ForceLogout] = true;
                    } else {
                        cnt++;
                    }
                }
                if (!valid && addSession && (ConcurrentSessionCount < 0 || cnt < ConcurrentSessionCount || logoutUser)) {
                    valid = true;
                    sessions.Add(new() {
                        { SessionId, sessionId },
                        { LastAccessedDateTime, StdCurrentDateTime() },
                        { ForceLogout, false }
                    });
                }
                // Remove unused sessions
                sessions = RemoveUnusedSessions(sessions);
                if (valid)
                    await SetConcurrentSessions(sessions).SaveToStorageAsync();
                return valid;
            } catch {
                if (Config.Debug)
                    throw;
            }
            return false;
        }

        // Remove unused sessions
        protected List<Dictionary<string, object>?> RemoveUnusedSessions(List<Dictionary<string, object>?> sessions)
        {
            int cleanupTime = TimeoutTime > 0 ? TimeoutTime : SessionCleanupTime; // Fallback to cleanup time if timeout not specified
            return sessions.Where(session => session != null &&
                session[LastAccessedDateTime] != null &&
                ConvertToDateTime(session[LastAccessedDateTime]) is DateTime dt &&
                ((TimeSpan)(DateTime.Now - dt)).TotalMinutes <= cleanupTime)
                .ToList();
        }

        // Remove user
        public async Task<bool> RemoveUser(string sessionId)
        {
            if (IsSystemAdmin()) // Ignore system admin
                return true;
            try {
                var sessions = GetConcurrentSessions();
                sessions = sessions.Where(session => session != null && !SameString(session[SessionId], sessionId)).ToList();
                return await SetConcurrentSessions(sessions).SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Reset concurrent user
        public async Task<bool> ResetConcurrentUser()
        {
            try {
                return await SetConcurrentSessions(null).SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Get active user session coount
        public int ActiveUserSessionCount(bool active = true)
        {
            try {
                var sessions = GetConcurrentSessions();
                if (active)
                    sessions = RemoveUnusedSessions(sessions);
                return sessions.Count();
            } catch {
                if (Config.Debug)
                    throw;
            }
            return 0;
        }

        // Force logout user
        public bool IsForceLogout(string sessionId = "")
        {
            if (IsSystemAdmin() || IsApi()) // Ignore system admin / API
                return false;
            try {
                bool isForceLogout = Empty(sessionId);
                var sessions = GetConcurrentSessions();
                foreach (var session in sessions) {
                    if (session == null)
                        continue;
                    if (Empty(sessionId)) { // All session must be force logout
                        if (!ConvertToBool(session[ForceLogout]))
                            return false;
                    } else if (SameString(session[SessionId], sessionId)) {
                        return ConvertToBool(session[ForceLogout]);
                    }
                }
                return isForceLogout;
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Force logout user
        public async Task<bool> ForceLogoutUser()
        {
            if (!IsForceLogoutUser)
                return false;
            try {
                var sessions = GetConcurrentSessions();
                sessions = RemoveUnusedSessions(sessions);
                foreach (var session in sessions)
                    session![ForceLogout] = true;
                return await SetConcurrentSessions(sessions).SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Exceed login retry
        public async Task<bool> ExceedLoginRetry()
        {
            if (IsSystemAdmin()) // Ignore system admin
                return false;
            try {
                int retrycount = GetLoginRetryCount();
                string dt = GetLastBadLoginDateTime();
                if (retrycount >= MaxRetryCount) {
                    if (DateTime.Compare(DateTime.Parse(dt).AddMinutes(RetryLockoutTime), DateTime.Now) > 0) {
                        return true;
                    } else {
                        await ResetLoginRetry();
                        return false;
                    }
                }
                return false;
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Reset login retry
        public async Task<bool> ResetLoginRetry()
        {
            try {
                return await SetLoginRetryCount(0).SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Check if password expired
        public bool PasswordExpired()
        {
            if (IsSystemAdmin()) // Ignore system admin
                return false;
            try {
                string dt = GetLastPasswordChangedDate();
                if (Empty(dt))
                    dt = DateTime.Today.ToString("yyyy'/'MM'/'dd");
                return (DateTime.Compare(DateTime.Parse(dt).AddDays(PasswordExpiryTime), DateTime.Today) < 0);
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Check if password change date is empty
        public bool EmptyPasswordChangedDate()
        {
            if (IsSystemAdmin()) // Ignore system admin
                return false;
            try {
                string dt = GetLastPasswordChangedDate();
                return Empty(dt);
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Set password expired
        public async Task<bool> SetPasswordExpired()
        {
            try {
                return await SetLastPasswordChangedDate(DateTime.Today.AddDays(-1 * (PasswordExpiryTime + 1)).ToString("yyyy'/'MM'/'dd")).SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Get login retry count
        public int GetLoginRetryCount() => GetValue<int>(LoginRetryCount);

        // Set login retry count
        public UserProfile SetLoginRetryCount(int? value)
        {
            SetValue(LoginRetryCount, value);
            return this;
        }

        // Get last bad login date time
        public string GetLastBadLoginDateTime() => GetValue<string>(LastBadLoginDateTime);

        // Set last bad login date time
        public UserProfile SetLastBadLoginDateTime(string? value)
        {
            SetValue(LastBadLoginDateTime, value);
            return this;
        }

        // Get last password changed date
        public string GetLastPasswordChangedDate() => GetValue<string>(LastPasswordChangedDate);

        // Set last password changed date
        public UserProfile SetLastPasswordChangedDate(string? value)
        {
            SetValue(LastPasswordChangedDate, value);
            return this;
        }

        // Get secret
        public string GetSecret() => GetValue<string>(Secret);

        // Set secret
        public UserProfile SetSecret(string? value)
        {
            SetValue(Secret, value);
            return this;
        }

        // Get secret create datetime
        public string GetSecretCreateDateTime() => GetValue<string>(SecretCreateDateTime);

        // Set secret create datetime
        public UserProfile SetSecretCreateDateTime(string? value)
        {
            SetValue(SecretCreateDateTime, value);
            return this;
        }

        // Get secret verify datetime
        public string GetSecretVerifyDateTime() => GetValue<string>(SecretVerifyDateTime);

        // Set secret verify datetime
        public UserProfile SetSecretVerifyDateTime(string? value)
        {
            SetValue(SecretVerifyDateTime, value);
            return this;
        }

        // Get secret last verify code
        public string GetSecretLastVerifyCode() => GetValue<string>(SecretLastVerifyCode);

        // Set secret last verify code
        public UserProfile SetSecretLastVerifyCode(string? value)
        {
            SetValue(SecretLastVerifyCode, value);
            return this;
        }

        // Get backup codes
        public string GetCodes() => GetValue<string>(BackupCodes);

        // Set backup codes
        public UserProfile SetCodes(string? value)
        {
            SetValue(BackupCodes, value);
            return this;
        }

        // Get one time password
        public string GetPassword() => GetValue<string>(OneTimePassword);

        // Set one time password
        public UserProfile SetPassword(string? value)
        {
            SetValue(OneTimePassword, value);
            return this;
        }

        // Get OTP account
        public string GetOtpAccount() => GetValue<string>(OTPAccount);

        // Set OTP account
        public UserProfile SetOtpAccount(string? value)
        {
            SetValue(OTPAccount, value);
            return this;
        }

        // Get OTP create datetime
        public string GetOtpCreateDateTime() => GetValue<string>(OTPCreateDateTime);

        // Set OTP create datetime
        public UserProfile SetOtpCreateDateTime(string? value)
        {
            SetValue(OTPCreateDateTime, value);
            return this;
        }

        // Get OTP verify datetime
        public string GetOtpVerifyDateTime() => GetValue<string>(OTPVerifyDateTime);

        // Set OTP verify datetime
        public UserProfile SetOtpVerifyDateTime(string? value)
        {
            SetValue(OTPVerifyDateTime, value);
            return this;
        }

        // Get chat enabled
        public bool GetChatEnabled() => GetValue<bool>(ChatEnabled);

        // Set OTP verify datetime
        public UserProfile SetChatEnabled(bool? value)
        {
            SetValue(ChatEnabled, value);
            return this;
        }

        // User has 2FA secret
        public bool HasUserSecret(bool verified = false)
        {
            try {
                string secret = GetSecret();
                bool valid = !Empty(secret); // Secret is not empty
                if (valid && verified) {
                    string verifyDateTime = GetSecretVerifyDateTime();
                    string verifyCode = GetSecretLastVerifyCode();
                    valid = !Empty(verifyDateTime) && !Empty(verifyCode);
                }
                return valid;
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Get User 2FA secret
        public async Task<string> GetUserSecret()
        {
            try {
                string secret = GetSecret();
                // Create new secret and save to profile
                if (Empty(secret)) {
                    secret = Current2FA.GenerateSecret();
                    List<string> backupCodes = Current2FA.GenerateBackupCodes();
                    await SetSecret(secret)
                        .SetSecretCreateDateTime(DbCurrentDateTime())
                        .SetBackupCodes(backupCodes)
                        .SaveToStorageAsync();
                }
                return secret;
            } catch {
                if (Config.Debug)
                    throw;
                return "";
            }
        }

        // Set one time passwword (Email/SMS)
        public async Task<bool> SetOneTimePassword(string account, string otp)
        {
            try {
                return await SetPassword(otp)
                    .SetOtpAccount(account)
                    .SetOtpCreateDateTime(DbCurrentDateTime())
                    .SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Get backup codes
        public List<string> GetBackupCodes()
        {
            try {
                List<string> codes = GetCodes().Split(',').ToList();
                return codes.Select(code => code.Length == Config.TwoFactorAuthenticationBackupCodeLength ? code : AesDecrypt(code)).ToList(); // Decrypt backup codes if necessary
            } catch {
                if (Config.Debug)
                    throw;
                return new();
            }
        }

        // Set backup codes to profile
        protected UserProfile SetBackupCodes(List<string> codes) {
            try {
                List<string> encryptedCodes = codes.Select(code => code.Length == Config.TwoFactorAuthenticationBackupCodeLength ? AesEncrypt(code) : code).ToList(); // Encrypt backup codes if necessary
                SetCodes(String.Join(",", encryptedCodes));
                return this;
            } catch {
                if (Config.Debug)
                    throw;
                return this;
            }
        }

        // Get new set of backup codes
        public async Task<List<string>> GetNewBackupCodes()
        {
            try {
                List<string> codes = Current2FA.GenerateBackupCodes();
                await SetBackupCodes(codes).SaveToStorageAsync();
                return codes;
            } catch {
                if (Config.Debug)
                    throw;
                return new();
            }
        }

        // Verify 2FA code
        public async Task<bool> Verify2FACode(string code)
        {
            try {
                string storedCode;
                if (SameText(Config.TwoFactorAuthenticationType, "google")) { // Check against secret
                    storedCode = GetSecret();
                } else { // Check against encrypted one time password
                    string secret = GetSecret();
                    storedCode = Decrypt(GetPassword(), secret);
                }
                if (!Empty(storedCode)) { // Stored code is not empty
                    bool valid = Current2FA.CheckCode(storedCode, code);
                    if (!valid && code.Length == Config.TwoFactorAuthenticationBackupCodeLength) { // Not valid, check if code is backup code
                        List<string> backupCodes = GetBackupCodes();
                        if (backupCodes.Contains(code)) {
                            backupCodes.Remove(code); // Remove used backup code
                            SetBackupCodes(backupCodes);
                            valid = true;
                        }
                    }
                    if (valid) { // Update verify date/time
                        SetSecretVerifyDateTime(DbCurrentDateTime()).SetSecretLastVerifyCode(code);
                        if (!SameText(Config.TwoFactorAuthenticationType, "google"))
                            SetOtpVerifyDateTime(DbCurrentDateTime()); // Set OTP verify date time
                        await SaveToStorageAsync();
                    }
                    return valid;
                }
                return false;
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }

        // Reset user secret
        public async Task<bool> ResetUserSecret()
        {
            try {
                return await SetSecret(null)
                    .SetSecretCreateDateTime(null)
                    .SetSecretVerifyDateTime(null)
                    .SetSecretLastVerifyCode(null)
                    .SetCodes(null)
                    .SaveToStorageAsync();
            } catch {
                if (Config.Debug)
                    throw;
                return false;
            }
        }
    }
    #pragma warning restore 169
} // End Partial class
