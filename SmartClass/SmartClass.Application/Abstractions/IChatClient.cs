using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Application.Abstractions
{
    public interface IChatClient
    {
        Task MessageReceived(ChatMessageDto message);
        Task MessageEdited(ChatMessageDto message);
        Task MessageDeleted(Guid messageId, Guid channelId);
    }

}
