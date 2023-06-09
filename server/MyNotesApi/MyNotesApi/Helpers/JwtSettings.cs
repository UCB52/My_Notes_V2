﻿namespace MyNotesApi.Helpers
{
    public class JwtSettings
    {
        public const string JwtTokenSettings = "JwtTokenSettings";
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public string IssuerSigningKey { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool SaveToken { get; set; }
    }
}
