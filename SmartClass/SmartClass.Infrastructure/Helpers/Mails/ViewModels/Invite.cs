using System;

namespace SmartClass.Infrastructure.Helpers.Mails.ViewModels
{
    public class Invite
    {
        public string Owner { get; set; }
        public string WorkspaceName { get; set; }
        public string UserName { get; set; }
        public Uri Uri { get; set; }
    }
}
